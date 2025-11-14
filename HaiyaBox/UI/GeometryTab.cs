using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine.Trigger;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;
using AEAssist.CombatRoutine.Module.Target;
using ECommons.DalamudServices;
using HaiyaBox.Settings;
using HaiyaBox.Utils;

namespace HaiyaBox.UI
{
    /// <summary>
    /// GeometryTab 用于处理几何计算及相关UI交互，记录鼠标点击点、计算距离与角度，并提供调试点添加及清理功能。
    /// </summary>
    public class GeometryTab
    {
        /// <summary>
        /// 获取全局的 GeometrySettings 配置单例，存储场地中心、朝向点及计算参数等配置。
        /// </summary>
        public GeometrySettings Settings => FullAutoSettings.Instance.GeometrySettings;

        /// <summary>
        /// 卫月字体大小。
        /// </summary>
        public static float scale => ImGui.GetFontSize() / 13.0f;

        /// <summary>
        /// 记录运行时的点1（一般通过按Ctrl记录）。
        /// </summary>
        public Vector3? Point1World { get; private set; }
        /// <summary>
        /// 记录运行时的点2（一般通过按Shift记录）。
        /// </summary>
        public Vector3? Point2World { get; private set; }

        /// <summary>
        /// 点1与点2在XZ平面的距离，实时计算，不保存到配置文件中。
        /// </summary>
        public float TwoPointDistanceXZ { get; private set; }
        /// <summary>
        /// 用于显示弦长、角度和半径计算后的结果描述与数值。
        /// </summary>
        public string ChordResultLabel { get; private set; } = "";
        private int _distributionMode = 0; // 0: 全圆均匀分布, 1: 直线间距分布, 2: 总计角度分布
        private float _distributionRadius = 19f;
        private float _distributionFirstOffset = 0f;
        private int _distributionCount = 8;
        private bool _distributionClockwise = true;
        private float _distributionSpacing = 3;      // 直线间距模式所用
        private float _fixedAngle = 45f; // 固定角度默认值
        private float _distributionTotalAngle = 90f;     // 总计角度模式所用
        private List<Vector3> _distributionPositions = new List<Vector3>();
        private bool _addDistributionToDebugPoints = true;
        private bool _copyCoordinatesWithF = false;

        // 固定数据：场地中心标签与对应的实际坐标值
        private readonly string[] _centerLabels = ["旧(0,0,0)", "新(100,0,100)"];
        private readonly Vector3[] _centerPositions =
        [
            new(0, 0, 0),
            new(100, 0, 100)
        ];

        // 固定数据：朝向点标签与对应的实际坐标值
        private readonly string[] _directionLabels = ["东(101,0,100)", "西(99,0,100)", "南(100,0,101)", "北(100,0,99)"];
        private readonly Vector3[] _directionPositions =
        [
            new(101, 0, 100),
            new(99, 0, 100),
            new(100, 0, 101),
            new(100, 0, 99)
        ];
        private int spellListIndex = 0;

        /// <summary>
        /// 在每一帧调用，主要用于更新鼠标点击记录（点1、点2、点3）。
        /// </summary>
        public void Update() =>
            // 每帧检查是否按下Ctrl/Shift/Alt键，记录对应的点信息
            CheckPointRecording();
        /// <summary>
        /// 事件记录管理器
        /// </summary>
        private readonly EventRecordManager _recordManager = EventRecordManager.Instance;
        private readonly HashSet<ITriggerCondParams> _triggerCondParamsList = new();
        private readonly List<Vector3> _pointList = new();
        private int 算法选择 = 0;
        private List<string> 旋转参考点 = new();
        private List<float> 旋转角度 = new();
        private List<string> 旋转中心 = new();
        private List<Vector3> 旋转计算结果 = new();
        private List<string> 延伸点 = new();
        private List<string> 延伸方向 = new();
        private List<float> 延伸距离 = new();
        private List<Vector3> 延伸计算结果 = new();

        private Vector3 StringToVector3(string str)
        {
            var v = str.Split(",");
            return new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
        }

        /// <summary>
        /// 绘制与更新 GeometryTab 的各项UI组件，展示实时鼠标位置、Debug点操作、距离、角度计算等信息。
        /// </summary>
        public void Draw()
        {
            // 绘制提示信息，说明如何使用键盘记录点以及如何选择夹角顶点模式
            ImGui.TextColored(new Vector4(1f, 0.85f, 0.4f, 1f),
                "提示: Ctrl 记录点1, Shift 记录点2");
            ImGui.Separator();
            ImGui.Spacing();

            // 显示鼠标当前在屏幕及转换后的世界坐标
            var mousePos = ImGui.GetMousePos();
            if (ScreenToWorld(mousePos, out var wPos3D))
            {
                ImGui.Text($"鼠标屏幕: <{mousePos.X:F2}, {mousePos.Y:F2}>\n鼠标世界: <{wPos3D.X:F2}, {wPos3D.Z:F2}>");
                // 计算鼠标与当前选定场地中心的距离，以及参考方向与鼠标之间的角度
                float distMouseCenter = GeometryUtilsXZ.DistanceXZ(wPos3D, _centerPositions[Settings.SelectedCenterIndex]);
                float angleMouseCenter = GeometryUtilsXZ.AngleXZ(_directionPositions[Settings.SelectedDirectionIndex], wPos3D, _centerPositions[Settings.SelectedCenterIndex]);
                ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f),
                    $"鼠标 -> 场地中心: 距离 {distMouseCenter:F2}, 角度 {angleMouseCenter:F2}°");
            }
            else
            {
                ImGui.Text("鼠标不在游戏窗口内");
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Debug点操作：提供添加或清理调试点的功能
            // 读取当前是否启用Debug点
            bool addDebug = Settings.AddDebugPoints;
            if (ImGui.Checkbox("添加Debug点", ref addDebug))
            {
                Settings.UpdateAddDebugPoints(addDebug);
            }
            ImGui.SameLine();
            if (ImGui.Button("清理Debug点"))
            {
                ClearDebugPoints();
            }

            ImGui.Spacing();
            // 显示记录的三个点坐标
            ImGui.Text($"点1: {FormatPointXZ(Point1World)}");
            ImGui.SameLine();
            if (ImGui.Button("复制##"))
            {
                if (Point1World != null)
                    ImGui.SetClipboardText(
                        $"({Point1World.Value.X:F2}f, {Point1World.Value.Y:F2}f, {Point1World.Value.Z:F2}f)");
            }
            ImGui.Text($"点2: {FormatPointXZ(Point2World)}");
            ImGui.SameLine();
            if (ImGui.Button("复制##"))
            {
                if (Point2World != null)
                    ImGui.SetClipboardText(
                        $"({Point2World.Value.X:F2}f, {Point2World.Value.Y:F2}f, {Point2World.Value.Z:F2}f)");
            }

            // 当记录了点1和点2后，计算并显示两点间的XZ平面距离，同时允许选择夹角顶点模式进行角度计算
            if (Point1World.HasValue && Point2World.HasValue)
            {
                ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f),
                    $"点1 -> 点2: 距离 {TwoPointDistanceXZ:F2}");
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("根据记录的读条事件计算：");

            ImGui.Text("当前可选中敌人");
            var enemyList = TargetMgr.Instance.Enemys.Values.ToList();
            if (enemyList.Count > 0)
            {
                foreach (var enemy in enemyList)
                {
                    ImGui.Text($"Name：{enemy.Name} ({enemy.DataId})");
                    ImGui.SameLine();
                    var pos = $"{enemy.Position.X:F2},{enemy.Position.Y:F2},{enemy.Position.Z:F2}";
                    ImGui.Text("位置：" + pos);
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText(pos);
                    }
                    ImGui.SameLine();
                    var rot = $"{enemy.Rotation * 180 / float.Pi:F2}";
                    ImGui.Text($"方向：{rot}");
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText(rot);
                    }
                }
            }
            else
            {
                ImGui.Text("当前没有可选中敌人");
            }
            ImGui.Spacing();



            ImGui.Text("选择读条事件：");
            var spellList = _recordManager.GetRecords("EnemyCastSpell").Select((p =>
            {
                if (p is EnemyCastSpellCondParams spellCondParams)
                    return $"{spellCondParams.SpellName}:{spellCondParams.SpellId}";
                return "";
            })).ToList();
            // 添加Combo选择框
            if (spellList.Count > 0)
            {
                if (spellListIndex >= spellList.Count)
                    spellListIndex = 0;

                string currentSelection = spellList[spellListIndex] + $"#{spellListIndex + 1}";
                if (ImGui.BeginCombo("##SpellList", currentSelection))
                {
                    for (int i = 0; i < spellList.Count; i++)
                    {
                        bool isSelected = (spellListIndex == i);
                        if (ImGui.Selectable(spellList[i] + $"#{i + 1}", isSelected))
                        {
                            spellListIndex = i;
                            _triggerCondParamsList.Add(_recordManager.GetRecords("EnemyCastSpell")[i]);
                        }
                    }
                    ImGui.EndCombo();
                }

            }
            else
            {
                ImGui.TextColored(new Vector4(1f, 0.5f, 0.5f, 1f), "暂无读条事件记录");
            }

            ImGui.Spacing();
            // 绘制选择的读条事件记录，每条记录的坐标和方向可以右键复制
            for (int i = 0; i < _triggerCondParamsList.Count; i++)
            {
                var record = _triggerCondParamsList.ToList()[i];
                if (record is EnemyCastSpellCondParams spellCondParams)
                {
                    ImGui.Text($"Name:{spellCondParams.SpellName} {spellCondParams.SpellId}");
                    ImGui.SameLine();
                    var castPos = FormatPosition(spellCondParams.CastPos);
                    ImGui.Text($"Pos:{castPos}");
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText($"{castPos.X},{castPos.Y},{castPos.Z}");
                    }
                    ImGui.SameLine();
                    var castRot = (float)Math.Round(spellCondParams.CastRot * 180 / float.Pi, 2);
                    ImGui.Text($"Rot:{castRot}");
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.SetClipboardText($"{castRot}");
                    }
                    ImGui.SameLine();
                    ImGui.Text($"可选中:{spellCondParams.Object.IsTargetable}");
                    ImGui.SameLine();
                    // 2. 给按钮添加唯一 ID（## 后面的内容是 ID 后缀，确保每行唯一）
                    if (ImGui.Button($"删除##{i}")) // 关键修改：用索引 i 作为唯一标识
                    {
                        try
                        {
                            // 3. 用索引删除（安全，避免迭代器失效）
                            _triggerCondParamsList.Remove(record);
                            // 重要：删除后索引回退，避免跳过下一个元素
                            i--;
                        }
                        catch (Exception e)
                        {
                            LogHelper.Print("删除失败");
                            LogHelper.Print(e.Message);
                        }
                    }
                }
            }
            ImGui.Spacing();
            string 算法 = 算法选择 switch
            {
                0 => "旋转",
                1 => "延伸",
            };
            ImGui.Text("选择计算方式：");
            if (ImGui.BeginCombo("算法选择##", 算法))
            {
                if (ImGui.Selectable("旋转", 算法选择 == 0))
                    算法选择 = 0;
                if (ImGui.Selectable("延伸", 算法选择 == 1))
                    算法选择 = 1;
                ImGui.EndCombo();
            }
            if (ImGui.Button("添加计算"))
            {
                if (算法选择 == 0)
                {
                    旋转中心.Add(string.Empty);
                    旋转角度.Add(0);
                    旋转参考点.Add(string.Empty);
                    旋转计算结果.Add(Vector3.Zero);
                }
                if (算法选择 == 1)
                {
                    延伸方向.Add(string.Empty);
                    延伸距离.Add(0);
                    延伸点.Add(string.Empty);
                    延伸计算结果.Add(Vector3.Zero);
                }
            }
            if (旋转计算结果.Count > 0)
            {
                for (int i = 0; i < 旋转计算结果.Count; i++)
                {
                    var buf = 旋转参考点[i];
                    if (ImGui.InputText($"旋转参考点##{i}", ref buf, 256))
                        旋转参考点[i] = buf;
                    var buf2 = 旋转中心[i];
                    if (ImGui.InputText($"旋转中心##{i}", ref buf2))
                        旋转中心[i] = buf2;
                    var buf3 = 旋转角度[i];
                    if (ImGui.InputFloat($"旋转角度##{i}", ref buf3))
                        旋转角度[i] = buf3;
                    if (ImGui.Button($"计算结果##旋转{i}"))
                    {
                        var result = GeometryUtilsXZ.RotateAroundPoint(StringToVector3(旋转参考点[i]), StringToVector3(旋转中心[i]), 旋转角度[i]);
                        延伸计算结果[i] = result;
                        ImGui.Text($"计算结果：{FormatPosition(result)}");
                        Share.TrustDebugPoint.Add(result);
                    }
                    if (ImGui.Button($"删除##计算{i}"))
                    {
                        旋转计算结果.RemoveAt(i);
                        i--;
                    }

                }
            }

            if (延伸计算结果.Count > 0)
            {
                for (int i = 0; i < 延伸计算结果.Count; i++)
                {
                    var buf4 = 延伸点[i];
                    if (ImGui.InputText($"延伸点##{i}", ref buf4))
                        延伸点[i] = buf4;
                    var buf5 = 延伸距离[i];
                    if (ImGui.InputFloat($"延伸距离##{i}", ref buf5))
                        延伸距离[i] = buf5;
                    var buf6 = 延伸方向[i];
                    if (ImGui.InputText($"延伸方向##{i}", ref buf6))
                        延伸方向[i] = buf6;
                    if (ImGui.Button($"计算结果##延伸{i}"))
                    {
                        var result = GeometryUtilsXZ.ExtendPoint( StringToVector3(延伸方向[i]), StringToVector3(延伸点[i]),延伸距离[i]);
                        延伸计算结果[i] = result;
                        ImGui.Text($"计算结果：{FormatPosition(result)}");
                        Share.TrustDebugPoint.Add(result);
                    }
                    if (ImGui.Button($"删除##计算{i}"))
                    {
                        延伸计算结果.RemoveAt(i);
                        i--;
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 1f, 1f), "扇形分散");
            string comboLabel = _distributionMode switch
            {
                0 => "全圆均匀分布",
                1 => "直线间距分布",
                2 => "固定角度分布",
                3 => "总计角度分布",
                _ => "未知模式"
            };
            if (ImGui.BeginCombo("##DistributionMode", comboLabel))
            {
                if (ImGui.Selectable("全圆均匀分布", _distributionMode == 0))
                    _distributionMode = 0;
                if (ImGui.Selectable("直线间距分布", _distributionMode == 1))
                    _distributionMode = 1;
                if (ImGui.Selectable("固定角度分布", _distributionMode == 2))
                    _distributionMode = 2;
                if (ImGui.Selectable("总计角度分布", _distributionMode == 3))
                    _distributionMode = 3;
                ImGui.EndCombo();
            }

            ImGui.InputFloat("半径", ref _distributionRadius, 1f, 5f, "%.2f");
            ImGui.InputFloat("第一人偏移角度", ref _distributionFirstOffset, 1f, 5f, "%.2f");
            ImGui.InputInt("人数", ref _distributionCount);
            ImGui.Checkbox("顺时针", ref _distributionClockwise);
            if (_distributionMode == 1)
            {
                ImGui.InputFloat("直线间距", ref _distributionSpacing, 1f, 5f, "%.2f");
            }
            if (_distributionMode == 2)
            {
                ImGui.InputFloat("固定角度", ref _fixedAngle, 1f, 5f, "%.2f");
            }
            if (_distributionMode == 3)
            {
                ImGui.InputFloat("总计角度", ref _distributionTotalAngle, 1f, 5f, "%.2f");
            }

            if (ImGui.Button("计算分布"))
            {
                var center = _centerPositions[Settings.SelectedCenterIndex];
                if (_distributionMode == 0)
                {
                    _distributionPositions = GeometryUtilsXZ.ComputeFullCirclePositions(center, _distributionRadius, _distributionFirstOffset, _distributionCount, _distributionClockwise);
                }
                else if (_distributionMode == 1)
                {
                    _distributionPositions = GeometryUtilsXZ.ComputeArcPositionsByChordSpacing(center, _distributionRadius, _distributionFirstOffset, _distributionCount, _distributionClockwise, _distributionSpacing);
                }
                else if (_distributionMode == 2)
                {
                    _distributionPositions = GeometryUtilsXZ.ComputePositionsByFixedAngle(center, _distributionRadius, _distributionFirstOffset, _distributionCount, _distributionClockwise, _fixedAngle);
                }
                else if (_distributionMode == 3)
                {
                    _distributionPositions = GeometryUtilsXZ.ComputeArcPositionsByTotalAngle(center, _distributionRadius, _distributionFirstOffset, _distributionCount, _distributionClockwise, _distributionTotalAngle);
                }
                // 如果选择添加至 Debug 点，则遍历计算结果并调用 AddDebugPoint
                if (_addDistributionToDebugPoints)
                {
                    foreach (var pos in _distributionPositions)
                    {
                        AddDebugPoint(pos);
                    }
                }
            }
            ImGui.SameLine();
            ImGui.Checkbox("添加计算结果到Debug点", ref _addDistributionToDebugPoints);


            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1f, 1f, 0.6f, 1f), "计算结果：");
            ImGui.SameLine();
            ImGui.Checkbox("复制时附加f和括号", ref _copyCoordinatesWithF);
            for (int i = 0; i < _distributionPositions.Count; i++)
            {
                var pos = _distributionPositions[i];
                string line = _copyCoordinatesWithF
                      ? $"({pos.X:F2}f, {pos.Y:F2}f, {pos.Z:F2}f)"
                      : $"{pos.X:F2}, {pos.Y:F2}, {pos.Z:F2}";
                ImGui.Text(line);
                ImGui.SameLine();
                if (ImGui.Button("复制##" + i))
                {
                    ImGui.SetClipboardText(line);
                }
            }
        }



        /// <summary>
        /// 通过监听键盘按键（Ctrl、Shift、Alt）记录鼠标在世界坐标中的位置，
        /// 同时在记录 Debug 点时更新点1/点2/点3的值，并计算点1与点2之间的距离。
        /// </summary>
        public void CheckPointRecording()
        {
            // 检查按键状态：Ctrl、Shift、Alt分别对应记录点1、点2、点3
            bool ctrl = ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) || ImGui.IsKeyPressed(ImGuiKey.RightCtrl);
            bool shift = ImGui.IsKeyPressed(ImGuiKey.LeftShift) || ImGui.IsKeyPressed(ImGuiKey.RightShift);

            // 获取当前鼠标屏幕坐标，并尝试转换到3D世界坐标
            var mousePos = ImGui.GetMousePos();
            if (ScreenToWorld(mousePos, out var wPos3D))
            {
                // 仅保留XZ分量，Y置0，适应2D平面计算
                var pointXZ = new Vector3(wPos3D.X, 0, wPos3D.Z);
                if (ctrl)
                    Point1World = pointXZ;
                else if (shift)
                    Point2World = pointXZ;

                // 如果启用了Debug点模式，则将记录的点添加到Debug点集合中
                if (Settings.AddDebugPoints && (ctrl || shift))
                    AddDebugPoint(pointXZ);
            }

            // 当记录了点1和点2后，计算并更新这两点在XZ平面的距离
            if (Point1World.HasValue && Point2World.HasValue)
            {
                TwoPointDistanceXZ = GeometryUtilsXZ.DistanceXZ(Point1World.Value, Point2World.Value);
            }
        }
        private Vector3 FormatPosition(Vector3 position)
        {
            // 对每个分量四舍五入保留2位小数，转换为 float 类型（匹配 Vector3 分量类型）
            float x = (float)Math.Round(position.X, 2);
            float y = (float)Math.Round(position.Y, 2);
            float z = (float)Math.Round(position.Z, 2);

            return new Vector3(x, y, z);
        }
        /// <summary>
        /// 格式化输出点的XZ坐标（如果点存在），否则返回"未记录"提示。
        /// </summary>
        /// <param name="p">需要格式化的点坐标</param>
        /// <returns>格式化后的字符串</returns>
        private string FormatPointXZ(Vector3? p) =>
            p.HasValue ? $"<{p.Value.X:F2}, 0, {p.Value.Z:F2}>" : "未记录";

        /// <summary>
        /// 将屏幕坐标转换为3D世界坐标。
        /// </summary>
        /// <param name="screenPos">当前鼠标在屏幕上的位置</param>
        /// <param name="worldPos">转换后的3D世界坐标</param>
        /// <returns>转换是否成功（当前实现始终返回true）</returns>
        private bool ScreenToWorld(Vector2 screenPos, out Vector3 worldPos)
        {
            Svc.GameGui.ScreenToWorld(screenPos, out worldPos);
            return true;
        }

        /// <summary>
        /// 添加一个调试点，用于在调试模式下显示具体点击位置的信息。
        /// </summary>
        /// <param name="point">在世界坐标中的调试点</param>
        private void AddDebugPoint(Vector3 point)
        {
            LogHelper.Print($"添加Debug点: {point}");
            Share.TrustDebugPoint.Add(point);
        }

        /// <summary>
        /// 清理所有已经记录的调试点。
        /// </summary>
        private void ClearDebugPoints()
        {
            LogHelper.Print("清理Debug点");
            Share.TrustDebugPoint.Clear();
        }
    }
}
