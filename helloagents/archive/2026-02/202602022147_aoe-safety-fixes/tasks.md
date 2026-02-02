# 任务清单: aoe-safety-fixes

> **@status:** completed | 2026-02-02 22:03

目录: `helloagents/plan/{YYYYMMDDHHMM}_aoe-safety-fixes/`

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
总任务: 9
已完成: 9
完成率: 100%
```

---

## 任务列表

### 1. AOESafetyCalculator/Shapes

- [√] 1.1 更新 `HaiyaBox/AOESafetyCalculator/Shapes/AOEShape.cs` 的 Distance/InvertedDistance 签名
  - 验证: 代码编译通过，派生类实现一致

- [√] 1.2 实现各 AOEShape 的距离计算（基于 DistanceField 形状，并处理 InvertForbiddenZone）
  - 验证: 手动检查返回符号与 Check 行为一致

### 2. AOESafetyCalculator/SafetyZone

- [√] 2.1 夹紧 `SafePositionQuery.MinDistanceBetween` 的最小值，避免网格计算异常
  - 验证: 负数或 0 输入时仍可生成候选点

- [√] 2.2 `SafeZoneCalculator.IsSafe` 将场地边界外视为不安全
  - 验证: 设置 ArenaBounds 后，边界外点返回 false

- [√] 2.3 `RectArenaBounds` 方向向量为零时使用默认方向 (1,0)
  - 验证: 零向量输入不再产生 NaN

### 3. AOESafetyCalculator/Core

- [√] 3.1 `WPos.InRect` 在 startToEnd 为零向量时直接返回 false
  - 验证: 零向量输入无除零

### 4. 测试

- [√] 4.1 补充 AOESafetyCalculator 的最小单元测试（距离符号、边界处理）
  - 验证: 测试可运行并覆盖关键分支

### 5. 知识库同步

- [√] 5.1 更新模块文档与索引（新增/补充 AOESafetyCalculator 模块）
  - 验证: `helloagents/modules/_index.md` 与模块文档一致

- [√] 5.2 更新 `helloagents/CHANGELOG.md`
  - 验证: 变更记录格式符合规范

---

## 执行备注

> 执行过程中的重要记录

| 任务 | 状态 | 备注 |
|------|------|------|
