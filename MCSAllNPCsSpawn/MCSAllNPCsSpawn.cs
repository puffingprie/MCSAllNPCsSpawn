using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bag;
using BepInEx;
using Fungus;
using GUIPackage;
using HarmonyLib;
using JiaoYi;
using JSONClass;
using KBEngine;
using KillSystem;
using Newtonsoft.Json.Linq;
using PaiMai;
using script.KillSystem;
using Tab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YSGame.TianJiDaBi;

namespace MCSAllNPCsSpawn
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MCSAllNPCsSpawn : BaseUnityPlugin
    {
        public const string pluginGuid = "arx.mcs.allnpcsspawn";
        public const string pluginName = "MCSAllNPCsSpawn";
        public const string pluginVersion = "1.0.0.0";

        private ConfigEntry<string> configGreeting;
        private ConfigEntry<bool> configDisplayGreeting;


        private static void UselessDebuggerPrefix()
        {
            Debug.Log("===================MCSAllNPCsSpawn=====================");
        }
        private void Awake()
        {
            UselessDebuggerPrefix();
            Debug.Log("MCSAllNPCsSpawn Loaded");
            configGreeting = Config.Bind("General",      // The section under which the option is shown
                                        "GreetingText",  // The key of the configuration option in the configuration file
                                        "Hello, world!", // The default value
                                        "A greeting text to show when the game is launched"); // Description of the option to show in the config file

            configDisplayGreeting = Config.Bind("General.Toggles",
                                                "DisplayGreeting",
                                                true,
                                                "Whether or not to show the greeting text");
            UselessDebuggerPrefix();
            Debug.Log("Hello, World!");
        }
        private void Start()
        {
            UselessDebuggerPrefix();
            Debug.Log("MCSAlNPCsSpawn: Game started...");
            Harmony.CreateAndPatchAll(typeof(MCSAllNPCsSpawn), null);
            MCSAllNPCsSpawn.Inst = this;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_firstCreateNpcs_Postfix(NPCFactory __instance)
        {
            UselessDebuggerPrefix();
            Debug.Log("MCSAllNPCsSpawn: calling NPCFactory_firstCreateNpcs_Postfix()");
            Debug.Log(jsonData.instance.NPCLeiXingDate);
        }

        private static MCSAllNPCsSpawn Inst;
    }
}
