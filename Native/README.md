# AMD FRS native host

Space Engineers is DX11. This folder builds `amd_frs.dll`, a small C ABI around
AMD FidelityFX Super Resolution **2.2.1** plus AMD's official DX11 backend patch
from [FidelityFX-FSR2-Unity-URP](https://github.com/GPUOpen-Effects/FidelityFX-FSR2-Unity-URP).

The DX11 backend files live under `third_party/FidelityFX-FSR2/src/ffx-fsr2-api/dx11/`
(from AMD's Unity URP patch). The Unity project itself is not vendored.

## Build

Requires Visual Studio 2022 (Desktop C++), CMake, and Windows SDK.

```bat
Native\build.bat
```

That writes `Assets\amd_frs.dll` (gitignored). Pulsar copies `Assets\*.dll` next to
the plugin on a local deploy. PluginHub downloads the same file from the GitHub
release named in `SpaceEngineersFRS.xml`.

## License

FSR 2.2.1 and the DX11 backend are MIT. See `third_party/FidelityFX-FSR2/LICENSE.txt`.
