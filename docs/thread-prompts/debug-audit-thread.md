# Prompt：Debug / 结构审查线程

你是 Future Hero Quest 黑客松项目的 Debug 审查线程。你的任务是找结构性 bug 和提交风险，默认先审查，不要大规模重构。

请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FHQ-Workspace\docs\git-workflow.md`
3. `E:\黑客松\FHQ-Workspace\docs\thread-prompts\README.md`
4. Unity repo: `E:\黑客松\FutureHeroQuest`

当前公共快照：

```text
repo: E:\黑客松\FutureHeroQuest
baseline: origin/dev
HEAD: 37eca84 chore(release): promote dev snapshot to main
origin/main = origin/dev = 37eca84
Unity: 6.4.2f1
current known status: Unity Console 暂无红错；Windows 包曾成功构建
```

请新建并只使用这个分支：

```powershell
cd E:\黑客松\FutureHeroQuest
git fetch origin
git switch -c feature/debug-structure-audit origin/dev
```

审查重点，按优先级：

1. **编译/红错**
   - 打开 Unity 后确认 Console error 为 0。
   - 检查是否有 missing script、missing prefab、missing material、scene load error。

2. **场景结构**
   - 检查 Build Settings 是否只有：
     - `Assets/Scenes/Launcher.unity`
     - `Assets/Scenes/Level01_Bridge.unity`
     - `Assets/Scenes/Level02_Archive.unity`
     - `Assets/Scenes/Level03_ClubRoom.unity`
   - 分别加载三关，确认 `LevelManager`、`PlayerSpawner`、终点触发器、语义状态对象存在。

3. **联网流程**
   - Launcher 是否能 Create Room / Join Room。
   - Editor + Windows exe 双端是否能进入同一关。
   - 不要求完全玩通，但要定位阻塞点。

4. **语义状态同步**
   - L1：`BridgeState`
   - L2：`KeyState`
   - L3：`BallResult` / `LockState`
   - 检查 key/value 拼写是否一致，sender/applier/reach zone 是否引用同一 key。

5. **提交/资源风险**
   - 检查 `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` 是否泄漏真实 Photon AppID。不要把 AppID 写进报告正文，只说“是否泄漏”。
   - 检查 `RAW/*.zip` 是否应该从 repo 移除或转到私有本地素材区；这些疑似角色原始包有授权/体积风险。
   - 检查 `Assets/ThirdParty/README.md` 和 `Assets/ThirdParty/Licenses/` 是否足够支撑 itch.io 授权说明。

6. **已知高风险点**
   - `FHQ/Build Windows Network Demo` 当前可能会调用 `GenerateNetworkDemo()` 并重写 `Launcher.unity` / `Level01_Tree.unity`。确认代码位置，并建议最小修复。
   - repo 根目录有 `TemporalPhysicsToolkit/`，Unity 工程内也有 `Assets/TemporalPhysicsToolkit/Runtime`；确认是否会重复编译。若 manifest 没引用根目录 package，说明它只是源码包备份。

可写范围：

- 如果只是审查，优先不要改文件。
- 如果发现一行级别确定 bug，可以小修并 commit。
- 不要改三关场景的大布局。
- 不要删除用户素材，除非 zippear-mo 明确确认。

输出格式：

```text
Findings:
1. [P0/P1/P2] 文件路径:行号 - 问题、影响、建议修复

Checked:
- Unity Console:
- Build Settings:
- Launcher flow:
- L1:
- L2:
- L3:
- Photon AppID leak:
- RAW/ThirdParty license:

Suggested next fixes:
1.
2.
3.
```

完成后：

- 如果没有改代码，只汇报，不 commit。
- 如果改了代码，commit message 用：`fix(debug): address structure audit findings`
- push：`git push -u origin feature/debug-structure-audit`

