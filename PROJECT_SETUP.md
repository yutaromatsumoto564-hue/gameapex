# ARIA: 异星觉醒 - 项目设置指南

## 第一步：打开Unity项目

1. 使用 **Unity 2022.3 LTS** 打开项目文件夹 `/opt/gameapex`
2. Unity会自动导入项目文件

## 第二步：创建初始场景

### 2.1 创建新场景
1. 在Unity中，点击 `File > New Scene`
2. 选择 `2D` 模板
3. 保存场景为 `Assets/Scenes/Game.unity`

### 2.2 设置相机
1. 选择 `Main Camera`
2. 设置 `Position` 为 `(25, 25, -10)`
3. 设置 `Size` 为 `20`
4. 设置 `Background` 为深色（例如：`#1a1a2e`）

### 2.3 创建GameBootstrapper
1. 在Hierarchy中创建空对象，命名为 `GameBootstrapper`
2. 添加 `GameBootstrapper.cs` 脚本
3. 确保设置中勾选 `CreateInitialCommandCenter`

### 2.4 创建基础游戏对象
1. 创建空对象命名为 `Managers` - 用于容纳所有管理器
2. 创建空对象命名为 `Buildings` - 用于容纳所有建筑
3. 创建空对象命名为 `Enemies` - 用于容纳所有敌人
4. 创建空对象命名为 `SpawnPoints` - 敌人出生点
5. 创建空对象命名为 `TargetPoint` - 敌人目标点（放在指挥中心附近）

### 2.5 添加出生点
1. 在 `SpawnPoints` 下创建几个空对象作为敌人出生点
2. 将它们放在地图边缘
3. 位置示例：
   - `SpawnPoint_1`: (5, 25, 0)
   - `SpawnPoint_2`: (45, 25, 0)
   - `SpawnPoint_3`: (25, 5, 0)
   - `SpawnPoint_4`: (25, 45, 0)

### 2.6 创建地图
1. 创建一个 `Tilemap` 作为地图背景
2. 或者创建一个大的 `Sprite` 作为地面
3. 确保地图尺寸至少为 `50x50`

## 第三步：生成游戏数据

### 3.1 使用数据生成器
1. 在Unity顶部菜单中点击 `ARIA > Generate Initial Data`
2. 等待数据生成完成
3. 检查 `Assets/Data` 文件夹下的所有数据是否已创建

### 3.2 验证数据
检查以下文件夹中是否有数据文件：
- `Assets/Data/Cards/`
- `Assets/Data/Buildings/`
- `Assets/Data/Resources/`
- `Assets/Data/Enemies/`
- `Assets/Data/Techs/`
- `Assets/Data/Waves/`

## 第四步：配置管理器

### 4.1 配置CardManager
1. 创建空对象命名为 `CardManager`
2. 添加 `CardManager.cs` 脚本
3. 将生成的所有CardData拖到 `All Cards` 列表中
4. 添加初始卡牌到 `Starting Cards`：
   - card_command_center (x1)
   - card_iron_miner (x2)
   - card_copper_miner (x1)
   - card_lumber_mill (x2)
   - card_furnace (x1)
   - card_steam_generator (x1)
   - card_storage (x2)
   - card_turret (x1)
   - card_wall (x10)

### 4.2 配置BuildingManager
1. 创建空对象命名为 `BuildingManager`
2. 添加 `BuildingManager.cs` 脚本
3. 设置网格参数：
   - Grid Width: 50
   - Grid Height: 50
   - Cell Size: 1
   - Grid Origin: (0, 0)
4. 设置Prefabs引用（后续创建）

### 4.3 配置ResourceManager
1. 创建空对象命名为 `ResourceManager`
2. 添加 `ResourceManager.cs` 脚本
3. 将生成的所有ResourceData拖到 `All Resources` 列表中
4. 设置初始资源：
   - resource_iron_ore (x50)
   - resource_copper_ore (x30)
   - resource_coal (x40)
   - resource_stone (x100)
   - resource_wood (x80)

### 4.4 配置EnemyManager
1. 创建空对象命名为 `EnemyManager`
2. 添加 `EnemyManager.cs` 脚本
3. 将生成的所有EnemyData拖到 `All Enemies` 列表中
4. 设置 `Spawn Point Container` 为刚才创建的 `SpawnPoints` 对象
5. 设置 `Target Point` 为刚才创建的 `TargetPoint` 对象

### 4.5 配置WaveManager
1. 创建空对象命名为 `WaveManager`
2. 添加 `WaveManager.cs` 脚本
3. 将生成的所有WaveData拖到 `Waves` 列表中

### 4.6 配置TechManager
1. 创建空对象命名为 `TechManager`
2. 添加 `TechManager.cs` 脚本
3. 将生成的所有TechData拖到 `All Techs` 列表中
4. 设置初始科技：
   - tech_improved_mining
   - tech_advanced_power

### 4.7 配置DayNightCycle
1. 创建空对象命名为 `DayNightCycle`
2. 添加 `DayNightCycle.cs` 脚本
3. （可选）添加Directional Light用于光照效果

### 4.8 配置其他管理器
- GameManager
- SaveSystem
- EventManager
- ResourceNetwork
- PowerManager
- UIManager

这些管理器可以使用默认设置创建。

## 第五步：创建预制体

### 5.1 创建建筑预制体
1. 创建简单的Sprite作为建筑外观（可以使用纯色方块暂时替代）
2. 为每种建筑创建预制体
3. 确保预制体上有 `Building.cs` 脚本组件

### 5.2 创建敌人预制体
1. 创建简单的Sprite作为敌人外观
2. 为每种敌人创建预制体
3. 确保预制体上有 `Enemy.cs` 脚本组件
4. 为敌人添加Collider2D和Rigidbody2D

### 5.3 创建UI预制体
1. 创建卡牌槽预制体
2. 创建资源显示预制体
3. 创建工具提示预制体

## 第六步：配置UI

### 6.1 创建基础UI Canvas
1. 创建Canvas对象
2. 设置 `Render Mode` 为 `Screen Space - Overlay`
3. 设置 `UI Scale Mode` 为 `Scale With Screen Size`

### 6.2 添加UI组件
1. 添加UIManager
2. 创建卡牌栏UI
3. 创建资源栏UI
4. 创建各种面板（暂停、设置、科技树等）

## 第七步：测试游戏

1. 点击Play按钮
2. 检查控制台是否有错误
3. 验证指挥中心是否自动放置
4. 验证卡牌是否正确显示
5. 等待夜晚到来，验证敌人生成

## 常见问题

### 问题1：找不到数据文件
- 确保已运行 `ARIA > Generate Initial Data`
- 检查 `Assets/Data` 文件夹是否存在

### 问题2：管理器引用缺失
- 确保所有管理器都已在场景中创建
- 检查脚本引用是否正确设置

### 问题3：编译错误
- 确保使用的是Unity 2022.3 LTS
- 检查所有脚本是否在正确的命名空间中
- 确保没有缺失的using语句

## 下一步

项目基础框架已完成，接下来可以：
1. 添加美术资源（像素风格建筑、敌人、UI）
2. 创建更多关卡数据
3. 添加音效和音乐
4. 实现更多游戏特性
5. 进行测试和优化
