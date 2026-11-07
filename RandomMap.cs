using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;

namespace RandomMap;

public class RandomMapPlugin : BasePlugin
{
    public override string ModuleName => "RandomStartupMap";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Antigravity";
    public override string ModuleDescription => "Randomly picks and changes to a startup map from map.txt";

    private bool _startupTriggered = false;
    private readonly List<string> _mapList = new();
    private readonly Random _random = new();

    public override void Load(bool hotReload)
    {
        LoadMapList();

        // 注册地图开始监听
        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        Logger.LogInformation("[RandomMap] Plugin loaded successfully. HotReload: {HotReload}", hotReload);

        // 如果是冷启动，并且当前地图已经处于某种状态，也可以挂起首次触发检查
        if (hotReload)
        {
            _startupTriggered = true; // 如果是热重载，避免干扰当前正在进行的对局
            Logger.LogInformation("[RandomMap] Hot reload detected, skipping startup random map trigger.");
        }
    }

    private void OnMapStart(string mapName)
    {
        // 仅在服务器首次启动（开服）后触发一次
        if (_startupTriggered)
        {
            return;
        }

        _startupTriggered = true;

        // 延迟 2.5 秒执行换图，确保服务器网络与各项核心服务完全初始化完毕
        AddTimer(2.5f, () =>
        {
            TriggerRandomMap(isStartup: true);
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    /// <summary>
    /// 从与插件同目录下的 map.txt 读取地图列表
    /// </summary>
    private bool LoadMapList()
    {
        _mapList.Clear();

        string filePath = Path.Combine(ModuleDirectory, "map.txt");

        if (!File.Exists(filePath))
        {
            Logger.LogWarning("[RandomMap] map.txt not found at: {Path}", filePath);

            // 若文件不存在，自动写入默认示例文件方便服主直接修改
            try
            {
                File.WriteAllText(filePath, "de_ancient\ncs_italy\nde_dust2\nde_mirage\nde_inferno\nde_nuke\nde_anubis\n");
                Logger.LogInformation("[RandomMap] Created default map.txt at: {Path}", filePath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[RandomMap] Failed to create default map.txt");
                return false;
            }
        }

        try
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();

                // 跳过空行或注释行 (# 或 //)
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//"))
                {
                    continue;
                }

                _mapList.Add(line);
            }

            Logger.LogInformation("[RandomMap] Loaded {Count} maps from map.txt", _mapList.Count);
            for (int i = 0; i < _mapList.Count; i++)
            {
                Logger.LogInformation("[RandomMap]   [{Index}] {Map}", i + 1, _mapList[i]);
            }

            return _mapList.Count > 0;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[RandomMap] Error reading map.txt");
            return false;
        }
    }

    /// <summary>
    /// 随机选择地图并执行切换
    /// </summary>
    private void TriggerRandomMap(bool isStartup = false)
    {
        if (_mapList.Count == 0)
        {
            LoadMapList();
            if (_mapList.Count == 0)
            {
                Logger.LogError("[RandomMap] Cannot change map: map list is empty!");
                return;
            }
        }

        string chosenMap = _mapList[_random.Next(_mapList.Count)];
        string currentMap = Server.MapName;

        Logger.LogInformation("[RandomMap] Selected random map: {Map} (Current: {CurrentMap})", chosenMap, currentMap);

        // 如果开服默认地图恰好就是选中的地图，无需多余重载一次
        if (isStartup && string.Equals(currentMap, chosenMap, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogInformation("[RandomMap] Current map is already the selected map ({Map}). No need to change.", chosenMap);
            return;
        }

        Logger.LogInformation("[RandomMap] Executing: changelevel {Map}", chosenMap);
        Server.ExecuteCommand($"changelevel {chosenMap}");
    }

    [ConsoleCommand("css_randommap", "Manually trigger a random map change")]
    [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnCommandRandomMap(CCSPlayerController? player, CommandInfo command)
    {
        command.ReplyToCommand("[RandomMap] Manually triggering random map change...");
        TriggerRandomMap(isStartup: false);
    }

    [ConsoleCommand("css_randommap_reload", "Reload the map list from map.txt")]
    [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnCommandReload(CCSPlayerController? player, CommandInfo command)
    {
        if (LoadMapList())
        {
            command.ReplyToCommand($"[RandomMap] Successfully reloaded {_mapList.Count} maps.");
        }
        else
        {
            command.ReplyToCommand("[RandomMap] Failed to load map list or map.txt is empty.");
        }
    }
}
