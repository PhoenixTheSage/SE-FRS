# Space Engineers FRS

Pulsar client plugin that adds **AMD FidelityFX Super Resolution** (branded **FRS**) and **NativeAA** to Space Engineers 1 (DX11). Frame Generation is not supported.

The native host is **FSR 2.2.1** plus AMD's official DX11 backend (MIT). FSR 3.1 / FSR 4 in the current FidelityFX SDK target DX12 and Vulkan, so they cannot run on Space Engineers' D3D11 device. FSR 2.2.1 is the latest fully open-source AMD temporal upscaler that does.

Settings live in the Pulsar plugin dialog. When [Anomaly Shader Framework](https://github.com/PhoenixTheSage/Anomaly) and [Rich HUD Master](https://steamcommunity.com/workshop/filedetails/?id=1965654081) are in the world, the same options appear under **Anomaly Shaders → FRS → Settings**. This plugin does not vendor a Rich HUD client.

## Requirements

- Space Engineers with [Pulsar](https://github.com/SpaceGT/Pulsar) 2.4.0 or later, Windows, Direct3D feature level 11_0 (NVIDIA, AMD, or Intel — not WARP)
- Native `amd_frs.dll` from `Native\build.bat` (writes `Assets/amd_frs.dll`, gitignored). Pulsar downloads the published [amd-frs-2.2.1](https://github.com/PhoenixTheSage/SE-FRS/releases/tag/amd-frs-2.2.1) release into Bin from `SpaceEngineersFRS.xml`. A local deploy also copies it next to the plugin in Pulsar `Local`. A 404 on that Asset URL shows as **Network!** and skips the plugin.

## Settings

Plugin config or **Options → Graphics → Anti-aliasing**:

- **Anti-aliasing** — Off, FXAA, FRS, and DLSS when [NVIDIA DLSS](https://github.com/PhoenixTheSage/SE-DLSS) is loaded and the GPU can offer it. Shared with **Options → Graphics** and with DLSS (Pulsar + Rich HUD). Only one upscaler can be selected.
- **Mode** — Quality (1.5×), Balanced (1.7×), Performance (2×), Ultra Performance (3×), or NativeAA (1.0× temporal AA)
- **Sharpness** — FSR 2 RCAS after upscale; 0 is off
- **Show Status** — GPU, FRS context, internal vs output resolution, Anomaly velocity / reactive / AfterUpscale

MSAA is not in the current graphics UI and is incompatible with FRS.

Motion vectors are camera-reprojected from depth unless [Anomaly Shader Framework](https://github.com/PhoenixTheSage/Anomaly) is also loaded. Anomaly is optional: the plugin attaches or detaches at runtime from type detection, with no config or terminal toggle ([shader developer wiki](https://github.com/PhoenixTheSage/Anomaly/wiki)):

- **Velocity** — `VelocityRegistry.Active` (object motion). Camera-from-depth remains the fallback.
- **Reactive mask** — catalog `reactiveMask`, bound as FSR 2 reactive when a pack marks pixels that must not use history.
- **AfterUpscale** — `ClaimUpscale("se-frs")` at init; `NotifyUpscaleComplete(rc, dest)` after evaluate. When `HasDisplayTenant`, dest is pre-tonemap HDR.
- **History** — `FrameTemporal.InvalidateHistory()` on camera cuts this plugin owns.

No compile-time Anomaly reference. FRS does not require an NVIDIA GPU; Anomaly itself does not either.

Optional [Rich HUD Master](https://steamcommunity.com/workshop/filedetails/?id=1965654081) plus Anomaly mirrors the Pulsar dialog as **Anomaly Shaders → FRS → Settings** via Anomaly's `TerminalConfigRegistry` (no compile-time Anomaly reference, no second Rich HUD client). Without Anomaly, use Pulsar MyGui. No PluginHub `DependencyIds`. See [Terminal config](https://github.com/PhoenixTheSage/Anomaly/wiki/Terminal-config).

## Native host

C# P/Invokes `amd_frs.dll`, a small C ABI around FSR 2.2.1 DX11 (`Native/`). Pulsar compiles only the C# plugin; build the native DLL separately with Visual Studio 2022 Desktop C++ and CMake (`Native\build.bat`).

The code fails closed on FSR 2 error codes and defaults to off.

## Building

- .NET Framework 4.8.1 targeting pack and .NET 10 SDK
- Visual Studio 2022 with Desktop C++ (for `amd_frs.dll`)
- Build `Native` first, then `ClientPlugin` (deploys to Pulsar `Legacy\Local` or `Interim\Local`; close the game if the DLL is in use)

Debug with Pulsar `Legacy.exe` / `Interim.exe` and `-sources`.

## License

`amd_frs.dll` links AMD FidelityFX FSR 2.2.1 (MIT). See [`Native/third_party/FidelityFX-FSR2/LICENSE.txt`](Native/third_party/FidelityFX-FSR2/LICENSE.txt). The DX11 backend is AMD's official patch from [FidelityFX-FSR2-Unity-URP](https://github.com/GPUOpen-Effects/FidelityFX-FSR2-Unity-URP) (also MIT).

The binary is served from the GitHub release named in `SpaceEngineersFRS.xml` (`Asset Name="AmdFrs"`), not from git.

## Known interactions

These plugins patch the same render-thread surfaces. FRS and DLSS handshake on anti-aliasing: each adds its option to the shared AA list (graphics, Pulsar, Rich HUD) and they keep one exclusive selection. A leftover DLSS selection on an unsupported GPU is coerced; enabling FRS selects FRS on both settings pages.

### DLSS

Overlap: anti-aliasing ownership and DRS. Well-known type `ClientPlugin.Frs.AntiAliasingHandshake` / `ClientPlugin.Dlss.AntiAliasingHandshake` (no compile-time reference). Graphics combo keys 101 (FRS) and 100 (DLSS). `ForeignUpscalerDrsPatch` still skips DLSS DRS while FRS owns it.

- **Safe now:** pick FRS or DLSS as the AA. Changing it in either plugin, Rich HUD, or Options → Graphics updates the other.

### HdrRender

Overlap: `MyToneMapping.Run`, `MyCopyToRT.Run`, and emissive billboards. HdrRender replaces Keen's SDR tone-map with an HDR path and owns the scRGB swapchain.

When Anomaly is loaded this plugin claims `ClaimUpscale("se-frs")` while FRS is live and **releases** the slot when anti-aliasing is Off or FXAA so a Display tenant can grade `LBuffer` without an upscaler (`CompleteDisplayWithoutUpscale`). If `HasDisplayTenant` (HdrRender-class BT.2390 registered AfterUpscale with `TemporalPolicy.Display`) **or** the swapchain is already scRGB, FRS skips Keen SDR and does not evaluate HdrRender's scRGB `MyToneMapping.Run` result. It evaluates catalog `hdrColor` (LBuffer) into an output-sized **fp16** dest (`R16G16B16A16_Float`) with FSR 2 HDR, then calls `NotifyUpscaleComplete(rc, dest)` so AfterUpscale reads `upscaledColor`. `CopyToRT` onto the scRGB swapchain yields when that Display tenant is present. PostPP / LDR billboards still draw onto the output dest without binding internal-res depth (Keen's `RenderPostPP` would pair mismatched DSV+RTV and drop the HUD). Show Status reports the last evaluate **path** (HDR hdrColor / HDR LBuffer / LDR tonemap) and color/dest formats; those persist across the Draw prefix so a pause dialog cannot relabel a successful HDR reconstruct as LDR.

Without a Display tenant, keep using one or the other. A HdrRender fork still has to `Register("hdr.tonemap", "AfterUpscale", …, Display)` and yield its tonemap prefix when `HasUpscaleConsumer`.

### SMAA

Overlap: anti-aliasing ownership. SMAA adds its own AA option; this plugin already shares the game's AA dropdown (Off / FXAA / FRS).

- **Safe now:** pick FRS or SMAA as the AA, not both.
- **Later — SMAA after FRS:** run SMAA at output resolution on the FRS LDR target (AfterUpscale). SMAA would need a public evaluate entry or an Anomaly-style owned pass.
- **Later — detect and yield:** if SMAA is loaded and selected, keep `WantsFrs` false.

### SmoothFrames

Overlap: render-thread camera interpolation plus this plugin's jitter on `DrawGameScene`. Interpolated camera vs jittered projection fights temporal history.

- **Safe now:** disable SmoothFrames camera interpolation while FRS is on.
- **Later — one temporal owner:** if SmoothFrames is interpolating, skip jitter; or if FRS is live, skip SmoothFrames interpolation.
- **Later — detect and yield:** skip `DrawGameSceneJitterPatch` when SmoothFrames types are present.

[Anomaly Shader Framework](https://github.com/PhoenixTheSage/Anomaly) is optional and complementary. It is discovered at runtime by type name (`VelocityRegistry`, `BufferCatalog`, `OwnedPassRegistry`, `FrameTemporal`). Packs that Harmony-patch `MyShader` or leave extra RT/SRV bound will fight Anomaly and can break Rich HUD.

## Bug reports

Open an issue with **Show Status** text, GPU, driver version, and `SpaceEngineers.log`.

Anomaly consumer verification and in-game acceptance procedure: [Tests/ANOMALY-ACCEPTANCE.md](Tests/ANOMALY-ACCEPTANCE.md). Numerical reprojection and ghosting acceptance remain pending in-game captures.
