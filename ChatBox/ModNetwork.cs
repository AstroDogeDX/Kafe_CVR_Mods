﻿using System.Text;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.IO.Social;
using DarkRift;
using DarkRift.Client;
using HarmonyLib;
using MelonLoader;

namespace Kafe.ChatBox;

public static class ModNetwork {

    private const ushort ModMsgTag = 13999;
    private const string ModId = $"MelonMod.Kafe.{nameof(ChatBox)}";

    private const int CharactersMaxCount = 2000;

    private enum SeedPolicy {
        ToSpecific = 0,
        ToAll = 1,
    }

    private enum MessageType : byte {
        Typing = 0,
        SendMessage = 1,
    }

    internal static void SendTyping(API.MessageSource source, bool isTyping, bool notification) {
        var sent = SendMsgToAllPlayers(MessageType.Typing, writer => {
            writer.Write((byte) source);
            writer.Write(isTyping);
            writer.Write(notification);
        });
        if (sent) API.OnIsTypingSent?.Invoke(source, isTyping, notification);
    }

    internal static void SendMessage(API.MessageSource source, string msg, bool notification, bool displayMessage) {
        if (msg.Length > CharactersMaxCount) {
            MelonLogger.Warning($"Messages can have a maximum of {CharactersMaxCount} characters.");
            return;
        }
        var sent = SendMsgToAllPlayers(MessageType.SendMessage, writer => {
            writer.Write((byte) source);
            writer.Write(msg, Encoding.UTF8);
            writer.Write(notification);
            writer.Write(displayMessage);
        });
        if (sent) API.OnMessageSent?.Invoke(source, msg, notification, displayMessage);
    }

    private static void OnMessage(object sender, MessageReceivedEventArgs e) {

        // Ignore Messages that are not Mod Network Messages
        if (e.Tag != ModMsgTag) return;

            using var message = e.GetMessage();
            using var reader = message.GetReader();
            var modId = reader.ReadString();

            // Ignore Messages not for our mod
            if (modId != ModId) return;

            var senderGuid = reader.ReadString();

        try {

            // Ignore messages from non-friends
            if (ModConfig.MeOnlyViewFriends.Value && !Friends.FriendsWith(senderGuid)) return;

            var msgTypeRaw = reader.ReadByte();

            // Ignore wrong msg types
            if (!Enum.IsDefined(typeof(MessageType), msgTypeRaw)) return;

            switch ((MessageType) msgTypeRaw) {

                case MessageType.Typing:

                    var typingSrcByte = reader.ReadByte();
                    if (!Enum.IsDefined(typeof(API.MessageSource), typingSrcByte)) {
                        throw new Exception($"[Typing] Received an invalid source byte ({typingSrcByte}).");
                    }

                    var isTyping = reader.ReadBoolean();
                    var notification = reader.ReadBoolean();

                    API.OnIsTypingReceived?.Invoke((API.MessageSource)typingSrcByte, senderGuid, isTyping, notification);
                    break;

                case MessageType.SendMessage:

                    var messageSrcByte = reader.ReadByte();
                    if (!Enum.IsDefined(typeof(API.MessageSource), messageSrcByte)) {
                        throw new Exception($"[SendMessage] Received an invalid source byte ({messageSrcByte}).");
                    }

                    var msg = reader.ReadString(Encoding.UTF8);
                    if (msg.Length > CharactersMaxCount) {
                        MelonLogger.Warning($"Ignored message from {sender} because it's over {CharactersMaxCount} characters.");
                        return;
                    }

                    var sendNotification = reader.ReadBoolean();
                    var displayMessage = reader.ReadBoolean();

                    API.OnMessageReceived?.Invoke((API.MessageSource)messageSrcByte, senderGuid, msg, sendNotification, displayMessage);
                    break;
            }
        }
        catch (Exception) {
            MelonLogger.Warning($"Received a malformed message from ${senderGuid}, they might be running an outdated " +
                                $"version of the mod, or I broke something, or they're trying to do something funny.");
        }
    }

    private static bool SendMsgToAllPlayers(MessageType msgType, Action<DarkRiftWriter> msgDataAction = null) {

        if (NetworkManager.Instance == null ||
            NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
            MelonLogger.Warning($"Attempted to {nameof(SendMsgToAllPlayers)} but the Game Network is Down...");
            return false;
        }

        // Mandatory message parameters
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToAll);

        // Set the message type (for our internal behavior)
        writer.Write((byte) msgType);

        // Set the parameters we want to send
        msgDataAction?.Invoke(writer);

        using var message = Message.Create(ModMsgTag, writer);
        if (message.DataLength > 60000) {
            MelonLogger.Warning($"The limit for data in a single packet should be {ushort.MaxValue} bytes, " +
                                $"so let's limit to 60000 bytes. This message won't be sent");
            return false;
        }
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
        return true;
    }

    private static void SendToSpecificPlayers(List<string> playerGuids, MessageType msgType, Action<DarkRiftWriter> msgDataAction = null) {

        if (NetworkManager.Instance == null ||
            NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
            MelonLogger.Warning($"Attempted to {nameof(SendToSpecificPlayers)} but the Game Network is Down...");
            return;
        }

        // Mandatory message parameters
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToSpecific);
        writer.Write(playerGuids.Count);
        foreach (var playerGuid in playerGuids) {
            writer.Write(playerGuid);
        }

        // Set the message type (for our internal behavior)
        writer.Write((byte) msgType);

        // Set the parameters we want to send
        msgDataAction?.Invoke(writer);

        using var message = Message.Create(ModMsgTag, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    [HarmonyPatch]
    internal class HarmonyPatches {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.Awake))]
        public static void After_NetworkManager_Awake(NetworkManager __instance) {
            try {
                MelonLogger.Msg($"Started the Game Server Messages Listener...");
                __instance.GameNetwork.MessageReceived += OnMessage;
            }
            catch (Exception e) {
                MelonLogger.Error($"Error during the patched function {nameof(After_NetworkManager_Awake)}");
                MelonLogger.Error(e);
            }
        }
    }

}
