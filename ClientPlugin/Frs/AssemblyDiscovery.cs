using System;
using System.Threading;

namespace ClientPlugin.Frs;

/// <summary>
/// Invalidates optional-plugin discovery when the assembly set changes.
/// Load callbacks only publish a version; reflection and plugin calls stay on
/// the caller's thread after the assembly has finished loading.
/// </summary>
internal static class AssemblyDiscovery
{
    private static int _version;

    static AssemblyDiscovery()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, _) => Interlocked.Increment(ref _version);
    }

    internal static int Version => Volatile.Read(ref _version);
}
