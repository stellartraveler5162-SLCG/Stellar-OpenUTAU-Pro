# Stellar OpenUTAU Pro

Stellar OpenUTAU Pro 是基于 [OpenUtau](https://github.com/stakira/OpenUtau) 的 UI 改造版本。我们在保留 OpenUtau 全部功能的基础上，重新设计了界面风格，使其更接近现代 DAW 的视觉体验。

![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-macOS%20%7C%20Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

---

## 本软件做了什么

### 界面改动
- 深色主题，靛蓝紫品牌色 (#7C6FF7)，全界面 8px 大圆角
- 左侧轨道面板 + 中央编曲区 + 底部钢琴卷帘的三栏布局
- 深黑底色 #0E0E12 的沉浸式暗色模式
- 默认中文界面 (zh-CN)

### 新增功能
- **Notes / Params 双模式编辑**：Notes 模式下可拖拽编辑音符，Params 模式下音符锁定、专用于编辑音高曲线
- **About 对话框**：Help 菜单中新增，含制作人信息和 Bilibili 链接
- **Singer Browser**：Tools 菜单中新增声库浏览器，对接服务器声库库
- **自定义更新服务**：更新检查指向自有服务器
- **Windows EXE 安装包**：一键安装，自动创建快捷方式

### 继承自 OpenUtau 的功能
- 支持 UTAU / DiffSinger / ENUNU / NNSVS 等歌声合成引擎
- 渲染器/音源插件系统
- 表达式曲线编辑
- 完整的多语言资源
- 所有原始菜单功能和快捷键

---

## 下载

| 平台 | 下载 |
|------|------|
| macOS (Apple Silicon) | [Stellar-OpenUTAU-Pro-macOS-arm64.dmg](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/releases/latest) |
| Windows (x64) | [Stellar-OpenUTAU-Pro-Windows-x64-Setup.exe](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/releases/latest) |

### 安装
- **macOS**: 双击 DMG，将 Stellar OpenUTAU Pro 拖入 Applications
- **Windows**: 运行 Setup.exe，自动安装到 Program Files 并创建快捷方式

---

## 从源码构建

```bash
git clone https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro.git
cd Stellar-OpenUTAU-Pro
dotnet restore
dotnet build -c Release

# macOS ARM64 发布
dotnet publish OpenUtau/OpenUtau.csproj -c Release -r osx-arm64 --self-contained true -o publish/osx-arm64

# Windows x64 发布
dotnet publish OpenUtau/OpenUtau.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64
```

---

## 项目结构

```
├── OpenUtau/                   # Avalonia UI 界面层
│   ├── Controls/               # PianoRoll, NotesCanvas 等自绘控件
│   ├── Views/                  # 窗口与对话框
│   ├── ViewModels/             # MVVM ViewModel 层
│   ├── Styles/                 # 全局样式
│   ├── Colors/                 # 主题色彩
│   └── Strings/                # 多语言资源
├── OpenUtau.Core/              # 核心引擎
├── OpenUtau.Plugin.Builtin/    # 内置音素转换器
├── StellarInstaller/           # Windows 安装器源码
└── OpenUtau.Test/              # 单元测试
```

---

## 许可证

本项目基于 [MIT License](LICENSE)。原始 [OpenUtau](https://github.com/stakira/OpenUtau) 代码版权归 stakira 及贡献者所有。
