using System.Text;
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
        ModConfig.InitializeMelonPrefs();
    }

    // Sender 13999
    private const string ModId = $"MelonMod.Kafe.{nameof(NetworkTest)}";
    private const string ModId2 = $"MelonMod.Kafe.{nameof(NetworkTest)}.2";

    private enum SeedPolicy : int {
        ToSpecific = 0,
        ToAll = 1,
    }

    //private const string ModId = "5affbc06-d288-f39b-6fd0-b54a51b86613";
    private const ushort ModMsg = 13999;

    private static void OnMessage(object sender, MessageReceivedEventArgs e) {
        using var message = e.GetMessage();

        // Ignore tags being used by the game
        // if (!Enum.IsDefined(typeof(Tags), (int) e.Tag) && e.Tag != 33652 && e.Tag != 5019) {
        //     MelonLogger.Msg($"[{e.Tag}] Received!");
        // }

        if (e.Tag == ModMsg) {

            using var reader = message.GetReader();
            var argsCount = reader.Length;

            MelonLogger.Msg($"Arg Count: {argsCount}");
            var modId = reader.ReadString();
            MelonLogger.Msg($"ModId: {modId}");
            var senderGuid = reader.ReadString();
            MelonLogger.Msg($"senderGuid: {senderGuid}");

            if (modId == ModId) {
                var readString = reader.ReadString();
                MelonLogger.Msg($"data [str]: {readString}");

                var dataListStrings = reader.ReadStrings();
                MelonLogger.Msg($"data [ListStr]: {string.Join(", ", dataListStrings)}");


                var dataBool = reader.ReadBoolean();
                MelonLogger.Msg($"data [dataBool]: {dataBool}");
                var dataBools = reader.ReadBooleans();
                MelonLogger.Msg($"data [dataBools]: {string.Join(", ", dataBools)}");


                var dataShort = reader.ReadInt16();
                MelonLogger.Msg($"data [dataShort]: {dataShort}");
                var dataShorts = reader.ReadInt16s();
                MelonLogger.Msg($"data [dataShorts]: {string.Join(", ", dataShorts)}");
                var dataUShort = reader.ReadUInt16();
                MelonLogger.Msg($"data [dataUShort]: {dataUShort}");
                var dataUShorts = reader.ReadUInt16s();
                MelonLogger.Msg($"data [dataUShorts]: {string.Join(", ", dataUShorts)}");

                var dataInt = reader.ReadInt32();
                MelonLogger.Msg($"data [dataInt]: {dataInt}");
                var dataInts = reader.ReadInt32s();
                MelonLogger.Msg($"data [dataInts]: {string.Join(", ", dataInts)}");
                var dataUInt = reader.ReadUInt32();
                MelonLogger.Msg($"data [dataUInt]: {dataUInt}");
                var dataUInts = reader.ReadUInt32s();
                MelonLogger.Msg($"data [dataUInts]: {string.Join(", ", dataUInts)}");

                var dataByte = reader.ReadByte();
                MelonLogger.Msg($"data [dataByte]: {dataByte}");
                var dataBytes = reader.ReadBytes();
                MelonLogger.Msg($"data [dataBytes]: {string.Join(", ", dataBytes)}");
                var dataSByte = reader.ReadSByte();
                MelonLogger.Msg($"data [dataSByte]: {dataSByte}");
                var dataSBytes = reader.ReadSBytes();
                MelonLogger.Msg($"data [dataSBytes]: {string.Join(", ", dataSBytes)}");

                var dataFloat = reader.ReadSingle();
                MelonLogger.Msg($"data [dataFloat]: {dataFloat}");
                var dataFloats = reader.ReadSingles();
                MelonLogger.Msg($"data [dataFloats]: {string.Join(", ", dataFloats)}");

                var dataLong = reader.ReadInt64();
                MelonLogger.Msg($"data [dataLong]: {dataLong}");
                var dataLongs = reader.ReadInt64s();
                MelonLogger.Msg($"data [dataLongs]: {string.Join(", ", dataLongs)}");
                var dataULong = reader.ReadUInt64();
                MelonLogger.Msg($"data [dataULong]: {dataULong}");
                var dataULongs = reader.ReadUInt64s();
                MelonLogger.Msg($"data [dataULongs]: {string.Join(", ", dataULongs)}");

                var dataDouble = reader.ReadDouble();
                MelonLogger.Msg($"data [dataDouble]: {dataDouble}");
                var dataDoubles = reader.ReadDoubles();
                MelonLogger.Msg($"data [dataDoubles]: {string.Join(", ", dataDoubles)}");

                var dataChar = reader.ReadChar();
                MelonLogger.Msg($"data [dataChar]: {dataChar}");
                var dataChars = reader.ReadChars(Encoding.UTF32);
                MelonLogger.Msg($"data [dataChars]: {string.Join(", ", dataChars)}");

                var dataStringUTF8 = reader.ReadString(Encoding.UTF32);
                MelonLogger.Msg($"data [dataStringUTF8]: {dataStringUTF8}");


                var dataNoachi = reader.ReadSerializable<Noachi>();
                MelonLogger.Msg($"data [dataNoachi]: deez: {dataNoachi.deez}, nuts: {dataNoachi.nuts}");

            }
            else if (modId == ModId2) {
                var dataLongString = reader.ReadString();
                MelonLogger.Msg($"data [dataLongString]: {dataLongString}");
            }
        }
    }

    class Noachi : IDarkRiftSerializable {

        internal bool deez;
        internal int nuts;

        public void Deserialize(DeserializeEvent e) {
            deez = e.Reader.ReadBoolean();
            nuts = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e) {
            e.Writer.Write(deez);
            e.Writer.Write(nuts);
        }
    }

    private static void SendMsgToSpecificPlayers(string msg, List<string> playerGuids) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToSpecific);
        writer.Write(playerGuids.Count);
        foreach (var playerGuid in playerGuids) {
            writer.Write(playerGuid);
        }



        writer.Write(msg);
        writer.Write(new [] {msg, msg, msg, msg, msg});

        writer.Write(true);
        writer.Write(new [] {true, true});

        writer.Write((short)1);
        writer.Write(new []{(short)1, (short)1});
        writer.Write((ushort)1);
        writer.Write(new []{(ushort)1, (ushort)1});
        writer.Write((uint)1);
        writer.Write(new []{(uint)1, (uint)1});
        writer.Write((int)1);
        writer.Write(new []{(int)1, (int)1});
        writer.Write((byte)1);
        writer.Write(new []{(byte)1, (byte)1});
        writer.Write((sbyte)1);
        writer.Write(new []{(sbyte)1, (sbyte)1});
        writer.Write((float)1);
        writer.Write(new []{(float)1, (float)1});
        writer.Write((long)1);
        writer.Write(new []{(long)1, (long)1});
        writer.Write((ulong)1);
        writer.Write(new []{(ulong)1, (ulong)1});
        writer.Write((double)1);
        writer.Write(new []{(double)1, (double)1});

        writer.Write('A');
        writer.Write(new [] {'A', 'B'}, Encoding.UTF32);

        const string testString = "👋 Hello, world! 😃 🌎";
        writer.Write(testString, Encoding.UTF32);

        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    // private static void SendMsgToAll(string msg) {
    //     using var writer = DarkRiftWriter.Create();
    //     writer.Write(ModId);
    //     writer.Write((int) SeedPolicy.ToAll);
    //     writer.Write(msg);
    //     writer.Write(msg);
    //     using var message = Message.Create(ModMsg, writer);
    //     NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    // }

    private static void SendMsgToAllKb(int bytes) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId2);
        writer.Write((int)SeedPolicy.ToAll);

        // Sending a string with bytes
        var sb = new StringBuilder();
        for (var i = 0; i < bytes / 2; i++) {
            sb.Append("A");
        }

        writer.Write(sb.ToString());

        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    private static void SendMsgToSpecificPlayersKb(int bytes, List<string> playerGuids) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId2);
        writer.Write((int)SeedPolicy.ToSpecific);
        writer.Write(playerGuids.Count);
        foreach (var playerGuid in playerGuids) {
            writer.Write(playerGuid);
        }

        // Sending a string with kbytes
        var sb = new StringBuilder();
        for (var i = 0; i < bytes / 2; i++) {
            sb.Append("A");
        }

        writer.Write(sb.ToString());

        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    private static void SendMsgToAll(string msg) {
        using var writer = DarkRiftWriter.Create();
        writer.Write(ModId);
        writer.Write((int) SeedPolicy.ToAll);

        writer.Write(msg);


        writer.Write(new [] {msg, msg, msg, msg, msg});

        writer.Write(true);
        writer.Write(new [] {true, true});

        writer.Write((short)1);
        writer.Write(new []{(short)1, (short)1});
        writer.Write((ushort)1);
        writer.Write(new []{(ushort)1, (ushort)1});
        writer.Write((uint)1);
        writer.Write(new []{(uint)1, (uint)1});
        writer.Write((int)1);
        writer.Write(new []{(int)1, (int)1});
        writer.Write((byte)1);
        writer.Write(new []{(byte)1, (byte)1});
        writer.Write((sbyte)1);
        writer.Write(new []{(sbyte)1, (sbyte)1});
        writer.Write((float)1);
        writer.Write(new []{(float)1, (float)1});
        writer.Write((long)1);
        writer.Write(new []{(long)1, (long)1});
        writer.Write((ulong)1);
        writer.Write(new []{(ulong)1, (ulong)1});
        writer.Write((double)1);
        writer.Write(new []{(double)1, (double)1});

        writer.Write('A');
        writer.Write(new [] {'A', 'B'}, Encoding.UTF32);

        const string testString = "👋 Hello, world! 😃 🌎";
        writer.Write(testString, Encoding.UTF32);

        // Class
        writer.Write(new Noachi {deez = true, nuts = 69});

        // Send 1MB of text
        // var sb = new StringBuilder();
        // for (var i = 0; i < 1024 * 1024 * 1 / 2; i++) {
        //     sb.Append("A");
        // }
        // writer.Write(sb.ToString());

        using var message = Message.Create(ModMsg, writer);
        NetworkManager.Instance.GameNetwork.SendMessage(message, SendMode.Reliable);
    }

    public override void OnUpdate() {

        if (Input.GetKeyDown(KeyCode.P)) {
            if (NetworkManager.Instance == null || NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
                MelonLogger.Warning($"Attempted to send a game network messaged without being connected to an online instance...");
                return;
            }
            MelonLogger.Msg("Sending everyone important info! P");
            SendMsgToAll($"[{MetaPort.Instance.username}] I like Peanuts!");
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            if (NetworkManager.Instance == null || NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
                MelonLogger.Warning($"Attempted to send a game network messaged without being connected to an online instance...");
                return;
            }
            MelonLogger.Msg("Sending noachi and marm important info! M");
            SendMsgToSpecificPlayers($"[{MetaPort.Instance.username}] I like Specific!", new List<string> {"72015a33-a590-e373-9891-c7288b8cc047", "b58c00f5-cafe-974f-f022-1e651f33f506"});
        }

        if (Input.GetKeyDown(KeyCode.G)) {
            if (NetworkManager.Instance == null || NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
                MelonLogger.Warning($"Attempted to send a game network messaged without being connected to an online instance...");
                return;
            }
            MelonLogger.Msg($"Sending everyone important info! G -> Headers + {ModConfig.MeStringSize.Value} Bytes");
            SendMsgToAllKb(ModConfig.MeStringSize.Value);
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            if (NetworkManager.Instance == null || NetworkManager.Instance.GameNetwork.ConnectionState != ConnectionState.Connected) {
                MelonLogger.Warning($"Attempted to send a game network messaged without being connected to an online instance...");
                return;
            }

            MelonLogger.Msg($"Sending noachi and marm important info! H -> Headers + {ModConfig.MeStringSize.Value} Bytes");
            SendMsgToSpecificPlayersKb(ModConfig.MeStringSize.Value, new List<string> {"72015a33-a590-e373-9891-c7288b8cc047", "b58c00f5-cafe-974f-f022-1e651f33f506"});
        }
        //
        // if (Input.GetKeyDown(KeyCode.E)) {
        //     MelonLogger.Msg("Sending everyone important info! E");
        //     // SendMsgToAll($"[{MetaPort.Instance.username}] I'm a Nerd!");
        //     //SendMsgToSpecificPlayers($"[{MetaPort.Instance.username}] I'm a Nerd!", new List<string> {"5affbc06-d288-f39b-6fd0-b54a51b86612"});
        //     SendMsgToAllEmpty();
        // }

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
