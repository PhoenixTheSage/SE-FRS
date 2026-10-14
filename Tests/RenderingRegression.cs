using System.Reflection;
using System.Reflection.Emit;
using ClientPlugin.Frs;
using ClientPlugin.Patches;
using ClientPlugin.RichHud;
using VRageMath;
using VRage.Render11.Resources;
using VRageRender;

// Source-linked production control flow with test render/producer boundaries.
// Matrix operations use the game's actual VRage.Math. No GPU work is performed.
public static class RenderingRegression
{
    static int assertions;
    public static void Main()
    {
        CameraReprojection();
        HdrRouting();
        Discovery();
        Console.WriteLine($"All {assertions} rendering/discovery assertions passed; GPU acceptance remains manual.");
    }

    static void CameraReprojection()
    {
        foreach (var origin in new[] { Vector3D.Zero, new Vector3D(1e12, -1e12, 1e12) })
        {
            foreach (var movement in new[] { Vector3D.Zero, new Vector3D(1,0,0), new Vector3D(0,1,0), new Vector3D(.25,-.5,1) })
            {
                Jitter.Reset();
                var projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI/3, 16f/9, .05f, 1000f);
                var env = MakeEnvironment(origin, Matrix.Identity, projection);
                MyRender11.Environment.Matrices = env;
                Jitter.BeginFrame(); Jitter.Apply(env); Jitter.Restore(env);
                env.CameraPosition += movement;
                // A rotation as well as translation ensures the correction is
                // composed in world space before the previous view transform.
                env.ViewAt0 = Matrix.CreateRotationY(.08f);
                env.ViewProjectionAt0 = env.ViewAt0 * projection;
                env.InvViewProjectionAt0 = Matrix.Invert(env.ViewProjectionAt0);
                Jitter.BeginFrame(); Jitter.Apply(env);
                var currentRelative = new Vector4(2f,1f,-20f,1f);
                var expected = Vector4.Transform(new Vector4(
                    currentRelative.X + (float)movement.X,
                    currentRelative.Y + (float)movement.Y,
                    currentRelative.Z + (float)movement.Z, 1), projection);
                var actual = Vector4.Transform(currentRelative, Jitter.PreviousViewProjection);
                Assert(Near(actual,expected), "previous projection includes origin movement at " + origin + " / " + movement);
                var first = Jitter.PreviousViewProjection;
                Jitter.Restore(env); Jitter.Apply(env);
                Assert(Near(Vector4.Transform(currentRelative,Jitter.PreviousViewProjection),Vector4.Transform(currentRelative,first)), "scheduler re-entry does not accumulate movement");
                Jitter.Restore(env);
            }
        }
        Jitter.Reset();
        Assert(!Jitter.HasPrevious && Jitter.PreviousViewProjection == default(Matrix), "reset drops camera history");
    }

    static MyEnvironmentMatrices MakeEnvironment(Vector3D pos, Matrix view, Matrix projection) => new()
    {
        CameraPosition=pos, ViewAt0=view, Projection=projection, ProjectionForSkybox=projection,
        ViewProjectionAt0=view*projection, InvViewProjectionAt0=Matrix.Invert(view*projection),
        InvProjection=Matrix.Invert(projection), ViewD=view, ViewProjectionD=view*projection,
        InvViewProjectionD=MatrixD.Invert(view*projection), FovV=(float)Math.PI/3
    };
    static bool Near(Vector4 a,Vector4 b) => Math.Abs(a.X-b.X)<.0001 && Math.Abs(a.Y-b.Y)<.0001 && Math.Abs(a.Z-b.Z)<.0001 && Math.Abs(a.W-b.W)<.0001;

    static bool Prefix(ref IBorrowedCustomTexture result)
    {
        object[] args={result};
        var run=(bool)typeof(ToneMappingPatch).GetMethod("Prefix",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,args);
        result=(IBorrowedCustomTexture)args[0];
        return run;
    }
    static void HdrRouting()
    {
        foreach(var scenario in new[]{"temporal","spatial","missing-source","missing-target","exception","already-ready","already-unwritten","off"})
        {
            FrsRuntime.ResetTest();
            var sentinel=new TestTexture(); IBorrowedCustomTexture result=sentinel;
            var ready=new TestTexture();
            FrsRuntime.Evaluate=()=>
            {
                if(scenario=="exception")throw new InvalidOperationException("injected dispatch/copy failure");
                if(scenario=="missing-source" || scenario=="missing-target")return false;
                FrsRuntime.EvaluatedThisFrame=true; FrsRuntime.HdrOutputThisFrame=ready; return true;
            };
            if(scenario.StartsWith("already")) { FrsRuntime.EvaluatedThisFrame=true; FrsRuntime.HdrOutputThisFrame=scenario=="already-ready"?ready:null; }
            if(scenario=="off")FrsRuntime.IsLive=false;
            var run=Prefix(ref result);
            var good=scenario=="temporal" || scenario=="spatial" || scenario=="already-ready";
            Assert(run!=good,"HDR routing "+scenario);
            Assert(ReferenceEquals(result,good?ready:sentinel),"only written output replaces scene "+scenario);
            if(!good && scenario!="off") Assert(FrsRuntime.Suspended && !FrsRuntime.IsLive,"unusable HDR path relinquishes ownership "+scenario);
        }
    }

    static void Discovery()
    {
        AnomalyHook.Reset(); PeerAntiAliasing.Reset(); AnomalyTerminalHook.Reset(); FrsRuntime.IsLive=false;
        AnomalyHook.Probe();
        Assert(!AnomalyHook.RegistryFound && !PeerAntiAliasing.Present && !AnomalyTerminalHook.TryInstall(),"optional plugins initially absent");
        for(int i=0;i<100;i++) { AnomalyHook.Probe(); _=PeerAntiAliasing.GetChoice(); AnomalyTerminalHook.TryInstall(); }
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100;i++) { AnomalyHook.Probe(); _=PeerAntiAliasing.GetChoice(); AnomalyTerminalHook.TryInstall(); }
        Assert(GC.GetAllocatedBytesForCurrentThread()-before==0,"cached absent discovery allocates no bytes");

        var module=AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("LateReviewProducer"),AssemblyBuilderAccess.Run).DefineDynamicModule("main");
        var velocity=module.DefineType(AnomalyHook.RegistryTypeName,TypeAttributes.Public|TypeAttributes.Abstract|TypeAttributes.Sealed);
        var active=Forward(velocity,"get_Active",nameof(Producer.ReadActive),Type.EmptyTypes);
        var property=velocity.DefineProperty("Active",PropertyAttributes.None,typeof(object),Type.EmptyTypes); property.SetGetMethod(active); velocity.CreateType();
        var peer=module.DefineType(PeerAntiAliasing.TypeName,TypeAttributes.Public|TypeAttributes.Abstract|TypeAttributes.Sealed);
        Forward(peer,"CanOffer",nameof(Producer.CanOffer),Type.EmptyTypes);
        Forward(peer,"GetChoice",nameof(Producer.GetChoice),Type.EmptyTypes);
        Forward(peer,"ApplyChoice",nameof(Producer.ApplyChoice),new[]{typeof(string)}); peer.CreateType();
        var terminal=module.DefineType(AnomalyTerminalHook.RegistryTypeName,TypeAttributes.Public|TypeAttributes.Abstract|TypeAttributes.Sealed);
        Forward(terminal,"RequestFolderPage",nameof(Producer.RequestPage),new[]{typeof(string),typeof(string)}); terminal.CreateType();
        AnomalyHook.Probe();
        Assert(AnomalyHook.RegistryFound,"late Anomaly assembly discovered");
        Assert(PeerAntiAliasing.Present && PeerAntiAliasing.CanOffer && PeerAntiAliasing.GetChoice()=="FRS","late DLSS handshake discovered");
        PeerAntiAliasing.ApplyChoice("FXAA"); Assert(Producer.Choice=="FXAA","late peer receives settings");
        Set(typeof(AnomalyHook),"_anomalyConfigCurrent",typeof(Producer).GetField(nameof(Producer.Config)));
        Set(typeof(AnomalyHook),"_velocityProbe",typeof(ProbeConfig).GetProperty(nameof(ProbeConfig.VelocityProbe)));
        Set(typeof(AnomalyHook),"_velocitySource",typeof(ProbeConfig).GetProperty(nameof(ProbeConfig.VelocitySource)));
        Assert(AnomalyHook.TryGetLive(1280,720,out var pointer,out _) && pointer==new IntPtr(123),"late velocity consumed with validated metadata");
        Producer.ThrowRead=true;
        Assert(!AnomalyHook.TryGetLive(1280,720,out _,out _),"producer read failure fails closed");
        Producer.ThrowRead=false;
        Assert(!AnomalyHook.TryGetLive(1280,720,out _,out _),"failed binding retries are throttled");
        Set(typeof(AnomalyHook),"_lastBindingFailureTick",unchecked(System.Environment.TickCount-1001));
        Assert(AnomalyHook.TryGetLive(1280,720,out _,out _),"transient producer failure recovers without another assembly load");

        Assert(!AnomalyTerminalHook.TryInstall() && Producer.PageRequests==1,"late terminal registry waits for world readiness");
        for(int i=0;i<50;i++)AnomalyTerminalHook.TryInstall();
        Assert(Producer.PageRequests==1,"page readiness requests throttled");
        Producer.PageReady=true;
        Set(typeof(AnomalyTerminalHook),"_lastRequestTick",unchecked(System.Environment.TickCount-1001));
        Assert(AnomalyTerminalHook.TryInstall() && Producer.PageRequests==2,"terminal installs when world becomes ready");
        Assert(Producer.Page.Controls==3 && Producer.Page.Buttons==0,"terminal keeps three settings controls and leaves diagnostics on Status");
        AnomalyHook.Reset(); PeerAntiAliasing.Reset(); AnomalyTerminalHook.Reset();
        AnomalyHook.Probe();
        Assert(AnomalyHook.RegistryFound && PeerAntiAliasing.Present && AnomalyTerminalHook.TryInstall(),"reset rediscovers already loaded plugins");
    }
    static MethodBuilder Forward(TypeBuilder type,string name,string target,Type[] parameters)
    {
        var method=typeof(Producer).GetMethod(target);
        var builder=type.DefineMethod(name,MethodAttributes.Public|MethodAttributes.Static|MethodAttributes.SpecialName,method.ReturnType,parameters);
        var il=builder.GetILGenerator(); for(int i=0;i<parameters.Length;i++)il.Emit(OpCodes.Ldarg,i); il.Emit(OpCodes.Call,method); il.Emit(OpCodes.Ret);return builder;
    }
    static void Set(Type type,string name,object value)=>type.GetField(name,BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,value);
    static void Assert(bool ok,string name) { if(!ok)throw new Exception(name); assertions++;Console.WriteLine("PASS "+name); }
}
public static class Producer
{
    public static ProbeConfig Config=new(); public static bool ThrowRead,PageReady; public static int PageRequests; public static string Choice="FRS";
    public static TestPage Page=new();
    public static object ReadActive()=>ThrowRead?throw new InvalidOperationException("injected producer failure"):new VelocityBuffer();
    public static bool CanOffer()=>true;public static string GetChoice()=>Choice;public static void ApplyChoice(string value)=>Choice=value;
    public static object RequestPage(string folder,string page) { PageRequests++;return PageReady?Page:null; }
}
public class ProbeConfig { public int VelocityProbe=>0;public string VelocitySource=>"GBuffer"; }
public class VelocityBuffer { public bool IsAvailable=>true;public IntPtr NativeResource=>new(123);public int Width=>1280;public int Height=>720;public int Convention=>15;public bool HistoryValid=>true; }
public class TestPage
{
    public int Controls;
    public int Buttons;
    public void Category(string label) { }
    public void Dropdown(string label,Type type,Func<object> getter,Action<object> setter,string description) { Controls++; }
    public void Slider(string label,float min,float max,Func<float> getter,Action<float> setter,string description,float step) { Controls++; }
    public void Button(string label,Action action,string description) { Controls++; Buttons++; }
    public void Refresh() { }
}
public class TestTexture:IBorrowedCustomTexture { public Vector2I Size=>new(1280,720); public void Release() { } }
namespace VRage.Render11.Resources { public interface IResource { Vector2I Size {get;} } public interface ISrvBindable:IResource { } public interface IBorrowedCustomTexture:ISrvBindable { void Release(); } }
namespace VRageRender
{
    public class MyEnvironmentMatrices { public Vector3D CameraPosition;public Matrix Projection,ProjectionForSkybox,ViewAt0,ViewProjectionAt0,InvViewProjectionAt0,InvProjection;public MatrixD ViewD,ViewProjectionD,InvViewProjectionD;public float FovV; }
    public class Environment { public MyEnvironmentMatrices Matrices=new(); }
    public static class MyRender11 { public static Environment Environment=new();public static Context RC=new(); }
    public class Context { public void ClearState() { } }
    public static class MyToneMapping { public static void Run() { } }
}
namespace VRage.Utils { public class MyLog { public static MyLog Default=new();public void WriteLine(string s) { }public void Warning(string s) { }public void Error(string s) { } } }
namespace ClientPlugin.Frs
{
    public static class FrsRuntime
    {
        public static bool IsLive=true,WantsHdrEvaluate=true,EvaluatedThisFrame,Suspended;
        public static string LastBindingEvidence;public static int InternalWidth=1280,InternalHeight=720,OutputWidth=1920;
        public static IBorrowedCustomTexture HdrOutputThisFrame;public static Func<bool> Evaluate;
        public static Vector2I OutputResolution()=>new(1920,1080);public static Vector2I DesiredInternalResolution()=>new(1280,720);
        public static bool TryEvaluateHdrDisplay()=>Evaluate();public static void SuspendHdrEvaluate() { Suspended=true;IsLive=false;EvaluatedThisFrame=false; }
        public static IBorrowedCustomTexture AcquireLdrOutput()=>null;public static bool TryEvaluate(IResource a,ISrvBindable b)=>false;
        public static void NoteLdrEvaluate(IResource a,ISrvBindable b) { } public static void ApplyOutputSpace() { }
        public static void ResetTest() { IsLive=WantsHdrEvaluate=true;EvaluatedThisFrame=Suspended=false;HdrOutputThisFrame=null; }
    }
    public static class FrsHost { public static bool IsReady=>true;public static bool TryGetJitter(int index,int width,int output,out float x,out float y) { x=y=0;return true; } }
    public static class DebugLog { public static void Write(string s) { }public static void WriteFrame(string s) { } }
}
namespace ClientPlugin.RichHud
{
    public static class RichHudOptions
    {
        public static object GetAntiAliasing()=>AntiAliasingChoice.FRS;public static void SetAntiAliasing(AntiAliasingChoice value) { }
        public static object GetMode()=>FrsMode.Quality;public static void SetMode(FrsMode value) { }
        public static float GetSharpness()=>0;public static void SetSharpness(float value) { }public static void ShowStatus() { }
    }
}
namespace HarmonyLib
{
    public class HarmonyPatch:Attribute { public HarmonyPatch(Type type,string name) { } }
    public class HarmonyPrefix:Attribute { }public class HarmonyPostfix:Attribute { }public class HarmonyPriority:Attribute { public HarmonyPriority(int value) { } }
    public static class Priority { public const int First=800,Low=200; }
}
