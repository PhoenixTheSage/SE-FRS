# Rendering and discovery regression checks

The October 1, 2026 changes restore LDR billboard viewport/blend state after the
depth pass; compensate camera-only motion for camera-origin movement; regenerate
the baked motion shader to match the source's current-to-previous direction; and
keep a valid HDR scene on reconstruction failure. Optional-plugin absence is
cached until the assembly set changes. Failed Anomaly bindings and terminal page
readiness retry at a bounded interval.

## Automated checks

Run from the repository root, with the game `Bin64` path configured in
`Directory.Build.props.user`, or pass `-p:Bin64=path-to-game-Bin64` to the latter
two commands. All harnesses target .NET 10.

```powershell
dotnet run --project Tests/AnomalyAcceptance.csproj -c Release
dotnet run --project Tests/RenderingRegression.csproj -c Release
dotnet run --project Tests/ShaderReadback.csproj -c Release
```

- AnomalyAcceptance: 23 existing producer metadata/history assertions.
- RenderingRegression: 51 assertions using production Jitter, Anomaly/DLSS/terminal
  consumers and the tonemapping prefix. Matrix operations use the game's
  VRage.Math. Checks translation and rotation, camera positions at 1e12 units,
  repeated scheduler entry, reset, HDR success/failure routing, zero-allocation
  cached absence, real late assembly loads, transient binding recovery, and
  delayed terminal readiness/reset. HDR runtime/render boundaries are stubbed;
  these cases do not execute the production spatial-copy GPU path or inject an
  FSR dispatch failure.
- ShaderReadback: an isolated hardware D3D11 context executes the production
  FrsD3d helper and baked shader, then checks all 2,048 RG16F motion samples.
  Expected vectors are (0,0), (2,0), (0,-2), and (2,-2) in current-to-previous
  internal pixel units. This checks direction as well as magnitude. It does not
  attach to the game, run native FSR dispatch, or certify in-game image quality.

If the motion source changes, regenerate the baked shader before testing:

```powershell
./ClientPlugin/Frs/Shaders/BakeMotionShader.ps1
```

The camera-origin delta is subtracted in double precision and composed into
`PreviousViewProjection` on the CPU, preserving the 208-byte constant-buffer
layout. HLSL receives current-camera-relative reconstructed positions and an
already corrected previous projection. Accepted Anomaly velocity is unchanged.

## HDR recovery behavior

When temporal dispatch fails, FRS spatially scales the current pre-tonemap HDR
scene into its persistent fp16 output, then uses the existing AfterUpscale display
path. Show Status labels this `HDR spatial fallback hdrColor` or `HDR spatial
fallback LBuffer`. After three failures, temporal dispatch stops until settings
change, while the current scene continues through the spatial fallback.

If a usable source/output/context is unavailable, or copying throws, the prefix
returns control to the ordinary tonemapping chain and releases the Anomaly
upscale claim. FRS restores its internal DRS to output resolution on the next
DrawScene, avoiding a mid-tonemap GBuffer resize. Changing settings permits retry.
The prefix never allocates a replacement texture as a successful result after a
failed reconstruction. FSR dispatch counts exclude spatial copies.

## In-game acceptance still required

Use [ANOMALY-ACCEPTANCE.md](ANOMALY-ACCEPTANCE.md) for reprojection, ghosting, cuts,
source/probe changes, and resize. Also verify:

1. Quality/Performance LDR billboards and transparent HUD content with both a
   successful output-depth pass and forced depth fallback; NativeAA and FRS Off.
2. Camera strafing/flying/rotation over static voxels, including large world
   coordinates, with Anomaly disabled and with accepted Anomaly object velocity.
3. First/third FSR dispatch failures in HDR: the scene stays live, HDR peaks
   remain unclipped, and AfterUpscale runs once using the populated output.
4. Missing HDR input, failed destination creation, and copy exceptions: no
   undefined scene result, no retained upscaler claim, output DRS restored on the
   following frame, and successful retry after settings change.
5. Late Anomaly/DLSS loading, delayed world/Rich HUD readiness, and all four
   settings controls. Configuration saves remain debounced worker writes.

## Isolated CPU benchmark result

On .NET 10.0.11, the source-linked Release harness used seven batches of 2,000
calls after 2,000 warm-up calls. With 110 loaded assemblies (100 synthetic), the
median average for an absent Anomaly probe fell from 133.800 to 0.029 microseconds,
and allocations from 117,752 to zero bytes per call. An absent DLSS choice lookup
fell from 28.918 to 0.031 microseconds and from 22,024 to zero bytes. Initial scans
and scans following assembly loads still occur. These are isolated steady-state
CPU measurements, not game FPS or GPU frame-time claims.
