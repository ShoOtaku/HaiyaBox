# 变更提案: aoe-safety-fixes

## 元信息
```yaml
类型: 修复
方案类型: implementation
优先级: P1
状态: 草稿
创建: 2026-02-02
```

---

## 1. 需求

### 背景
从 bossmod 迁移 AOESafetyCalculator 后，发现 AOEShape 距离计算仍是占位实现，且部分输入边界会导致除零或无效结果。

### 目标
- 为 AOEShape 距离计算提供可用实现
- 修复边界条件导致的异常/不稳定行为
- 将场地边界纳入安全性判断

### 约束条件
```yaml
时间约束: 无
性能约束: 不引入显著额外开销（单次调用为常量复杂度）
兼容性约束: 仅对 AOEShape 距离 API 做必要调整
业务约束: 不改变现有安全区计算的核心逻辑
```

### 验收标准
- [ ] AOEShape.Distance/InvertedDistance 返回可用距离值，支持所有现有 AOE 形状
- [ ] MinDistanceBetween 传入 ≤0 时自动夹到最小值，不产生除零/溢出
- [ ] RectArenaBounds 方向向量为零时使用默认方向
- [ ] WPos.InRect 在 startToEnd 为零向量时返回 false
- [ ] IsSafe 对场地边界外位置返回不安全

---

## 2. 方案

### 技术方案
- 将 AOEShape 距离计算改为基于 DistanceField 形状实现，统一返回带符号距离
- 为 MinDistanceBetween 引入最小阈值，稳定候选网格生成
- 对零向量方向进行默认化处理，避免归一化 NaN
- 将场地边界作为安全性前置条件

### 影响范围
```yaml
涉及模块:
  - AOESafetyCalculator/Shapes: 距离 API 与实现
  - AOESafetyCalculator/SafetyZone: 边界判定与查询参数
  - AOESafetyCalculator/Core: 几何基础方法边界处理
预计变更文件: 5
```

### 风险评估
| 风险 | 等级 | 应对 |
|------|------|------|
| AOEShape 距离 API 变更影响外部调用 | 中 | 当前项目内无调用，变更同时更新文档说明 |
| 方向默认化改变极端输入表现 | 低 | 仅在零向量输入时触发，行为更稳定 |
