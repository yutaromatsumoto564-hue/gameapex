# ARIA: 异星觉醒

一款融合工厂建造、塔防和卡牌合成元素的2D俯视角策略手游。

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/               # 核心系统
│   │   ├── GameManager.cs      # 游戏主管理器
│   │   ├── SaveSystem.cs       # 存档系统
│   │   └── EventManager.cs     # 事件系统
│   │
│   ├── Card/               # 卡牌系统
│   │   ├── CardData.cs         # 卡牌数据定义
│   │   ├── CardManager.cs      # 卡牌管理器
│   │   └── CardUI.cs           # 卡牌UI组件
│   │
│   ├── Building/           # 建筑系统
│   │   ├── BuildingData.cs     # 建筑数据定义
│   │   ├── BuildingManager.cs  # 建筑管理器
│   │   └── Building.cs         # 建筑实体
│   │
│   ├── Resource/           # 资源系统
│   │   ├── ResourceData.cs     # 资源数据定义
│   │   ├── ResourceManager.cs  # 资源管理器
│   │   └── ResourceNetwork.cs  # 资源网络
│   │
│   ├── Power/              # 电力系统
│   │   ├── PowerManager.cs     # 电力管理器
│   │   └── PowerNetwork.cs     # 电力网络数据
│   │
│   ├── Enemy/              # 敌人系统
│   │   ├── EnemyData.cs        # 敌人数据定义
│   │   ├── EnemyManager.cs     # 敌人管理器
│   │   └── Enemy.cs            # 敌人实体
│   │
│   ├── DayNight/           # 昼夜系统
│   │   ├── DayNightCycle.cs    # 昼夜循环
│   │   ├── WaveData.cs         # 波次数据
│   │   └── WaveManager.cs      # 波次管理器
│   │
│   ├── Tech/               # 科技系统
│   │   ├── TechData.cs         # 科技数据定义
│   │   └── TechManager.cs      # 科技管理器
│   │
│   └── UI/                 # UI系统
│       ├── UIManager.cs        # UI管理器
│       ├── CardBarUI.cs        # 卡牌栏UI
│       └── ResourceBarUI.cs    # 资源栏UI
│
├── Art/                    # 美术资源
│   ├── Sprites/               # 精灵图
│   ├── Icons/                 # 图标
│   └── UI/                    # UI素材
│
├── Audio/                   # 音频资源
│   ├── Music/                 # 音乐
│   └── SFX/                   # 音效
│
├── Data/                    # ScriptableObject数据
│   ├── Cards/                 # 卡牌数据
│   ├── Buildings/             # 建筑数据
│   ├── Resources/             # 资源数据
│   ├── Enemies/               # 敌人数据
│   ├── Techs/                 # 科技数据
│   └── Waves/                 # 波次数据
│
├── Prefabs/                 # 预制体
│   ├── Buildings/             # 建筑预制体
│   ├── Enemies/               # 敌人预制体
│   ├── UI/                    # UI预制体
│   └── Effects/               # 特效预制体
│
└── Scenes/                  # 场景
    ├── MainMenu.unity         # 主菜单
    ├── Game.unity             # 游戏主场景
    └── Loading.unity          # 加载场景
```

## 核心系统说明

### 1. 卡牌系统
- 卡牌类型: 资源卡、材料卡、建筑卡、特殊卡
- 卡牌操作: 拖拽放置、堆叠合成
- 卡牌获取: 初始卡包、合成、商店、科技解锁

### 2. 建筑系统
- 建筑分类: 资源采集、生产加工、能源供应、网络枢纽、防御建筑
- 建筑放置: 从卡牌栏拖拽到地图
- 建筑尺寸: 1×1、2×2、3×3、5×5

### 3. 资源网络系统
- 所有建筑自动连接到资源网络
- 资源自动存入/取出全局库存
- 无需传送带，简化操作

### 4. 昼夜循环
- 白天 (5分钟): 建设、采集、研究
- 黄昏 (30秒): 警告提示
- 夜晚 (2-5分钟): 敌人入侵、防御战斗
- 黎明 (30秒): 清理战场、获得战利品

### 5. 敌人入侵系统
- 敌人类型: 小型虫族、中型虫族、大型虫族、特殊敌人、Boss
- 波次设计: 难度递增，每5天出现Boss
- 战利品: 有机物、甲壳、虫族核心

### 6. 科技系统
- 科技分类: 物理学、工程学、社会学
- 研究机制: 实验室消耗研究包进行研究
- 科技解锁: 新建筑、新卡牌、属性加成

## 开发环境

- **引擎**: Unity 2022.3 LTS
- **语言**: C#
- **平台**: iOS / Android
- **美术风格**: 像素风

## 快速开始

1. 使用 Unity 2022.3 LTS 打开项目
2. 打开 `Assets/Scenes/Game.unity` 场景
3. 运行游戏

## 文档

- [游戏设计文档 (GDD)](./GDD_游戏设计文档.md)

## 开发进度

- [x] 核心框架搭建
- [x] 卡牌系统
- [x] 建筑系统
- [x] 资源网络系统
- [x] 电力系统
- [x] 敌人入侵系统
- [x] 昼夜循环
- [x] 科技系统
- [x] UI框架
- [ ] 美术资源
- [ ] 音效音乐
- [ ] 关卡设计
- [ ] 测试与优化

## 许可证

版权所有 © 2026 GameApex Team
