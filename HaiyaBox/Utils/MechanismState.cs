using System;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace HaiyaBox.Utils;

/// <summary>
/// 机制状态标记接口，实现此接口的状态类可使用 Clear 扩展方法
/// </summary>
public interface IMechanismState { }

/// <summary>
/// 机制状态扩展方法，提供自动清理功能
/// </summary>
public static class MechanismStateExtensions
{
    /// <summary>
    /// 自动清空状态实例中的所有字段
    /// string 字段设为 null，集合类型调用 Clear()，值类型设为 default，引用类型设为 null
    /// </summary>
    public static void Reset(this IMechanismState state)
    {
        foreach (var field in state.GetType().GetFields())
        {
            if (field.IsStatic)
                continue;

            if (field.FieldType == typeof(string))
            {
                field.SetValue(state, null);
            }
            else if (typeof(ICollection).IsAssignableFrom(field.FieldType))
            {
                var collection = field.GetValue(state);
                if (collection != null)
                {
                    var clearMethod = field.FieldType.GetMethod("Clear");
                    clearMethod?.Invoke(collection, null);
                }
            }
            else if (field.FieldType.IsValueType)
            {
                field.SetValue(state, Activator.CreateInstance(field.FieldType));
            }
            else
            {
                field.SetValue(state, null);
            }
        }
    }
}
