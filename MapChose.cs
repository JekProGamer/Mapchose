using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Microsoft.Extensions.Logging;

namespace MapChose;

public sealed class MapChosePlugin : BasePlugin
{
    public override string ModuleName => "MapChose";
    public override string ModuleVersion => "1.2.1";
    public override string ModuleAuthor => "JekProGamer";
    public override string ModuleDescription => "Alternates between two configured CS2 Workshop maps using a map time limit.";

    private string[] _maps = Array.Empty<string>();
    private string _currentMap = string.Empty;
    private bool _switching;
    private int _mapTimeLimitMinutes = 20;
    private Timer? _mapTimer;
    private Timer? _notificationTimer;

    public override void Load(bool hotReload)
    {
        LoadConfig();
        LoadMapList();

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Logger.LogInformation("MapChose loaded. Maps: {Count}; map time limit: {Minutes} minutes.",
            _maps.Length, _mapTimeLimitMinutes);
    }

    private void OnMapStart(string mapName)
    {
        // Stop any previous timers before starting timers for this map.
        _mapTimer?.Kill();
        _mapTimer = null;
        _notificationTimer?.Kill();
        _notificationTimer = null;

        _currentMap = mapName;
        _switching = false;

        Logger.LogInformation("MapChose: map {Map} started. Next map change in {Minutes} minutes.",
            _currentMap, _mapTimeLimitMinutes);

        if (_maps.Length != 2)
        {
            Logger.LogError("MapChose: maplist.txt must contain exactly 2 valid map names. Map timer was not started.");
            return;
        }

        // Announce five minutes before the change. If the configured map time is
        // shorter than five minutes, announce at map start with the actual time.
        var noticeMinutes = Math.Min(5, _mapTimeLimitMinutes);
        var noticeDelaySeconds = (_mapTimeLimitMinutes - noticeMinutes) * 60.0f;

        _notificationTimer = AddTimer(noticeDelaySeconds, () =>
        {
            Server.PrintToChatAll(
                $"{ChatColors.White} [{ChatColors.Gold} Rez{ChatColors.Red} Top{ChatColors.White}.cc] До смены карты осталось {ChatColors.Green} {noticeMinutes} {ChatColors.White}Минут.");
            _notificationTimer = null;
        }, TimerFlags.STOP_ON_MAPCHANGE);

        _mapTimer = AddTimer(_mapTimeLimitMinutes * 60.0f, ChangeToNextMap, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeToNextMap()
    {
        if (_switching || _maps.Length != 2)
            return;

        var currentIndex = Array.FindIndex(
            _maps,
            map => string.Equals(map, _currentMap, StringComparison.OrdinalIgnoreCase));

        var nextMap = currentIndex < 0
            ? _maps[0]
            : _maps[(currentIndex + 1) % _maps.Length];

        _switching = true;

        Logger.LogInformation("MapChose: time limit reached. Changing map from {Current} to {Next}.",
            _currentMap, nextMap);

        Server.ExecuteCommand($"ds_workshop_changelevel {nextMap}");
    }

    private void LoadConfig()
    {
        var path = Path.Combine(ModuleDirectory, "mapchose.json");

        if (!File.Exists(path))
        {
            var defaultConfig = new MapChoseConfig();
            File.WriteAllText(path, JsonSerializer.Serialize(defaultConfig,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<MapChoseConfig>(json) ?? new MapChoseConfig();

            if (config.MapTimeLimit < 1 || config.MapTimeLimit > 1440)
            {
                Logger.LogError("MapChose: map_timelimit must be between 1 and 1440 minutes. Using default 20.");
                _mapTimeLimitMinutes = 20;
                return;
            }

            _mapTimeLimitMinutes = config.MapTimeLimit;
        }
        catch (Exception ex)
        {
            Logger.LogError("MapChose: failed to read mapchose.json: {Error}. Using default 20 minutes.", ex.Message);
            _mapTimeLimitMinutes = 20;
        }
    }

    private void LoadMapList()
    {
        var path = Path.Combine(ModuleDirectory, "maplist.txt");

        if (!File.Exists(path))
        {
            File.WriteAllLines(path, new[]
            {
                "dm_nixware_mirage_d",
                "dm_nixware_fysnow_d"
            });
        }

        _maps = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("#"))
            .Where(line => Regex.IsMatch(line, @"^[a-zA-Z0-9_]+$"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (_maps.Length != 2)
            Logger.LogError("MapChose: maplist.txt must contain exactly 2 valid map names; found {Count}.", _maps.Length);
    }

    private sealed class MapChoseConfig
    {
        [JsonPropertyName("map_timelimit")]
        public int MapTimeLimit { get; set; } = 20;
    }
}
