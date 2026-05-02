# Future Hero Quest · 团队文档目录

> 这里是公共可分享的项目文档。
> 如果你是新加入的队友，**强烈推荐**按以下顺序读：

| 顺序 | 文档 | 用时 | 用途 |
|------|------|------|------|
| 1 | [`../README.md`](../README.md) | 3 分钟 | 项目简介 + 快速开始 |
| 2 | [`../ONBOARD.md`](../ONBOARD.md) | 10 分钟 | 完整入坑指南（环境配置 + Git 流程 + Unity 设置） |
| 3 | [`ARCHITECTURE.md`](./ARCHITECTURE.md) | 10 分钟 | 技术架构总览 + 代码组织 |
| 4 | [`PLAN.md`](./PLAN.md) 附录 R | 5 分钟 | 最新关卡设计（v2 三方向） |
| 5 | [`thread-prompts/README.md`](./thread-prompts/README.md) | 3 分钟 | 多线程任务分工与 handoff prompts |
| 6 | [`PLAN.md`](./PLAN.md) 全文 | 30-60 分钟 | 完整设计文档（按需精读） |

---

## 文档分工

| 文档 | 维护者 | 更新频率 | 是否权威 |
|------|--------|---------|---------|
| `README.md`（仓库根） | 项目主 | 偶尔 | 是（项目门面） |
| `ONBOARD.md`（仓库根） | 项目主 | 偶尔 | 是（团队入口） |
| `docs/ARCHITECTURE.md` | 程序 | 架构变化时 | 是（技术权威） |
| `docs/PLAN.md` | 项目主 + 策划 | 设计调整时 | 是（设计权威） |
| `docs/thread-prompts/` | 项目主 + AI 主控线程 | 多线程分工变化时 | 是（线程 handoff 权威） |
| `docs/README.md`（本文件） | 任意人 | 加新文档时 | 是（导航） |

---

## 重要提示

- 📌 **PLAN.md 很长（65KB）**，不要尝试一次读完。用 IDE 的搜索 / 大纲面板按需查阅
- 📌 **决策记录**在 PLAN.md 各附录的开头，看附录 J/K/Q/R 即可掌握最新设计
- 📌 **ARCHITECTURE.md 是技术权威**，PLAN.md 里的代码示例如果跟它冲突，以 ARCHITECTURE 为准
- 📌 **个人的 chat session 日志、AI 对话上下文等私人内容不应该 push 到本仓库**

---

## 不在本仓库的资源

| 资源 | 位置 | 谁能访问 |
|------|------|---------|
| 美术原始素材（lowpoly 包等） | 个人本地 + Google Drive 链接 | 团队成员 |
| BGM / SFX 原文件 | 个人本地 + Google Drive 链接 | 团队成员 |
| Photon AppID | 项目主本地 PhotonServerSettings.asset | 项目主单人 |
| 个人工作区 / AI 对话日志 | 个人本地 | 个人 |

---

## 文档命名约定

- 大写 + 下划线：仓库根的关键文档（README.md, ONBOARD.md）
- 大写 + 短名：架构与计划文档（PLAN.md, ARCHITECTURE.md, CHANGELOG.md）
- 小写连字符：操作指南类（如 git-workflow.md, build-guide.md）
- 中文文档名：临时性 / 个人性内容
