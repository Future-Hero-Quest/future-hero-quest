# Photon PUN2 本地配置

> 目标：队友 clone 后能快速填好联网配置，但真实 Photon AppID 不进入 Git。

## 结论

- 不要把真实 Photon AppID 提交到仓库。
- `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` 已在 `.gitignore` 中忽略。
- 项目主通过私聊/群公告发 AppID，队友只填到自己的本地 Unity 工程。

## 配置步骤

1. 打开 Unity 项目：`E:\黑客松\FutureHeroQuest`
2. 等 Unity 编译完成，确认 Console 没有红色编译错误。
3. 打开菜单：

```text
Window / Photon Unity Networking / PUN Wizard
```

4. 把项目主私发的 **PUN Realtime AppID** 填进去。
5. 保存后运行：

```text
FHQ / Check Photon Setup
```

如果 Unity Console 输出 `[FHQ] Photon setup looks ready`，说明本机配置完成。

## 手动检查路径

如果 PUN Wizard 没弹出，可以手动选中：

```text
Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset
```

在 Inspector 中找到：

```text
App Settings / App Id Realtime
```

填入项目主私发的 AppID。

## Git 注意事项

配置完成后，正常情况下 `git status` 不应该显示 `PhotonServerSettings.asset`。

如果看到下面文件出现在 staged/modified 列表里，不要提交：

```text
Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset
Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset.meta
```

需要撤出暂存时：

```powershell
git restore --staged Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset
git restore --staged Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset.meta
```

如果只是本地文件已被 Unity 修改，不需要处理；它会被 `.gitignore` 忽略。

## 常见问题

### `ConnectUsingSettings failed` 或无法连接 Photon

优先检查：

- AppID 是否填到了 `App Id Realtime`，不是 Chat/Voice/Fusion。
- AppID 是否来自当前项目主私发的值。
- 网络是否能访问 Photon Cloud。
- 是否有 Unity Console 红错阻止脚本运行。

### clone 后没有 `PhotonServerSettings.asset`

这是正常的。该文件是本地配置文件，不进 Git。

使用 PUN Wizard 填 AppID 后，Unity 会创建/更新本地文件。

### 是否可以把 AppID 发到 GitHub？

不可以。AppID 绑定 Photon quota，不是传统密码，但进 Git 历史后很难彻底移除。只允许私下发送给队友。

