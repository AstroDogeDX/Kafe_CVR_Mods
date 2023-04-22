using ABI_RC.Core.Networking;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using DarkRift;
using DarkRift.Client;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Kafe.NetworkTest;

public class NetworkTest : MelonMod {

    public override void OnInitializeMelon() {
        //ModConfig.InitializeMelonPrefs();
    }

    // Sender 13999
    // private const string ModId = $"MelonMod.Kafe.{nameof(NetworkTest)}";

    private enum SeedPolicy : int {
        ToSpecific = 0,
        ToAll = 1,
    }

    private static void OnMessage(object sender, MessageReceivedEventArgs e) {
        using var message = e.GetMessage();

        // Ignore tags being used by the game
        if (!Enum.IsDefined(typeof(Tags), (int) e.Tag) && e.Tag != 33652 && e.Tag != 5019) {
            MelonLogger.Msg($"[{e.Tag}] Received!");
            if (e.Tag == ModMsg) {
                using var reader = message.GetReader();
                var argsCount = reader.Length;
                MelonLogger.Msg($"Args: {argsCount}");
                MelonLogger.Msg($"1st: {reader.ReadString()}");
                MelonLogger.Msg($"2st: {reader.ReadString()}");
            }
        }
    }


    private const string ModId = "5affbc06-d288-f39b-6fd0-b54a51b86613";
    private const ushort ModMsg = 13999;

    private static void SendMsgToSpecificPlayers(string msg, List<string> playerGuids) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToSpecific);
        writer.Write(playerGuids.Count);
        foreach (var playerGuid in playerGuids) {
            writer.Write(playerGuid);
        }
        writer.Write(msg);
        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    private static void SendMsgToAll(string msg) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToAll);
        writer.Write(msg);
        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    public override void OnUpdate() {
        if (Input.GetKeyDown(KeyCode.P)) {
            MelonLogger.Msg("Sending everyone important info! P");
            SendMsgToAll($"[{MetaPort.Instance.username}] I like Peanuts!");
            //SendMsgToSpecificPlayers($"[{MetaPort.Instance.username}] I like Peanuts!", new List<string> {"5affbc06-d288-f39b-6fd0-b54a51b86612"});
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            MelonLogger.Msg("Sending everyone important info! N");
            SendMsgToAll($"[{MetaPort.Instance.username}] I'm a Nerd!");
            //SendMsgToSpecificPlayers($"[{MetaPort.Instance.username}] I'm a Nerd!", new List<string> {"5affbc06-d288-f39b-6fd0-b54a51b86612"});
        }

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
