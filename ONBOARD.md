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
- 在 Hub 里安装 **Unity 2022.3 LTS**（不要 2023/6000）
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
git config user.name "你的真实名字或昵称"
git config user.email "你的GitHub注册邮箱"
git checkout dev
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
- Windows: `C:\Program Files\Unity\Hub\Editor\2022.3.XXf1\Editor\Data\Tools\UnityYAMLMerge.exe`
- Mac: `/Applications/Unity/Hub/Editor/2022.3.XXf1/Unity.app/Contents/Tools/UnityYAMLMerge`

在仓库根目录跑（路径按实际改）：

**Windows:**
```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/2022.3.XXf1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```

**Mac:**
```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'/Applications/Unity/Hub/Editor/2022.3.XXf1/Unity.app/Contents/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
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
| `feat:` | 新功能 | `feat: 第一关种树触发器` |
| `fix:` | bug 修复 | `fix: 联网时角色穿模` |
| `art:` | 美术资源 | `art: 导入 Synty 校园场景` |
| `level:` | 关卡数据 | `level: 调整种树点位置` |
| `chore:` | 杂活/配置 | `chore: 升级 Photon` |
| `docs:` | 文档 | `docs: 更新 README` |

### 高风险实验（用完即焚）
```bash
git checkout -b experimental/synty-import
# 试...
# 成功：git checkout dev && git merge experimental/synty-import --squash
# 失败：git checkout dev && git branch -D experimental/synty-import
```

## 团队分工建议

| 角色 | 负责范围 | 主要改的文件 |
|------|---------|------------|
| 主程 | 网络 / 事件总线 / 核心架构 | `Assets/Scripts/Core/*` |
| 关卡 | Scenes / Prefab / 关卡数据 | `Assets/Scenes/*`, `Assets/Prefabs/*` |
| 美术 | 模型 / Sprite / 材质 | `Assets/Art/*` |
| UI/音 | UI Prefab / 音频 | `Assets/UI/*`, `Assets/Audio/*` |

**铁律**：不要两个人同时改同一个 `.unity` 场景文件！要改先在群里喊一声。

## 出问题时怎么办

| 症状 | 怎么救 |
|------|-------|
| commit 一直被拒绝 | 99% 是 user.name/email 没配（回 Step 3） |
| push 一直被拒绝 | 先 `git pull --rebase` 再 `git push` |
| rebase 冲突 | `git rebase --abort` 取消，找冲突方协调 |
| 场景文件冲突 | 喊出冲突方，约定一个人保留改动，另一个手动重做 |
| Unity 一直转圈 | 关掉 Unity，删除 `Library/` 目录，重新打开会重新生成（10 分钟） |
| 不知道发生了什么 | `git reflog` 看历史，`git reset --hard <hash>` 回退到任意时刻 |

## 关键时间节点

| 节点 | 时间 | 必须达成 |
|------|------|---------|
| T+6h | 5/2 03:00 | 联网双人胶囊体跑通 |
| T+18h | 5/2 18:00 | 第 1 关《种树》闭环 |
| T+30h | 5/3 06:00 | 第 2 关《开关》闭环 |
| T+40h | 5/3 14:00 | 美术整合完成 |
| T+44h | 5/3 17:00 | 必须开始打包 |
| **DDL** | **5/3 19:00** | **itch.io 提交** |

## 完整策划见

- [README.md](./README.md) — 项目简介 + Git 工作流
- 主 Plan 文档（在主程本地）：`Future Hero Quest.md` — 包含完整玩法/架构/关卡设计/回退方案

## 致谢

- Unity Technologies
- Photon Engine
- Synty Studios (POLYGON Starter Pack)
- Kenney.nl (CC0 Audio & Sprites)
- Suno AI (BGM)
- 灵感致敬：谷川流《凉宫春日》系列、Respawn《泰坦陨落 2》
