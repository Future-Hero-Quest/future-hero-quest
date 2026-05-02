# Prompt：第 1 关 L1 Bridge 反馈式关卡强化

你是 Future Hero Quest 黑客松项目的第 1 关线程。请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FHQ-Workspace\docs\git-workflow.md`
3. `E:\黑客松\FHQ-Workspace\docs\thread-prompts\README.md`
4. Unity repo: `E:\黑客松\FutureHeroQuest`

当前公共快照：

```text
branch baseline: origin/dev
HEAD: 37eca84 chore(release): promote dev snapshot to main
Unity: 6.4.2f1
scene: Assets/Scenes/Level01_Bridge.unity
builder: Assets/Editor/FhqLevel01BridgeBuilder.cs
core scripts: Assets/Scripts/Core/SemanticStateStore.cs
level scripts: Assets/Scripts/Level/SemanticStateSender.cs, SemanticStateApplier.cs, SemanticReachZone.cs
```

请新建并只使用这个分支：

```powershell
cd E:\黑客松\FutureHeroQuest
git fetch origin
git switch -c feature/l1-bridge-feedback origin/dev
```

任务目标：保留现有 L1 Bridge，不推倒重建，把玩法表达强化成“未来玩家观察结果，反馈过去玩家修正动作”的黑箱推理闭环。

设计约束：

- 过去玩家 A/K 能操作桥梁维修点，例如 `RepairSupportA` / `RepairSupportB` / `RepairSupportC`。
- 未来玩家 B/M 只看到桥的最终状态，例如断桥、歪桥、可通桥、红/绿状态灯。
- B 通过语音或队内沟通反馈 A：哪一段仍然坏、哪一段修对了。
- A 再去调整维修点，直到未来桥可通。
- 同步只使用语义状态，不做刚体网络同步。

推荐状态表：

```text
StateKey: BridgeState
StateValue:
  Broken          // 初始：未来桥断，不能过
  HalfSupported   // 修了一处，未来看到桥歪/半通
  Supported       // 修对，未来桥完整，终点可达
```

可写范围：

- `Assets/Scenes/Level01_Bridge.unity`
- `Assets/Editor/FhqLevel01BridgeBuilder.cs`
- `Assets/LevelData/LevelData_Level01Bridge.asset`
- 必要时只小改 `Assets/Scripts/Level/*Semantic*.cs`

不要改：

- `Assets/Scenes/Level02_Archive.unity`
- `Assets/Scenes/Level03_ClubRoom.unity`
- `Assets/ThirdParty/**`
- Photon 配置、Build Settings，除非 zippear-mo 明确要求

验收标准：

- Unity Console 无红错。
- Editor 能加载 `Level01_Bridge.unity`。
- A 端触发维修点后，B 端通过 `BridgeState` 看到桥状态变化。
- `BridgeState=Supported` 前 B 不能正常过关；之后 `SemanticReachZone` 能触发过关。
- commit message 建议：`feat(level1): strengthen bridge feedback loop`
- push：`git push -u origin feature/l1-bridge-feedback`

完成后请汇报：

- 改了哪些文件。
- `BridgeState` 的完整状态表。
- 在 Unity 里如何手动验收。
- 是否有任何未解决红错或场景脏改动。

