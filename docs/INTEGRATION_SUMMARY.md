# DS-Temporal-Outline Integration Summary

## 整合目标

将 `level-outline-scenes`（Outline 场景原型）与 `temporal-fracture-integration`（Temporal Physics Toolkit）合并到 `ds-temporal-outline-integration` 分支，实现：

1. **Outline 场景框架**：5 个可单独打开的 Outline 场景 + SceneFlow 流程控制
2. **Temporal Physics Toolkit**：过去/未来双时间线物理投影系统
3. **Scene02 重点升级**：隧道塌方从 active-toggle 占位升级为 Temporal Projection 驱动

## 文件结构

```
Assets/
├── Editor/
│   └── FhqOutlineSceneBuilder.cs          # Outline 场景验证工具
├── Materials/
│   └── Outline/                            # Outline 场景专用材质
├── Scenes/
│   └── Outline/                            # 5 个 Outline 场景
│       ├── Scene00_Prologue_SafeHouse.unity
│       ├── Scene01_CollapsedCorridor.unity
│       ├── Scene02_MountainTunnel.unity    ← 重点升级目标
│       ├── Scene03_ArchiveCrane.unity
│       └── Scene04_FinalRewrite.unity
├── Scripts/
│   └── SceneFlow/                          # SceneFlow 框架
│       ├── OutlineInteractable.cs          # 原交互组件
│       ├── OutlineSceneFlowController.cs   # 场景流程控制器
│       ├── LevelBoundaryTransition.cs      # 边界切换触发器
│       ├── LevelTransitionAnchor.cs        # 锚点标记
│       ├── OutlineLocalPlayerController.cs # 本地玩家控制器
│       ├── TemporalOutlineInteractable.cs  # [NEW] Temporal 感知交互组件
│       └── TunnelCollapseController.cs     # [NEW] 隧道塌方控制器
└── TemporalPhysicsToolkit/                 # Temporal Physics Toolkit
    └── Runtime/
        ├── TemporalPhysicsProjector.cs     # 核心投影器
        ├── PastFutureTimelineController.cs # 过去/未来时间线控制器
        ├── TemporalPhysicsBody.cs          # 物理体标记
        ├── TemporalWorldState.cs           # 世界状态序列化
        ├── TemporalProjectionFractureOnCollision.cs  # 碰撞碎裂
        ├── TemporalProjectionRuntimeBehaviourAllowList.cs  # 运行时行为白名单
        ├── TemporalCameraFollow.cs         # 摄像机跟随
        ├── TemporalPastPlayerController.cs # 过去玩家控制器
        ├── TemporalSplitScreenFutureView.cs # 分屏未来视图
        ├── TemporalProjectionBoundary.cs   # 投影边界
        └── TemporalProjectionExclusion.cs  # 投影排除
```

## Scene02 升级方案

### 原方案（Outline 占位）
- 隧道塌方用 `OutlineInteractable` 的 `activateAfterUse` / `deactivateAfterUse` 直接切换预制体 active 状态
- 没有物理模拟，没有过去/未来时间线概念

### 升级方案
1. **`TunnelCollapseController`**：管理 N1-N7 隧道节点的 intact/collapsed 状态切换
   - 支持 `useTemporalProjection` 开关（true = 触发 Temporal Projection；false = active-toggle 回退）
   - 每个节点有 intactVisual、collapsedVisual、intactCollider、rubbleColliders
   - 节点被"wired"后触发 `PastFutureTimelineController.NotifyPastInfluenceEnded()`

2. **`TemporalOutlineInteractable`**：感知 Temporal 的交互组件
   - 作为 `OutlineInteractable` 的 companion 组件
   - 检测到交互完成后自动触发 Temporal Projection
   - 无 TimelineController 时回退到原始行为

3. **Temporal Physics Toolkit 集成**
   - `TemporalPhysicsProjector` 在投影场景中模拟物理
   - `PastFutureTimelineController` 管理过去/未来状态
   - 投影完成后通过 `OnProjectionCompleted` 事件将未来状态应用到场景

### 场景搭建指南

在 Scene02_MountainTunnel 中：

1. 添加 `TunnelCollapseController` 到场景根对象
2. 配置 N1-N7 节点：
   - `intactVisual`: 完整隧道段（默认 active）
   - `collapsedVisual`: 塌方碎块（默认 inactive）
   - `intactCollider`: 完整状态阻挡碰撞体
   - `rubbleColliders`: 塌方后可通行的碎块碰撞体
3. 每个节点放置一个 `OutlineInteractable` + `TemporalOutlineInteractable`
4. `OutlineInteractable` 的 `activateAfterUse` 留空（由 TunnelCollapseController 管理）
5. `OutlineInteractable` 的 `successMessage` 设为 `"Wired Nx"`
6. 添加 `PastFutureTimelineController` + `TemporalPhysicsProjector` 到场景
7. 将 `TunnelCollapseController.useTemporalProjection` 设为 true
8. 将 `TunnelCollapseController.timelineController` 指向场景中的 `PastFutureTimelineController`

## 验证

```powershell
E:\unity\6000.4.2f1\Editor\Unity.exe -quit -batchmode -projectPath E:\黑客松\FHQ-Workspace\worktrees\ds-temporal-outline-integration -executeMethod FutureHeroQuest.EditorTools.FhqOutlineSceneBuilder.ValidateOutlineScenes
```

## 已知限制

- Temporal Physics Toolkit 的完整运行时破碎（OpenFracture）尚未接入
- 分屏未来视图（TemporalSplitScreenFutureView）需要额外场景配置
- Photon 房间流程尚未接入 Outline 场景
