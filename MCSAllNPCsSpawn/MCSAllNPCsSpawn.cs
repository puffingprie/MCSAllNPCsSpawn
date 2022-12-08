using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            MCSAllNPCsSpawn.spawnMinCultivationLevel = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMinCultivationLevel", 1, "Limit NPC spawning to NPCs withh AT LEAST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Greater than or equals");
            MCSAllNPCsSpawn.spawnMaxCultivationLevel = base.Config.Bind<int>("MCSAllNPCsSpawnConfig", "SpawnMaxCultivationLevel", 15, "Limit NPC spawning to NPCs withh AT MOST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Less than or equals");
            MCSAllNPCsSpawn.npcPowerIncreaseAmount = base.Config.Bind<double>("MCSAllNPCsSpawnConfig", "NPCPowerIncreaseAmount", 1.0, "Not Yet Implemented - Because of the insane amount of NPCs that get spawned into the game, plugins like UniqueCream's ExtraChoicesForTalents plugin which constantly updates NPC data every tick lags the game out immensely. Thought it might be better if NPCs just got a flat increase in the beginning. Again, potato computer reasons.");

            Harmony.CreateAndPatchAll(typeof(MCSAllNPCsSpawn), null);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_FirstCreateNpcs_Postfix(NPCFactory __instance)
        {
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {
                List<string> listItem = new List<string>();
                List<List<string>> validNPCLeiXingDateSortedByLevels = new List<List<string>>{
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
                        int level = jsonData.instance.NPCLeiXingDate[i]["Level"].I;
                        validNPCLeiXingDateSortedByLevels[level].Add(jsonData.instance.NPCLeiXingDate[i].ToString());
                    }
                }

                List<string> npcImportantDateIdList = new List<string>();
                List<string> avatarJsonDataNameList = new List<string>();
                for (int i = 0; i < jsonData.instance.NPCImportantDate.Count; i++)
                {
                    npcImportantDateIdList.Add(jsonData.instance.NPCImportantDate[i]["id"].I.ToString());
                    avatarJsonDataNameList.Add(
                        ToolsEx.ToCN(
                            jsonData.instance.AvatarJsonData[
                                jsonData.instance.NPCImportantDate[i]["id"].I.ToString()
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
                // var npcData = MCSAllNPCsSpawn.prioritizeHigherCultivationLevelNPCSpawn.Value ?(jsonData.instance.AvatarJsonData.ToList() as List<string>).OrderByDescending(dict => dict["Level"]).ToList() : jsonData.instance.AvatarJsonData;
                // var npcData = from entry in jsonData.instance.AvatarJsonData.ToDictionary() orderby entry.Value["Level"].I descending select entry;

                var npcData = jsonData.instance.AvatarJsonData;

                int npcSpawnAmount = MCSAllNPCsSpawn.maxSpawnCount.Value < jsonData.instance.AvatarJsonData.Count ? MCSAllNPCsSpawn.maxSpawnCount.Value : jsonData.instance.AvatarJsonData.Count;

                for (int i = 0; i < npcSpawnAmount; i++)
                {
                    string npcIdString = npcData[i]["id"].I.ToString();
                    int npcId = npcData[i]["id"].I;
                    string npcName = ToolsEx.ToCN(npcData[i]["Name"].Str);
                    string npcTitle = ToolsEx.ToCN(npcData[i]["Title"].Str);
                    int npcAvatarType = npcData[i]["AvatarType"].I;
                    int npcChenghaoIdx = npcChenghaoDataChenghaoList.FindIndex((string xxt) => xxt == npcTitle);
                    int npcLevel = npcData[i]["Level"].I;

                    bool hasName = npcName != "";
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
                            num4 = npcId;
                        }
                        JSONObject newNPCAvatarTypeJSONObject = null;
                        for (int m = 0; m < validNPCLeiXingDateSortedByLevels[npcLevel].Count - 1; m++)
                        {
                            JSONObject jsonobject2 = new JSONObject(list[npcLevel][m].ToString());
                            bool flag7 =
                                jsonobject2["Type"].I.ToString() == text2
                                && (
                                    jsonobject2["AvatarType"].I == i5
                                    || jsonobject2["AvatarType"].I == 1
                                    || (i5 == 1 && jsonobject2["AvatarType"].I == 3)
                                );
                            if (flag7)
                            {
                                newNPCAvatarTypeJSONObject = jsonobject2;
                                break;
                            }
                        }
                        if (newNPCAvatarTypeJSONObject)
                        {
                            WriteToShittyLog(string.Concat(new string[] { text, "|", text2, "\n" }));
                        }
                        if (newNPCAvatarTypeJSONObject != null)
                        {

                            try
                            {
                                Tools.instance.getPlayer();
                                JSONObject newNPCJSONObject = new JSONObject();
                                newNPCJSONObject.SetField("id", 1000000 + npcId);
                                JSONObject newNPCStatusJSONObject = new JSONObject();
                                newNPCStatusJSONObject.SetField("StatusId", 1);
                                newNPCStatusJSONObject.SetField("StatusTime", 60000);
                                newNPCJSONObject.SetField("Status", newNPCStatusJSONObject);
                                newNPCJSONObject.SetField("isImportant", false);
                                newNPCJSONObject.SetField("Name", npcName);
                                newNPCJSONObject.SetField("IsTag", false);
                                newNPCJSONObject.SetField("FirstName", "");
                                newNPCJSONObject.SetField(
                                    "face",
                                    npcData[i]["face"].I
                                );
                                newNPCJSONObject.SetField(
                                    "fightFace",
                                    npcData[i]["fightFace"].I
                                );
                                int random;
                                do
                                {
                                    random = __instance.getRandom(
                                        newNPCAvatarTypeJSONObject["NPCTag"][0].I,
                                        newNPCAvatarTypeJSONObject["NPCTag"][1].I
                                    );
                                } while (
                                    random <= 0
                                    || random == 2
                                    || random == 3
                                    || random == 22
                                    || random == 23
                                );
                                newNPCJSONObject.SetField(
                                    "XingGe",
                                    __instance.getRandomXingGe(
                                        jsonData.instance.NPCTagDate[random.ToString()][
                                            "zhengxie"
                                        ].I
                                    )
                                );
                                newNPCJSONObject.SetField("NPCTag", random);
                                newNPCJSONObject.SetField("ActionId", 1);
                                newNPCJSONObject.SetField("IsKnowPlayer", false);
                                newNPCJSONObject.SetField("HuaShenLingYu", newNPCJSONObject["HuaShenLingYu"].I);
                                newNPCJSONObject.SetField("QingFen", 0);
                                newNPCJSONObject.SetField("CyList", JSONObject.arr);
                                newNPCJSONObject.SetField("TuPoMiShu", JSONObject.arr);
                                newNPCJSONObject.SetField("Title", ToolsEx.ToCN(npcData[i]["Title"].Str));
                                newNPCJSONObject.SetField("ChengHaoID", num3);
                                newNPCJSONObject.SetField("GongXian", 0);
                                newNPCJSONObject.SetField("SexType", npcData[i]["SexType"].I);
                                newNPCJSONObject.SetField("AvatarType", i5);
                                newNPCJSONObject.SetField("Level", i4);
                                newNPCJSONObject.SetField("WuDaoValue", 0);
                                newNPCJSONObject.SetField("WuDaoValueLevel", 0);
                                newNPCJSONObject.SetField("EWWuDaoDian", 0);
                                newNPCJSONObject.SetField("IsNeedHelp", false);
                                newNPCJSONObject.SetField("HP", npcData[i]["HP"].I);
                                newNPCJSONObject.SetField("dunSu", npcData[i]["dunSu"].I);
                                newNPCJSONObject.SetField("ziZhi", npcData[i]["ziZhi"].I);
                                newNPCJSONObject.SetField("wuXin", npcData[i]["wuXin"].I);
                                newNPCJSONObject.SetField("shengShi", npcData[i]["shengShi"].I);
                                newNPCJSONObject.SetField("shaQi", 0);
                                int num5 = i4 * 200 + 200;
                                bool flag11 = npcData[i]["shouYuan"].I < num5;
                                if (flag11)
                                {
                                    newNPCJSONObject.SetField("shouYuan", num5);
                                }
                                else
                                {
                                    newNPCJSONObject.SetField("shouYuan", npcData[i]["shouYuan"].I + 200);
                                }
                                newNPCJSONObject.SetField("age", npcData[i]["age"].I * 12);
                                newNPCJSONObject.SetField("exp", 15000);
                                bool flag12 = i4 <= 14;
                                if (flag12)
                                {
                                    newNPCJSONObject.SetField("NextExp", jsonData.instance.NPCChuShiShuZiDate[(i4 + 1).ToString()]["xiuwei"].I);
                                }
                                else
                                {
                                    newNPCJSONObject.SetField("NextExp", 0);
                                }
                                newNPCJSONObject.SetField("equipList", new JSONObject());
                                bool flag13 =
                                    npcData[i]["skills"].Count == 7
                                    && npcData[i]["skills"][0].I == 1
                                    && npcData[i]["skills"][1].I == 201
                                    && npcData[i]["skills"][2].I == 101
                                    && npcData[i]["skills"][3].I == 301
                                    && npcData[i]["skills"][4].I == 401
                                    && npcData[i]["skills"][5].I == 501
                                    && npcData[i]["skills"][6].I == 504
                                    && npcData[i]["staticSkills"].Count
                                        <= 1;
                                if (flag13)
                                {
                                    newNPCJSONObject.SetField("skills", newNPCAvatarTypeJSONObject["skills"]);
                                    newNPCJSONObject.SetField("staticSkills", newNPCAvatarTypeJSONObject["staticSkills"]);
                                }
                                else
                                {
                                    newNPCJSONObject.SetField("skills", npcData[i]["skills"]);
                                    newNPCJSONObject.SetField("staticSkills", npcData[i]["staticSkills"]);
                                }
                                newNPCJSONObject.SetField("JinDanType", newNPCAvatarTypeJSONObject["JinDanType"]);
                                newNPCJSONObject.SetField("LingGen", newNPCAvatarTypeJSONObject["LingGen"]);
                                newNPCJSONObject.SetField("equipWeapon", newNPCAvatarTypeJSONObject["equipWeapon"].I);
                                newNPCJSONObject.SetField("equipClothing", newNPCAvatarTypeJSONObject["equipClothing"].I);
                                newNPCJSONObject.SetField("equipRing", newNPCAvatarTypeJSONObject["equipRing"].I);
                                newNPCJSONObject.SetField("Type", newNPCAvatarTypeJSONObject["Type"]);
                                newNPCJSONObject.SetField("LiuPai", newNPCAvatarTypeJSONObject["LiuPai"]);
                                newNPCJSONObject.SetField("MenPai", newNPCAvatarTypeJSONObject["MengPai"]);
                                newNPCJSONObject.SetField("equipWeaponPianHao", newNPCAvatarTypeJSONObject["equipWeapon"]);
                                newNPCJSONObject.SetField("equipWeapon2PianHao", newNPCAvatarTypeJSONObject["equipWeapon"]);
                                newNPCJSONObject.SetField("equipClothingPianHao", newNPCAvatarTypeJSONObject["equipClothing"]);
                                newNPCJSONObject.SetField("equipRingPianHao", newNPCAvatarTypeJSONObject["equipRing"]);
                                newNPCJSONObject.SetField("yuanying", newNPCAvatarTypeJSONObject["yuanying"]);
                                newNPCJSONObject.SetField("canjiaPaiMai", newNPCAvatarTypeJSONObject["canjiaPaiMai"].I);
                                newNPCJSONObject.SetField("paimaifenzu", newNPCAvatarTypeJSONObject["paimaifenzu"]);
                                newNPCJSONObject.SetField("wudaoType", newNPCAvatarTypeJSONObject["wudaoType"]);
                                newNPCJSONObject.SetField(
                                    "xiuLianSpeed",
                                    __instance.getXiuLianSpeed(
                                        newNPCAvatarTypeJSONObject["staticSkills"],
                                        (float)newNPCJSONObject["ziZhi"].I
                                    )
                                );
                                newNPCJSONObject.SetField("MoneyType", npcData[i]["MoneyType"].I);
                                newNPCJSONObject.SetField("IsRefresh", 0);
                                newNPCJSONObject.SetField("dropType", 0);
                                newNPCJSONObject.SetField("wudaoType", newNPCAvatarTypeJSONObject["wudaoType"]);
                                newNPCJSONObject.SetField("XinQuType", newNPCAvatarTypeJSONObject["XinQuType"]);
                                newNPCJSONObject.SetField("gudingjiage", 0);
                                newNPCJSONObject.SetField("sellPercent", 0);
                                newNPCJSONObject.SetField("useItem", new JSONObject());
                                newNPCJSONObject.SetField("NoteBook", new JSONObject());
                                __instance.SetNpcWuDao(i4, newNPCAvatarTypeJSONObject["wudaoType"].I, newNPCJSONObject);
                                __instance.UpNpcWuDaoByTag(newNPCJSONObject["NPCTag"].I, newNPCJSONObject);
                                bool flag14 = num4 > 0;
                                if (flag14)
                                {
                                    newNPCJSONObject.SetField("mybangding", num4);
                                }
                                npcData.SetField(
                                    (100000 + i3).ToString(),
                                    newNPCJSONObject
                                );
                            }
                            catch (Exception error)
                            {
                                WriteToShittyLog(string.Concat(
                                        new object[] { error.Message, "\n", error.Source, "\n", error.TargetSite, "\n", error.StackTrace.Substring(
                                            ex.StackTrace.LastIndexOf("\\") + 1, ex.StackTrace.Length - ex.StackTrace.LastIndexOf("\\") - 1 ),
                                            "\n", i3, "\n", newNPCAvatarTypeJSONObject.Print(false), "\n\n\n" }));
                            }
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
