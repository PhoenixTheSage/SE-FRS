# DX11 initialization regression

Build the shared project normally on Windows. It builds both the shared DLL and
the DX11 companion for Legacy (net48) and Interim (net10.0). Deployment copies
both DLLs. Linux continues to build only the shared guest plugin.

Run from the repository root after a Debug build, with Bin64 set to the game:

```powershell
foreach ($tfm in @('net48', 'net10.0')) {
    foreach ($mode in @('local', 'sources')) {
        dotnet run --project Tests/Dx11Initialization.csproj -f $tfm -- "ClientPlugin/bin/Debug/$tfm" $bin64 SpaceEngineersFRS $mode
        if ($LASTEXITCODE -ne 0) { throw "DX11 initialization regression failed" }
    }
}
```

The harness inspects and loads the actual built DLLs. The sources mode gives the
core a generated assembly identity, matching Pulsar's source compilation. Checks
cover empty Publicizer access attributes, live enum identity and internal calls,
one shared core, early native asset paths, readable initialization failures, and
refusal to load or invoke the device companion on pending/Vulkan routes.

The original failure was reproduced on CoreCLR by loading the pre-fix net48 DLLs:
an empty IgnoresAccessChecksTo attribute caused ArgumentException at JIT access
checks. Publicizer is unnecessary in the shared project. The companion explicitly
declares its VRage.Render11 access, including when the input is already publicized.
The companion reference is rebound in memory when Pulsar names the live core
differently; Legacy also needs an exact AssemblyResolve handler for that core.

These checks do not start Space Engineers, initialize NGX/FSR on a GPU, or prove
the visible Pulsar/Rich HUD controls. After restarting Pulsar, check FRS's AA
selection, Show Status, shared peer selection, and Rich HUD selection on DX11.
Check the Vulkan guest status/selection separately in a Vulkan session.
