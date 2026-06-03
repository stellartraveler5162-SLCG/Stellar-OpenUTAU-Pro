# Stellar OpenUTAU Pro 🎤

> 现代化 AI 歌声合成工作站 | ACE Studio & SynthV 风格界面 | DeepSeek 驱动

**Stellar OpenUTAU Pro** 是基于 [OpenUtau](https://github.com/stakira/OpenUtau) 的深度现代化改造版本。我们完全重写了 UI 设计语言，使其拥有 ACE Studio 和 Synthesizer V 等专业 DAW 的视觉体验，同时保留了 OpenUtau 全部的强大功能。

![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-macOS%20%7C%20Windows%20%7C%20Linux-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

---

## ✨ 核心特性

### 界面设计
- 🎨 **ACE Studio × SynthV 融合界面** — 深色主题，靛蓝紫品牌色 (#7C6FF7)，全界面 8px 大圆角
- 🖥️ **三栏 DAW 布局** — 左侧轨道面板 (280px) + 中央编曲区 + 底部钢琴卷帘
- 🌙 **沉浸式暗色模式** — 深黑底 #0E0E12，Transport Bar 独立面板，卡片式轨道头
- 📱 **2020s 设计语言** — 毛玻璃效果、渐变音符块、页面切换淡入淡出动画

### 双模式编辑
- 🎵 **Notes 模式** — 像 MIDI DAW 一样拖拽编辑音符：创建、移动、拉伸、切分、删除
- 📐 **Params 模式** — 音符锁定不动，专注于编辑音高曲线和颤音参数

### 声库 & 渲染
- 🎤 支持 UTAU / DiffSinger / ENUNU / NNSVS / VOCALOID 等所有主流歌声合成引擎
- 🔌 完整的渲染器/音源插件系统 (Wavtool, Resampler)
- 🎛️ 10 条表达式曲线 (Dynamics, Gender, Tension, Breathiness 等)
- 🇨🇳 **默认中文界面** (zh-CN)，内嵌完整中文字符串资源

---

## 📥 下载安装

| 平台 | 下载 | 说明 |
|------|------|------|
| 🍎 macOS (Apple Silicon) | [Stellar-OpenUTAU-Pro-macOS-arm64.dmg](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/releases/latest) | 双击打开，拖入 Applications |
| 🪟 Windows (x64) | [Stellar-OpenUTAU-Pro-Windows-x64.zip](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/releases/latest) | 解压后运行 `OpenUtau.exe` |

---

## 🚀 使用教程

### 安装声库
1. **菜单栏** → Tools → Install Singer
2. 选择声库的 `.zip` 包或含有 `character.txt` 的文件夹
3. 等待导入完成，声库会出现在左侧歌手面板

### 基础工作流
```
新建工程 → 双击【歌手空白】选择声库 → 双击时间线创建片段
→ 双击片段打开钢琴卷帘 → 画笔工具写音符 → 输入歌词 → 播放
```

### 双模式编辑
- **Notes 模式** (默认): 点击工具栏 `♪ Notes` 按钮，正常编辑 MIDI 音符
- **Params 模式**: 点击工具栏 `〰 Params` 按钮，音符锁定，专注编辑音高曲线
  - 画音高: 选择画笔工具，在参数区自由绘制
  - 画直线: 选择直线工具，拖拽绘制线性音高变化
  - 编辑控制点: 选择选择工具，拖拽已有的音高控制点

### 快捷键
| 快捷键 | 功能 |
|--------|------|
| `Ctrl+N` | 新建工程 |
| `Ctrl+O` | 打开工程 |
| `Ctrl+S` | 保存 |
| `Ctrl+Z` / `Ctrl+Y` | 撤销 / 重做 |
| `Ctrl+F` | 搜索音符 |
| `Ctrl+A` | 全选 |
| `Ctrl+C/V/X` | 复制 / 粘贴 / 剪切 |
| `Space` | 播放 / 暂停 |
| `F11` | 全屏 |
| `Alt+1~0` | 切换表达式曲线 |

### 表情参数
在钢琴卷帘底部拖拽调整参数曲线：
- Dynamics (DYN) — 音量动态
- Gender (GEN) — 性别参数
- Tension (TEN) — 张力
- Breathiness (BRE) — 气声
- Voicing (VOI) — 发声强度
- Velocity (VEL) — 速率

---

## 🛠️ 从源码构建

### 前置要求
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- macOS / Windows / Linux

### 构建步骤
```bash
# 克隆仓库
git clone https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro.git
cd Stellar-OpenUTAU-Pro

# 下载运行时依赖 (首次)
# 请从 OpenUtau 官方仓库下载 runtimes/ 目录到项目根目录

# 还原 & 构建
dotnet restore
dotnet build -c Release

# 发布 (macOS ARM64)
dotnet publish OpenUtau/OpenUtau.csproj -c Release -r osx-arm64 --self-contained true -o publish/osx-arm64

# 发布 (Windows x64)
dotnet publish OpenUtau/OpenUtau.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64
```

---

## 📂 项目结构

```
Stellar-OpenUTAU-Pro/
├── OpenUtau/                   # Avalonia UI 界面层
│   ├── Controls/               # 自绘控件 (PianoRoll, NotesCanvas, TrackBackground...)
│   ├── Views/                  # 窗口/对话框 XAML
│   ├── ViewModels/             # MVVM ViewModel 层
│   ├── Styles/                 # 全局 + 钢琴卷帘样式
│   ├── Colors/                 # 暗色/亮色/自定义主题色彩
│   └── Strings/                # 多语言资源 (zh-CN, en-US, ja-JP...)
├── OpenUtau.Core/              # 核心引擎 (渲染、音源、格式解析)
├── OpenUtau.Plugin.Builtin/    # 内置音素转换器
└── OpenUtau.Test/              # 单元测试
```

---

## 🎨 设计哲学

我们追求 **"像使用专业 DAW 一样使用歌声合成器"**：

1. **无学习曲线** — 如果你用过 Logic Pro / FL Studio / Ableton，你会立刻上手
2. **美学优先** — 2020s 极简设计，大圆角、渐变色、暗色沉浸
3. **不删功能** — 100% 保持 OpenUtau 全部原始功能
4. **工程师友好** — 清晰的 MVVM 架构，C# + Avalonia UI

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

- 🐛 Bug 报告 → [Issues](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/issues)
- 💡 功能建议 → [Discussions](https://github.com/stellartraveler5162-SLCG/Stellar-OpenUTAU-Pro/discussions)
- 📖 改进文档 → 直接 PR 到 `main` 分支

---

## 📜 许可证

本项目基于 [MIT License](LICENSE)。原始 [OpenUtau](https://github.com/stakira/OpenUtau) 代码版权归 stakira 及贡献者所有。

---

## ⭐ Star History

如果你喜欢这个项目，请给一个 ⭐ Star！

---

<p align="center">
  <sub>Built with ❤️ by the Stellar OpenUTAU community</sub>
</p>
