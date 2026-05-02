# Onboard 指南 — Future Hero Quest

> 给新加入的队友看：从 0 到第一次 push，约 30 分钟。

## 你需要的东西

- GitHub 账号 + 已收到 `Future-Hero-Quest` 组织的协作邀请
- Windows 或 Mac 电脑（Linux 也行）
- 至少 15GB 可用磁盘空间（Unity + 项目）
- 联网网速 > 5MB/s（下载 Unity 用）

## Step 1 · 接受 GitHub 邀请（30 秒）

收件箱搜索 `Future-Hero-Quest invited you to collaborate` 邮件。
点 **View invitation** → **Accept invitation**。

如果没收到邮件：登录 GitHub → 点右上角铃铛 → 看通知列表。

## Step 2 · 装开发环境（15-30 分钟）

### Git
- Windows: 装 [Git for Windows](https://git-scm.com/download/win)（默认勾选项即可）
- Mac: 终端跑 `xcode-select --install`

### Unity
- 装 [Unity Hub](https://unity.com/download)
- 在 Hub 里安装 **Unity 6.4.2f1 / 6000.4.2f1**（当前工程版本；不要用 2022.3 打开，Unity 不支持降级）
- 安装时勾选这些 Module：
  - ✅ **WebGL Build Support**（最终交付用）
  - ✅ **Windows Build Support (IL2CPP)**（PC 队友）
  - ✅ **Mac Build Support (IL2CPP)**（Mac 队友）
  - ✅ **Documentation**

下载约 5-8GB，需要 30-45 分钟。

## Step 3 · Clone 仓库（1 分钟）

打开终端（Windows 用 PowerShell 或 Git Bash），cd 到你想存放项目的目录：

```bash
git clone https://github.com/Future-Hero-Quest/future-hero-quest.git
cd future-hero-quest
git checkout dev

# 推荐配全局 user.name/email（一次配，所有仓库都用）
git config --global user.name "你的真实名字或昵称"
git config --global user.email "你的GitHub注册邮箱"
git config --global --list   # 验证

# 如果不想配全局, 把 --global 去掉就只对本仓库生效
```

## Step 4 · 用 Unity 打开项目（5 分钟）

1. 打开 Unity Hub
2. 点 **Open** → 选择刚 clone 的 `future-hero-quest` 文件夹
3. 第一次打开 Unity 会编译 5-10 分钟
4. 编译完后：`Edit → Project Settings → Editor`
   - 确认 `Asset Serialization Mode` = **Force Text**
   - 确认 `Version Control` = **Visible Meta Files**

## Step 5 · 配置 Unity Smart Merge（防场景文件冲突 · 3 分钟）

找到你的 Unity Editor 安装路径（默认）：
- Windows: `E:\unity\6000.4.2f1\Editor\Data\Tools\UnityYAMLMerge.exe`（或你自己的 Unity 6.4 安装路径）
- Mac: `/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/Tools/UnityYAMLMerge`

在仓库根目录跑（路径按实际改）：

**Windows:**
```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'E:/unity/6000.4.2f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```

**Mac:**
```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
```

## Step 6 · 验证你能 push（1 分钟）

```bash
# 在 dev 分支
echo "你的名字" >> CONTRIBUTORS.md
git add CONTRIBUTORS.md
git commit -m "chore: add 你的名字 to contributors"
git push
```

第一次 push 时：
- Windows 会自动弹浏览器让你登录 GitHub 授权
- Mac 用 macOS Keychain 存凭据，可能需要输入一次
- 之后再也不用登录

如果 push 成功 → ✅ Onboard 完成！

## 日常工作流

### 早上开工
```bash
git checkout dev
git pull --rebase
```

### 提交代码
```bash
git add .
git commit -m "feat: 描述你做了啥"
git push

# 如果 push 被拒绝（队友先推了）：
git pull --rebase
git push
```

### 改 commit message 规范
| 前缀 | 用法 | 示例 |
|------|------|------|
| `feat:` | 新功能 | `feat: 第一关断桥回声触发器` |
| `fix:` | bug 修复 | `fix: 联网时角色穿模` |
| `art:` | 美术资源 | `art: 导入 Synty 校园场景` |
| `level:` | 关卡数据 | `level: 调整断桥回声交互点` |
| `chore:` | 杂活/配置 | `chore: 升级 Photon` |
| `docs:` | 文档 | `docs: 更新 README` |

### 高风险实验（用完即焚）
```bash
git checkout -b experimental/synty-import
# 试...
# 成功：git checkout dev && git merge experimental/synty-import --squash
# 失败：git checkout dev && git branch -D experimental/synty-import
```

## 出问题时怎么办

| 症状 | 怎么救 |
|------|-------|
| commit 一直被拒绝 | 99% 是 user.name/email 没配（回 Step 3） |
| push 一直被拒绝 | 先 `git pull --rebase` 再 `git push` |
| rebase 冲突 | `git rebase --abort` 取消，找冲突方协调 |
| 场景文件冲突 | 喊出冲突方，约定一个人保留改动，另一个手动重做 |
| Unity 一直转圈 | 关掉 Unity，删除 `Library/` 目录，重新打开会重新生成（10 分钟） |
| 不知道发生了什么 | `git reflog` 看历史，`git reset --hard <hash>` 回退到任意时刻 |

## 关键时间节点（v1.0《都市怪谈篇》）

| 节点 | 时间 | 必须达成 |
|------|------|---------|
| T+6h | 5/2 上午 | 联网双人胶囊体跑通 |
| T+18h | 5/2 下午 | 第 1 关《断桥回声》闭环 |
| T+30h | 5/3 凌晨 | 第 2 关《314号档案》闭环 |
| T+38h | 5/3 上午 | 第 3 关《最后的社团室》闭环 |
| T+40h | 5/3 中午 | 美术整合完成 |
| T+44h | 5/3 下午 | 必须开始打包 |
| **DDL** | **5/3 19:00** | **itch.io 提交** |

> 当前提交版三关以 `README.md` 和 `docs/THREAD_PLAN.md` 为准。旧 `PLAN.md` 中的早期关卡方案保留为历史参考。

## 必读文档（按优先级）

| 优先 | 文档 | 用时 | 用途 |
|------|------|------|------|
| 🔥 | [`README.md`](./README.md) | 3 分钟 | 项目简介 + Git 工作流 |
| 🔥 | [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md) | 10 分钟 | **技术架构总览**（程序必读） |
| ⭐ | [`docs/THREAD_PLAN.md`](./docs/THREAD_PLAN.md) | 5 分钟 | 当前发布线程计划与里程碑 |
| ⭐ | [`docs/CONTENT_ROADMAP.md`](./docs/CONTENT_ROADMAP.md) | 5 分钟 | v1.0 后内容方向 |
| 📖 | [`docs/PLAN.md`](./docs/PLAN.md) 全文 | 30-60 分钟 | 完整设计（按需精读，不要一次读完） |
| 📖 | [`docs/README.md`](./docs/README.md) | 1 分钟 | 文档目录导航 |

## 团队角色与脚本所有权

参考 `docs/ARCHITECTURE.md` 的"代码组织"章节，明确每个目录由谁主要维护。简单说：

| 角色 | 主要负责 | 主要改的目录 |
|------|---------|------------|
| 主程 (zippear-mo) | 网络 / 事件总线 / 核心架构 | `Assets/Scripts/Core/`、`Assets/Scripts/Players/` |
| 关卡程序 | 谜题逻辑 / 关卡数据 | `Assets/Scripts/Puzzle/`、`Assets/Scripts/Level/`、`Assets/ScriptableObjects/` |
| 美术 + 场景 | 模型 / Sprite / 场景搭建 | `Assets/Art/*`、`Assets/Scenes/*`、`Assets/Prefabs/*` |
| UI + 音频 | UI Prefab / 音频导入 | `Assets/Scripts/UI/`、`Assets/UI/`、`Assets/Audio/` |

### ⚠️ 三条铁律

1. **不要两个人同时改同一个 `.unity` 场景文件！** 要改先在群里喊一声（YAML 冲突极难合）
2. **commit message 用英文 + 前缀**（`feat:` / `fix:` / `art:` / `level:` / `chore:` / `docs:`）
3. **每天至少 push 2 次**（早会前 + 睡前），不要本地憋大改

## 致谢

- Unity Technologies
- Photon Engine
- Synty Studios (POLYGON Starter Pack)
- Kenney.nl (CC0 Audio & Sprites)
- Suno AI (BGM)
- 灵感致敬：谷川流《凉宫春日》系列、Respawn《泰坦陨落 2》
