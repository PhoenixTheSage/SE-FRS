# SE-FRS changelog

## 1.6.1 — 2026-10-14

- Rebind the DX11 companion to the live shared core in Pulsar source mode and reset assembly resolution on disposal.
- Serialize companion initialization, refuse device calls on pending/Vulkan routes, and retain readable missing-companion and invocation failures.
- Build and deploy matching Windows companions with isolated intermediates; remove unnecessary shared Publicizer access and preserve explicit device access grants.
- Add a dedicated renderer-neutral Status page, correct enum dropdown identity, and update initialization, acceptance, and rendering regressions for Legacy and Interim.

## 1.6.0 — previous release

Release 1.6.0 Vulkan FSR evaluate path

Version: 1.5.1 -> 1.6.0 (minor). DX11 stays on FSR 2.2.1.

Changelog:
- Move the DX11 FSR 2.2.1 device path into SpaceEngineersFRS.Dx11.dll.
- On Vulkan, ask the renderer to evaluate FSR 3.1.4 when amd_fidelityfx_vk.dll sits beside the plugin. FSR 4 is not used.
- Route the Anomaly host through RendererRoute. The temporal upscale remains the fallback when that library is absent.
- Keep the anti-aliasing handshake and quality dropdown on the shared plugin.

Co-authored-by: Cursor <cursoragent@cursor.com>
