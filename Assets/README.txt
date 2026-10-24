Local builds: run Native\build.bat (writes amd_frs.dll here). ClientPlugin
deploy copies it next to the plugin in Pulsar Legacy\Local / Interim\Local.

Pulsar also downloads the published GitHub release from SpaceEngineersFRS.xml
<Asset Name="AmdFrs"> into Bin. That URL must stay live; a 404 shows as
Network! and the plugin never starts.

- amd_frs.dll
  Native FSR 2.2.1 DX11 host. Not committed to git. Release tag: amd-frs-2.2.1.

- amd_fidelityfx_vk.dll
  AMD-signed FidelityFX SDK 1.1.4 Vulkan runtime (FSR 3.1.4), MIT. Copy of
  Native/third_party/FidelityFX-SDK-1.1.4. Release tag: fidelityfx-vk-1.1.4.

- libamd_fidelityfx_vk.so
  Linux build of the same SDK 1.1.4 Vulkan runtime (FSR 3.1.4 / 2.3.3 upscale),
  built from source; see Native/third_party/FidelityFX-SDK-1.1.4/linux-build.
