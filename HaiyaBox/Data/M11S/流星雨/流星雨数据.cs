using HaiyaBox.Utils;

namespace HaiyaBox.Data.M11S.流星雨;

public class 流星雨数据 :IMechanismState
{
    public static 流星雨数据 Instance { get; } = new();
    public Dictionary<uint, uint> 黑洞连线  = new();
}