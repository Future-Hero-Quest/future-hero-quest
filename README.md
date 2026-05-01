# Future Hero Quest

> 2.5D 双人合作时空解谜游戏 · GameJam 2026 · 致敬《泰坦陨落 2》"Effect and Cause" 与《凉宫春日》系列

一个在过去，一个在未来。你的每一个选择，都将永远改变 ta 的世界。

## 游戏概念

- 类型：2.5D 双人联机合作解谜
- 平台：WebGL / Windows / Mac
- 流程：约 15 分钟，2-3 关
- 引擎：Unity 6.4.2f1 (6000.4.2f1) + Photon PUN2

详细策划见 [`../Future Hero Quest.md`](../Future%20Hero%20Quest.md)（项目 Plan，含玩法、架构、关卡设计、回退方案）。

## 开发环境

- **Unity**: 6.4.2f1 / 6000.4.2f1（当前工程版本；不要用 2022.3 打开，Unity 不支持降级）
- **Render Pipeline**: Built-in for the current network milestone; URP deferred until art polish
- **联网**: Photon PUN 2 (Asset Store 免费版)
- **目标平台**: WebGL（首要）+ Windows + Mac

## Git 工作流（main + dev 双分支）

```
main      ●━━━━━━━━━━●━━━━●━━━━●     仅接受合并 + 受保护
              tag      tag    submit
dev       ●━●━●━●━●━●━●━●━●━●━●━●     所有人日常推这里
```

### 日常命令
```bash
git checkout dev
git pull --rebase
# ... 写代码 ...
git add .
git commit -m "feat: 描述"
git push
```

### 里程碑合并（组长执行）
```bash
git checkout main
git merge dev --no-ff -m "milestone: vX.Y"
git tag -a vX.Y-name -m "描述"
git push origin main --tags
```

### 高风险实验
```bash
git checkout -b experimental/feature-name
# ... 实验 ...
# 成功：git merge --squash 回 dev
# 失败：git branch -D 删除
```

## 团队分工建议

| 角色 | 负责 | 改文件类型 |
|------|------|-----------|
| 主程 A | 网络 / 事件总线 / 核心架构 | `*.cs` |
| 关卡 B | Scenes / Prefab / 关卡数据 | `*.unity`, `*.prefab`, `*.asset` |
| 美术 C | 模型 / Sprite / 材质 | `Assets/Art/*` |
| UI/音 D | UI Prefab / 音频 | `Assets/UI/*`, `Assets/Audio/*` |

**铁律**：不要两个人同时改同一个 `.unity` 场景文件。要改先在群里喊。

## Unity 必备设置（建项目后立刻做）

1. `Edit → Project Settings → Editor`
   - `Asset Serialization Mode` = **Force Text**
   - `Version Control` = **Visible Meta Files**

2. 配置 Unity Smart Merge（解决 .scene 冲突）：

```bash
# 在项目根目录的 .git/config 里加（Unity 路径按你本地实际改）：
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'E:/unity/6000.4.2f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```

## 第一次打开项目

```bash
# 1. Clone
git clone https://github.com/Future-Hero-Quest/future-hero-quest.git
cd future-hero-quest
git checkout dev

# 2. 配置 git user
git config user.name "你的名字"
git config user.email "你的GitHub邮箱"

# 3. 配置 Smart Merge（参考上面）

# 4. 用 Unity Hub 打开项目根目录
```

## 关键里程碑（参考 Plan）

- [x] T+6h · v0.1 · 联网双人胶囊体移动
- [ ] T+18h · v0.2 · 第 1 关《种树》完整闭环
- [ ] T+30h · v0.3 · 第 2 关《时空信件》完整闭环
- [ ] T+38h · v0.4 · 第 3 关《镜像》完整闭环
- [ ] T+40h · v0.5 · 美术整合 + 音乐
- [ ] T+44h · v0.9 · 打包测试
- [ ] T+46h · v1.0 · itch.io 提交（5/3 19:00 北京时间 DDL）

## 致谢

- Unity Technologies
- Photon Engine
- Synty Studios (POLYGON Starter Pack)
- Kenney.nl (CC0 Audio & Sprites)
- Suno AI (BGM)
- 灵感致敬：谷川流《凉宫春日》系列、Respawn《泰坦陨落 2》

## License

待定（视致敬式原创最终边界决定）
