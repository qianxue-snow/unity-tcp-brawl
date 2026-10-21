# unity-tcp-brawl

自己写的 **TCP 多人同步 demo**：一个 .NET 手写 socket 服务端 + 一个 Unity 客户端。**这两个必须放一起用**，所以合在一个仓库里。

```
/                    ← Unity 2017.3.1p4 工程根（Assets / ProjectSettings / Packages）
  Assets/Scripts/      客户端逻辑：Main / NetManager / BaseHuman / CtrlHuman / SyncHuman
  Assets/Sources/      自制模型与动画（.fbx 2.3MB）
  Assets/Standard Assets/  Unity 官方内置包（场景引用了其中 64 个资源，删掉就打不开，故入库）
/server/             ← .NET 8 服务端（原在 source\repos\EchoServer，为便于同仓使用复制进来）
  EchoServer.sln
  EchoServer/          Program.cs（socket 主循环）/ MsgHandler.cs（协议分发）/ EventHandler.cs（断线广播）
```

## 怎么跑

1. 服务端：`cd server && dotnet run --project EchoServer` → 监听 `127.0.0.1:8888`
2. 客户端：用 **Unity 2017.3.1p4** 打开工程根目录，播放 `Assets/Scenes/SampleScene`
3. 多个客户端实例同机开（或打包后多台机器改 IP）即可看到互相的角色位置/朝向同步

## 协议（纯文本，`|` 分方法名，`,` 分参数）

| 方向 | 消息 | 说明 |
|---|---|---|
| 客户端→服务端 | `Enter,<desc>,<x>,<y>,<z>,<eulY>` | 进入房间并上报初始位姿；服务端给该连接建 `ClientState`（hp=100） |
| 服务端→所有客户端 | `Enter,<...>` | 广播新成员 |
| 服务端→所有客户端 | `Leave,<endpoint>,` | 有连接断开时广播 |

实现要点：`Program.cs` 用单线程 `Socket.Poll(0)` 轮询 listenfd 和所有 clientfd，靠 `Thread.Sleep(1)` 压 CPU；`msgHandlers` 字典把方法名映射到委托（`MsgDelegate`）。

## 已知问题

- 工程是 **Unity 2017.3**（其余仓库都是 2022.3.48f1c1），新版打开会提示升级
- `.gitignore` 排除了 `Library/`、`Temp/`、`obj/`、`.vs/`、Unity 自动生成的 `.csproj/.sln`（`server/` 下的手写工程文件**保留**）
- 部分 `.cs` 注释在 GitHub 网页上显示乱码（文件是 GBK 编码），本地用 VS / Rider 打开正常
- 未做公网监听、无鉴权，纯本机学习用

## 状态

学习存档。最后一次改动 2026-07-03。
