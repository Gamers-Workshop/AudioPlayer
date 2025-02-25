﻿using AudioPlayer.Other;
using AudioPlayer.Other.EventsArgs;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using Exiled.Events.EventArgs.Player;
using Mirror;
using PlayerRoles;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioPlayer.Plugin;

namespace AudioPlayer;

internal class EventHandler
{
    private List<AudioFile> LobbyPlaylist = plugin.Config.LobbyPlaylist;
    internal void OnWaitingForPlayers()
    {
        if (plugin.Config.SpawnBot)
        {
            if (plugin.Config.SpecialEventsEnable)
            {
                if (LobbyPlaylist.Count > 0)
                    LobbyPlaylist[UnityEngine.Random.Range(0, LobbyPlaylist.Count)].Play(true);
            }
        }
    }
    public void SpawnDummy(string name = "Dedicated Server", bool showplayer = false, string badgetext = "AudioPlayer BOT", string bagdecolor = "orange", int id = 99)
    {
        if (plugin.FakeConnectionsIds.ContainsKey(id))
        {
            Log.Error("This id is already in use");
            return;
        }
        GameObject newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
        FakeConnection fakeConnection = new FakeConnection(id);
        ReferenceHub hubPlayer = newPlayer.GetComponent<ReferenceHub>();
        plugin.FakeConnectionsIds.Add(id, new FakeConnectionList()
        {
            BotID = id,
            BotName = name,
            fakeConnection = fakeConnection.identity,
            audioplayer = AudioPlayerBase.Get(hubPlayer),
            hubPlayer = hubPlayer,
        });
        if (RecyclablePlayerId.FreeIds.Contains(id))
        {
            RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
        }
        else if (RecyclablePlayerId._autoIncrement >= id)
        {
            RecyclablePlayerId._autoIncrement = id = RecyclablePlayerId._autoIncrement + 1;
        }
        NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
        if (!showplayer)
        {
            try
            {
                hubPlayer.characterClassManager._privUserId = $"{id}@audioplayerbot";
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        try
        {
            hubPlayer.nicknameSync.SetNick(name);
        }
        catch (Exception)
        {
            //Ignore
        }
        MEC.Timing.CallDelayed(1, () =>
        {
            try
            {
                hubPlayer.roleManager.ServerSetRole(RoleTypeId.Overwatch, RoleChangeReason.RemoteAdmin);
            }
            catch (Exception e)
            {
                Log.Error($"Error on {nameof(SpawnDummy)}: Error on set dummy role {e}");
            }
            hubPlayer.characterClassManager.GodMode = true;
            hubPlayer.serverRoles.SetText(badgetext);
            hubPlayer.serverRoles.SetColor(bagdecolor);
        });
        
    }
    internal void OnRoundStarted()
    {
        if (plugin.LobbySong)
        {
            LobbyPlaylist[UnityEngine.Random.Range(0, LobbyPlaylist.Count)].Stop();
            plugin.LobbySong = false;
        }
    }
    //AudioEvents
    internal void OnFinishedTrack(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos)
    {
        if (!Round.IsLobby || !plugin.Config.SpecialEventsEnable || !plugin.LobbySong)
            return;

        if (LobbyPlaylist.Count > 0)
            LobbyPlaylist[UnityEngine.Random.Range(0, LobbyPlaylist.Count)].Play(true);
    }
    internal void OnTrackSelected(AudioPlayerBase playerBase, bool directPlay, int queuePos, ref string track) =>
        Log.Info("Loading Audio (Debug SCPSLAudioApi)\n" +
            $"playerBase - {playerBase} \n" +
            $"directPlay - {directPlay} \n" +
            $"queuePos - {queuePos} \n" +
            $"track - {track} \n" +
            "");

    internal void OnTrackLoaded(AudioPlayerBase playerBase, bool directPlay, int queuePos, string track) =>
        Log.Info($"Play music {directPlay} (Debug SCPSLAudioApi) \n" +
            $"playerBase - {playerBase}\n" +
            $"directPlay - {directPlay}\n" +
            $"queuePos - {queuePos} \n" +
            $"track - {track} \n" +
            "");

    internal void OnTrackSelecting(AudioPlayerBase playerBase, bool directPlay, ref int queuePos) =>
        Log.Info($"Selecting Audio.... (Debug SCPSLAudioApi) \n" +
            $"playerBase - {playerBase} \n" +
            $"directPlay - {directPlay} \n" +
            $"queuePos - {queuePos} \n" +
            "");

    internal void OnFinishedTrackLog(AudioPlayerBase playerBase, string track, bool directPlay, ref int nextQueuePos) =>
        Log.Info($"Track Complete. (Debug SCPSLAudioApi) \n" +
            $"playerBase - {playerBase} \n" +
            $"track - {track} \n" +
            $"directPlay - {directPlay} \n" +
            $"nextQueuePos - {nextQueuePos} \n" +
            "");
    internal void OnAudioPlayerDiedAttacker(AudioPlayerDiedAttackerEventArgs ev)
    {
        Log.Debug($"OnAudioPlayerDiedAttacker\nPlayer - {ev.Player} | BotList {ev.BotsList} | Path - {ev.Path}");
    }

    internal void OnAudioPlayerDiedTarget(AudioPlayerDiedTargetEventArgs ev)
    {
        Log.Debug($"OnAudioPlayerDiedTarget\nPlayer - {ev.Player} | BotList {ev.BotsList} | Path - {ev.Path}");
    }

    internal void HandleInstanceModeChange(ReferenceHub arg1, ClientInstanceMode arg2)
    {
        if ((arg2 != ClientInstanceMode.Unverified || arg2 != ClientInstanceMode.Host) && arg1.characterClassManager._privUserId.Contains("@audioplayerbot"))
        {
            Log.Debug($"Replaced instancemode for dummy to host.");
            arg1.characterClassManager.InstanceMode = ClientInstanceMode.Host;
        }
    }

    internal void OnGenerated()
    {
        if (plugin.FakeConnectionsIds != null)
            plugin.FakeConnectionsIds.Clear();
        if (plugin.Config.SpawnBot)
        {
            foreach (var cfg in plugin.Config.BotsList)
            {
                SpawnDummy(cfg.BotName, cfg.ShowPlayerList, cfg.BadgeText, cfg.BadgeColor, cfg.BotId);
            }
        }
    }
}
