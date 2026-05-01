# drafts/ — Future Hero Quest 脚本预热草稿

这个目录存放**Unity 项目建好之前**预先准备的 C# 脚本。
等你用 Unity Hub 在 `e:\黑客松\FutureHeroQuest\` 下创建项目后，按下面步骤整体迁移过去。

## 当前已起草的脚本（T+0 ~ T+6h 联网验证用）

```
Scripts/
├── Core/
│   ├── TimelineEvent.cs        # 事件数据结构
│   ├── GameRole.cs             # Past/Future 角色枚举
│   ├── TimelineEventBus.cs     # 中央事件总线（核心架构）
│   └── NetworkManager.cs       # Photon 连接 + 房间 + 角色分配
├── Players/
│   ├── PlayerController.cs     # WASD + 重力 + CharacterController
│   ├── BillboardSprite.cs      # 2.5D Sprite 永远面向相机
│   └── PlayerSpawner.cs        # 进入关卡场景时按角色 spawn 对应 Prefab
└── UI/
    └── ConnectUI.cs            # 主菜单：创建/加入房间按钮
```

## Unity 项目建好后的迁移步骤

### Step 1: 用 Unity Hub 创建项目
- Editor: Unity 2022.3 LTS
- Template: 3D (URP)
- Project Name: `FutureHeroQuest`
- Location: `E:\黑客松\`（Unity Hub 会自动创建 `E:\黑客松\FutureHeroQuest\`）

### Step 2: 导入 Photon PUN2
- Unity 内打开 Asset Store
- 搜 "PUN 2 - FREE"
- Import 全部
- 弹出 PUN Wizard 时填入你的 Photon AppID

### Step 3: 复制脚本
```powershell
# 在 PowerShell 里执行
Copy-Item "e:\黑客松\drafts\Scripts" "e:\黑客松\FutureHeroQuest\Assets\Scripts" -Recurse
```

### Step 4: 复制 .gitignore
```powershell
Copy-Item "e:\黑客松\drafts\unity.gitignore" "e:\黑客松\FutureHeroQuest\.gitignore"
```

### Step 5: 在 Unity 里创建场景
- New Scene -> 命名 `MainMenu`，保存到 `Assets/Scenes/`
- New Scene -> 命名 `Level01_Tree`，保存到 `Assets/Scenes/`
- File -> Build Settings -> Add Open Scenes（按顺序：MainMenu 第 0 位，Level01_Tree 第 1 位）

### Step 6: 在 MainMenu 场景中
- 创建空 GameObject 名为 `_NetworkManager`，挂 `NetworkManager.cs`
- 创建空 GameObject 名为 `_TimelineEventBus`，挂 `TimelineEventBus.cs` + `PhotonView`
- 创建 Canvas + 两个 Button + 一个 Text，挂 `ConnectUI.cs`，把字段拖好

### Step 7: 在 Level01_Tree 场景中
- 创建空 GameObject 名为 `_PlayerSpawner`，挂 `PlayerSpawner.cs`
- 创建 PastSpawnPoint + FutureSpawnPoint 两个 Transform 用作出生点
- 准备 PastPlayer、FuturePlayer 两个 Prefab 放到 `Assets/Resources/`
- 简单起见，第一次验证可以两个 Prefab 都用胶囊体 + 不同颜色

### Step 8: 验证联网（T+6h 的硬性目标）
- Build & Run 一份（生成 .exe）
- Editor 跑一份
- 一边创建房间，一边加入房间
- 两边都能看到对方胶囊体移动 = 验证通过

## 关键提醒

- **Photon AppID 不要提交到 Git**（已在 .gitignore 注释里标注）
- **MainMenu 场景必须挂 PhotonView 组件**（TimelineEventBus 需要它做 RPC）
- **加载关卡用 `PhotonNetwork.LoadLevel()`，不要用 SceneManager**

## 还没起草但 T+6h 后会需要的脚本

- `Puzzle/PuzzleObject.cs` — 可被改写物体基类
- `Puzzle/TreeObject.cs` — 第 1 关树苗 -> 大树
- `Level/LevelData.cs` — ScriptableObject 关卡定义
- `Level/LevelManager.cs` — 关卡加载、过关判定
- `UI/DialogueButtonPanel.cs` — 8 个台词按钮
- `UI/DialogueBubble.cs` — 对方头顶气泡
- `VFX/AlphaSwitcher.cs` — 旧/新状态 alpha 过渡
- `VFX/FootprintSpawner.cs` — 蓝色脚印 decal

按需开干，不一次写完。
