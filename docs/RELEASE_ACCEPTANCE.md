# Future Hero Quest - 技术架构文档

本文档概述了 **Future Hero Quest** 的技术栈、网络架构、时间线同步机制以及核心模块设计。项目基于 Unity 引擎与 Photon PUN2 构建，旨在实现跨时空的双人协作解谜体验。

---

## ️ 技术栈概览

| 模块 | 技术选型 | 说明 |
| :--- | :--- | :--- |
| **游戏引擎** | Unity 6.4.2f1 (6000.4.2f1) | Built-in 渲染管线 |
| **网络框架** | Photon PUN2 | 负责房间管理、RPC 调用与状态同步 |
| **目标平台** | Windows (PC) | 首发平台，支持双端（Editor + Build） |
| **版本控制** | Git | `dev` 为集成分支，`main` 为发布门面 |
| **核心机制** | 语义时间线事件层 | 自定义的状态机与时间线管理器 |

---

##  网络架构 (Networking)

项目采用 **Photon PUN2** 实现轻量级的客户端-服务器（Client-Server）架构。

### 房间与匹配
- **Launcher 场景**：作为唯一的网络入口场景。
- **房主逻辑 (Host)**：负责创建房间并加载关卡场景。
- **加入逻辑 (Client)**：通过房间名或ID加入，自动同步房主当前所在的场景。

### 玩家角色与视角
- **双客户端设计**：
  - **过去 (1996)**：通常由 Unity Editor 端扮演。
  - **未来 (2026)**：通常由 Windows Exe 端扮演。
- **状态同步**：玩家的位置、旋转及基础交互状态通过 PhotonView 进行实时同步。

### 数据流向
1. **连接**：两端连接至 Photon Cloud 同一房间。
2. **场景同步**：房主触发 `PhotonNetwork.LoadLevel`，所有客户端同步加载对应关卡（如 `Level01_Bridge`）。
3. **事件同步**：通过 RPC 或自定义语义层同步解谜进度。

---

##  时间线同步机制 (Timeline Sync)

这是本项目的核心架构，旨在解决“过去改变，未来即时反馈”的玩法需求。

### 语义事件驱动 (Semantic Event-Driven)
不同于传统的物理帧同步，本项目采用**语义事件层**来传递时间线的改变。

- **事件定义**：将解谜步骤抽象为语义事件（例如：`Bridge_Repaired`, `Archive_Found`, `Door_Unlocked`）。
- **状态管理**：
  - 每个关卡包含一个 **Timeline Manager**。
  - 当“过去”玩家触发机关时，发送一个带有时间戳或序列ID的事件。
  - “未来”客户端接收事件，并根据预定义的规则更新当前世界的状态（如：破碎的桥变为完整，缺失的档案出现）。

### 状态一致性
- **关键数据**：关卡进度、机关状态（开/关）、物品持有状态。
- **同步策略**：关键节点采用 **RPC (Reliable)** 确保两端状态绝对一致，避免因网络延迟导致的逻辑死锁。

---

## ️ 场景与关卡流 (Scene Flow)

根据 `Build Settings`，项目严格限制为以下四个场景，通过线性流程串联：

1. **Launcher (`Launcher.unity`)**
   - **功能**：网络握手、房间创建/加入、角色身份分配。
   - **持久化**：包含 `DontDestroyOnLoad` 的网络管理单例。

2. **第一关：断桥
 (`Level01_Bridge.unity`)**
   - **核心逻辑**：验证时间线同步的基础闭环。
   - **对象**：断桥（未来）、修桥工具（过去）。

3. **第二关：档案 (`Level02_Archive.unity`)**
   - **核心逻辑**：信息不对称解谜。
   - **对象**：档案柜、缺失的文档。

4. **第三关：社团 (`Level03_ClubRoom.unity`)**
   - **核心逻辑**：复杂的因果链触发。
   - **对象**：台球桌、电子门锁、最终出口 (`L3_Exit`)。

---

##  目录结构规范

```text
Assets/
├── Scenes/                 # 核心场景 (仅包含上述4个)
├── Scripts/
│   ├── Network/            # Photon 相关逻辑 (Launcher, RoomManager)
│   ├── Timeline/           # 时间线管理器、事件定义
│   ├── Gameplay/           # 关卡专用逻辑 (Bridge, Archive, ClubRoom)
│   └── UI/                 # 界面交互
├── ThirdParty/             # 第三方资产 (Kenney, OpenFracture等)
└── Resources/              # 动态加载资源
