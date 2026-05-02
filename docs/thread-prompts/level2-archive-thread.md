# Prompt：第 2 关 L2 Archive 档案反馈闭环

你是 Future Hero Quest 黑客松项目的第 2 关线程。请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FHQ-Workspace\docs\git-workflow.md`
3. `E:\黑客松\FHQ-Workspace\docs\thread-prompts\README.md`
4. Unity repo: `E:\黑客松\FutureHeroQuest`

当前公共快照：

```text
branch baseline: origin/dev
HEAD: 37eca84 chore(release): promote dev snapshot to main
Unity: 6.4.2f1
scene: Assets/Scenes/Level02_Archive.unity
builder: Assets/Editor/FhqLevel02ArchiveBuilder.cs
level data: Assets/Data/LevelData_Level02_Archive.asset
known objects from previous check:
  Future_Archive314_CluePoint
  Future_Door_KeyApplier
  Future_ExitReachZone
```

请新建并只使用这个分支：

```powershell
cd E:\黑客松\FutureHeroQuest
git fetch origin
git switch -c feature/l2-archive-feedback origin/dev
```

任务目标：保留现有 L2 Archive，把它整理成“未来看到门/档案结果，反馈过去放置正确线索”的合作闭环。

设计约束：

- 未来玩家 B/M 位于档案室未来侧，能看到锁门、编号、缺失档案或异常结果。
- 过去玩家 A/K 位于过去侧，能操作档案柜、线索点、钥匙/文件放置点。
- B 不能直接改过去，只能根据看到的结果告诉 A：例如“314 档案不对”“门还缺钥匙”“蓝色柜子亮了”。
- A 根据反馈选择正确档案/钥匙位置。
- 不做复杂 UI，不做自由输入密码；用固定交互点和颜色/编号提示。

推荐状态表：

```text
StateKey: KeyState
StateValue:
  Missing     // 初始：未来门锁住
  WrongFile   // A 放错档案，B 看到错误反馈
  Placed      // A 放对关键档案/钥匙
  Unlocked    // 未来门可开，出口可达
```

可写范围：

- `Assets/Scenes/Level02_Archive.unity`
- `Assets/Editor/FhqLevel02ArchiveBuilder.cs`
- `Assets/Data/LevelData_Level02_Archive.asset`
- 必要时只小改 `Assets/Scripts/Level/*Semantic*.cs`

不要改：

- `Assets/Scenes/Level01_Bridge.unity`
- `Assets/Scenes/Level03_ClubRoom.unity`
- `Assets/ThirdParty/**`
- Photon 配置、Build Settings，除非 zippear-mo 明确要求

验收标准：

- Unity Console 无红错。
- Editor 能加载 `Level02_Archive.unity`。
- 初始未来门不可过。
- A 端交互错误线索时，B 端能看到明显但不致命的错误反馈。
- A 端交互正确线索后，`KeyState=Placed/Unlocked`，B 端门打开，`Future_ExitReachZone` 可触发过关。
- commit message 建议：`feat(level2): strengthen archive feedback loop`
- push：`git push -u origin feature/l2-archive-feedback`

完成后请汇报：

- 改了哪些文件。
- `KeyState` 的完整状态表。
- 玩家 A/B 的逐步操作流程。
- 是否有任何未解决红错或场景脏改动。

