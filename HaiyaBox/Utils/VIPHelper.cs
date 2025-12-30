using System;
using System.Reflection;
using AEAssist.Verify;
using AEAssist.Helper;

namespace HaiyaBox.Utils
{
    public static class VIPHelper
    {
        /// <summary>
        /// 尝试修改AEAssist运行时的VIP级别
        /// </summary>
        /// <param name="targetLevel">目标VIP级别</param>
        /// <returns>是否修改成功</returns>
        public static bool TryChangeVIPLevel(VIPLevel targetLevel)
        {
            try
            {
                LogHelper.Print("开始尝试修改VIP级别");
                
                // 1. 获取AEAssist程序集
                var aeAssistAssembly = Assembly.Load("AEAssist");
                if (aeAssistAssembly == null)
                {
                    LogHelper.PrintError("无法加载AEAssist程序集");
                    return false;
                }
                LogHelper.Print("成功加载AEAssist程序集");

                // 2. 首先尝试查找SPxs6ghJadTHestWagv类（从VIP构造函数中看到的调用）
                LogHelper.Print("尝试查找SPxs6ghJadTHestWagv类");
                Type spxsType = null;
                var allTypesInAssembly = aeAssistAssembly.GetTypes();
                foreach (var type in allTypesInAssembly)
                {
                    if (type.Name == "SPxs6ghJadTHestWagv")
                    {
                        spxsType = type;
                        LogHelper.Print($"找到SPxs6ghJadTHestWagv类: {type.FullName}");
                        break;
                    }
                }

                if (spxsType != null)
                {
                    // 尝试调用其WmLo396ylZ方法
                    try
                    {
                        var wmLoMethod = spxsType.GetMethod("WmLo396ylZ", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (wmLoMethod != null)
                        {
                            LogHelper.Print("找到WmLo396ylZ方法，尝试调用");
                            wmLoMethod.Invoke(null, null);
                            LogHelper.Print("成功调用WmLo396ylZ方法");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.PrintError($"调用WmLo396ylZ方法失败: {ex.Message}");
                    }

                    // 检查SPxs6ghJadTHestWagv类是否包含VIP相关的字段或属性
                    var spxsFields = spxsType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var field in spxsFields)
                    {
                        if (field.FieldType == typeof(VIP))
                        {
                            LogHelper.Print($"在SPxs6ghJadTHestWagv类中找到VIP字段: {field.Name}");
                            var vipInstance = (VIP)field.GetValue(null);
                            if (vipInstance != null)
                            {
                                LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                field.SetValue(null, new VIP { Level = targetLevel, Key = vipInstance.Key });
                                LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                return true;
                            }
                        }
                    }
                }

                // 3. 搜索整个AEAssist程序集，查找所有包含VIP字段或属性的类
                LogHelper.Print("开始搜索整个AEAssist程序集");
                foreach (var type in allTypesInAssembly)
                {
                    // 跳过泛型类型和编译器生成的类型
                    if (type.IsGenericType || type.Name.Contains("<>"))
                    {
                        continue;
                    }

                    // 检查静态字段
                    var staticFields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var field in staticFields)
                    {
                        if (field.FieldType == typeof(VIP))
                        {
                            LogHelper.Print($"在{type.FullName}中找到静态VIP字段: {field.Name}");
                            try
                            {
                                var vipInstance = (VIP)field.GetValue(null);
                                if (vipInstance != null)
                                {
                                    LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                    // 修改Level属性
                                    field.SetValue(null, new VIP { Level = targetLevel, Key = vipInstance.Key });
                                    LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                    return true;
                                }
                                else
                                {
                                    LogHelper.Print("VIP实例为null，尝试创建新实例");
                                    var newVip = new VIP { Level = targetLevel, Key = string.Empty };
                                    field.SetValue(null, newVip);
                                    LogHelper.Print($"成功创建并设置VIP级别为: {targetLevel}");
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.PrintError($"访问VIP字段失败: {ex.Message}");
                            }
                        }
                    }

                    // 检查静态属性
                    var staticProperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var property in staticProperties)
                    {
                        if (property.PropertyType == typeof(VIP))
                        {
                            LogHelper.Print($"在{type.FullName}中找到静态VIP属性: {property.Name}");
                            try
                            {
                                var vipInstance = (VIP)property.GetValue(null);
                                if (vipInstance != null)
                                {
                                    LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                    // 修改Level属性
                                    property.SetValue(null, new VIP { Level = targetLevel, Key = vipInstance.Key });
                                    LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                    return true;
                                }
                                else
                                {
                                    LogHelper.Print("VIP实例为null，尝试创建新实例");
                                    var newVip = new VIP { Level = targetLevel, Key = string.Empty };
                                    property.SetValue(null, newVip);
                                    LogHelper.Print($"成功创建并设置VIP级别为: {targetLevel}");
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.PrintError($"访问VIP属性失败: {ex.Message}");
                            }
                        }
                    }

                    // 检查是否有返回VIP的静态方法
                    var staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var method in staticMethods)
                    {
                        if (method.ReturnType == typeof(VIP) && method.GetParameters().Length == 0)
                        {
                            LogHelper.Print($"在{type.FullName}中找到返回VIP的静态方法: {method.Name}");
                            try
                            {
                                var vipInstance = (VIP)method.Invoke(null, null);
                                if (vipInstance != null)
                                {
                                    LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                    // 直接修改VIP实例的Level属性
                                    var levelProperty = typeof(VIP).GetProperty("Level");
                                    if (levelProperty != null)
                                    {
                                        levelProperty.SetValue(vipInstance, targetLevel);
                                        LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                        return true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.PrintError($"调用方法失败: {ex.Message}");
                            }
                        }
                    }
                }

                // 4. 尝试查找所有可能的VIP管理类
                LogHelper.Print("尝试查找所有可能的VIP管理类");
                foreach (var type in allTypesInAssembly)
                {
                    // 跳过泛型类型和编译器生成的类型
                    if (type.IsGenericType || type.Name.Contains("<>"))
                    {
                        continue;
                    }

                    // 检查类名是否包含VIP或Verify相关的关键词
                    if (type.Name.Contains("VIP", StringComparison.OrdinalIgnoreCase) ||
                        type.Name.Contains("Verify", StringComparison.OrdinalIgnoreCase) ||
                        type.Name.Contains("Auth", StringComparison.OrdinalIgnoreCase) ||
                        type.Name.Contains("License", StringComparison.OrdinalIgnoreCase))
                    {
                        LogHelper.Print($"检查VIP管理类: {type.FullName}");

                        // 尝试获取实例
                        try
                        {
                            // 检查是否有默认构造函数
                            var constructor = type.GetConstructor(Type.EmptyTypes);
                            if (constructor != null)
                            {
                                LogHelper.Print($"创建{type.FullName}的实例");
                                var instance = Activator.CreateInstance(type);
                                if (instance != null)
                                {
                                    // 检查实例的字段
                                    var instanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    foreach (var field in instanceFields)
                                    {
                                        if (field.FieldType == typeof(VIP))
                                        {
                                            LogHelper.Print($"在{type.FullName}实例中找到VIP字段: {field.Name}");
                                            var vipInstance = (VIP)field.GetValue(instance);
                                            if (vipInstance != null)
                                            {
                                                LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                                field.SetValue(instance, new VIP { Level = targetLevel, Key = vipInstance.Key });
                                                LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                                return true;
                                            }
                                        }
                                    }

                                    // 检查实例的属性
                                    var instanceProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    foreach (var property in instanceProperties)
                                    {
                                        if (property.PropertyType == typeof(VIP))
                                        {
                                            LogHelper.Print($"在{type.FullName}实例中找到VIP属性: {property.Name}");
                                            var vipInstance = (VIP)property.GetValue(instance);
                                            if (vipInstance != null)
                                            {
                                                LogHelper.Print($"当前VIP级别: {vipInstance.Level}");
                                                property.SetValue(instance, new VIP { Level = targetLevel, Key = vipInstance.Key });
                                                LogHelper.Print($"成功修改VIP级别为: {targetLevel}");
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.PrintError($"创建实例失败: {ex.Message}");
                        }
                    }
                }

                // 5. 尝试直接修改VIP类的默认构造函数行为
                LogHelper.Print("尝试修改VIP类的构造函数行为");
                try
                {
                    // 获取VIP类的构造函数
                    var vipConstructor = typeof(VIP).GetConstructor(Type.EmptyTypes);
                    if (vipConstructor != null)
                    {
                        // 创建一个新的VIP实例
                        var newVip = new VIP { Level = targetLevel, Key = string.Empty };
                        LogHelper.Print($"创建新的VIP实例，级别: {newVip.Level}");

                        // 尝试查找所有引用VIP实例的地方
                        foreach (var type in allTypesInAssembly)
                        {
                            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            foreach (var field in fields)
                            {
                                if (field.FieldType == typeof(VIP))
                                {
                                    LogHelper.Print($"尝试将新VIP实例设置到: {type.FullName}.{field.Name}");
                                    field.SetValue(null, newVip);
                                    LogHelper.Print($"成功设置VIP实例");
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.PrintError($"修改VIP构造函数行为失败: {ex.Message}");
                }

                LogHelper.Print("未找到可修改的VIP实例");
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.PrintError($"修改VIP级别失败: {ex.Message}");
                return false;
            }
        }
    }
}