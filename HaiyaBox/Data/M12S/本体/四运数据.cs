using System.Numerics;
using HaiyaBox.Utils;

namespace XXLZFA.HaiyaBox.HaiyaBox.Data.M12S.本体;

public class 四运数据 :IMechanismState
{
    public static 四运数据 Instance { get; } = new();

    public HashSet<uint> 已记录分身 = new();
    public List<Vector3> 大圈分身 = new();
    public List<Vector3> 分摊分身 = new();
    public List<string> 左组光 = new();
    public List<string> 右组光 = new();
    public int 初始第一轮分身方位;
    public int 南北安全;
    public int 南北钢铁;
    public int 玩家分身count;
    public Dictionary<int,string> 连线玩家 = new();
    public Dictionary<string, Vector3> 接线起点 = new();
    public Dictionary<string, Vector3> 接线终点 = new();
    public List<Vector3> 风塔 = new();
    public List<Vector3> 暗塔 = new();
    public List<Vector3> 土塔 = new();
    public List<Vector3> 火塔 = new();
}