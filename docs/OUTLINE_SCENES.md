# Outline Scene Prototype

本分支把 `关卡设计大纲.md` 压缩为一组可单独打开的 Unity Outline 场景，用于验证“效果优先”的剧情/玩法落点和关卡之间的伪一镜切换。

## 大纲压缩

| 段落 | 可执行效果 |
|---|---|
| 序幕 | 安全屋、DMAIL 信物、过去/未来双视角概念建立 |
| 第一关 | 坍塌实验楼通廊；过去修复支撑，未来通路从阻断变为可通行 |
| 第二关 | 山体隧道；接通坍塌节点后，未来出现碎块/追逐路线变化 |
| 第三关 | 档案室与吊车工地；读档案后释放预制通道，未来核心实验室通路出现 |
| 终幕 | 警告纸条/参数修正；废墟时间线切换为正常校园时间线 |

## 场景与锚点

| 场景 | 入口锚点 | 出口锚点 | 关键效果 |
|---|---|---|---|
| `Assets/Scenes/Outline/Scene00_Prologue_SafeHouse.unity` | 本地出生点 | `Anchor_Exit_CorridorDoor` | 交互后显示 DMAIL 信物和 Past/Future 概念面板 |
| `Assets/Scenes/Outline/Scene01_CollapsedCorridor.unity` | `Anchor_Entry_CorridorDoor` | `Anchor_Exit_TunnelMouth` | `E` 修复过去支撑后，未来坍塌阻挡消失、通路出现 |
| `Assets/Scenes/Outline/Scene02_MountainTunnel.unity` | `Anchor_Entry_TunnelMouth` | `Anchor_Exit_ArchiveIronDoor` | `E` 接通 N7 节点后，未来坍塌碎块和追逐改线路径出现 |
| `Assets/Scenes/Outline/Scene03_ArchiveCrane.unity` | `Anchor_Entry_ArchiveIronDoor` | `Anchor_Exit_CorePassage` | 先读档案，再释放吊车预制段，未来核心通路打开 |
| `Assets/Scenes/Outline/Scene04_FinalRewrite.unity` | `Anchor_Entry_CorePassage` | 无 | `E` 修正参数后，废墟 root 关闭，正常校园 root 打开 |

## 伪一镜切换

- `LevelBoundaryTransition` 负责出口触发器和 `E: continue` 提示。
- `LevelTransitionAnchor` 标记每个场景的入口/出口近景锚点。
- `OutlineSceneFlowController` 记录玩家相对出口锚点的位置/朝向，短黑场加载下一独立场景，再按目标锚点恢复相对构图。
- 每个场景都有相似的门框、隧道口或通道框架，保证切换时近景关系连续，但远景和关卡内容已经改变。
- Scene00 到 Scene03 的出口触发器默认关闭，必须完成本关关键交互后才会出现，避免玩家绕过效果直接切关。

## 可玩与占位

真实可玩：

- 所有 Outline 场景可单独打开。
- 每个场景都有离线本地玩家、灯光、地面/墙、出生点、关键交互点。
- Scene00 -> Scene03 可通过边界触发器连续切换，Scene04 为终幕。
- L1/L2/L3/终幕的关键效果用 `OutlineInteractable` 真实切换 GameObject 和阻挡碰撞体。
- `FHQ/Apply Outline Build Settings` 可临时把 Build Settings 切到 5 个 Outline 场景，方便本地从序幕连续试玩；发布构建前需要恢复原最终四场景列表。

占位：

- 第二关碎裂是预摆碎块/active 切换，不是 OpenFracture 真实运行时破碎。
- 第三关吊车是语义化释放，不是完整载具/物理吊装。
- 终幕是同场景两套 environment root 切换，不是长时段投影。
- Outline 线暂不接入 Photon 房间流程；它优先验证单场景编辑、局部效果和伪一镜体验。

## 验证

使用 Unity `6000.4.2f1` batchmode 执行：

```powershell
E:\unity\6000.4.2f1\Editor\Unity.exe -quit -batchmode -projectPath E:\黑客松\FHQ-Workspace\worktrees\level-outline-scenes -executeMethod FutureHeroQuest.EditorTools.FhqOutlineSceneBuilder.ValidateOutlineScenes
```

验证结果：5 个场景均可打开，锚点/本地玩家/流程控制器/边界触发器数量符合预期；4 条跨场景链接均确认源锚点和目标锚点存在。batchmode 日志没有 C# 编译错误；仅有既有 Photon obsolete warnings、Unity licensing access-token refresh 噪音和退出时的 curl abort 噪音。
