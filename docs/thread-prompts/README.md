# Future Hero Quest 发布阶段 handoff prompts

更新时间：2026-05-02 12:05 UTC+8

当前阶段已经不再分派 L1 / L2 / L3 / art-audio 开发线程。那些 feature 分支已经由主控回收进 `origin/dev`。

现在只开发布阶段线程：

| 线程 | Prompt 文件 | 写权限 | 目标 |
|---|---|---|---|
| 验收线程 | `acceptance-thread.md` | 默认只读 | Editor + exe 双端跑通 Chapter 1《都市怪谈篇》 |
| 发布素材线程 | `release-assets-thread.md` | 文档/截图清单 | 准备 itch.io 页面、截图清单、上传说明 |
| 阻塞修复线程 | `blocker-fix-thread.md` | 仅明确 P0/P1 修复 | 修验收阻塞 bug，不重做关卡 |
| 主控备份线程 | `main-backup-thread.md` | 默认只读 | 主控线程死亡时接管发布状态 |

旧文件 `level1-bridge-thread.md`、`level2-archive-thread.md`、`level3-clubroom-thread.md`、`art-audio-thread.md` 保留为历史记录，不再作为当前任务分发。

## 当前原则

- `v1.0 Chapter 1《都市怪谈篇》` 是黑客松提交版，不是项目内容终点。
- DDL 前唯一目标是验收、构建、发布，不扩新关卡。
- UE5 / MCP 只是提交后可选技术探索，时间不足，不进入 v1.0。
- 不强推 `main` / `dev`。
- 不提交 Photon AppID、`Library/`、`Temp/`、`Logs/`、build 产物。
- 主 Unity 工作区可能被 Unity 或其他线程占用，切分支/重置前必须确认。
- 最终玩法代码只有 Editor + exe 双端验收通过后，才从 `dev` promote 到 `main`。
