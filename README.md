# CS2 CounterStrikeSharp 随机启动地图插件 (RandomMap)

这是一个基于 **CounterStrikeSharp (CSS)** 的 CS2 服务器插件。
当服务器开服启动完成首次地图加载后，插件会自动读取与自身**同目录**下的 `map.txt`，并随机抽取一张地图执行 `changelevel`，实现每次开服随机启动地图的功能。

---

## 目录结构与安装部署

CS2 服务器需预先安装好 **Metamod:Source** 与 **CounterStrikeSharp**。

在服务器的 `plugins` 目录下新建一个名为 `RandomMap` 的文件夹，将编译生成的 `RandomMap.dll` 与 `map.txt` 放入其中：

```text
game/csgo/addons/counterstrikesharp/plugins/
└── RandomMap/
    ├── RandomMap.dll     # 编译产物（跨平台通用，Windows/Linux 直接运行）
    └── map.txt           # 地图池列表（与 DLL 处于同目录）
```

> **提示**：如果服务器正在运行，放入文件后在服务器控制台输入 `css plugins load RandomMap` 即可热加载，无需重启。

---

## 地图列表配置 (`map.txt`)

地图列表每行一个地图名（回车分隔），自动过滤首尾多余空格与换行符，支持 `#` 和 `//` 注释行：

```text
# 官方常规/竞技地图池
de_ancient
cs_italy
de_dust2
de_mirage
de_inferno
de_nuke
de_anubis
de_vertigo
cs_office
```

---

## 控制台命令

| 控制台命令 | 执行权限 | 功能说明 |
| :--- | :--- | :--- |
| `css_randommap` | 仅服务器控制台 | 手动强制触发一次随机抽图与切图 |
| `css_randommap_reload` | 仅服务器控制台 | 重新读取同目录下的 `map.txt` 地图池 |

---

## 本地编译

本项目基于 .NET SDK 构建，产物直接跨平台运行在 Windows 和 Linux 服务器上：

```bash
# 还原并编译 Release 版本
dotnet build -c Release
```

编译输出产物位于：`bin/Release/net10.0/RandomMap.dll`。
