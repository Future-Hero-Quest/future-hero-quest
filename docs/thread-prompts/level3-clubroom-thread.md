# Prompt：第 3 关 L3 ClubRoom 台球黑箱反馈闭环

你是 Future Hero Quest 黑客松项目的第 3 关线程。请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FHQ-Workspace\docs\git-workflow.md`
3. `E:\黑客松\FHQ-Workspace\docs\thread-prompts\README.md`
4. Unity repo: `E:\黑客松\FutureHeroQuest`

当前公共快照：

```text
branch baseline: origin/dev
HEAD: 37eca84 chore(release): promote dev snapshot to main
Unity: 6.4.2f1
scene: Assets/Scenes/Level03_ClubRoom.unity
builder: Assets/Editor/FhqLevel03ClubRoomBuilder.cs
level data: Assets/Data/LevelData_Level03_ClubRoom.asset
known objects from previous check:
  BilliardsTable_L3_Billiards
  FinalDoor_LockState_Applier
```

请新建并只使用这个分支：

```powershell
cd E:\黑客松\FutureHeroQuest
git fetch origin
git switch -c feature/l3-clubroom-feedback origin/dev
```

任务目标：保留现有 L3 ClubRoom，把它整理成“过去选择台球动作，未来只看到门锁/球局结果，反馈过去调整”的最终关闭环。

设计约束：

- 不做完整台球物理。用 3 个固定击球选择点或触发区模拟结果。
- 过去玩家 A/K 选择击球线路，例如 `ShotA` / `ShotB` / `ShotC`。
- 未来玩家 B/M 看到最终结果：球袋、锁灯、门是否开。
- B 反馈 A：哪个球袋/灯是对的，A 重新选择。
- 正确结果打开最终门。这个关卡追求稳定演示，不追求复杂物理。

推荐状态表：

```text
StateKey: BallResult
StateValue:
  None       // 初始：门锁住
  Pocket_1   // 错误结果 1，未来红灯
  Pocket_2   // 错误结果 2，未来黄灯/半对
  Pocket_3   // 正确结果，未来绿灯

StateKey: LockState
StateValue:
  Locked
  Unlocked
```

可写范围：

- `Assets/Scenes/Level03_ClubRoom.unity`
- `Assets/Editor/FhqLevel03ClubRoomBuilder.cs`
- `Assets/Data/LevelData_Level03_ClubRoom.asset`
- 必要时只小改 `Assets/Scripts/Level/*Semantic*.cs`

不要改：

- `Assets/Scenes/Level01_Bridge.unity`
- `Assets/Scenes/Level02_Archive.unity`
- `Assets/ThirdParty/**`
- Photon 配置、Build Settings，除非 zippear-mo 明确要求

验收标准：

- Unity Console 无红错。
- Editor 能加载 `Level03_ClubRoom.unity`。
- 初始最终门锁住。
- A 端选择不同击球点后，B 端能看到不同 `BallResult`。
- `BallResult=Pocket_3` 或 `LockState=Unlocked` 后，最终门打开，出口可达。
- commit message 建议：`feat(level3): strengthen clubroom feedback loop`
- push：`git push -u origin feature/l3-clubroom-feedback`

完成后请汇报：

- 改了哪些文件。
- `BallResult` / `LockState` 的完整状态表。
- 玩家 A/B 的逐步操作流程。
- 是否有任何未解决红错或场景脏改动。

