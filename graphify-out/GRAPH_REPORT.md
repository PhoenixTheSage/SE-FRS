# Graph Report - SE-FRS  (2026-09-16)

## Corpus Check
- 152 files · ~318,344 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3040 nodes · 6786 edges · 129 communities (120 shown, 9 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 184 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- ffx core gpu common
- cbuffer
- ffx fsr2 callbacks glsl.h
- ffx core glsl.h
- Buffer
- ffx fsr2 compute luminance
- F F X M
- D3 D12 D X
- C D3 D X12
- Post Pp Hud Space
- Ffx Assert Callback
- ffx core gpu common.h
- Frs Runtime
- F S R R
- Assembly
- Config
- Ffx Pipeline State
- D3 D12 D E
- D3 D12 C A
- Checkbox
- Game Assemblies To Publicize
- Backend Context V K
- D3 D12 N O
- F S R2 Accumulate
- D3 D12 R A
- Frs Host
- D3 D12 P A
- ffx common types.h
- Hdr Uav Target
- Anti Aliasing Choice
- Ffx Fsr2 Context Private
- Jitter
- ffx fsr2 common.h
- Always
- D3 D12 R A 2
- Ffx Float Coords2 D
- Render Trace Bind
- Color
- Bilinear Sampling Data
- Ffx Resource
- A M D F
- Debug Log
- D3 D12 C L
- accquire Dynamic U B
- Preloader Helpers
- Fetched Bicubic Samples
- Element
- ffx fsr2 accumulate.h
- Ffx Command
- Anomaly Terminal Hook
- D3 D12 C O
- I D3 D11 Buffer
- Settings Generator
- I D3 D12 Descriptor
- Gpu Support
- Ffx Resource Flags
- I D3 D12 Resource
- F F X A
- Ffx Device
- Anti Aliasing Handshake
- Frs Mode
- Velocity Acceptance
- Foreign Upscaler Drs Patch
- F F X A 2
- R G B To
- ffx Get Vk Image
- Copy To Rt Patch
- Com Ptr
- Button
- Graphics Options Aa Patch
- Control Button Data
- ffx fsr2 postprocess lock
- ffx fsr2 upsample.h
- Assets amd frs.dll Deploy
- Button 2
- Device
- D3 D12 B L
- I D3 D12 Device
- String Builder
- Slider
- Ffx Fsr2 Generate Reactive
- Ffx Dimensions2 D
- I D3 D11 Device
- ffx fsr2 reproject.h
- F F X A 3
- Settings Screen
- Status Screen
- Stone Cloister Courtyard
- convert Format
- I D3 D11 Resource
- convert Format 2
- B O O L
- Dropdown
- Simple
- Tools
- F S R 2.2.1
- Ffx Create Resource Description
- Direct X Shaders
- Assembly Definition
- Client Plugin
- Button 3
- D3 D12 P I
- amd frs Shared Library
- F F X A 4
- Device Dispose Patch
- Update Screen Size Patch
- Binding
- D3 D12 B O
- D3 D12 R E
- ffx fsr2 lock.h
- ffx fsr2 reconstruct dilated
- Copilot Instruction to Read
- Get Screen Device Resolution
- D3 D12 C O 2
- F S R2 Sample
- F S R2 H
- F S R2 Reactive
- G Buffer Pass Begin
- Fsr2 Generate Reactive Constants
- Fsr2 Generate Reactive Constants2
- I D3 D11 Compute
- Microsoft. N E T.
- clean.sh
- Deploy.sh
- verify props.sh
- Create Locks
- Depth Clip
- Compute Luminance Pyramid
- Reconstruct Dilate

## God Nodes (most connected - your core abstractions)
1. `VKFunctionTable` - 81 edges
2. `FrsRuntime` - 69 edges
3. `BackendContext_VK` - 56 edges
4. `AnomalyHook` - 51 edges
5. `CD3DX12_PIPELINE_STATE_STREAM1` - 49 edges
6. `CD3DX12_PIPELINE_STATE_STREAM` - 46 edges
7. `FrsD3d` - 42 edges
8. `BillboardOutputPass` - 42 edges
9. `ClientPlugin.Frs` - 41 edges
10. `FfxFsr2DispatchDescription` - 37 edges

## Surprising Connections (you probably didn't know these)
- `Space Engineers FRS Plugin` --semantically_similar_to--> `FSR2 Quality Scaling Modes`  [INFERRED] [semantically similar]
  README.md → Native/third_party/FidelityFX-FSR2/README.md
- `AMD FSR 2.2.1` --semantically_similar_to--> `FidelityFX Super Resolution 2.2.1`  [INFERRED] [semantically similar]
  README.md → Native/third_party/FidelityFX-FSR2/README.md
- `FSR2 Accumulate Pass` --semantically_similar_to--> `Ghosting Acceptance`  [INFERRED] [semantically similar]
  Native/third_party/FidelityFX-FSR2/src/ffx-fsr2-api/vk/CMakeLists.txt → Tests/ANOMALY-ACCEPTANCE.md
- `HLSL Skill Overview` --semantically_similar_to--> `HLSL Skill Overview`  [INFERRED] [semantically similar]
  .agents/skills/a5c-ai-babysitter-hlsl/README.md → .cursor/skills/a5c-ai-babysitter-hlsl/README.md
- `DirectX Shaders` --semantically_similar_to--> `DX11 FXC cs_5_0 Shader Compile`  [INFERRED] [semantically similar]
  .agents/skills/a5c-ai-babysitter-hlsl/README.md → Native/third_party/FidelityFX-FSR2/src/ffx-fsr2-api/dx11/CMakeLists.txt

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **FSR2 Six-Stage Algorithm Pipeline** — native_third_party_fidelityfx_fsr2_readme_luminance_pyramid, native_third_party_fidelityfx_fsr2_readme_reconstruct_dilate, native_third_party_fidelityfx_fsr2_readme_depth_clip, native_third_party_fidelityfx_fsr2_readme_create_locks, native_third_party_fidelityfx_fsr2_readme_reproject_accumulate, native_third_party_fidelityfx_fsr2_readme_rcas [EXTRACTED 1.00]
- **SE-FRS D3D11 Native Host Stack** — readme_space_engineers_frs, readme_amd_frs_dll, readme_fsr_2_2_1, native_cmakelists_amd_frs, native_readme_amd_frs_host, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_dx11_cmakelists_dx11_backend [INFERRED 0.95]
- **Mirrored HLSL Skill Copies** — _agents_skills_a5c_ai_babysitter_hlsl_readme_hlsl_skill, _agents_skills_a5c_ai_babysitter_hlsl_skill_hlsl, _cursor_skills_a5c_ai_babysitter_hlsl_readme_hlsl_skill, _cursor_skills_a5c_ai_babysitter_hlsl_skill_hlsl [INFERRED 0.95]
- **FSR2 Vulkan Pass Shaders** — native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_tcr_autogen_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_autogen_reactive_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_accumulate_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_compute_luminance_pyramid_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_depth_clip_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_lock_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_reconstruct_previous_depth_pass, native_third_party_fidelityfx_fsr2_src_ffx_fsr2_api_vk_cmakelists_ffx_fsr2_rcas_pass [EXTRACTED 1.00]
- **In-Game Acceptance Suite** — tests_anomaly_acceptance_reprojection, tests_anomaly_acceptance_ghosting, tests_anomaly_acceptance_camera_cuts, tests_anomaly_acceptance_resolution, tests_anomaly_acceptance_source_probe_changes [EXTRACTED 1.00]
- **Velocity Consumption Contract** — tests_anomaly_acceptance_velocity_convention_15, tests_anomaly_acceptance_velocityprobe, tests_anomaly_acceptance_catalog_not_velocity_fallback, tests_anomaly_acceptance_fail_closed, tests_anomaly_acceptance_motion_vector_scale [EXTRACTED 1.00]
- **Some Settings Control Catalog** — docs_configdialogexample_some_settings, docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown [EXTRACTED 1.00]
- **More Settings Control Catalog** — docs_configdialogexample_more_settings, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind [EXTRACTED 1.00]
- **Config Dialog Widget Catalog** — docs_configdialogexample_config_demo, docs_configdialogexample_toggle, docs_configdialogexample_integer, docs_configdialogexample_number, docs_configdialogexample_text, docs_configdialogexample_dropdown, docs_configdialogexample_color, docs_configdialogexample_color_with_alpha, docs_configdialogexample_keybind, docs_configdialogexample_button [INFERRED 0.85]
- **FSR2 Quality Stress Features** — native_third_party_fidelityfx_fsr2_screenshot_green_woven_tapestry, native_third_party_fidelityfx_fsr2_screenshot_hanging_ivy, native_third_party_fidelityfx_fsr2_screenshot_romanesque_arches, native_third_party_fidelityfx_fsr2_screenshot_directional_sunlight [INFERRED 0.75]

## Communities (129 total, 9 thin omitted)

### Community 0 - "ffx core gpu common"
Cohesion: 0.08
Nodes (95): ffxApproximateReciprocalHalf(), ffxApproximateReciprocalSquareRootHalf(), ffxApproximateSqrtHalf(), ffxCopySignBitHalf(), ffxCubeRootHalf(), ffxFloatToSortableIntegerHalf(), ffxGammaFromLinearHalf(), ffxIsGreaterThanZeroHalf() (+87 more)

### Community 1 - "cbuffer"
Cohesion: 0.06
Nodes (80): cbuffer, FSR2_BIND_CB_FSR2, AutoExposure(), DeltaTime(), DeviceToViewSpaceTransformFactors(), DisplaySize(), DownscaleFactor(), DynamicResChangeFactor() (+72 more)

### Community 2 - "ffx fsr2 callbacks glsl.h"
Cohesion: 0.06
Nodes (80): AutoExposure(), cbFSR2_t(), DeltaTime(), DeviceToViewSpaceTransformFactors(), DisplaySize(), DownscaleFactor(), DynamicResChangeFactor(), Exposure() (+72 more)

### Community 3 - "ffx core glsl.h"
Cohesion: 0.09
Nodes (76): AShrSU1(), AWaveXorF1(), AWaveXorF2(), AWaveXorF3(), AWaveXorF4(), AWaveXorU1(), AWaveXorU2(), AWaveXorU3() (+68 more)

### Community 4 - "Buffer"
Cohesion: 0.07
Nodes (27): Buffer, FrsD3d, Device, DeviceContext, IntPtr, Resource, ShaderResourceView, UnorderedAccessView (+19 more)

### Community 5 - "ffx fsr2 compute luminance"
Cohesion: 0.12
Nodes (69): ComputeAutoExposure(), FfxFloat16x4, FfxFloat32x2, FfxFloat32x4, FfxInt32x2, FfxUInt32, FfxUInt32x3, SpdGetAtomicCounter() (+61 more)

### Community 6 - "F F X M"
Cohesion: 0.09
Nodes (69): FFX_MIN16_I, FFX_MIN16_I3, FFX_MIN16_I4, FFX_MIN16_U, FFX_MIN16_U3, FFX_MIN16_U4, AShrSU1(), AWaveXorF1() (+61 more)

### Community 7 - "D3 D12 D X"
Cohesion: 0.05
Nodes (38): D3D12_DXIL_LIBRARY_DESC, D3D12_DXIL_SUBOBJECT_TO_EXPORTS_ASSOCIATION, D3D12_EXISTING_COLLECTION_DESC, D3D12_EXPORT_DESC, D3D12_HIT_GROUP_DESC, D3D12_HIT_GROUP_TYPE, D3D12_SUBOBJECT_TO_EXPORTS_ASSOCIATION, ID3D12StateObject (+30 more)

### Community 8 - "C D3 D X12"
Cohesion: 0.04
Nodes (67): CD3DX12_PIPELINE_STATE_STREAM_BLEND_DESC, CD3DX12_PIPELINE_STATE_STREAM_CACHED_PSO, CD3DX12_PIPELINE_STATE_STREAM_CS, CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL1, CD3DX12_PIPELINE_STATE_STREAM_DEPTH_STENCIL_FORMAT, CD3DX12_PIPELINE_STATE_STREAM_DS, CD3DX12_PIPELINE_STATE_STREAM_FLAGS, CD3DX12_PIPELINE_STATE_STREAM_GS (+59 more)

### Community 9 - "Post Pp Hud Space"
Cohesion: 0.08
Nodes (19): PostPpHudSpace, List, MatrixD, MyBillboard, BillboardAddPatch, BillboardAddRangePatch, BillboardFrameCompletePatch, BillboardLdrPatch (+11 more)

### Community 10 - "Ffx Assert Callback"
Cohesion: 0.06
Nodes (47): FfxAssertCallback, Fsr2ShaderBlobVK, FfxFsr2Pass, Fsr2ShaderBlobDX12, fsr2GetAccumulatePassPermutationBlobByIndex(), fsr2GetAutogenReactivePassPermutationBlobByIndex(), fsr2GetComputeLuminancePyramidPassPermutationBlobByIndex(), fsr2GetDepthClipPassPermutationBlobByIndex() (+39 more)

### Community 11 - "ffx core gpu common.h"
Cohesion: 0.13
Nodes (60): ffxApproximateGamma2ToPQ(), ffxApproximateGamma2ToPQHigh(), ffxApproximateGamma2ToPQMedium(), ffxApproximateLinearToPQ(), ffxApproximateLinearToPQHigh(), ffxApproximateLinearToPQMedium(), ffxApproximatePQToGamma2Medium(), ffxApproximatePQToLinear() (+52 more)

### Community 12 - "Frs Runtime"
Cohesion: 0.06
Nodes (29): FrsRuntime, BindingContext, EvaluateCount, EvaluatedThisFrame, InternalHeight, InternalWidth, IsHdrSwapchainLive, IsLive (+21 more)

### Community 13 - "F S R R"
Cohesion: 0.08
Nodes (58): FSR_RCAS_PASSTHROUGH_ALPHA, ffxApproximateReciprocalMediumHalf(), ffxFsrEasuFloat(), ffxFsrPopulateEasuConstants(), ffxFsrPopulateEasuConstantsOffset(), FsrEasuH(), fsrEasuSetFloat(), FsrEasuSetH() (+50 more)

### Community 14 - "Assembly"
Cohesion: 0.08
Nodes (16): Assembly, AssemblyLoadEventArgs, AnomalyHook, CanNotifyUpscale, CatalogFound, ClaimedUpscale, HasDisplayTenant, RegistryFound (+8 more)

### Community 15 - "Config"
Cohesion: 0.06
Nodes (19): ShaderBytecode, ChromaticAberrationPatch, HarmonyPrefix, FxaaEnabledPatch, HarmonyPrefix, ClientPlugin, ClientPlugin.Frs, VRage.Render11.Resources (+11 more)

### Community 16 - "Ffx Pipeline State"
Cohesion: 0.04
Nodes (54): FfxPipelineState, DestroyBackendContextVK(), DestroyPipelineVK(), DestroyResourceVK(), VKFunctionTable, vkDestroyBuffer, vkDestroyDescriptorPool, vkDestroyDescriptorSetLayout (+46 more)

### Community 17 - "D3 D12 D E"
Cohesion: 0.09
Nodes (23): D3D12_DESCRIPTOR_RANGE, D3D12_DESCRIPTOR_RANGE1, D3D12_DESCRIPTOR_RANGE_TYPE, D3D12_ROOT_DESCRIPTOR_TABLE, D3D12_ROOT_DESCRIPTOR_TABLE1, D3D12_ROOT_PARAMETER, D3D12_ROOT_PARAMETER1, D3D12_ROOT_SIGNATURE_DESC (+15 more)

### Community 18 - "D3 D12 C A"
Cohesion: 0.07
Nodes (14): D3D12_CACHED_PIPELINE_STATE, D3D12_INDEX_BUFFER_STRIP_CUT_VALUE, D3D12_INPUT_LAYOUT_DESC, D3D12_PIPELINE_STATE_FLAGS, D3D12_PIPELINE_STATE_STREAM_DESC, D3D12_PRIMITIVE_TOPOLOGY_TYPE, D3D12_SHADER_BYTECODE, D3D12_STREAM_OUTPUT_DESC (+6 more)

### Community 19 - "Checkbox"
Cohesion: 0.05
Nodes (38): Attribute, CheckboxAttribute, SupportedTypes, Action, Func, List, Type, ColorAttribute (+30 more)

### Community 20 - "Game Assemblies To Publicize"
Cohesion: 0.09
Nodes (22): Hashing, CodeInstruction, IEnumerable, Instruction, MethodImpl, MethodInfo, CodeInstructionNotFound, TranspilerHelpers (+14 more)

### Community 21 - "Backend Context V K"
Cohesion: 0.05
Nodes (43): BackendContext_VK, allocatedPipelineLayoutCount, bufferMemoryBarriers, descPool, device, dstStageMask, extensionProperties, gpuJobCount (+35 more)

### Community 22 - "D3 D12 N O"
Cohesion: 0.09
Nodes (19): D3D12_NODE_MASK, D3D12_STATE_OBJECT_DESC, D3D12_STATE_OBJECT_TYPE, D3D12_STATE_SUBOBJECT, CD3DX12_NODE_MASK_SUBOBJECT, m_Desc, CD3DX12_STATE_OBJECT_DESC, m_Desc (+11 more)

### Community 23 - "F S R2 Accumulate"
Cohesion: 0.07
Nodes (42): FSR2 Accumulate Pass, ffx_fsr2_api_vk, FFX_FSR2_API_VK, FSR2 Autogen Reactive Pass, FSR2 Compute Luminance Pyramid Pass, FSR2 Depth Clip Pass, FSR2 Lock Pass, FSR2 RCAS Pass (+34 more)

### Community 24 - "D3 D12 R A"
Cohesion: 0.06
Nodes (29): D3D12_RANGE, D3D12_ROOT_CONSTANTS, D3D12_ROOT_DESCRIPTOR, D3D12_ROOT_DESCRIPTOR1, D3D12_STATIC_SAMPLER_DESC, D3D12_SUBRESOURCE_TILING, D3D12_TILE_REGION_SIZE, D3D12_TILE_SHAPE (+21 more)

### Community 25 - "Frs Host"
Cohesion: 0.09
Nodes (17): FrsHost, CurrentPresetHint, FeatureIsHdr, IsLoaded, IsReady, IsSupported, LastError, SupportKnown (+9 more)

### Community 26 - "D3 D12 P A"
Cohesion: 0.09
Nodes (22): D3D12_PACKED_MIP_INFO, D3D12_RANGE_UINT64, D3D12_RESOURCE_ALLOCATION_INFO, D3D12_RESOURCE_DESC, D3D12_RESOURCE_DIMENSION, D3D12_SUBRESOURCE_FOOTPRINT, D3D12_SUBRESOURCE_RANGE_UINT64, D3D12_TEXTURE_LAYOUT (+14 more)

### Community 27 - "ffx common types.h"
Cohesion: 0.13
Nodes (33): AShrSU1(), f32tof16(), ffxAsUInt32(), ffxDot2(), ffxDot3(), ffxDot4(), ffxFract(), ffxLerp() (+25 more)

### Community 28 - "Hdr Uav Target"
Cohesion: 0.07
Nodes (31): HdrUavTarget, Format, Linear, MipLevels, Name, Resource, Size, Size3 (+23 more)

### Community 29 - "Anti Aliasing Choice"
Cohesion: 0.15
Nodes (8): AntiAliasingChoice, DLSS, FRS, FXAA, Off, GameAntiAliasing, MyGraphicsSettings, MyGuiControlCombobox

### Community 30 - "Ffx Fsr2 Context Private"
Cohesion: 0.15
Nodes (30): FfxFsr2Context_Private, FfxDevice, FfxErrorCode, FfxPipelineState, FfxResourceInternal, createPipelineStates(), FfxFsr2Context, data (+22 more)

### Community 31 - "Jitter"
Cohesion: 0.08
Nodes (19): Jitter, FromFsr, HasPrevious, JitteredInvViewProjection, OffsetX, OffsetY, PreviousViewProjection, UnjitteredViewProjection (+11 more)

### Community 32 - "ffx fsr2 common.h"
Cohesion: 0.21
Nodes (28): ComputeAutoExposureFromLavg(), GetMaxDistanceInMeters(), GetPlaneFromPoints(), GetViewSpaceDepth(), GetViewSpaceDepthInMeters(), GetViewSpacePosition(), GetViewSpacePositionInMeters(), FFX_MIN16_F (+20 more)

### Community 33 - "Always"
Cohesion: 0.11
Nodes (17): _Always_, D3D12_CPU_DESCRIPTOR_HANDLE, D3D12_GPU_DESCRIPTOR_HANDLE, D3D12_PLACED_SUBRESOURCE_FOOTPRINT, D3D12_TEXTURE_COPY_LOCATION, D3D_ROOT_SIGNATURE_VERSION, INT, CD3DX12_CPU_DESCRIPTOR_HANDLE (+9 more)

### Community 34 - "D3 D12 R A 2"
Cohesion: 0.07
Nodes (11): D3D12_RAYTRACING_PIPELINE_CONFIG, D3D12_RAYTRACING_SHADER_CONFIG, D3D12_STATE_OBJECT_CONFIG, D3D12_STATE_OBJECT_FLAGS, D3D12_STATE_SUBOBJECT_TYPE, CD3DX12_RAYTRACING_PIPELINE_CONFIG_SUBOBJECT, m_Desc, CD3DX12_RAYTRACING_SHADER_CONFIG_SUBOBJECT (+3 more)

### Community 35 - "Ffx Float Coords2 D"
Cohesion: 0.07
Nodes (28): FfxFloatCoords2D, FfxFsr2DispatchDescription, autoReactiveMax, autoReactiveScale, autoTcScale, autoTcThreshold, cameraFar, cameraFovAngleVertical (+20 more)

### Community 36 - "Render Trace Bind"
Cohesion: 0.14
Nodes (11): RenderTraceBind, Exception, MethodInfo, ToneMappingPatch, Exception, HarmonyPostfix, HarmonyPrefix, HarmonyPriority (+3 more)

### Community 37 - "Color"
Cohesion: 0.09
Nodes (14): Layout, SettingsPanelSize, Func, List, MyGuiControlBase, Vector2, None, SettingsPanelSize (+6 more)

### Community 38 - "Bilinear Sampling Data"
Cohesion: 0.18
Nodes (25): BilinearSamplingData, ClampLoad(), ClampUv(), ComputeHrPosFromLrPos(), ComputeNdc(), GetBilinearSamplingData(), FFX_MIN16_I2, FFX_PARAMETER_OUT (+17 more)

### Community 39 - "Ffx Resource"
Cohesion: 0.13
Nodes (24): FfxResourceType, addBarrier(), FfxResource, FfxResourceStates, FfxSurfaceFormat, wchar_t, ffxGetBufferResourceVK(), ffxGetSurfaceFormatVK() (+16 more)

### Community 40 - "A M D F"
Cohesion: 0.15
Nodes (23): AMD_FRS_API, FfxFsr2MsgType, FfxFsr2QualityMode, FrsCreateDesc, FrsDispatchDesc, FfxErrorCode, FfxResource, ID3D11Resource (+15 more)

### Community 41 - "Debug Log"
Cohesion: 0.11
Nodes (14): DebugLog, FilePath, FrameSite, ApplySettingsPatch, HarmonyPrefix, BorrowCustomPatch, HarmonyPrefix, IBorrowedCustomTexture (+6 more)

### Community 42 - "D3 D12 C L"
Cohesion: 0.11
Nodes (19): D3D12_CLEAR_VALUE, D3D12_CPU_PAGE_PROPERTY, D3D12_HEAP_DESC, D3D12_HEAP_PROPERTIES, D3D12_HEAP_TYPE, D3D12_MEMORY_POOL, D3D12_RENDER_PASS_BEGINNING_ACCESS, D3D12_RENDER_PASS_BEGINNING_ACCESS_CLEAR_PARAMETERS (+11 more)

### Community 43 - "accquire Dynamic U B"
Cohesion: 0.10
Nodes (25): accquireDynamicUBO(), FfxCreateResourceDescription, FfxResourceUsage, CreateBackendContextVK(), CreateResourceVK(), findMemoryTypeIndex(), getVKBufferUsageFlagsFromResourceUsage(), getVKImageUsageFlagsFromResourceUsage() (+17 more)

### Community 44 - "Preloader Helpers"
Cohesion: 0.21
Nodes (9): PreloaderHelpers, CodeInstructionPredicate, Instruction, List, Collection, FieldReference, MethodDefinition, MethodReference (+1 more)

### Community 45 - "Fetched Bicubic Samples"
Cohesion: 0.23
Nodes (23): FetchedBicubicSamples, FetchedBicubicSamplesMin16, FetchedBilinearSamples, FetchedBilinearSamplesMin16, Bilinear(), ClampCoord(), FFX_MIN16_F, FFX_MIN16_F2 (+15 more)

### Community 46 - "Element"
Cohesion: 0.20
Nodes (22): Element, Path, _detect_pulsar_dir(), _detect_space_engineers(), _generate_guid(), _get_install_locations(), _get_linux_steam_path(), _get_steam_path() (+14 more)

### Community 47 - "ffx fsr2 accumulate.h"
Cohesion: 0.20
Nodes (21): Accumulate(), ComputeBaseAccumulationWeight(), ComputeLumaInstabilityFactor(), ComputeTemporalReactiveFactor(), FinalizeLockStatus(), GetPxHrVelocity(), AccumulationPassCommonParams, FFX_MIN16_F (+13 more)

### Community 48 - "Ffx Command"
Cohesion: 0.15
Nodes (21): FfxCommandList, FfxErrorCode, FfxGpuJobDescription, executeGpuJobClearFloat(), executeGpuJobCompute(), executeGpuJobCopy(), ExecuteGpuJobsVK(), ffxGetCommandListVK() (+13 more)

### Community 49 - "Anomaly Terminal Hook"
Cohesion: 0.16
Nodes (8): AnomalyTerminalHook, Installed, MethodInfo, Type, RichHudSession, MyObjectBuilder_SessionComponent, MySessionComponentBase, ParameterInfo

### Community 50 - "D3 D12 C O"
Cohesion: 0.16
Nodes (9): D3D12_COMPARISON_FUNC, D3D12_DEPTH_STENCIL_DESC, D3D12_DEPTH_STENCIL_DESC1, D3D12_DEPTH_WRITE_MASK, D3D12_RT_FORMAT_ARRAY, D3D12_STENCIL_OP, CD3DX12_DEPTH_STENCIL_DESC, CD3DX12_DEPTH_STENCIL_DESC1 (+1 more)

### Community 51 - "I D3 D11 Buffer"
Cohesion: 0.10
Nodes (20): ID3D11Buffer, ID3D11SamplerState, ID3D11ShaderResourceView, ID3D11UnorderedAccessView, BackendContext_DX11, constantBuffers, descHeapSrvCpu, descHeapUavCpu (+12 more)

### Community 52 - "Settings Generator"
Cohesion: 0.16
Nodes (11): AttributeInfo, SettingsGenerator, ActiveLayout, Dialog, Action, Func, List, MethodInfo (+3 more)

### Community 53 - "I D3 D12 Descriptor"
Cohesion: 0.11
Nodes (19): ID3D12DescriptorHeap, BackendContext_DX12, barrierCount, barriers, descHeapSrvCpu, descHeapUavCpu, descHeapUavGpu, descRingBuffer (+11 more)

### Community 54 - "Gpu Support"
Cohesion: 0.11
Nodes (13): GpuSupport, AdapterName, CanAttemptFrs, CanOfferFrs, FeatureLevel, IsAmd, IsNvidia, IsWarp (+5 more)

### Community 55 - "Ffx Resource Flags"
Cohesion: 0.11
Nodes (18): FfxResourceFlags, FfxResourceUsage, FfxSurfaceFormat, wchar_t, Fsr2ResourceDescription, flags, format, height (+10 more)

### Community 56 - "I D3 D12 Resource"
Cohesion: 0.14
Nodes (18): ID3D12Resource, addBarrier(), D3D12_RESOURCE_STATES, FfxResourceDescription, FfxResourceInternal, FfxResourceStates, DestroyResourceDX12(), ffxGetDX12ResourcePtr() (+10 more)

### Community 57 - "F F X A"
Cohesion: 0.22
Nodes (17): FFX_API, FfxDevice, FfxDeviceCapabilities, FfxErrorCode, FfxFsr2Interface, FfxFsr2Pass, FfxPipelineDescription, FfxPipelineState (+9 more)

### Community 58 - "Ffx Device"
Cohesion: 0.12
Nodes (18): FfxDevice, FfxDeviceCapabilities, FfxFsr2Pass, FfxPipelineDescription, CreatePipelineVK(), ffxGetDeviceVK(), getDefaultSubgroupSize(), GetDeviceCapabilitiesVK() (+10 more)

### Community 59 - "Anti Aliasing Handshake"
Cohesion: 0.14
Nodes (7): AntiAliasingHandshake, PeerAntiAliasing, CanOffer, GraphicsComboKey, Present, MethodInfo, Type

### Community 60 - "Frs Mode"
Cohesion: 0.16
Nodes (7): FrsMode, Balanced, NativeAA, Performance, Quality, UltraPerformance, RichHudOptions

### Community 61 - "Velocity Acceptance"
Cohesion: 0.15
Nodes (11): VelocityAcceptance, Buffer, Convention, Height, HistoryValid, IsAvailable, NativeResource, Width (+3 more)

### Community 62 - "Foreign Upscaler Drs Patch"
Cohesion: 0.17
Nodes (7): ForeignUpscalerDrsPatch, Harmony, Type, ConfigStorage, ConfigFilePath, Config, XmlSerializer

### Community 63 - "F F X A 2"
Cohesion: 0.19
Nodes (16): FFX_API, FfxDevice, FfxDeviceCapabilities, FfxFsr2Interface, FfxFsr2Pass, FfxPipelineDescription, FfxPipelineState, CreateBackendContextDX12() (+8 more)

### Community 64 - "R G B To"
Cohesion: 0.25
Nodes (16): RGBToYCoCg(), ComputeReprojectedUVs(), AccumulationPassCommonParams, FFX_PARAMETER_OUT, LockState, ReprojectHistoryColor(), ReprojectHistoryLockStatus(), ComputeAabbOverlap() (+8 more)

### Community 65 - "ffx Get Vk Image"
Cohesion: 0.12
Nodes (17): ffxGetVkImage(), ffxGetVkImageView(), Resource, allMipsImageView, aspectFlags, bufferResource, deviceMemory, imageResource (+9 more)

### Community 66 - "Copy To Rt Patch"
Cohesion: 0.19
Nodes (9): CopyToRtPatch, HarmonyPrefix, IRtvBindable, CameraUpdateScreenSizePatch, CameraUpdateViewportPatch, UpdateScreenSizePatch, HarmonyPrefix, MyCamera (+1 more)

### Community 67 - "Com Ptr"
Cohesion: 0.16
Nodes (6): ComPtr, ID3D12RootSignature, CD3DX12_GLOBAL_ROOT_SIGNATURE_SUBOBJECT, m_pRootSig, CD3DX12_LOCAL_ROOT_SIGNATURE_SUBOBJECT, m_pRootSig

### Community 68 - "Button"
Cohesion: 0.18
Nodes (15): Action Button, Color Picker, Color With Alpha, Config Demo Dialog, Dropdown, Integer Slider, Keybind Control, Label-Left Control-Right Row (+7 more)

### Community 69 - "Graphics Options Aa Patch"
Cohesion: 0.22
Nodes (9): GraphicsOptionsAaPatch, GraphicsOptionsClosePatch, HarmonyPostfix, HarmonyPrefix, MyGraphicsSettings, MyGuiScreenBase, FieldRef, HarmonyPatch (+1 more)

### Community 70 - "Control Button Data"
Cohesion: 0.25
Nodes (10): ControlButtonData, KeybindAttribute, SupportedTypes, Action, Func, List, Type, MyControl (+2 more)

### Community 71 - "ffx fsr2 postprocess lock"
Cohesion: 0.18
Nodes (13): GetShadingChangeLuma(), AccumulationPassCommonParams, FFX_MIN16_F4, FFX_MIN16_I2, FFX_PARAMETER_INOUT, FFX_PARAMETER_OUT, FfxFloat32, FfxFloat32x2 (+5 more)

### Community 72 - "ffx fsr2 upsample.h"
Cohesion: 0.23
Nodes (13): ComputeMaxKernelWeight(), ComputeUpsampledColorAndWeight(), Deringing(), GetUpsampleLanczosWeight(), AccumulationPassCommonParams, FFX_MIN16_F, FFX_MIN16_F2, FFX_PARAMETER_INOUT (+5 more)

### Community 73 - "Assets amd frs.dll Deploy"
Cohesion: 0.18
Nodes (13): Assets amd_frs.dll Deploy Note, amd-frs-2.2.1 GitHub Release Asset, AMD FRS Native Host, FidelityFX FSR 2.2 MIT License, FSR2 Quality Scaling Modes, Microsoft MIT License 2015, amd_frs.dll Native Host, FRS DLSS Anti-Aliasing Handshake (+5 more)

### Community 74 - "Button 2"
Cohesion: 0.17
Nodes (9): Button, Config, AntiAliasing, Enabled, EnabledCompat, Mode, Sharpness, INotifyPropertyChanged (+1 more)

### Community 75 - "Device"
Cohesion: 0.21
Nodes (6): Device, Plugin, Instance, Harmony, MethodImpl, IPlugin

### Community 76 - "D3 D12 B L"
Cohesion: 0.21
Nodes (7): D3D12_BLEND_DESC, D3D12_VIEW_INSTANCE_LOCATION, D3D12_VIEW_INSTANCING_DESC, D3D12_VIEW_INSTANCING_FLAGS, CD3DX12_BLEND_DESC, CD3DX12_DEFAULT, CD3DX12_VIEW_INSTANCING_DESC

### Community 77 - "I D3 D12 Device"
Cohesion: 0.32
Nodes (13): ID3D12Device, ID3D12GraphicsCommandList, FfxCommandList, FfxErrorCode, FfxGpuJobDescription, ID3D12CommandList, executeGpuJobClearFloat(), executeGpuJobCompute() (+5 more)

### Community 78 - "String Builder"
Cohesion: 0.26
Nodes (4): StringBuilder, FrsStatus, CurrentText, StringBuilder

### Community 79 - "Slider"
Cohesion: 0.18
Nodes (10): SliderAttribute, SupportedTypes, SliderType, Float, Integer, Action, Func, List (+2 more)

### Community 80 - "Ffx Fsr2 Generate Reactive"
Cohesion: 0.17
Nodes (12): FfxFsr2GenerateReactiveDescription, binaryValue, colorOpaqueOnly, colorPreUpscale, commandList, cutoffThreshold, flags, outReactive (+4 more)

### Community 81 - "Ffx Dimensions2 D"
Cohesion: 0.18
Nodes (11): FfxDimensions2D, FfxFsr2Message, FfxFsr2ContextDescription, callbacks, device, displaySize, flags, fpMessage (+3 more)

### Community 82 - "I D3 D11 Device"
Cohesion: 0.29
Nodes (11): ID3D11Device, ID3D11DeviceContext, FfxCommandList, FfxGpuJobDescription, HRESULT, executeGpuJobClearFloat(), executeGpuJobCompute(), executeGpuJobCopy() (+3 more)

### Community 83 - "ffx fsr2 reproject.h"
Cohesion: 0.31
Nodes (10): GetMotionVector(), FFX_MIN16_F4, FFX_MIN16_I2, FfxBoolean, FfxFloat32x2, FfxFloat32x4, FfxInt32x2, IsUvInside() (+2 more)

### Community 84 - "F F X A 3"
Cohesion: 0.22
Nodes (11): FFX_API, FfxFsr2Interface, FfxResourceDescription, FfxResourceInternal, ffxFsr2GetInterfaceVK(), ffxFsr2GetScratchMemorySizeVK(), GetResourceDescriptorVK(), loadVKFunctions() (+3 more)

### Community 85 - "Settings Screen"
Cohesion: 0.22
Nodes (6): SettingsScreen, Func, List, MyGuiControlBase, Vector2, MyGuiScreenBase

### Community 86 - "Status Screen"
Cohesion: 0.44
Nodes (3): StatusScreen, IEnumerable, StringBuilder

### Community 87 - "Stone Cloister Courtyard"
Cohesion: 0.29
Nodes (10): Stone Cloister Courtyard, Directional Courtyard Sunlight, FidelityFX Super Resolution 2, Gold Embroidered Trim, Green Woven Tapestry, Hanging Ivy Foliage, High-Frequency Temporal Reconstruction, Latin Arch Inscription (+2 more)

### Community 88 - "convert Format"
Cohesion: 0.27
Nodes (10): convertFormat(), DXGI_FORMAT, FfxResource, FfxResourceStates, FfxSurfaceFormat, wchar_t, ffxGetDX11FormatFromSurfaceFormat(), ffxGetResourceDX11() (+2 more)

### Community 89 - "I D3 D11 Resource"
Cohesion: 0.20
Nodes (10): ID3D11Resource, ffxGetDX11ResourcePtr(), getDX11ResourcePtr(), Resource, resourceDescriptor, resourcePtr, srvDescIndex, state (+2 more)

### Community 90 - "convert Format 2"
Cohesion: 0.27
Nodes (10): convertFormat(), DXGI_FORMAT, FfxResource, FfxSurfaceFormat, UINT, wchar_t, ffxGetDX12FormatFromSurfaceFormat(), ffxGetResourceDX12() (+2 more)

### Community 91 - "B O O L"
Cohesion: 0.28
Nodes (6): BOOL, D3D12_CONSERVATIVE_RASTERIZATION_MODE, D3D12_CULL_MODE, D3D12_FILL_MODE, D3D12_RASTERIZER_DESC, CD3DX12_RASTERIZER_DESC

### Community 92 - "Dropdown"
Cohesion: 0.28
Nodes (6): DropdownAttribute, SupportedTypes, Action, Func, List, Type

### Community 93 - "Simple"
Cohesion: 0.33
Nodes (7): Simple, SettingsPanelSize, List, MyGuiControlBase, Vector2, MyGuiControlParent, MyGuiControlScrollablePanel

### Community 94 - "Tools"
Cohesion: 0.31
Nodes (4): Tools, Color, Match, Regex

### Community 95 - "F S R 2.2.1"
Cohesion: 0.25
Nodes (9): FSR 2.2.1 Changelog, FSR2 Camera Jitter, FidelityFX Super Resolution 2.2.1, FSR2 Host API, FSR2 Motion Vectors, FSR2 Replaces Separate TAA, FSR 2.2.1 Release Notes, NativeAA (+1 more)

### Community 96 - "Ffx Create Resource Description"
Cohesion: 0.25
Nodes (9): FfxCreateResourceDescription, FfxResourceDescription, FfxResourceInternal, FfxResourceUsage, UINT, CreateResourceDX11(), DestroyResourceDX11(), ffxGetDX11BindFlags() (+1 more)

### Community 97 - "Direct X Shaders"
Cohesion: 0.25
Nodes (8): DirectX Shaders, HLSL Skill Overview, Constant Buffer Management, HLSL Skill, Microsoft HLSL Documentation, HLSL Skill Overview, HLSL Skill, DX11 FXC cs_5_0 Shader Compile

### Community 98 - "Assembly Definition"
Cohesion: 0.25
Nodes (4): AssemblyDefinition, IEnumerable, Preloader, TargetDLLs

### Community 99 - "Client Plugin"
Cohesion: 0.25
Nodes (6): ClientPlugin, net10.0, net48, Krafs.Publicizer (2.3.0), Lib.Harmony (2.4.2), Mono.Cecil (0.11.6)

### Community 100 - "Button 3"
Cohesion: 0.29
Nodes (6): ButtonAttribute, SupportedTypes, Action, Func, List, Type

### Community 101 - "D3 D12 P I"
Cohesion: 0.29
Nodes (6): D3D12_PIPELINE_STATE_SUBOBJECT_TYPE, InnerStructType, CD3DX12_PIPELINE_STATE_STREAM_SUBOBJECT, _Inner, _Type, D3DX12GetBaseSubobjectType()

### Community 102 - "amd frs Shared Library"
Cohesion: 0.25
Nodes (8): amd_frs Shared Library Target, Force FSR2 DX11 Backend Only, FidelityFX-FSR2-Unity-URP DX11 Backend, FSR2 Modular Backend, Robust Contrast Adaptive Sharpening, ffx_fsr2_api Core Library, FSR2 Shader Permutation Flags, ffx_fsr2_api DX11 Backend

### Community 103 - "F F X A 4"
Cohesion: 0.21
Nodes (8): FFX_API, FfxResource, ffxFsr2ResourceIsNull(), Fsr2SpdConstants, mips, numworkGroups, renderSize, workGroupOffset

### Community 104 - "Device Dispose Patch"
Cohesion: 0.33
Nodes (4): DeviceDisposePatch, Harmony, MethodInfo, DisposeBase

### Community 105 - "Update Screen Size Patch"
Cohesion: 0.29
Nodes (4): CameraViewportSizePatch, HarmonyPostfix, MethodBase, Vector2

### Community 106 - "Binding"
Cohesion: 0.48
Nodes (3): Binding, IMyInput, MyKeys

### Community 107 - "D3 D12 B O"
Cohesion: 0.38
Nodes (5): D3D12_BOX, D3D12_RECT, LONG, CD3DX12_BOX, CD3DX12_RECT

### Community 108 - "D3 D12 R E"
Cohesion: 0.29
Nodes (7): D3D12_RESOURCE_FLAGS, FfxCreateResourceDescription, FfxResourceUsage, HRESULT, CreateResourceDX12(), ffxGetDX12ResourceFlags(), TIF()

### Community 109 - "ffx fsr2 lock.h"
Cohesion: 0.48
Nodes (6): ClearResourcesForNextFrame(), ComputeLock(), ComputeThinFeatureConfidence(), FfxBoolean, FfxInt32x2, in

### Community 110 - "ffx fsr2 reconstruct dilated"
Cohesion: 0.52
Nodes (6): ComputeLockInputLuma(), FfxFloat32, FfxFloat32x2, FfxInt32x2, ReconstructAndDilate(), ReconstructPrevDepth()

### Community 111 - "Copilot Instruction to Read"
Cohesion: 0.40
Nodes (6): Copilot Instruction to Read AGENTS.md, VS Code Instruction to Read AGENTS.md, Anomaly Owns Shared Rendering Gaps, Rich HUD Slider Config Save Rule, se-dev Skill, Space Engineers Plugin Developer Role

### Community 112 - "Get Screen Device Resolution"
Cohesion: 0.40
Nodes (3): GetDeviceVSyncModePatch, GetScreenDeviceResolutionPatch, HarmonyPrefix

### Community 114 - "F S R2 Sample"
Cohesion: 0.33
Nodes (6): FSR2 Sample GitLab CI DX12 Build, FSR2 Sample GitLab CI Vulkan Build, FidelityFX clang-tidy Style, FSR2_Sample CMake Project, ffx_fsr2_api DX12 Backend, DX12 Wave32 and Wave64 Permutations

### Community 115 - "F S R2 H"
Cohesion: 0.33
Nodes (6): FSR2 HDR Support, FSR2 Camera Jump Cuts, Anomaly Shader Framework, ClaimUpscale se-frs, HdrRender Interaction, Rich HUD Master

### Community 116 - "F S R2 Reactive"
Cohesion: 0.33
Nodes (6): FSR2 Reactive Mask, Reproject and Accumulate, Reactivity Mask Subject to Change, D3D11 Blocks FSR 3.1 and FSR 4, AMD FSR 2.2.1, Anomaly Reactive Mask Catalog

### Community 117 - "G Buffer Pass Begin"
Cohesion: 0.40
Nodes (3): GBufferPassBeginPatch, HarmonyPrefix, MyGBufferPass

### Community 118 - "Fsr2 Generate Reactive Constants"
Cohesion: 0.40
Nodes (5): Fsr2GenerateReactiveConstants, binaryValue, flags, scale, threshold

### Community 119 - "Fsr2 Generate Reactive Constants2"
Cohesion: 0.40
Nodes (5): Fsr2GenerateReactiveConstants2, autoReactiveMax, autoReactiveScale, autoTcScale, autoTcThreshold

### Community 120 - "I D3 D11 Compute"
Cohesion: 0.67
Nodes (3): ID3D11ComputeShader, Pipeline_DX11, shader

## Ambiguous Edges - Review These
- `FSR2 Autogen Reactive Pass` → `Ghosting Acceptance`  [AMBIGUOUS]
  Tests/ANOMALY-ACCEPTANCE.md · relation: conceptually_related_to
- `More Settings Section` → `Action Button`  [AMBIGUOUS]
  Docs/ConfigDialogExample.png · relation: conceptually_related_to

## Knowledge Gaps
- **426 isolated node(s):** `net10.0`, `net48`, `Lib.Harmony (2.4.2)`, `Mono.Cecil (0.11.6)`, `Krafs.Publicizer (2.3.0)` (+421 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 811 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **9 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What is the exact relationship between `FSR2 Autogen Reactive Pass` and `Ghosting Acceptance`?**
  _Edge tagged AMBIGUOUS (relation: conceptually_related_to) - confidence is low._
- **What is the exact relationship between `More Settings Section` and `Action Button`?**
  _Edge tagged AMBIGUOUS (relation: conceptually_related_to) - confidence is low._
- **Why does `ClientPlugin.Frs` connect `Config` to `Buffer`, `Post Pp Hud Space`, `Update Screen Size Patch`, `String Builder`, `Dropdown`, `Get Screen Device Resolution`, `Velocity Acceptance`, `G Buffer Pass Begin`, `Anti Aliasing Handshake`, `Frs Mode`, `Anti Aliasing Choice`, `Jitter`?**
  _High betweenness centrality (0.032) - this node is a cross-community bridge._
- **Why does `VKFunctionTable` connect `Ffx Pipeline State` to `Ffx Resource`, `accquire Dynamic U B`, `Ffx Command`, `F F X A 3`, `Backend Context V K`, `Ffx Device`?**
  _High betweenness centrality (0.025) - this node is a cross-community bridge._
- **Why does `System.Runtime.CompilerServices` connect `Game Assemblies To Publicize` to `Checkbox`, `Config`?**
  _High betweenness centrality (0.021) - this node is a cross-community bridge._
- **What connects `net10.0`, `net48`, `Lib.Harmony (2.4.2)` to the rest of the system?**
  _426 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ffx core gpu common` be split into smaller, more focused modules?**
  _Cohesion score 0.07894736842105263 - nodes in this community are weakly interconnected._