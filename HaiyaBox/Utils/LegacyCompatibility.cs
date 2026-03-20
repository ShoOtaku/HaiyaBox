using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.Module;
using AEAssist.Extension;
using AEAssist.Helper;
using Dalamud.Game.ClientState.Objects.Types;

namespace HaiyaBox.Utils;

public static class IBattleCharaExtensions
{
    private static List<Jobs> Dps =
    [
        Jobs.Bard, Jobs.BlackMage, Jobs.Archer, Jobs.Dancer, Jobs.Dragoon, Jobs.Lancer, Jobs.Machinist, Jobs.Ninja,
        Jobs.Pictomancer, Jobs.Monk, Jobs.Reaper, Jobs.RedMage, Jobs.Summoner, Jobs.Viper, Jobs.Samurai
    ];
    public static bool IsDps(this IGameObject battleChara)
    {
        var job = ((IBattleChara)battleChara).CurrentJob();
        return Dps.Contains(job);
    }
}
public static class 坐标计算
{
    public static int PositionTo8Dir(Vector3 point, Vector3 centre)
    {
        return GeometryUtilsXZ.PositionTo8Dir(point, centre);
    }
    
    public static Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
    {
        return GeometryUtilsXZ.RotatePoint(point, centre, radian);
    }
    
    public static Vector3 CalculatePointOnLine(Vector3 start, Vector3 end, float distance)
    {
        return GeometryUtilsXZ.CalculatePointOnLine(start, end, distance);
    }
}

public static class 位移
{
    public static void Tp(Vector3 pos)
    {
        RemoteControl.SetPos(AI.Instance.PartyRole, pos);
    }
}


public static class Vector3Extensions
{
    public static void SharePoint(this Vector3 pos)
    {
        DebugPoint.Add(pos);
    }
}

public static class LegacyRoleExtensions
{
    private static readonly string[] RoleOrder = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];

    public static string GetRoleByPlayerObjct(this IGameObject? gameObject)
    {
        if (gameObject == null)
            return string.Empty;

        return RemoteControl.GetRoleByPlayerName(gameObject.Name.TextValue) ?? string.Empty;
    }

    public static int GetRoleByPlayerObjctIndex(this IGameObject? gameObject)
    {
        var role = gameObject.GetRoleByPlayerObjct();
        var index = Array.IndexOf(RoleOrder, role);
        if (index >= 0)
            return index;

        var selfIndex = Array.IndexOf(RoleOrder, AI.Instance.PartyRole);
        return selfIndex >= 0 ? selfIndex : 0;
    }
}
