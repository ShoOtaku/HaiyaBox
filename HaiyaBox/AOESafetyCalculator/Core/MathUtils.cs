using System.Runtime.CompilerServices;

namespace AOESafetyCalculator.Core;

/// <summary>
/// 数学扩展工具类，提供常用的数学计算方法
/// </summary>
[SkipLocalsInit]
public static class MathUtils
{
    /// <summary>
    /// 判断一个无符号整数是否为质数（素数）
    /// </summary>
    /// <remarks>
    /// 质数定义：大于1的自然数，除了1和它本身外，不能被其他自然数整除
    ///
    /// 算法说明：
    /// 1. 小于等于1的数不是质数
    /// 2. 2是最小的质数
    /// 3. 对于大于2的数，检查从2到√n的所有整数是否能整除该数
    ///
    /// 时间复杂度: O(√n)
    /// </remarks>
    public static bool IsPrime(uint number)
    {
        if (number <= 1u)
            return false;
        if (number == 2u)
            return true;
        for (var i = 2u; i <= Math.Sqrt(number); ++i)
        {
            if (number % i == default)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 判断一个无符号整数是否能被另一个无符号整数整除
    /// </summary>
    /// <remarks>
    /// 整除定义：如果 a ÷ b 的余数为0，则称 a 能被 b 整除
    /// </remarks>
    public static bool IsDivisible(uint dividend, uint divisor)
    {
        return dividend % divisor == default;
    }
}
