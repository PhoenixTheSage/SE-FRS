# Shared Graphics AA regression

Run from the repository root:

```powershell
dotnet run --project Tests/GraphicsAa/GraphicsAa.csproj -c Release -f net48
dotnet run --project Tests/GraphicsAa/GraphicsAa.csproj -c Release -f net10.0
```

Links the production GameAntiAliasing controller and choice enum. Thirty-two
checks cover guest and DX11 routes: plugin selection and rebinding, old event
cleanup, graphics preview/cancel/OK, custom-key remapping, native FXAA selection,
peer notification and bounce suppression, and unavailable capability filtering.
Game settings, controls, config persistence and hardware capability boundaries
are stand-ins. These checks do not render UI or evaluate the vendor SDK.

The existing Dx11Initialization fixture additionally loads the actual built
plugin, installs/uninstalls its Graphics AA hooks on the real game method
bodies while the route is Vulkan, verifies no DX11 companion loads, and checks
companion calls under local and Pulsar-renamed assembly identities. Run its
matrix on both runtimes as documented in Tests/DX11-INITIALIZATION.md.