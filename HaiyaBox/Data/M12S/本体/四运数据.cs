using System.Numerics;
using AEAssist.CombatRoutine.Trigger;
using Dalamud.Game.ClientState.Objects.Types;
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
    public Dictionary<string, int> 最终接线方位 = new();
    public List<string> 第一轮大圈 = new();
    public List<string> 第一轮分摊 = new();
    public List<string> 第二轮大圈 = new();
    public List<string> 第二轮分摊 = new();
    public int 分摊先后;
    public List<Vector3> 风塔 = new();
    public List<Vector3> 暗塔 = new();
    public List<Vector3> 土塔 = new();
    public List<Vector3> 火塔 = new();
    public List<四运玩家> 玩家分身 = new List<四运玩家>();
    public List<四运Boss分身> Boss分身 = new List<四运Boss分身>();
}

public class 四运玩家
{
    public int 方位;
    public 类型 玩家分身类型 = new();
    public Vector3 机制处理位置 = new Vector3(100, 0, 100);
    public int 轮次 = 0;
    public string 玩家Name;
    public uint 玩家Id;

    public void Update(TetherCondParams tetherCondParams)
    {
        var left = tetherCondParams.Left;
        方位 = GeometryUtilsXZ.PositionTo8Dir(left.Position, new Vector3(100, 0, 100));
        玩家Name = tetherCondParams.Right.Name.TextValue;
        玩家Id = tetherCondParams.Left.EntityId;
        玩家分身类型 = 方位 switch
        {
            1 or 3 or 4 or 6 => 类型.大圈,
            0 or 2 or 5 or 7 => 类型.分摊,
            _ => 类型.无
        };
        if (四运数据.Instance.初始第一轮分身方位 % 2 == 0)
        {
            轮次 = 方位 switch
            {
                0 or 2 or 4 or 6 => 1,
                1 or 3 or 5 or 7 => 2,
                _ => 0
            };
        }
        else
        {
            轮次 = 方位 switch
            {
                0 or 2 or 4 or 6 => 2,
                1 or 3 or 5 or 7 => 1,
                _ => 0
            };
        }
    }
}
public class 四运Boss分身
{
    public uint 分身Id;
    public uint 连线玩家Id;
    public int 方位;
    public 类型 分身类型 = new();
    public Vector3 分身位置 = new Vector3(100, 0, 100);
    public int 释放轮次;
    public bool 已正确接线 = false;

    public void Update(TetherCondParams tetherCondParams )
    {
        var left = tetherCondParams.Left;
        分身Id = left.EntityId;
        分身位置 = left.Position;
        方位 = GeometryUtilsXZ.PositionTo8Dir(left.Position, new Vector3(100, 0, 100));
        var right = tetherCondParams.Right;
        连线玩家Id = right.EntityId;
        分身类型 = tetherCondParams.Args0 switch
        {
            368 => 类型.大圈,
            369 => 类型.分摊,
            _ => 类型.无
        };
        释放轮次 = 方位 switch
        {
            0 or 4 => 1,
            1 or 5 => 2,
            2 or 6 => 3,
            3 or 7 => 4,
            _ => 0
        };
    }
}

public enum 类型
{
    无,
    分摊,
    大圈,
}