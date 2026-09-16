Local builds: run Native\build.bat (writes amd_frs.dll here). ClientPlugin
deploy copies it next to the plugin in Pulsar Legacy\Local / Interim\Local.

Do not add a GitHub <Asset> in SpaceEngineersFRS.xml until that release
exists. Pulsar downloads URL assets before loading a local folder plugin;
a 404 shows as Network! and the plugin never starts.

- amd_frs.dll
  Native FSR 2.2.1 DX11 host. Not committed to git.
