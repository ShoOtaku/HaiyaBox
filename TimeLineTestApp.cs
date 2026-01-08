using System;
using System.Threading.Tasks;
using HaiyaBox.TimeLine;

namespace TimeLineTestApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 时间轴系统测试应用 ===");
        Console.WriteLine();

        // 运行所有测试
        await TimeLineTest.RunAllTests();
        
        Console.WriteLine();
        Console.WriteLine("=== 运行上尾人龙示例 ===");
        
        // 运行上尾人龙示例
        await ShangWeiRenLong.RunExample();
        
        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
