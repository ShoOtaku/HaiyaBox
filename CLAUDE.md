# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HaiyaBox is a C# plugin for the AEAssist framework in Final Fantasy XIV. It provides automation tools, raid utilities, and geometry-based positioning systems for the game. The project targets .NET 9.0 Windows and uses the Dalamud plugin framework.

## Build Commands

### Building the Plugin
```bash
# Build in Debug mode
dotnet build HaiyaBox.sln -c Debug

# Build in Release mode
dotnet build HaiyaBox.sln -c Release

# Clean and rebuild
dotnet clean HaiyaBox.sln
dotnet build HaiyaBox.sln
```

### Output Location
Both Debug and Release configurations output to:
`F:\FF14act\AEAssist 国服 1024\Plugins\HaiyaBox`

### Environment Variables
- `AEPath`: If set, points to the AEAssist installation directory. Otherwise defaults to `..\..\..\AE\AEAssistCNVersion\AEAssist\`

### Compiler Configuration
- Allows unsafe code blocks for direct memory access
- Suppresses multiple warnings (CS1998, CS8601-8605, CS8618, CS8620, etc.)
- Warning CS4014 is treated as an error (fire-and-forget async calls)

## Architecture

### Core Structure
- **Plugin/**: Main plugin entry point and initialization
  - `AutoRaidHelper.cs`: Main plugin class implementing IAEPlugin, manages all UI tabs and services
  - `TreasureOpenerService.cs`: Automated treasure chest opener using signature scanning and packet injection
  - `XSZToolboxIpc.cs`: IPC client for XSZToolbox remote control integration
- **UI/**: User interface components for different functionality tabs
  - `AutomationTab.cs`: Main automation interface (countdown, leave, queue, Island Sanctuary)
  - `GeometryTab.cs`: Positioning and geometry utilities
  - `FaGeneralSettingTab.cs`: General settings (debug output, actor control logging)
  - `EventRecordTab.cs`: Event recording and display
  - `BlackListTab.cs`: Blacklist management
  - `DangerAreaTab.cs`: Danger area visualization and safe point calculation
  - `DutyBattleTab.cs`: Battle-specific duty automation features
- **Settings/**: Configuration management
  - `FullAutoSettings.cs`: Global settings singleton with JSON persistence (per-character storage)
  - `BattleData.cs`: Temporary battle data singleton for danger area calculations
- **Triggers/**: AEAssist trigger system integration
  - `TriggerAction/`: Custom trigger actions (positioning, BMR launching)
  - `TriggerCondition/`: Custom trigger conditions (position detection)
- **Hooks/**: Game memory and function hooks
  - `ActorControlHook.cs`: Actor control packet interception (currently disabled)
- **Rendering/**: Overlay rendering system
  - `DangerAreaRenderer.cs`: ImGui-based overlay renderer for circles, lines, dots, and text
  - `DangerAreaDisplay.cs`: Display object management and conversion
- **Utils/**: Utility functions
  - `GeometryUtilsXZ.cs`: XZ plane geometry calculations
  - `Utilities.cs`: General utility functions
  - `DangerArea.cs`: Safe point calculation algorithm with circle/rectangle danger zones
  - `EventRecordManager.cs`: Event recording manager (max 15 records per event type)
- **TimeLine/**: Timeline system - behavior tree-based combat automation (in development)

### Key Dependencies
- **AEAssist**: Main combat automation framework (referenced from `$(AELibPath)`)
- **ECommons**: Dalamud convenience library (referenced from `$(AELibPath)`)
- **AEAssist.NET**: NuGet package for types (ExcludeAssets: runtime)
- **Dalamud**: FFXIV plugin framework (via AEAssist.NET)
- **FFXIVClientStructs**: Direct game memory access (via AEAssist.NET)
- **GitInfo**: Assembly version information

### Configuration System
- Settings stored per-character in JSON format
- Path: `Share.CurrentDirectory\..\..\Settings\HaiyaBox\FullAutoSettings\{LocalContentId}.json`
- Thread-safe singleton pattern with read-only fallback (when file is locked)
- Configuration includes:
  - GeometrySettings: Field center, direction points, chord/angle/radius calculations
  - AutomationSettings: Countdown, leave, queue, duty completion tracking, Island Sanctuary automation
  - FaGeneralSetting: Debug info and actor control logging flags
  - DebugPrintSettings: Event-specific debug output toggles
  - RecordSettings: Event recording toggles

### Trigger System
The plugin integrates with AEAssist's trigger system to provide:
- Role-based positioning (MT, ST, H1, H2, D1, D2, D3, D4)
- Position detection and validation
- BMR (battle maneuver system) automation
- Registered under category "嗨呀AE工具"

### Event Recording System
- Tracks up to 15 records per event type with timestamps
- Supported event types: EnemyCastSpell, Tether, TargetIconEffect, UnitCreate
- Thread-safe concurrent queue implementation
- Provides both raw and timestamped record access

### Danger Area & Safe Point System
- Supports circle and rectangle danger zones
- Safe point calculation using grid sampling with configurable parameters:
  - `minSafePointDistance`: Minimum distance between safe points
  - `closeToRefCount`: Number of points to prioritize near reference point
  - `maxFarDistance`: Maximum distance for distributed points
  - `sampleStep`: Grid sampling resolution
- Supports both rectangular and circular boundary constraints
- Real-time overlay rendering via ImGui

### Treasure Opener System
- Uses signature scanning to find game functions at runtime
- Signature patterns:
  - UpdatePositionInstance opcode: `C7 44 24 ?? ?? ?? ?? ?? 48 8D 54 24 ?? 48 C7 44 24 ?? ?? ?? ?? ?? 0F 11 44 24`
  - SendPacket: `E8 ?? ?? ?? ?? 48 8B D6 48 8B CF E8 ?? ?? ?? ?? 48 8B 8C 24`
  - OpenTreasure: `48 89 5C 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 3D`
- Command: `/hbtreasure` to toggle automatic treasure opening
- Implements teleport-to-treasure and auto-open via packet injection
- Maintains opened treasure ID tracking with automatic pruning

### XSZToolbox IPC Integration
- Multi-client remote control system via Dalamud IPC
- `XSZToolboxIpc.cs` provides client-side interface for consuming XSZToolbox services
- Supports remote commands for position control, movement, skill usage, targeting, and role management
- Key IPC endpoints (all prefixed with `XSZToolbox.RemoteControl.`):
  - Connection: `GetRoomId`, `IsConnected`
  - Position Control: `SetPos`, `LockPos`, `SlideTp`, `SetRot`
  - Movement: `MoveTo`, `MoveStop`, `Stop`, `Jump`
  - Combat: `UseSkill`, `UseSkillWithTarget`, `SetTarget`
  - Communication: `Echo`, `Cmd`
  - Role Management: `SetRole`, `GetRoleByPlayerName`, `GetRoleByPlayerCID`
  - Room Management: `Kick`, `GetMemberCount`, `GetOnlineMemberCount`
- Enables coordinated multi-character automation for raid mechanics

### Timeline System (In Development)
- Behavior tree-based combat automation framework inspired by AEAssist's trigger system
- **Core Architecture**:
  - **TreeNodeBase**: Base class for all nodes with async execution model
  - **TreeCompBase**: Composite nodes (Sequence, Parallel, Select) for organizing execution flow
  - **TreeActionBase**: Action nodes (Condition, Action, Delay, Script) for specific operations
- **Node Types**:
  - Sequence: Execute children in order, fail-fast or sequential mode
  - Parallel: Execute all children simultaneously
  - Select: Try children until one succeeds
  - Condition: Wait for or check game events (And/Or logic, reverse result)
  - Action: Execute trigger actions from AEAssist system
  - Delay: Pause execution for specified duration
  - Script: Dynamic C# code compilation and execution
- **Script System**:
  - Runtime compilation using Roslyn (Microsoft.CodeAnalysis.CSharp)
  - Two execution modes: Event-driven (Check method) or Direct execution (Run method)
  - ScriptEnv for inter-script communication (KV storage, Task synchronization)
  - Supports ITriggerCondParams for event handling (EnemyCastSpell, AddStatus, etc.)
- **Runtime System**:
  - TriggerlineData manages execution state and event dispatch
  - Event-driven condition awakening via TaskCompletionSource
  - Blackboard pattern for shared node state
  - Cancellation token support for clean shutdown
- **Data Flow**: Game Events → CalTriggerLine → Check waiting conditions → SetResult → Continue execution
- See `时间轴系统分析文档.md` for detailed architecture documentation

## Development Notes

### Safety Considerations
- This is a game automation plugin that interacts with FFXIV memory
- Uses unsafe code blocks for direct memory access
- Hook-based approach for intercepting game functions (ActorControlHook currently disabled)
- Signature scanning patterns may break with game updates

### Chinese Naming
Many files and classes use Chinese names reflecting the plugin's origin and target audience. Key examples:
- `指定职能tp指定位置.cs` (Role-specified position teleporting)
- `检测目标位置.cs` (Target position detection)
- `启动bmr.cs` (Launch BMR system)
- UI tabs: "几何计算" (Geometry), "自动化" (Automation), "FA全局设置" (General Settings), "事件记录" (Event Record), "黑名单管理" (Blacklist), "危险区域" (Danger Area)

### Code Style
- Uses C# 9.0+ features (init-only setters, records, top-level statements where applicable)
- Nullable reference types enabled
- Implicit usings enabled
- ImGui for UI rendering (via Dalamud bindings)
- Extensive use of singleton pattern for managers and settings

## Working with Timeline System

The timeline branch contains a behavior tree-based automation framework. Key files to understand:

### Timeline Documentation
- `时间轴系统分析文档.md`: Complete architecture analysis (Chinese)
- `.kiro/specs/timeline-system/requirements.md`: Formal requirements specification
- `.kiro/specs/timeline-system/design.md`: Design documentation
- `.kiro/specs/timeline-system/tasks.md`: Implementation tasks

### Key Concepts
1. **Nodes are async by default**: All node execution uses `async/await` pattern
2. **Event-driven waiting**: Condition nodes register with runtime and wait for game events
3. **Script compilation**: C# scripts are compiled at runtime using Roslyn
4. **Shared state**: ScriptEnv provides KV storage and Task synchronization between scripts
5. **Tree execution**: Root node recursively executes children based on node type logic

### Integration with AEAssist
- Timeline system is designed to work alongside AEAssist's existing trigger system
- Reuses ITriggerCond and ITriggerAction interfaces from AEAssist
- Custom trigger actions in `Triggers/TriggerAction/` can be used in timeline action nodes
- Custom trigger conditions in `Triggers/TriggerCondition/` can be used in timeline condition nodes

## Branch Information

- **Main branch**: `master` (stable releases)
- **Current branch**: `timeline` (timeline system development)
- When creating PRs, target the `master` branch unless specifically working on timeline features
