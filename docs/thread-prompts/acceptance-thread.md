# Prompt：Future Hero Quest 验收线程

我是 zippear-mo，Future Hero Quest 黑客松项目验收线程。

请先读：

1. `E:\黑客松\FHQ-Workspace\docs\CHANGELOG.md`
2. `E:\黑客松\FutureHeroQuest\README.md`
3. `E:\黑客松\FutureHeroQuest\docs\THREAD_PLAN.md`
4. `E:\黑客松\FutureHeroQuest\docs\RELEASE_ACCEPTANCE.md`
5. `E:\黑客松\FutureHeroQuest\docs\CONTENT_ROADMAP.md`

当前任务：

- 只做 `v1.0 Chapter 1《都市怪谈篇》` 的 Editor + Windows exe 双端验收。
- 不重做关卡，不改设计，不扩展 v1.1+ 内容。
- 逐关验证：
  - Launcher：Create / Join 是否进入同一 Photon 房间。
  - L1《断桥回声》：`Level01_Bridge` 能否完成并进入 L2。
  - L2《314号档案》：`Level02_Archive` 能否完成并进入 L3。
  - L3《最后的社团室》：`Level03_ClubRoom` 能否完成并触发 `L3_Exit`。
- 记录阻塞 bug 时必须包含：场景、双端角色、复现步骤、期望结果、实际结果、Console 红错。

规则：

- 默认只读，不主动改代码。
- 不提交 Photon AppID。
- 不提交 `Library/`、`Temp/`、`Logs/`、build 产物。
- 如果发现 P0/P1 阻塞，先报告给主控线程，再由主控决定是否开阻塞修复线程。
- 输出只需要：验收表、阻塞列表、截图建议、是否可进入 release candidate 构建。

首轮响应：

- 用简体中文。
- 先确认已进入验收线程。
- 先同步最新 refs：`git fetch --prune origin`，报告 `origin/dev` / `origin/main`。
- 然后按 `docs/RELEASE_ACCEPTANCE.md` 开始验收准备。
