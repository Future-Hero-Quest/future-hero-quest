# Prompt：美术与音频资源整合线程

你是 Future Hero Quest 黑客松项目的美术/音频线程。请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FHQ-Workspace\docs\git-workflow.md`
3. `E:\黑客松\FHQ-Workspace\docs\thread-prompts\README.md`
4. Unity repo: `E:\黑客松\FutureHeroQuest`

当前公共快照：

```text
branch baseline: origin/dev
HEAD: 37eca84 chore(release): promote dev snapshot to main
Unity: 6.4.2f1
resource root: Assets/ThirdParty
resource doc: Assets/ThirdParty/README.md
license root: Assets/ThirdParty/Licenses
target scenes:
  Assets/Scenes/Level01_Bridge.unity
  Assets/Scenes/Level02_Archive.unity
  Assets/Scenes/Level03_ClubRoom.unity
```

请新建并只使用这个分支：

```powershell
cd E:\黑客松\FutureHeroQuest
git fetch origin
git switch -c feature/art-audio-pass origin/dev
```

任务目标：在不破坏玩法触发器和场景逻辑的前提下，把三关从白盒/占位体提升到可展示的 low-poly 观感，并补 BGM/SFX。

设计约束：

- 优先使用现有 `Assets/ThirdParty` 资源，不再大量下载新资源。
- 如果必须新增资源，只能用 CC0 / MIT / 明确可商用免费授权，并把来源写进 `Assets/ThirdParty/README.md` 和 `Assets/ThirdParty/Licenses/`。
- 不改 Photon 配置，不改核心玩法脚本。
- 不移动关键触发器到会破坏验收的位置；如果要移动，必须同步调整碰撞体并说明。
- 不提交构建产物，不提交 `Library/`、`Temp/`、`Logs/`。

优先任务：

1. L1 Bridge：用低模建筑/自然/破损件替换纯 cube 占位；桥梁、维修点、终点要一眼能看懂。
2. L2 Archive：用柜子、书架、门、编号牌、钥匙/档案道具强化“档案室”主题。
3. L3 ClubRoom：用台球桌、球、门锁灯、俱乐部家具强化最终关主题。
4. 音频：
   - 全局或每关 1 个低音量 ambient/BGM loop。
   - L1 repair / bridge restore SFX。
   - L2 paper / door unlock SFX。
   - L3 ball / lock / final door SFX。

建议可写范围：

- `Assets/Scenes/Level01_Bridge.unity`
- `Assets/Scenes/Level02_Archive.unity`
- `Assets/Scenes/Level03_ClubRoom.unity`
- `Assets/Materials/**`
- `Assets/Prefabs/Art/**` 或新建该目录
- `Assets/Audio/**` 或新建该目录
- `Assets/ThirdParty/README.md`
- `Assets/ThirdParty/Licenses/**`

不要改：

- `Assets/Scripts/Core/**`
- `Assets/Scripts/Level/**`
- `ProjectSettings/EditorBuildSettings.asset`
- Photon Server Settings

验收标准：

- Unity Console 无红错。
- 三个目标场景都能加载。
- 玩法触发器仍可见或可理解，不被装饰物挡住。
- 每个第三方资源都有来源/授权说明。
- commit message 建议：`art(audio): polish levels with lowpoly assets`
- push：`git push -u origin feature/art-audio-pass`

完成后请汇报：

- 改了哪些场景和资源。
- 每关使用了哪些资源包。
- BGM/SFX 放在哪些对象上。
- 是否移动过任何 gameplay trigger/collider。

