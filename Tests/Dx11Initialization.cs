using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

internal static class Dx11Initialization
{
    static int Main(string[] args)
    {
        string scratch = Path.Combine(Path.GetTempPath(), "SeDx11-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        try
        {
            string directory = Path.GetFullPath(args[0]);
            string game = Path.GetFullPath(args[1]);
            string name = args[2];
            string ns = name == "SpaceEngineersDLSS" ? "Dlss" : "Frs";
            bool sources = args.Length > 3 && args[3] == "sources";
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
                string file = new AssemblyName(e.Name).Name + ".dll";
                foreach (string root in new[] { directory, game, AppDomain.CurrentDomain.BaseDirectory })
                {
                    string path = Path.Combine(root, file);
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
                return null;
            };
            string corePath = Path.Combine(directory, name + ".dll");
            string devicePath = Path.Combine(directory, name + ".Dx11.dll");
            using (var resolver = new DefaultAssemblyResolver())
            {
                resolver.AddSearchDirectory(directory);
                resolver.AddSearchDirectory(game);
                foreach (string path in new[] { corePath, devicePath })
                {
                    using (var module = ModuleDefinition.ReadModule(path, new ReaderParameters { AssemblyResolver = resolver }))
                    {
                        Check(!module.Assembly.CustomAttributes.Any(a =>
                            a.AttributeType.Name == "IgnoresAccessChecksToAttribute" &&
                            string.IsNullOrWhiteSpace(a.ConstructorArguments[0].Value as string)),
                            Path.GetFileName(path) + " has no empty access-check attribute");
                        if (path == corePath)
                            Check(!module.AssemblyReferences.Any(r => r.Name == "VRage.Render11" || r.Name.StartsWith("SharpDX")),
                                "shared assembly has no DX11 references");
                        if (sources && path == corePath)
                        {
                            module.Assembly.Name.Name = name + ".PulsarRegression";
                            corePath = Path.Combine(scratch, "plugin.dll");
                            module.Write(corePath);
                        }
                    }
                }
            }
            var core = Assembly.LoadFrom(corePath);
            var bridge = core.GetType("ClientPlugin." + ns + ".Dx11Bridge", true);
            var call = bridge.GetMethods().Single(m => m.Name == "Call" && !m.IsGenericMethod);
            foreach (int route in new[] { 0, 2 })
            {
                ClientPlugin.Backend.RendererRoute.Active = route;
                call.Invoke(null, new object[] { "OnUpdate", new object[0] });
                Check(!(bool)bridge.GetProperty("Ready").GetValue(null), "route " + route + " keeps device bridge closed");
                string status = bridge.GetProperty("StatusText").GetValue(null) as string;
                Check(!string.IsNullOrWhiteSpace(status), "closed bridge has readable status");
                Check(!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name + ".Dx11"),
                    "route " + route + " never loads DX11 companion");
            }
            ClientPlugin.Backend.RendererRoute.Active = 1;
            var loader = core.GetType("ClientPlugin." + ns + ".SatelliteAssembly", true);
            var device = (Assembly)loader.GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { devicePath, core });
            var entry = device.GetType("ClientPlugin." + ns + ".Dx11Entry", true);
            var choice = core.GetType("ClientPlugin." + ns + ".AntiAliasingChoice", true);
            var displayed = entry.GetMethod("DisplayedChoice");
            Check(displayed.GetParameters()[0].ParameterType == choice,
                "companion binds the live core enum (" + (sources ? "Pulsar generated name" : "local name") + ")");
            object off = Enum.ToObject(choice, 0);
            Check(displayed.Invoke(null, new[] { off }).Equals(off), "DX11 selection executes across internal access checks");
            Check(!(bool)entry.GetMethod("FillCombo").Invoke(null, new object[] { null }), "unavailable combo is reported to the UI");
            var paths = (System.Collections.Generic.List<string>)core.GetType("ClientPlugin.Plugin", true)
                .GetField("SearchPaths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            paths.Add(scratch);
            entry.GetMethod("EnsurePaths", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            var nativePaths = (System.Collections.Generic.List<string>)device.GetType("ClientPlugin." + ns + ".FrsHost", true)
                .GetField("SearchPaths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Check(nativePaths.Contains(scratch, StringComparer.OrdinalIgnoreCase), "early native asset callback is replayed to the companion");
            Check(nativePaths.Contains(Path.GetDirectoryName(core.Location), StringComparer.OrdinalIgnoreCase), "native assets resolve beside the live core");
            entry.GetField("failed", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);
            entry.GetField("initializationError", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, "fixture startup failure");
            Check(((string)entry.GetMethod("StatusText").Invoke(null, null)).Contains("fixture startup failure"),
                "initialization failure remains readable in Show Status");
            entry.GetField("failed", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, false);
            device.GetType("ClientPlugin." + ns + ".AnomalyHook", true).GetMethod("SyncUpscaleClaim").Invoke(null, null);
            Check(!AppDomain.CurrentDomain.GetAssemblies().Any(a => sources && a.GetName().Name == name),
                "Pulsar path never loads a second disk core");
            // A cached companion must still be refused after the host closes DX11.
            bridge.GetField("entry", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, entry);
            ClientPlugin.Backend.RendererRoute.Active = 2;
            Check(!(bool)bridge.GetProperty("Ready").GetValue(null), "cached bridge reports closed on Vulkan");
            call.Invoke(null, new object[] { "OnUpdate", new object[0] });
            Check(!(bool)entry.GetField("routed", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null),
                "closed route cannot initialize an already loaded companion");
            Console.WriteLine("DX11 initialization regressions passed.");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
        finally
        {
            // Loaded assemblies remain locked on Windows until process exit.
            // Delete only this fixture, and only if its canonical path is in Temp.
            string root = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (Path.GetFullPath(scratch).StartsWith(root, StringComparison.OrdinalIgnoreCase))
                try { Directory.Delete(scratch, true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
        }
    }
    static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS " + name);
    }
}

namespace ClientPlugin.Backend
{
    public static class RendererRoute
    {
        public static int Active { get; set; }
        public static string ActiveName => Active == 1 ? "DX11" : Active == 2 ? "Vulkan" : "pending";
        public static bool UseDx11Device => Active == 1;
        public static bool UseGuestImages => Active == 2;
    }
}
