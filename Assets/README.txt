Local builds: run Native\build.bat (writes amd_frs.dll here). ClientPlugin
deploy copies it next to the plugin in Pulsar Legacy\Local / Interim\Local.

Pulsar also downloads the published GitHub release from SpaceEngineersFRS.xml
<Asset Name="AmdFrs"> into Bin. That URL must stay live; a 404 shows as
Network! and the plugin never starts.

- amd_frs.dll
  Native FSR 2.2.1 DX11 host. Not committed to git. Release tag: amd-frs-2.2.1.
