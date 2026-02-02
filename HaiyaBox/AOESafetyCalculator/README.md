# AOESafetyCalculator 使用文档

## 目录

- [概述](#概述)
- [核心概念](#核心概念)
- [场地边界](#场地边界)
- [快速开始](#快速开始)
- [基础API](#基础api)
- [链式查询API](#链式查询api)
- [使用场景](#使用场景)
- [性能考虑](#性能考虑)
- [常见问题](#常见问题)

---

## 概述

AOESafetyCalculator 是一个基于**有向距离场（Signed Distance Field, SDF）**的安全区域计算库，用于在复杂的危险区域环境中快速计算安全位置。

### 主要特性

- ✅ **20种基础形状**：圆形、矩形、扇形、环形、三角形、胶囊体等
- ✅ **布尔运算**：并集、交集、反转
- ✅ **约束查询**：支持距离、角度、分散等多种约束
- ✅ **自动排序**：结果按指定参考点距离排序
- ✅ **高性能**：40×40场地 + 40个禁止区域 + 8个安全点 ≈ 5-10ms
- ✅ **零依赖**：独立编译，无游戏引擎依赖

### 适用场景

- 游戏AI的位置决策
- 多人游戏的站位计算
- 路径规划的安全点查找
- 实时战斗的躲避计算

---

## 核心概念

### 1. 有向距离场（SDF）

距离场是一个函数，对于空间中的任意点，返回该点到形状边界的距离：

```
Distance(P) > 0  → P 在形状外部（正值 = 到边界的距离）
Distance(P) = 0  → P 在形状边界上
Distance(P) < 0  → P 在形状内部（负值 = 深入内部的深度）
```

**示例：圆形距离场**
```
圆心 (0, 0)，半径 10

点 (5, 0)  → Distance = -5  (在圆内，距离边界5米)
点 (10, 0) → Distance = 0   (在圆边界上)
点 (15, 0) → Distance = 5   (在圆外，距离边界5米)
```

### 2. 禁止区域（ForbiddenZone）

禁止区域是一个危险区域的抽象，包含：
- **Shape**：形状距离场（ShapeDistance）
- **Activation**：激活时间（DateTime）

```csharp
var zone = new ForbiddenZone
{
    Shape = new SDCircle(new WPos(0, 0), 10f),  // 圆形，半径10米
    Activation = DateTime.Now.AddSeconds(3)      // 3秒后激活
};
```

### 3. 安全区计算器（SafeZoneCalculator）

计算器管理多个禁止区域，提供安全性查询和安全位置查找功能。

---

## 场地边界

### 什么是场地边界？

场地边界（Arena Bounds）定义了战斗场地的可活动范围。在实际游戏中，场地通常有明确的边界（如圆形竞技场、矩形房间），玩家不能超出这些边界。

**为什么需要场地边界？**
- 确保计算的安全点在场地内
- 避免推荐玩家移动到场地外
- 支持动态变化的场地（如场地缩小）

### 支持的场地类型

#### 1. 圆形场地（CircleArenaBounds）

最常见的场地类型，以中心点和半径定义。

```csharp
var arenaBounds = new CircleArenaBounds(
    center: new WPos(0, 0),  // 场地中心
    radius: 40f               // 场地半径（米）
);
```

#### 2. 矩形场地（RectArenaBounds）

矩形场地，支持旋转方向。

```csharp
var arenaBounds = new RectArenaBounds(
    center: new WPos(0, 0),           // 场地中心
    direction: new WDir(1, 0),        // 朝向（东）
    halfWidth: 15f,                   // 半宽（总宽30米）
    halfLength: 20f                   // 半长（总长40米）
);
```

### 如何使用场地边界

**步骤1：设置场地边界**

```csharp
var calculator = new SafeZoneCalculator();

// 设置圆形场地
calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 40f));
```

**步骤2：使用简化的查询API**

设置场地边界后，可以使用简化的 `FindSafePositions` 重载：

```csharp
// 自动使用场地边界，无需指定搜索中心和半径
var safePoints = calculator.FindSafePositions(8, DateTime.Now)
    .NearTarget(targetPos, 20f)
    .MinDistanceBetween(4f)
    .Execute();
```

**步骤3：场地变化时更新**

```csharp
// 阶段1：大场地（半径40米）
calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 40f));
var phase1Points = calculator.FindSafePositions(8, DateTime.Now).Execute();

// 阶段2：场地缩小（半径25米）
calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 25f));
var phase2Points = calculator.FindSafePositions(8, DateTime.Now).Execute();
```

### 两种查询方式对比

**方式1：使用场地边界（推荐）**

```csharp
// 先设置场地边界
calculator.SetArenaBounds(new CircleArenaBounds(center, 40f));

// 简化的查询（自动使用场地边界）
var points = calculator.FindSafePositions(8, currentTime)
    .NearTarget(targetPos)
    .Execute();
```

**优点：**
- API更简洁
- 自动限制在场地内
- 支持场地动态变化

**方式2：手动指定搜索范围**

```csharp
// 直接指定搜索中心和半径
var points = calculator.FindSafePositions(8, searchCenter, searchRadius, currentTime)
    .NearTarget(targetPos)
    .Execute();
```

**优点：**
- 无需预先设置场地
- 适合临时查询
- 向后兼容旧代码

---

## 快速开始

### 安装

将 AOESafetyCalculator.dll 添加到项目引用。

### 基础示例

```csharp
using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;
using AOESafetyCalculator.SafetyZone;

// 1. 创建计算器
var calculator = new SafeZoneCalculator();

// 2. 添加危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(new WPos(0, 0), 10f),  // 圆形危险区
    Activation = DateTime.Now                    // 立即激活
});

// 3. 检查位置是否安全
var playerPos = new WPos(5, 0);
var currentTime = DateTime.Now;

if (calculator.IsSafe(playerPos, currentTime))
{
    Console.WriteLine("当前位置安全");
}
else
{
    Console.WriteLine("当前位置危险！");
}

// 4. 计算到最近危险的距离
var distance = calculator.DistanceToNearestDanger(playerPos, currentTime);
Console.WriteLine($"距离最近危险: {distance:F2}米");
```

---

## 基础API

### SafeZoneCalculator 类

#### 构造函数

```csharp
var calculator = new SafeZoneCalculator();
```

#### 场地边界管理

```csharp
// 设置场地边界
void SetArenaBounds(ArenaBounds bounds);

// 获取当前场地边界
ArenaBounds? GetArenaBounds();
```

**示例：**
```csharp
// 设置圆形场地
calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 40f));

// 设置矩形场地
calculator.SetArenaBounds(new RectArenaBounds(
    center: new WPos(0, 0),
    direction: new WDir(1, 0),
    halfWidth: 20f,
    halfLength: 20f
));

// 获取当前场地边界
var bounds = calculator.GetArenaBounds();
if (bounds != null)
{
    Console.WriteLine($"场地中心: {bounds.Center}");
    Console.WriteLine($"近似半径: {bounds.ApproximateRadius}");
}
```

#### 添加/清除禁止区域

```csharp
// 添加单个区域
calculator.AddForbiddenZone(ForbiddenZone zone);

// 添加多个区域
calculator.AddForbiddenZones(IEnumerable<ForbiddenZone> zones);

// 清除所有区域
calculator.Clear();
```

#### 安全性查询

```csharp
// 检查位置是否安全
bool IsSafe(WPos position, DateTime currentTime);

// 计算到最近危险的距离
float DistanceToNearestDanger(WPos position, DateTime currentTime);

// 获取活跃的禁止区域数量
int GetActiveZoneCount(DateTime currentTime);

// 获取所有活跃的禁止区域
IEnumerable<ForbiddenZone> GetActiveZones(DateTime currentTime);
```

#### 简单位置查找

```csharp
// 查找最安全的方向（8个方向采样）
WDir FindSafestDirection(WPos position, DateTime currentTime, int sampleCount = 8);

// 查找最安全的位置（网格搜索）
WPos FindSafestPosition(WPos center, float radius, DateTime currentTime, float gridResolution = 1.0f);
```

---

## 链式查询API

### 概述

链式查询API提供了灵活的约束配置方式，用于查找满足特定条件的多个安全位置。

### 基本用法

```csharp
// 启动链式查询
var query = calculator.FindSafePositions(
    count: 8,                    // 需要的安全点数量
    searchCenter: arenaCenter,   // 搜索区域中心
    searchRadius: 40f,           // 搜索半径
    currentTime: DateTime.Now    // 当前时间
);

// 添加约束（可选）
query.NearTarget(targetPos, maxDistance: 20f)
     .MinDistanceBetween(5f)
     .WithMinAngle(centerPoint, 45f.Degrees());

// 执行查询，获取结果
List<WPos> safePoints = query.Execute();
```

### 约束方法

#### NearTarget - 靠近目标点

限制安全点必须靠近指定的目标点，并自动按距离目标点排序。

```csharp
SafePositionQuery NearTarget(WPos target, float? maxDistance = null)
```

**参数：**
- `target`：目标点位置
- `maxDistance`：最大距离限制（可选，单位：米）

**效果：**
- 过滤掉距离目标点超过 `maxDistance` 的点
- 结果自动按距离目标点排序（近的在前）

**示例：**
```csharp
// 查找靠近Boss的5个安全点，最远不超过20米
var points = calculator.FindSafePositions(5, arenaCenter, 40f, currentTime)
    .NearTarget(bossPos, maxDistance: 20f)
    .Execute();

// points[0] 是最靠近Boss的安全点
// points[4] 是第5靠近Boss的安全点
```

#### MinDistanceBetween - 点之间最小距离

确保所有安全点之间保持最小距离，避免聚集。

```csharp
SafePositionQuery MinDistanceBetween(float minDistance)
```

**参数：**
- `minDistance`：最小距离（单位：米）

**效果：**
- 使用泊松圆盘采样生成候选点
- 选择点时确保与已选点的距离 ≥ `minDistance`

**示例：**
```csharp
// 8个玩家站位，彼此至少相距5米
var points = calculator.FindSafePositions(8, center, 40f, currentTime)
    .MinDistanceBetween(5f)
    .Execute();
```


#### WithMinAngle - 角度约束

确保安全点相对于中心点的角度间隔不小于指定值，实现方向分散。

```csharp
SafePositionQuery WithMinAngle(WPos center, Angle minAngle)
```

**参数：**
- `center`：中心点（角度计算的参考点）
- `minAngle`：最小角度间隔

**效果：**
- 计算每个点相对于中心点的角度
- 确保任意两个点的角度差 ≥ `minAngle`

**示例：**
```csharp
// 8个方向分散站位，相对于Boss的角度至少相差45度
var points = calculator.FindSafePositions(8, arenaCenter, 40f, currentTime)
    .WithMinAngle(bossPos, 45f.Degrees())
    .Execute();

// 结果：8个点均匀分布在Boss周围的8个方向
```

#### OrderByDistanceTo - 自定义排序

明确指定排序参考点（默认使用 NearTarget 的目标点）。

```csharp
SafePositionQuery OrderByDistanceTo(WPos reference)
```

**参数：**
- `reference`：排序参考点

**效果：**
- 结果按距离参考点的距离排序（近的在前）

**示例：**
```csharp
// 查找安全点，按距离治疗位置排序
var points = calculator.FindSafePositions(5, center, 40f, currentTime)
    .OrderByDistanceTo(healerPos)
    .Execute();
```

### 约束组合

多个约束可以链式组合使用：

```csharp
var points = calculator.FindSafePositions(8, arenaCenter, 40f, currentTime)
    .NearTarget(tankPos, maxDistance: 20f)  // 靠近坦克，最远20米
    .MinDistanceBetween(4f)                  // 彼此至少4米
    .WithMinAngle(bossPos, 30f.Degrees())   // 相对Boss角度至少30度
    .Execute();                              // 自动按距离坦克排序
```


---

## 使用场景

### 场景1：单点安全检查

**需求：** 检查玩家当前位置是否安全

```csharp
var calculator = new SafeZoneCalculator();

// 添加危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(new WPos(0, 0), 10f),
    Activation = DateTime.Now
});

// 检查位置
var playerPos = new WPos(5, 0);
var currentTime = DateTime.Now;

if (!calculator.IsSafe(playerPos, currentTime))
{
    // 位置不安全，计算到最近危险的距离
    var distance = calculator.DistanceToNearestDanger(playerPos, currentTime);
    Console.WriteLine($"危险！距离最近危险区域: {-distance:F1}米");
    
    // 查找安全方向
    var safeDir = calculator.FindSafestDirection(playerPos, currentTime);
    Console.WriteLine($"建议移动方向: ({safeDir.X:F2}, {safeDir.Z:F2})");
}
```

### 场景2：近战DPS站位

**需求：** 查找靠近Boss的5个安全点，按距离Boss排序

```csharp
var calculator = new SafeZoneCalculator();
var bossPos = new WPos(0, 0);

// Boss脚下危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(bossPos, 8f)
});

// 查找靠近Boss的安全点
var meleePositions = calculator.FindSafePositions(5, bossPos, 40f, DateTime.Now)
    .NearTarget(bossPos, maxDistance: 15f)  // 近战范围15米
    .MinDistanceBetween(2f)                  // 避免重叠
    .Execute();

// meleePositions[0] 是最靠近Boss的安全点
Console.WriteLine($"最佳近战位置: ({meleePositions[0].X:F1}, {meleePositions[0].Z:F1})");
```

### 场景3：8人分散站位

**需求：** 8个玩家需要分散站位，避免AOE连锁

```csharp
var calculator = new SafeZoneCalculator();
var arenaCenter = new WPos(0, 0);

// 添加多个危险区域
for (int i = 0; i < 10; i++)
{
    calculator.AddForbiddenZone(new ForbiddenZone
    {
        Shape = new SDCircle(RandomPosition(), 5f)
    });
}

// 查找8个分散的安全点
var spreadPositions = calculator.FindSafePositions(8, arenaCenter, 40f, DateTime.Now)
    .MinDistanceBetween(6f)  // 玩家之间至少6米
    .Execute();

// 分配给8个玩家
for (int i = 0; i < spreadPositions.Count; i++)
{
    Console.WriteLine($"玩家{i + 1}站位: {spreadPositions[i]}");
}
```


### 场景4：坦克+治疗组队站位

**需求：** 坦克和治疗需要靠近，其他DPS分散

```csharp
var calculator = new SafeZoneCalculator();
var tankPos = new WPos(15, 0);

// 添加危险区域（省略）

// 查找治疗位置（靠近坦克）
var healerPositions = calculator.FindSafePositions(2, tankPos, 40f, DateTime.Now)
    .NearTarget(tankPos, maxDistance: 10f)
    .MinDistanceBetween(3f)
    .Execute();

// 查找DPS位置（分散）
var dpsPositions = calculator.FindSafePositions(6, arenaCenter, 40f, DateTime.Now)
    .MinDistanceBetween(5f)
    .Execute();
```

### 场景5：方向分散（钟表站位）

**需求：** 8个玩家按8个方向分散（类似钟表12点、3点、6点、9点等）

```csharp
var calculator = new SafeZoneCalculator();
var bossPos = new WPos(0, 0);

// 添加危险区域（省略）

// 查找8个方向分散的安全点
var clockPositions = calculator.FindSafePositions(8, bossPos, 40f, DateTime.Now)
    .WithMinAngle(bossPos, 45f.Degrees())  // 45度间隔（360/8=45）
    .MinDistanceBetween(3f)
    .Execute();

// 结果：8个点均匀分布在Boss周围
```

### 场景6：动态变化的场地

**需求：** Boss战斗中场地会缩小，需要在不同阶段计算安全点

```csharp
var calculator = new SafeZoneCalculator();
var bossPos = new WPos(0, 0);

// 阶段1：大场地（半径40米）
calculator.SetArenaBounds(new CircleArenaBounds(bossPos, 40f));

// 添加危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(new WPos(10, 0), 8f)
});

// 查找阶段1的安全点
var phase1Points = calculator.FindSafePositions(8, DateTime.Now)
    .MinDistanceBetween(5f)
    .Execute();

Console.WriteLine($"阶段1（大场地）: 找到 {phase1Points.Count} 个安全点");

// 阶段2：场地缩小（半径25米）
calculator.SetArenaBounds(new CircleArenaBounds(bossPos, 25f));

// 重新计算安全点（自动限制在新场地内）
var phase2Points = calculator.FindSafePositions(8, DateTime.Now)
    .MinDistanceBetween(5f)
    .Execute();

Console.WriteLine($"阶段2（小场地）: 找到 {phase2Points.Count} 个安全点");
```

### 场景7：矩形竞技场

**需求：** 在矩形竞技场中计算安全站位

```csharp
var calculator = new SafeZoneCalculator();
var arenaCenter = new WPos(0, 0);

// 设置矩形竞技场（40米×30米）
calculator.SetArenaBounds(new RectArenaBounds(
    center: arenaCenter,
    direction: new WDir(1, 0),  // 朝向东
    halfWidth: 15f,              // 半宽15米（总宽30米）
    halfLength: 20f              // 半长20米（总长40米）
));

// 添加Boss脚下危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(arenaCenter, 8f)
});

// 查找安全点（自动限制在矩形场地内）
var safePoints = calculator.FindSafePositions(8, DateTime.Now)
    .MinDistanceBetween(4f)
    .Execute();

Console.WriteLine($"矩形场地内找到 {safePoints.Count} 个安全点");
foreach (var point in safePoints)
{
    Console.WriteLine($"  ({point.X:F1}, {point.Z:F1})");
}
```

### 场景8：预判未来危险

**需求：** 考虑即将激活的AOE，提前移动到安全位置

```csharp
var calculator = new SafeZoneCalculator();

// 当前危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(new WPos(0, 0), 10f),
    Activation = DateTime.Now
});

// 3秒后激活的危险区域
calculator.AddForbiddenZone(new ForbiddenZone
{
    Shape = new SDCircle(new WPos(20, 0), 10f),
    Activation = DateTime.Now.AddSeconds(3)
});

var playerPos = new WPos(15, 0);
var futureTime = DateTime.Now.AddSeconds(3);

// 检查3秒后是否安全
if (!calculator.IsSafe(playerPos, futureTime))
{
    // 查找3秒后仍然安全的位置
    var safePos = calculator.FindSafestPosition(playerPos, 20f, futureTime);
    Console.WriteLine($"建议移动到: {safePos}");
}
```

---

## 性能考虑

### 时间复杂度

**单点查询：**
```
IsSafe(position)              → O(zones)
DistanceToNearestDanger(pos)  → O(zones)
```

**多点查询（链式API）：**
```
FindSafePositions(count)      → O(candidates × zones)

其中：
- candidates ≈ (searchRadius / minDistance)² × π
- 40×40场地，minDistance=2m → candidates ≈ 1200个
- 40个zones → 总计算 ≈ 48,000次距离计算
- 预计耗时：5-10ms（现代CPU）
```

### 性能优化建议

**1. 减少候选点数量**
```csharp
// 增大最小距离，减少候选点
.MinDistanceBetween(3f)  // 而不是 1f
```

**2. 限制搜索范围**
```csharp
// 缩小搜索半径
calculator.FindSafePositions(8, center, 30f, currentTime)  // 而不是 40f
```

**3. 使用距离限制**
```csharp
// 提前过滤远距离点
.NearTarget(targetPos, maxDistance: 20f)
```

**4. 缓存计算器实例**
```csharp
// 避免频繁创建计算器
private readonly SafeZoneCalculator _calculator = new();

// 更新时清除并重新添加区域
_calculator.Clear();
_calculator.AddForbiddenZones(newZones);
```

### 性能测试数据

```
测试环境: Intel i7-12700K, .NET 8.0
场地大小: 40×40米
禁止区域: 40个圆形（半径5米）
查询数量: 8个安全点
最小距离: 3米

结果:
- 候选点生成: 2ms
- 安全性过滤: 3ms
- 评分排序: 1ms
- 总耗时: 6ms
```


---

## 常见问题

### Q1: 为什么找不到足够的安全点？

**可能原因：**
1. 危险区域太多，安全空间不足
2. 约束条件太严格（如 `minDistance` 太大）
3. 搜索范围太小

**解决方案：**
```csharp
// 检查实际找到的点数
var points = calculator.FindSafePositions(8, center, 40f, currentTime)
    .MinDistanceBetween(3f)
    .Execute();

if (points.Count < 8)
{
    Console.WriteLine($"只找到 {points.Count} 个安全点");
    
    // 放宽约束重试
    points = calculator.FindSafePositions(8, center, 40f, currentTime)
        .MinDistanceBetween(2f)  // 减小最小距离
        .Execute();
}
```

### Q2: 如何处理动态变化的危险区域？

**方案：** 每次更新时清除并重新添加

```csharp
// 更新危险区域
public void UpdateDangerZones(List<ForbiddenZone> newZones)
{
    calculator.Clear();
    calculator.AddForbiddenZones(newZones);
}

// 然后查询
var safePoints = calculator.FindSafePositions(8, center, 40f, DateTime.Now)
    .Execute();
```

### Q3: 结果的排序规则是什么？

**排序规则：**
- 使用 `.NearTarget(pos)` 时，自动按距离 `pos` 排序
- 使用 `.OrderByDistanceTo(pos)` 时，按距离 `pos` 排序
- 不使用排序方法时，按评分排序（安全距离优先）

```csharp
// 示例：按距离Boss排序
var points = calculator.FindSafePositions(5, center, 40f, currentTime)
    .NearTarget(bossPos)
    .Execute();
// points[0] 最靠近Boss，points[4] 最远离Boss
```

### Q4: 如何自定义评分规则？

**当前版本不支持自定义评分。** 内置评分规则：
- 安全距离（距离危险越远越好）× 10
- 目标距离（距离目标越近越好）× -5

**替代方案：** 获取结果后自行排序

```csharp
var points = calculator.FindSafePositions(10, center, 40f, currentTime)
    .Execute();

// 自定义排序
points.Sort((a, b) =>
{
    var scoreA = CustomScore(a);
    var scoreB = CustomScore(b);
    return scoreB.CompareTo(scoreA);
});

// 取前8个
var topPoints = points.Take(8).ToList();
```

### Q5: 泊松圆盘采样是什么？

**泊松圆盘采样** 是一种生成均匀分布点的算法，确保任意两点之间的距离不小于指定值。

**优势：**
- 避免点聚集
- 覆盖范围均匀
- 生成速度快

**效果对比：**
```
随机采样:        泊松圆盘采样:
  ●  ●●           ●   ●   ●
 ●●   ●          ●   ●   ●
  ●  ●            ●   ●   ●
（聚集）         （均匀分散）
```

### Q6: 必须设置场地边界吗？

**不是必须的。** 有两种使用方式：

**方式1：使用场地边界（推荐）**
```csharp
calculator.SetArenaBounds(new CircleArenaBounds(center, 40f));
var points = calculator.FindSafePositions(8, currentTime).Execute();
```

**方式2：手动指定搜索范围**
```csharp
var points = calculator.FindSafePositions(8, center, 40f, currentTime).Execute();
```

**建议：** 如果场地有明确边界，使用方式1更简洁且能自动限制在场地内。

### Q7: 场地边界可以动态变化吗？

**可以。** 随时调用 `SetArenaBounds` 更新场地边界：

```csharp
// 阶段1：大场地
calculator.SetArenaBounds(new CircleArenaBounds(center, 40f));
var phase1Points = calculator.FindSafePositions(8, currentTime).Execute();

// 阶段2：场地缩小
calculator.SetArenaBounds(new CircleArenaBounds(center, 25f));
var phase2Points = calculator.FindSafePositions(8, currentTime).Execute();
```

---

## 形状参考

### 基础形状

#### 圆形（SDCircle）
```csharp
new SDCircle(WPos origin, float radius)
```
- **origin**: 圆心位置
- **radius**: 半径

#### 反转圆形（SDInvertedCircle）
```csharp
new SDInvertedCircle(WPos origin, float radius)
```
- 圆形外部是危险区域，内部是安全区域

#### 矩形（SDRect）
```csharp
new SDRect(WPos origin, WDir direction, float halfWidth, float halfLength, float maxDistance)
```
- **origin**: 矩形中心
- **direction**: 矩形朝向
- **halfWidth**: 半宽
- **halfLength**: 半长

#### 扇形（SDCone）
```csharp
new SDCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
```
- **origin**: 扇形顶点
- **radius**: 扇形半径
- **centerDir**: 中心方向
- **halfAngle**: 半角（扇形总角度 = halfAngle × 2）

#### 环形（SDDonut）
```csharp
new SDDonut(WPos origin, float innerRadius, float outerRadius)
```
- **innerRadius**: 内圈半径
- **outerRadius**: 外圈半径
- 环形区域（内圈和外圈之间）是危险区域

#### 胶囊体（SDCapsule）
```csharp
new SDCapsule(WPos origin, WDir direction, float halfLength, float radius)
```
- 两端是半圆，中间是矩形的组合形状

### 布尔运算

#### 并集（SDUnion）
```csharp
new SDUnion(ShapeDistance[] shapes)
```
- 多个形状的并集（任意一个形状内部都是危险区域）

#### 交集（SDIntersection）
```csharp
new SDIntersection(ShapeDistance[] shapes)
```
- 多个形状的交集（所有形状都重叠的区域是危险区域）

#### 反转并集（SDInvertedUnion）
```csharp
new SDInvertedUnion(ShapeDistance[] shapes)
```
- 所有形状外部的区域是危险区域

### 形状选择建议

| 场景 | 推荐形状 | 说明 |
|------|---------|------|
| Boss脚下AOE | SDCircle | 简单圆形 |
| 直线AOE | SDRect | 矩形 |
| 扇形AOE | SDCone | 扇形 |
| 环形AOE | SDDonut | 环形（击退后安全） |
| 多个重叠AOE | SDUnion | 并集 |
| 安全区域 | SDInvertedCircle | 反转圆形 |


---

## 完整示例

### 实战场景：Boss战斗站位计算

```csharp
using AOESafetyCalculator.Core;
using AOESafetyCalculator.DistanceField;
using AOESafetyCalculator.SafetyZone;

public class BossFightPositioning
{
    private readonly SafeZoneCalculator calculator = new();
    
    // Boss战斗状态
    private WPos bossPosition;
    private WPos tankPosition;
    private List<AOEInfo> activeAOEs = new();
    
    // 更新危险区域
    public void UpdateDangerZones(DateTime currentTime)
    {
        calculator.Clear();
        
        // 添加所有活跃的AOE
        foreach (var aoe in activeAOEs)
        {
            calculator.AddForbiddenZone(new ForbiddenZone
            {
                Shape = CreateShape(aoe),
                Activation = aoe.ActivationTime
            });
        }
    }
    
    // 计算近战DPS站位（5个）
    public List<WPos> CalculateMeleePositions(DateTime currentTime)
    {
        return calculator.FindSafePositions(5, bossPosition, 40f, currentTime)
            .NearTarget(bossPosition, maxDistance: 12f)  // 近战范围12米
            .MinDistanceBetween(2f)                       // 避免重叠
            .Execute();
    }
    
    // 计算远程DPS站位（3个）
    public List<WPos> CalculateRangedPositions(DateTime currentTime)
    {
        return calculator.FindSafePositions(3, bossPosition, 40f, currentTime)
            .MinDistanceBetween(5f)  // 远程分散
            .Execute();
    }
    
    // 计算治疗站位（2个，靠近坦克）
    public List<WPos> CalculateHealerPositions(DateTime currentTime)
    {
        return calculator.FindSafePositions(2, tankPosition, 40f, currentTime)
            .NearTarget(tankPosition, maxDistance: 15f)
            .MinDistanceBetween(3f)
            .Execute();
    }
    
    // 检查玩家位置是否安全
    public bool IsPlayerSafe(WPos playerPos, DateTime currentTime)
    {
        return calculator.IsSafe(playerPos, currentTime);
    }
    
    // 获取最近的安全位置
    public WPos GetNearestSafePosition(WPos playerPos, DateTime currentTime)
    {
        if (calculator.IsSafe(playerPos, currentTime))
            return playerPos;
        
        return calculator.FindSafestPosition(playerPos, 20f, currentTime, 1.5f);
    }
    
    private ShapeDistance CreateShape(AOEInfo aoe)
    {
        return aoe.Type switch
        {
            AOEType.Circle => new SDCircle(aoe.Position, aoe.Radius),
            AOEType.Cone => new SDCone(aoe.Position, aoe.Radius, 
                                       Angle.FromDirection(aoe.Direction), 
                                       aoe.HalfAngle),
            AOEType.Rect => new SDRect(aoe.Position, aoe.Direction, 
                                       aoe.HalfWidth, aoe.HalfLength, aoe.Radius),
            AOEType.Donut => new SDDonut(aoe.Position, aoe.InnerRadius, aoe.OuterRadius),
            _ => new SDCircle(aoe.Position, aoe.Radius)
        };
    }
}

// AOE信息类
public class AOEInfo
{
    public AOEType Type { get; set; }
    public WPos Position { get; set; }
    public WDir Direction { get; set; }
    public float Radius { get; set; }
    public float HalfWidth { get; set; }
    public float HalfLength { get; set; }
    public float InnerRadius { get; set; }
    public float OuterRadius { get; set; }
    public Angle HalfAngle { get; set; }
    public DateTime ActivationTime { get; set; }
}

public enum AOEType
{
    Circle,
    Cone,
    Rect,
    Donut
}
```

---

## 总结

### 核心优势

1. **简单易用** - 链式API，直观的约束配置
2. **高性能** - 5-10ms完成复杂查询
3. **灵活强大** - 支持多种约束组合
4. **自动排序** - 结果预先排序，无需手动处理

### 快速参考

```csharp
// 基础查询
calculator.IsSafe(pos, time);
calculator.DistanceToNearestDanger(pos, time);

// 链式查询
calculator.FindSafePositions(count, center, radius, time)
    .NearTarget(target, maxDistance)      // 靠近目标
    .MinDistanceBetween(minDistance)      // 点之间最小距离
    .WithMinAngle(center, minAngle)       // 角度约束
    .OrderByDistanceTo(reference)         // 自定义排序
    .Execute();                           // 执行查询
```

### 相关资源

- **示例代码**:
  - `Examples/BasicUsage.cs` - 基础用法示例
  - `Examples/ConstrainedSafetyExample.cs` - 约束查询示例
  - `Examples/ArenaBoundsExample.cs` - 场地边界示例
- **源代码**:
  - `SafetyZone/SafeZoneCalculator.cs` - 安全区计算器
  - `SafetyZone/SafePositionQuery.cs` - 链式查询构建器
  - `SafetyZone/ArenaBounds.cs` - 场地边界抽象类
  - `SafetyZone/RectArenaBounds.cs` - 矩形场地边界

---

**版本**: 1.0.0  
**最后更新**: 2026-02-02

