> **@status:** completed | 2026-02-06 19:13

﻿# 任务清单: auto-draw-distancefield

目录: `helloagents/plan/202602061852_auto-draw-distancefield/`

---

## 任务状态符号说明

| 符号 | 状态 | 说明 |
|------|------|------|
| `[ ]` | pending | 待执行 |
| `[√]` | completed | 已完成 |
| `[X]` | failed | 执行失败 |
| `[-]` | skipped | 已跳过 |
| `[?]` | uncertain | 待确认 |

---

## 执行状态
```yaml
总任务: 5
已完成: 0
完成率: 0%
```

---

## 任务列表

### 1. AOESafetyCalculator 扩展
- [ ] 1.1 新增 SafeZoneDrawRegistry（弱引用注册表 + 安全点通知）
  - 验证: 新建 SafeZoneCalculator 时自动注册
- [ ] 1.2 在 SafeZoneCalculator 添加构造函数注册 + 内部访问器
  - 验证: 可枚举到 Zones 与 ArenaBounds
- [ ] 1.3 在 SafePositionQuery.Execute 完成后通知安全点结果
  - 验证: 调用 Execute 后收到回调

### 2. 自动绘制与轮廓生成
- [ ] 2.1 新增 SafeZoneAutoDraw 服务（启停、计数、清理）
  - 验证: 开关关闭时不输出 DisplayObject
- [ ] 2.2 新增 DistanceFieldContourBuilder（marching squares）
  - 验证: SDCircle/SDRect 产生连续轮廓线
- [ ] 2.3 Overlay 管理器整合 AOE/自动绘制开关
  - 验证: 任一开关开启即可渲染

### 3. 生命周期与清理
- [ ] 3.1 在 AutoRaidHelper 初始化 SafeZoneAutoDraw
  - 验证: 插件启动后可注册回调
- [ ] 3.2 在 SafeZoneAutoDraw 内处理副本重置场景清理
  - 验证: DutyWiped/DutyCompleted/离开副本/重新进入/战斗计时回退触发清空

### 4. UI 与配置
- [ ] 4.1 在 FullAutoSettings 新增 SafeZone 绘制开关配置
  - 验证: 保存/读取生效
- [ ] 4.2 在 DangerAreaTab/GeometryTab 添加开关、数量显示、清空按钮
  - 验证: 两处 UI 同步控制

### 5. 测试
- [ ] 5.1 为轮廓生成添加基础单元测试（Circle/Rect）
  - 验证: 输出线段数量 > 0

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
