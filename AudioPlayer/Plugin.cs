﻿using AudioPlayer.Other.DLC;
using Exiled.API.Features;
using SCPSLAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using AudioPlayer.API;
using AudioPlayer.Other;

namespace AudioPlayer;

public class Plugin : Plugin<Config>
{
    public override string Prefix => "AudioPlayer";
    public override string Name => "AudioPlayer";
    public override string Author => "Rysik5318 and Mariki";
    public override Version Version { get; } = new Version(2, 1, 9);
    public override Version RequiredExiledVersion { get; } = AutoUpdateExiledVersion.AutoUpdateExiledVersion.RequiredExiledVersion;

    public static Plugin plugin; //troll 

    public Dictionary<int, FakeConnectionList> FakeConnectionsIds = new(); // It's more convenient.
    internal EventHandler handlers;
    internal SpecialEvents specialEvents;
    public bool LobbySong;
    public readonly string AudioPath = Path.Combine(Paths.Plugins, "audio");

    public override void OnEnabled()
    {
        try
        {
            plugin = this;
            handlers = new EventHandler();
            CharacterClassManager.OnInstanceModeChanged +=  handlers.HandleInstanceModeChange;
            Exiled.Events.Handlers.Map.Generated += handlers.OnGenerated;
            Exiled.Events.Handlers.Server.WaitingForPlayers += handlers.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += handlers.OnRoundStarted;
            //SpecialEvents
            if (Config.SpecialEventsEnable)
            {
                specialEvents = new SpecialEvents();
                Exiled.Events.Handlers.Map.AnnouncingNtfEntrance += specialEvents.OnAnnouncingNtfEntrance;
                Exiled.Events.Handlers.Server.RoundStarted += specialEvents.OnRoundStarted;
                Exiled.Events.Handlers.Server.RoundEnded += specialEvents.OnRoundEnded;
                Exiled.Events.Handlers.Server.RespawningTeam += specialEvents.OnRespawningTeam;
                Exiled.Events.Handlers.Warhead.Starting += specialEvents.OnWarheadStarting;
                Exiled.Events.Handlers.Warhead.Detonated += specialEvents.OnWarheadDetonated;
                Exiled.Events.Handlers.Warhead.Stopping += specialEvents.OnWarheadStopping;
                Exiled.Events.Handlers.Player.Verified += specialEvents.OnVerified;
                Exiled.Events.Handlers.Player.Died += specialEvents.OnDied;
            }
            //End SpecialEvents

            //AudioEvents
            SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += handlers.OnFinishedTrack;

            if (Config.Debug)
            {
                AudioEvents.AudioPlayerDiedAttacker += handlers.OnAudioPlayerDiedAttacker;
                AudioEvents.AudioPlayerDiedTarget += handlers.OnAudioPlayerDiedTarget;
            }

            if (Config.ScpslAudioApiDebug)
            {
                SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelecting += handlers.OnTrackSelecting;
                SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelected += handlers.OnTrackSelected;
                SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackLoaded += handlers.OnTrackLoaded;
                SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack += handlers.OnFinishedTrackLog;
                Log.Warn($"SCPSLAudioApi Debug Enable!");
            }
            //End AudioEvents

            Startup.SetupDependencies();
            Extensions.CreateDirectory();
            Log.Info("Loading AudioPlayer Event...");
        }
        catch (Exception e)
        {
            Log.Error($"Error loading plugin: {e}");
        }
        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        plugin = null;
        handlers = null;
        Exiled.Events.Handlers.Map.Generated -= handlers.OnGenerated;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= handlers.OnWaitingForPlayers;
        Exiled.Events.Handlers.Server.RoundStarted -= handlers.OnRoundStarted;

        specialEvents = null;
        Exiled.Events.Handlers.Map.AnnouncingNtfEntrance -= specialEvents.OnAnnouncingNtfEntrance;
        Exiled.Events.Handlers.Server.RoundStarted -= specialEvents.OnRoundStarted;
        Exiled.Events.Handlers.Server.RoundEnded -= specialEvents.OnRoundEnded;
        Exiled.Events.Handlers.Server.RespawningTeam -= specialEvents.OnRespawningTeam;
        Exiled.Events.Handlers.Warhead.Starting -= specialEvents.OnWarheadStarting;
        Exiled.Events.Handlers.Warhead.Detonated -= specialEvents.OnWarheadDetonated;
        Exiled.Events.Handlers.Warhead.Stopping -= specialEvents.OnWarheadStopping;
        Exiled.Events.Handlers.Player.Verified -= specialEvents.OnVerified;
        Exiled.Events.Handlers.Player.Died -= specialEvents.OnDied;

        AudioEvents.AudioPlayerDiedAttacker -= handlers.OnAudioPlayerDiedAttacker;
        AudioEvents.AudioPlayerDiedTarget -= handlers.OnAudioPlayerDiedTarget;

        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= handlers.OnFinishedTrack;

        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelecting -= handlers.OnTrackSelecting;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelected -= handlers.OnTrackSelected;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackLoaded -= handlers.OnTrackLoaded;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= handlers.OnFinishedTrackLog;

        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelecting -= handlers.OnTrackSelecting;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackSelected -= handlers.OnTrackSelected;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnTrackLoaded -= handlers.OnTrackLoaded;
        SCPSLAudioApi.AudioCore.AudioPlayerBase.OnFinishedTrack -= handlers.OnFinishedTrackLog;

        base.OnDisabled();
    }
}
