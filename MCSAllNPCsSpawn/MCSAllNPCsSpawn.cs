using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace MCSAllNPCsSpawn
{
    [BepInPlugin(modGuid, modName, modVersion)]
    public class MCSAllNPCsSpawn : BaseUnityPlugin
    {
        public const string modGuid = "Arx.MCS.AllNpcsSpawn";
        public const string modName = "MCSAllNPCsSpawn";
        public const string modVersion = "1.0.0.0";

        private static void LogStuff(string message)
        {
            Debug.Log("===================MCSAllNPCsSpawn=====================");
            Debug.Log(message);
            Debug.Log("=======================================================");
        }

        private static void WriteToShittyLog(string message, bool createNewOrOverwrite = false)
        {
            if (createNewOrOverwrite)
            {
                File.WriteAllText(@"C:\Users\Shadow\Documents\MiChangSheng\AllNPCsSpawn\Log.txt", message);
            }
            else
            {
                File.AppendAllText(@"C:\Users\Shadow\Documents\MiChangSheng\AllNPCsSpawn\Log.txt", message);
            }
        }

        private void Start()
        {
            MCSAllNPCsSpawn.Inst = this;

            // Create Shitty Log
            WriteToShittyLog("MCSAllNPCsSpawn Log");

            MCSAllNPCsSpawn.enableAllNPCsSpawn = base.Config.Bind<bool>("MCSAllNPCsSpawnConfig", "EnableAllNPCsSpawning", true, "Enable this mod (enable all npcs spawning)");
            MCSAllNPCsSpawn.maxSpawnCount = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "MaxSpawnCount", 30000, "Controls the max amount of NPCs that spawn - Increase this number at your own risk. Personally I have a potato of a computer and it's always entertaining when I can barely move on the map because there are 80,000 NPCs.");
            MCSAllNPCsSpawn.prioritizeHigherCultivationLevelNPCSpawn = base.Config.Bind<bool>("MCSAllNPCsSpawnConfig", "PrioritizeHigherCultivationLevelNPCSpawn", true, "Whether to prioritize higher cultivation level NPCs when NPC spawn count is limited. Used in conjunction with MaxSpawnCount. If MaxSpawnCount is -1, this has no effect.");
            MCSAllNPCsSpawn.onlyHumans = base.Config.Bind<bool>("MCSAllNPCsSpawnConfig", "onlyHumans", true, "Only Humans Spawn");
            MCSAllNPCsSpawn.spawnMinMoney = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMinMoney", 0, "Limit NPC spawning to NPCs withh AT LEAST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Greater than or equals");
            MCSAllNPCsSpawn.spawnMaxMoney = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMaxMoney", 11, "Limit NPC spawning to NPCs withh AT MOST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Less than or equals");
            MCSAllNPCsSpawn.spawnMinCultivationLevel = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMinCultivationLevel", true, "Limit NPC spawning to NPCs withh AT LEAST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Greater than or equals");
            MCSAllNPCsSpawn.spawnMaxCultivationLevel = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMaxCultivationLevel", true, "Limit NPC spawning to NPCs withh AT MOST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Less than or equals");
            MCSAllNPCsSpawn.npcPowerIncreaseAmount = base.Config.Bind<double>("MCSAllNPCsSpawnConfig", "NPCPowerIncreaseAmount", 1.0, "Not Yet Implemented - Because of the insane amount of NPCs that get spawned into the game, plugins like UniqueCream's ExtraChoicesForTalents plugin which constantly updates NPC data every tick lags the game out immensely. Thought it might be better if NPCs just got a flat increase in the beginning. Again, potato computer reasons.");

            Harmony.CreateAndPatchAll(typeof(MCSAllNPCsSpawn), null);
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_firstCreateNpcs_Postfix_Old(NPCFactory __instance)
        {
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {

                List<string> item = new List<string>();
                List<List<string>> list = new List<List<string>>
            {
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item,
                item
            };
                for (int i = 0; i < jsonData.instance.NPCLeiXingDate.Count; i++)
                {
                    if (jsonData.instance.NPCLeiXingDate[i]["NPCTag"].Count == 2)
                    {
                        int i2 = jsonData.instance.NPCLeiXingDate[i]["Level"].I;
                        list[i2].Add(jsonData.instance.NPCLeiXingDate[i].ToString());
                    }
                }
                List<string> npcImportantDateIdList = new List<string>();
                List<string> avatarJsonDataNameList = new List<string>();
                for (int j = 0; j < jsonData.instance.NPCImportantDate.Count; j++)
                {
                    npcImportantDateIdList.Add(jsonData.instance.NPCImportantDate[j]["id"].I.ToString());
                    avatarJsonDataNameList.Add(
                        ToolsEx.ToCN(
                            jsonData.instance.AvatarJsonData[
                                jsonData.instance.NPCImportantDate[j]["id"].I.ToString()
                            ]["Name"].Str
                        )
                    );
                }
                List<int> npcChenghaoDataIdList = new List<int>();
                List<string> npcChenghaoDataChenghaoList = new List<string>();
                List<string> npcChenghaoNPCTypeList = new List<string>();
                for (int k = 0; k < jsonData.instance.NPCChengHaoData.Count; k++)
                {
                    npcChenghaoDataIdList.Add(jsonData.instance.NPCChengHaoData[k]["id"].I);
                    npcChenghaoDataChenghaoList.Add(ToolsEx.ToCN(jsonData.instance.NPCChengHaoData[k]["ChengHao"].Str));
                    npcChenghaoNPCTypeList.Add(jsonData.instance.NPCChengHaoData[k]["NPCType"].I.ToString());
                }
                int count = jsonData.instance.AvatarJsonData.Count;
                for (int l = 0; l < count; l++)
                {
                    string text = jsonData.instance.AvatarJsonData[l]["id"].I.ToString();
                    int i3 = jsonData.instance.AvatarJsonData[l]["id"].I;
                    string npcname = ToolsEx.ToCN(jsonData.instance.AvatarJsonData[l]["Name"].Str);
                    bool flag3 =
                        jsonData.instance.AvatarJsonData[l]["Title"].Str != ""
                        && i3 < 20000
                        && jsonData.instance.AvatarJsonData[l]["MoneyType"].I <= 10
                        && jsonData.instance.AvatarJsonData[l]["MoneyType"].I > 0;
                    if (flag3)
                    {
                        bool flag4 = i3 > 20000;
                        if (flag4)
                        {
                            break;
                        }
                        bool flag5 =
                            !(npcname != "") || avatarJsonDataNameList.FindIndex((string xxt) => xxt == npcname) <= -1;
                        if (flag5)
                        {
                            int i4 = jsonData.instance.AvatarJsonData[l]["Level"].I;
                            string npctitle = ToolsEx.ToCN(
                                jsonData.instance.AvatarJsonData[l]["Title"].Str
                            );
                            int i5 = jsonData.instance.AvatarJsonData[l]["AvatarType"].I;
                            int npcChenghaoIdx = npcChenghaoDataChenghaoList.FindIndex((string xxt) => xxt == npctitle);
                            bool flag6 = npcChenghaoIdx > -1;
                            int num3;
                            string text2;
                            int num4;
                            if (flag6)
                            {
                                num3 = npcChenghaoDataIdList[npcChenghaoIdx];
                                text2 = npcChenghaoNPCTypeList[npcChenghaoIdx];
                                num4 = 0;
                            }
                            else
                            {
                                num3 = 418;
                                text2 = "10";
                                num4 = i3;
                            }
                            JSONObject jsonobject = null;
                            for (int m = 0; m < list[i4].Count - 1; m++)
                            {
                                JSONObject jsonobject2 = new JSONObject(
                                    list[i4][m].ToString(),
                                    -2,
                                    false,
                                    false
                                );
                                bool flag7 =
                                    jsonobject2["Type"].I.ToString() == text2
                                    && (
                                        jsonobject2["AvatarType"].I == i5
                                        || jsonobject2["AvatarType"].I == 1
                                        || (i5 == 1 && jsonobject2["AvatarType"].I == 3)
                                    );
                                if (flag7)
                                {
                                    jsonobject = jsonobject2;
                                    break;
                                }
                            }
                            bool flag8 = jsonobject == null;
                            if (flag8)
                            {
                                bool flag9 = !File.Exists("d:/diy_nocreateNPC.log");
                                if (flag9)
                                {
                                    File.WriteAllText("d:/diy_nocreateNPC.log", "");
                                }
                                File.AppendAllText(
                                    "d:/diy_nocreateNPC.log",
                                    string.Concat(new string[] { text, "|", text2, "\n" })
                                );
                            }
                            bool flag10 = jsonobject != null;
                            if (flag10)
                            {
                                try
                                {
                                    Tools.instance.getPlayer();
                                    JSONObject jsonobject3 = new JSONObject();
                                    jsonobject3.SetField("id", 100000 + i3);
                                    JSONObject jsonobject4 = new JSONObject();
                                    jsonobject4.SetField("StatusId", 1);
                                    jsonobject4.SetField("StatusTime", 60000);
                                    jsonobject3.SetField("Status", jsonobject4);
                                    jsonobject3.SetField("isImportant", false);
                                    jsonobject3.SetField("Name", npcname);
                                    jsonobject3.SetField("IsTag", false);
                                    jsonobject3.SetField("FirstName", "");
                                    jsonobject3.SetField(
                                        "face",
                                        jsonData.instance.AvatarJsonData[l]["face"].I
                                    );
                                    jsonobject3.SetField(
                                        "fightFace",
                                        jsonData.instance.AvatarJsonData[l]["fightFace"].I
                                    );
                                    int random;
                                    do
                                    {
                                        random = __instance.getRandom(
                                            jsonobject["NPCTag"][0].I,
                                            jsonobject["NPCTag"][1].I
                                        );
                                    } while (
                                        random <= 0
                                        || random == 2
                                        || random == 3
                                        || random == 22
                                        || random == 23
                                    );
                                    jsonobject3.SetField(
                                        "XingGe",
                                        __instance.getRandomXingGe(
                                            jsonData.instance.NPCTagDate[random.ToString()][
                                                "zhengxie"
                                            ].I
                                        )
                                    );
                                    jsonobject3.SetField("NPCTag", random);
                                    jsonobject3.SetField("ActionId", 1);
                                    jsonobject3.SetField("IsKnowPlayer", false);
                                    jsonobject3.SetField(
                                        "HuaShenLingYu",
                                        jsonobject["HuaShenLingYu"].I
                                    );
                                    jsonobject3.SetField("QingFen", 0);
                                    jsonobject3.SetField("CyList", JSONObject.arr);
                                    jsonobject3.SetField("TuPoMiShu", JSONObject.arr);
                                    jsonobject3.SetField(
                                        "Title",
                                        ToolsEx.ToCN(jsonData.instance.AvatarJsonData[l]["Title"].Str)
                                    );
                                    jsonobject3.SetField("ChengHaoID", num3);
                                    jsonobject3.SetField("GongXian", 0);
                                    jsonobject3.SetField(
                                        "SexType",
                                        jsonData.instance.AvatarJsonData[l]["SexType"].I
                                    );
                                    jsonobject3.SetField("AvatarType", i5);
                                    jsonobject3.SetField("Level", i4);
                                    jsonobject3.SetField("WuDaoValue", 0);
                                    jsonobject3.SetField("WuDaoValueLevel", 0);
                                    jsonobject3.SetField("EWWuDaoDian", 0);
                                    jsonobject3.SetField("IsNeedHelp", false);
                                    jsonobject3.SetField(
                                        "HP",
                                        jsonData.instance.AvatarJsonData[l]["HP"].I
                                    );
                                    jsonobject3.SetField(
                                        "dunSu",
                                        jsonData.instance.AvatarJsonData[l]["dunSu"].I
                                    );
                                    jsonobject3.SetField(
                                        "ziZhi",
                                        jsonData.instance.AvatarJsonData[l]["ziZhi"].I
                                    );
                                    jsonobject3.SetField(
                                        "wuXin",
                                        jsonData.instance.AvatarJsonData[l]["wuXin"].I
                                    );
                                    jsonobject3.SetField(
                                        "shengShi",
                                        jsonData.instance.AvatarJsonData[l]["shengShi"].I
                                    );
                                    jsonobject3.SetField("shaQi", 0);
                                    int num5 = i4 * 200 + 200;
                                    bool flag11 =
                                        jsonData.instance.AvatarJsonData[l]["shouYuan"].I < num5;
                                    if (flag11)
                                    {
                                        jsonobject3.SetField("shouYuan", num5);
                                    }
                                    else
                                    {
                                        jsonobject3.SetField(
                                            "shouYuan",
                                            jsonData.instance.AvatarJsonData[l]["shouYuan"].I + 200
                                        );
                                    }
                                    jsonobject3.SetField(
                                        "age",
                                        jsonData.instance.AvatarJsonData[l]["age"].I * 12
                                    );
                                    jsonobject3.SetField("exp", 15000);
                                    bool flag12 = i4 <= 14;
                                    if (flag12)
                                    {
                                        jsonobject3.SetField(
                                            "NextExp",
                                            jsonData.instance.NPCChuShiShuZiDate[(i4 + 1).ToString()][
                                                "xiuwei"
                                            ].I
                                        );
                                    }
                                    else
                                    {
                                        jsonobject3.SetField("NextExp", 0);
                                    }
                                    jsonobject3.SetField("equipList", new JSONObject());
                                    bool flag13 =
                                        jsonData.instance.AvatarJsonData[l]["skills"].Count == 7
                                        && jsonData.instance.AvatarJsonData[l]["skills"][0].I == 1
                                        && jsonData.instance.AvatarJsonData[l]["skills"][1].I == 201
                                        && jsonData.instance.AvatarJsonData[l]["skills"][2].I == 101
                                        && jsonData.instance.AvatarJsonData[l]["skills"][3].I == 301
                                        && jsonData.instance.AvatarJsonData[l]["skills"][4].I == 401
                                        && jsonData.instance.AvatarJsonData[l]["skills"][5].I == 501
                                        && jsonData.instance.AvatarJsonData[l]["skills"][6].I == 504
                                        && jsonData.instance.AvatarJsonData[l]["staticSkills"].Count
                                            <= 1;
                                    if (flag13)
                                    {
                                        jsonobject3.SetField("skills", jsonobject["skills"]);
                                        jsonobject3.SetField(
                                            "staticSkills",
                                            jsonobject["staticSkills"]
                                        );
                                    }
                                    else
                                    {
                                        jsonobject3.SetField(
                                            "skills",
                                            jsonData.instance.AvatarJsonData[l]["skills"]
                                        );
                                        jsonobject3.SetField(
                                            "staticSkills",
                                            jsonData.instance.AvatarJsonData[l]["staticSkills"]
                                        );
                                    }
                                    jsonobject3.SetField("JinDanType", jsonobject["JinDanType"]);
                                    jsonobject3.SetField("LingGen", jsonobject["LingGen"]);
                                    jsonobject3.SetField("equipWeapon", jsonobject["equipWeapon"].I);
                                    jsonobject3.SetField(
                                        "equipClothing",
                                        jsonobject["equipClothing"].I
                                    );
                                    jsonobject3.SetField("equipRing", jsonobject["equipRing"].I);
                                    jsonobject3.SetField("Type", jsonobject["Type"]);
                                    jsonobject3.SetField("LiuPai", jsonobject["LiuPai"]);
                                    jsonobject3.SetField("MenPai", jsonobject["MengPai"]);
                                    jsonobject3.SetField(
                                        "equipWeaponPianHao",
                                        jsonobject["equipWeapon"]
                                    );
                                    jsonobject3.SetField(
                                        "equipWeapon2PianHao",
                                        jsonobject["equipWeapon"]
                                    );
                                    jsonobject3.SetField(
                                        "equipClothingPianHao",
                                        jsonobject["equipClothing"]
                                    );
                                    jsonobject3.SetField("equipRingPianHao", jsonobject["equipRing"]);
                                    jsonobject3.SetField("yuanying", jsonobject["yuanying"]);
                                    jsonobject3.SetField("canjiaPaiMai", jsonobject["canjiaPaiMai"].I);
                                    jsonobject3.SetField("paimaifenzu", jsonobject["paimaifenzu"]);
                                    jsonobject3.SetField("wudaoType", jsonobject["wudaoType"]);
                                    jsonobject3.SetField(
                                        "xiuLianSpeed",
                                        __instance.getXiuLianSpeed(
                                            jsonobject["staticSkills"],
                                            (float)jsonobject3["ziZhi"].I
                                        )
                                    );
                                    jsonobject3.SetField(
                                        "MoneyType",
                                        jsonData.instance.AvatarJsonData[l]["MoneyType"].I
                                    );
                                    jsonobject3.SetField("IsRefresh", 0);
                                    jsonobject3.SetField("dropType", 0);
                                    jsonobject3.SetField("wudaoType", jsonobject["wudaoType"]);
                                    jsonobject3.SetField("XinQuType", jsonobject["XinQuType"]);
                                    jsonobject3.SetField("gudingjiage", 0);
                                    jsonobject3.SetField("sellPercent", 0);
                                    jsonobject3.SetField("useItem", new JSONObject());
                                    jsonobject3.SetField("NoteBook", new JSONObject());
                                    __instance.SetNpcWuDao(i4, jsonobject["wudaoType"].I, jsonobject3);
                                    __instance.UpNpcWuDaoByTag(jsonobject3["NPCTag"].I, jsonobject3);
                                    bool flag14 = num4 > 0;
                                    if (flag14)
                                    {
                                        jsonobject3.SetField("mybangding", num4);
                                    }
                                    jsonData.instance.AvatarJsonData.SetField(
                                        (100000 + i3).ToString(),
                                        jsonobject3
                                    );
                                }
                                catch (Exception ex)
                                {
                                    bool flag15 = !File.Exists("c:/diy_firstCreateNpcs.log");
                                    if (flag15)
                                    {
                                        File.WriteAllText("c:/diy_firstCreateNpcs.log", "");
                                    }
                                    File.AppendAllText(
                                        "c:/diy_firstCreateNpcs.log",
                                        string.Concat(
                                            new object[]
                                            {
                                            ex.Message,
                                            "\n",
                                            ex.Source,
                                            "\n",
                                            ex.TargetSite,
                                            "\n",
                                            ex.StackTrace.Substring(
                                                ex.StackTrace.LastIndexOf("\\") + 1,
                                                ex.StackTrace.Length
                                                    - ex.StackTrace.LastIndexOf("\\")
                                                    - 1
                                            ),
                                            "\n",
                                            i3,
                                            "\n",
                                            jsonobject.Print(false),
                                            "\n\n\n"
                                            }
                                        )
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_FirstCreateNpcs_Postfix(NPCFactor __instance)
        {
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {
                List<string> listItem = new List<string>();
                List<List<string>> validNPCLeiXingDate = new List<List<string>>{
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem,
                    listItem
                };
                for (int i = 0; i < jsonData.instance.NPCLeiXingDate.Count; i++)
                {
                    if (jsonData.instance.NPCLeiXingDate[i]["NPCTag"].Count == 2)
                    {
                        int i2 = jsonData.instance.NPCLeiXingDate[i]["Level"].I;
                        validNPCLeiXingDate[i2].Add(jsonData.instance.NPCLeiXingDate[i].ToString());
                    }
                }

                List<string> npcImportantDateIdList = new List<string>();
                List<string> avatarJsonDataNameList = new List<string>();
                for (int i = 0; i < jsonData.instance.NPCImportantDate.Count; i++)
                {
                    npcImportantDateIdList.Add(jsonData.instance.NPCImportantDate[j]["id"].I.ToString());
                    avatarJsonDataNameList.Add(
                        ToolsEx.ToCN(
                            jsonData.instance.AvatarJsonData[
                                jsonData.instance.NPCImportantDate[j]["id"].I.ToString()
                            ]["Name"].Str
                        )
                    );
                }

                List<int> npcChenghaoDataIdList = new List<int>();
                List<string> npcChenghaoDataChenghaoList = new List<string>();
                List<string> npcChenghaoNPCTypeList = new List<string>();
                for (int k = 0; k < jsonData.instance.NPCChengHaoData.Count; k++)
                {
                    npcChenghaoDataIdList.Add(jsonData.instance.NPCChengHaoData[k]["id"].I);
                    npcChenghaoDataChenghaoList.Add(ToolsEx.ToCN(jsonData.instance.NPCChengHaoData[k]["ChengHao"].Str));
                    npcChenghaoNPCTypeList.Add(jsonData.instance.NPCChengHaoData[k]["NPCType"].I.ToString());
                }

                //Sort by cultivation level if prioritizeHigherCultivationLevelNPCSpawn enabled
                var npcData = MCSAllNPCsSpawn.prioritizeHigherCultivationLevelNPCSpawn.Value ? JSONObject(jsonData.instance.AvatarJsonData.OrderByDescending(dict => dict["Level"]).ToList()) : jsonData.instance.AvatarJsonData;

                int npcSpawnAmount = MCSAllNPCsSpawn.maxSpawnCount.Value < jsonData.instance.AvatarJsonData.Count ? MCSAllNPCsSpawn.maxSpawnCount.Value : jsonData.instance.AvatarJsonData.Count;

                for (int i = 0; i < npcSpawnAmount; i++)
                {
                    string npcIdString = npcData[i]["id"].I.ToString();
                    int npcId = npcData[i]["id"].I;
                    string npcName = ToolsEx.ToCN(npcData[i]["Name"].Str);
                    string npcTitle = ToolsEx.ToCN(npcData[i]["Title"].Str);
                    int npcAvatarType = npcData[i]["AvatarType"].I;
                    int npcChenghaoIdx = npcChenghaoDataChenghaoList.FindIndex((string xxt) => xxt == npcTitle);

                    bool hasName = npcName && npcName != "";
                    bool hasCorrespondingName = avatarJsonDataNameList.FindIndex((string xxt) => xxt == npcName) <= -1;
                    bool validAvatarType = MCSAllNPCsSpawn.onlyHumans.Value ? npcAvatarType == 1 : true;
                    bool passesMinMoney = npcData[i]["MoneyType"].I >= MCSAllNPCsSpawn.spawnMinMoney.Value;
                    bool passesMaxMoney = npcData[i]["MoneyType"].I <= MCSAllNPCsSpawn.spawnMaxMoney.Value;
                    bool passesMinCultivation = npcData[i]["Level"].I >= MCSAllNPCsSpawn.spawnMinCultivationLevel.Value;
                    bool passesMaxCultivation = npcData[i]["Level"].I <= MCSAllNPCsSpawn.spawnMaxCultivationLevel.Value;

                    bool passesChecks = hasName && hasCorrespondingName && validAvatarType && passesMinMoney && passesMaxMoney && passesMinCultivation && passesMaxCultivation;

                    if (passesChecks)
                    {
                        int npcChenghaoId;
                        string npcChenghaoNPCType;
                        int num4;
                        if (npcChenghaoIdx > -1)
                        {
                            npcChenghaoId = npcChenghaoDataIdList[npcChenghaoIdx];
                            npcChenghaoNPCType = npcChenghaoNPCTypeList[npcChenghaoIdx];
                            num4 = 0;
                        }
                        else
                        {
                            npcChenghaoId = 418;
                            npcChenghaoNPCType = "10";
                            num4 = i3;
                        }
                    }
                }
            }
        }

        public static MCSAllNPCsSpawn Inst;
        public static ConfigEntry<bool> enableAllNPCsSpawn;
        public static ConfigEntry<int> maxSpawnCount;
        public static ConfigEntry<bool> prioritizeHigherCultivationLevelNPCSpawn;
        public static ConfigEntry<bool> onlyHumans;
        public static ConfigEntry<int> spawnMinMoney; //Greater than or equals
        public static ConfigEntry<int> spawnMaxMoney; //Less than or equals
        public static ConfigEntry<int> spawnMinCultivationLevel; //Greater than or equals
        public static ConfigEntry<int> spawnMaxCultivationLevel; //Less than or equals
        public static ConfigEntry<double> npcPowerIncreaseAmount;
    }
}
