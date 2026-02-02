# 变更提案: aoe-shape-debug-render

## 元信息
```yaml
类型: 新功能
方案类型: implementation
优先级: P1
状态: 已完成
创建: 2026-02-03
```

---

## 1. 需求

### 背景
AOESafetyCalculator 迁移完成后，需要在游戏画面中直观看到 AOE 形状，以验证形状与旋转是否准确。

### 目标
- 使用现有绘制开关控制是否显示调试 AOE
- 通过 DangerAreaRenderer 绘制所有 AOEShape 轮廓
- 以 `shape + origin + rotation + duration` 方式绘制并自动过期
- 同尺寸形状使用同色，位置与旋转不影响颜色归类

### 约束条件
```yaml
时间约束: 无
性能约束: 避免过多临时对象，按 duration 自动清理
兼容性约束: 复用现有 DangerAreaRenderer 与 Overlay 开关
业务约束: 仅用于调试显示，不影响现有危险区域逻辑
```

### 验收标准
- [x] Overlay 开关关闭时不绘制，开启时显示调试形状
- [x] 支持全部 AOEShape 类型的轮廓绘制
- [x] 同尺寸形状颜色一致，持续时间到期后自动消失

---

## 2. 方案

### 技术方案
在 Utils 增加 AOEShapeDebug，注册 DangerAreaRenderer 的临时回调；由 Draw 接口写入条目并按时间过期；根据形状类型与尺寸生成轮廓线 DisplayObject，并使用 5 色固定调色板进行颜色分桶。

### 影响范围
```yaml
涉及模块:
  - Utils: 新增 AOEShapeDebug 工具
  - Rendering: 使用临时回调绘制轮廓线
  - Plugin: 生命周期中初始化/释放调试绘制
预计变更文件: 6
```

### 风险评估
| 风险 | 等级 | 应对 |
|------|------|------|
| 临时对象过多影响渲染 | 低 | 通过 duration 自动过期，避免无限积累 |

---

## 4. 核心场景

### 场景: AOE 形状调试绘制
**模块**: Utils / Rendering  
**条件**: 调用 `AOEShapeDebug.Draw(shape, origin, rotation, durationSeconds)`  
**行为**: 生成轮廓线对象并通过 DangerAreaRenderer 绘制  
**结果**: 游戏画面中出现对应 AOE 轮廓，超时自动消失

---

## 5. 技术决策

### aoe-shape-debug-render#D001: 使用 DangerAreaRenderer 临时回调绘制 AOE 轮廓
**日期**: 2026-02-03  
**状态**: ✅采纳  
**背景**: 项目已有统一的 Overlay 开关与渲染器，复用可减少 UI 复杂度  
**选项分析**:
| 选项 | 优点 | 缺点 |
|------|------|------|
| A: 新增独立绘制系统 | 不影响现有逻辑 | 需要新增开关与渲染流程 |
| B: 复用 DangerAreaRenderer | 复用开关与渲染能力 | 调试与危险区域共享开关 |
**决策**: 选择方案 B  
**理由**: 复用现有渲染与开关，成本低且易维护  
**影响**: Utils、Rendering、Plugin
