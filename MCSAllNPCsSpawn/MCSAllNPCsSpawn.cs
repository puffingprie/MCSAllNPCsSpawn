using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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

        private static void WriteToShittyLog(string message, bool createNewOrOverwrite = false)
        {
            if (createNewOrOverwrite)
            {
                File.WriteAllText(
                    @"C:\Users\Shadow\Documents\MiChangSheng\AllNPCsSpawn\Log.log",
                    string.Concat("[", DateTime.Now.ToString("HH:mm:ss.ffff"), "]: ", message, "\n")
                );
            }
            else
            {
                File.AppendAllText(
                    @"C:\Users\Shadow\Documents\MiChangSheng\AllNPCsSpawn\Log.log",
                    string.Concat("[", DateTime.Now.ToString("HH:mm:ss.ffff"), "]: ", message, "\n")
                );
            }
        }

        private void Start()
        {
            MCSAllNPCsSpawn.Inst = this;

            Logger.LogInfo("===================MCSAllNPCsSpawn=====================");
            Logger.LogInfo(
                "Starting Write to Shitty Log: C:\\Users\\Shadow\\Documents\\MiChangSheng\\AllNPCsSpawn\\Log.log"
            );
            Logger.LogInfo("=======================================================");
            // Create Shitty Log
            WriteToShittyLog("MCSAllNPCsSpawn Log", true);

            MCSAllNPCsSpawn.enableAllNPCsSpawn = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "EnableAllNPCsSpawning",
                true,
                "Enable this mod (enable all npcs spawning)"
            );
            MCSAllNPCsSpawn.maxSpawnCount = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "MaxSpawnCount",
                400,
                "Controls the max amount of extra NPCs that spawn - Increase this number at your own risk. Personally I have a potato of a computer and it's always entertaining when I can barely move on the map because there are 80,000 NPCs."
            );
            MCSAllNPCsSpawn.createRandomNamesWhenSpawning = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "CreateRandomNamesWhenSpawning",
                false,
                "Whether to create random names for NPCs when they spawn. If false, NPCs will spawn with their default names."
            );
            MCSAllNPCsSpawn.spawnImportantNPCs = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "SpawnImportantNPCs",
                false,
                "Whether to spawn important NPCs (turn this off usually to avoid disruptions to storyline)."
            );
            MCSAllNPCsSpawn.useNPCFactoryInitValuesToSpawn = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "UseNPCFactoryInitValuesToSpawn",
                false,
                "Whether to use NPCFactory init values (NPCChuShiShuZiDate.json) to spawn NPCs. If false, NPCs will use values from default AvatarJsonData.json"
            );
            MCSAllNPCsSpawn.minNPCLifeSpan = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "MinNPCLifeSpan",
                120,
                "Minimum NPC lifespan when initially spawning"
            );
            MCSAllNPCsSpawn.onlyHumans = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "onlyHumans",
                true,
                "Only Humans Spawn"
            );
            MCSAllNPCsSpawn.spawnMinMoney = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMinMoney",
                0,
                "Limit NPC spawning to NPCs withh AT LEAST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Greater than or equals"
            );
            MCSAllNPCsSpawn.spawnMaxMoney = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMaxMoney",
                11,
                "Limit NPC spawning to NPCs withh AT MOST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Less than or equals"
            );
            MCSAllNPCsSpawn.spawnMinCultivationLevel = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMinCultivationLevel",
                1,
                "Limit NPC spawning to NPCs withh AT LEAST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Greater than or equals"
            );
            MCSAllNPCsSpawn.spawnMaxCultivationLevel = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMaxCultivationLevel",
                15,
                "Limit NPC spawning to NPCs withh AT MOST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Less than or equals"
            );

            Harmony.CreateAndPatchAll(typeof(MCSAllNPCsSpawn), null);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_FirstCreateNpcs_Postfix_New(NPCFactory __instance)
        {
            System.Random randomizer = new System.Random(); //Used for randomizing stuff in code
            Dictionary<int, List<JSONObject>> npcChengHaos =
                new Dictionary<int, List<JSONObject>>(); //Used for storing NPC ChengHao data
            npcChengHaos.Clear(); //An apple a day makes the doctor go away
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {
                parseNPCChengHao();
                int numNPCsToSpawn = maxSpawnCount.Value;
                // Create a corresponding NPCLeixingDate list for each NPC to create
                // for (int i = 0; i < numNPCsToSpawn; i++)
                // {
                //     int idx = i;
                //     if (i >= jsonData.instance.NPCLeiXingDate.Count)
                //     {
                //         idx = idx % jsonData.instance.NPCLeiXingDate.Count;
                //     }

                // }
                for (int i = 0; i < numNPCsToSpawn; i++)
                {
                    int idx = i;
                    if (i >= jsonData.instance.AvatarJsonData.Count)
                    {
                        idx = idx % jsonData.instance.AvatarJsonData.Count;
                    }
                    WriteToShittyLog("Currently on loop: ");
                    WriteToShittyLog(i.ToString());
                    if (getAvatarValidity(idx, spawnImportantNPCs.Value))
                    {
                        customCreateNpcs(idx, i);
                    }
                }
            }
            // max inclusive because getRandom() is inclusive...my head hurts. i'm a terrible coder.
            int generateRandomInt(int min = 1, int max = 10)
            {
                return __instance.getRandom(min, max);
            }
            int generateRandomIntFromList(List<int> list)
            {
                return list[generateRandomInt(0, list.Count - 1)];
            }
            int generateRandomIntFromRange(List<int> rangeList, int step = 1)
            {
                if (rangeList[0] >= rangeList[1])
                {
                    rangeList.Reverse();
                }
                List<int> newRangeList = new List<int>(rangeList);
                for (int i = rangeList[0] + 1; i < rangeList.Last(); i += step)
                {
                    newRangeList.Add(i);
                }
                return generateRandomIntFromList(newRangeList); //Uhhh, too lazy for any kinf of error handling or edge casing. I don't give a shit any more.
            }
            int randomizeStat(int stat, int min = 1, int max = 10, double powerIncreaseAmouunt = 0)
            {
                int randomNumber = generateRandomInt(min, max);
                if (powerIncreaseAmouunt > 0)
                {
                    return (int)(stat * randomNumber * powerIncreaseAmouunt);
                }
                else
                {
                    return stat * randomNumber;
                }
            }
            bool getAvatarValidity(int idx, bool shouldSpawnImportantNPCs = true)
            {
                try
                {
                    if (
                        !shouldSpawnImportantNPCs
                        && jsonData.instance.NPCImportantDate.HasField(idx.ToString())
                    )
                    {
                        return false;
                    }
                    if (
                        onlyHumans.Value
                        && jsonData.instance.AvatarJsonData[idx]["AvatarType"].I != 1
                    )
                    {
                        return false;
                    }
                    int avatarMoneyType = jsonData.instance.AvatarJsonData[idx]["MoneyType"].I;
                    if (
                        avatarMoneyType < spawnMinMoney.Value
                        || avatarMoneyType > spawnMaxMoney.Value
                    )
                    {
                        return false;
                    }
                    int avatarCultivationLevel = jsonData.instance.AvatarJsonData[idx]["Level"].I;
                    if (
                        avatarCultivationLevel < spawnMinCultivationLevel.Value
                        || avatarCultivationLevel > spawnMaxCultivationLevel.Value
                    )
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    WriteToShittyLog(ex.ToString());
                }
                return true;
            }
            //Psuedo random name generator
            string createRandomName(int idx, bool genderMatters = true)
            {
                string name = jsonData.instance.AvatarJsonData[idx]["Name"].Str;
                int randomNum = generateRandomInt(0, jsonData.instance.AvatarJsonData.Count - 1);
                if (genderMatters)
                {
                    while (
                        jsonData.instance.AvatarJsonData[idx]["SexType"].I
                        != jsonData.instance.AvatarJsonData[randomNum]["SexType"].I
                    )
                    {
                        randomNum = generateRandomInt(
                            0,
                            jsonData.instance.AvatarJsonData.Count - 1
                        );
                    }
                }
                string randomName = jsonData.instance.AvatarJsonData[randomNum]["Name"].Str;
                // Take surname of `name` and firstname of `randomName`
                // Make sure there's a valid name and randomName? If not just leave name as original.
                if (name.Length >= 1 && randomName.Length >= 1)
                {
                    if (name.Length >= 4)
                    {
                        name = name.Substring(0, 2) + randomName.Substring(2);
                    }
                    else
                    {
                        name = name.Substring(0, 1) + randomName.Substring(1);
                    }
                }
                return ToolsEx.ToCN(name);
            }
            Dictionary<string, int> getRandomNPCTagAndXingge(int idx, List<int> validNPCTags = null)
            {
                List<int> defaultNPCTags = new List<int>()
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    21,
                    22,
                    23,
                    24,
                    25,
                    26,
                    27,
                    31,
                    32,
                    33,
                    34
                };
                validNPCTags = validNPCTags ?? defaultNPCTags;

                //Turn into a range of numbers instead of just 2 numbers
                if (validNPCTags.Count() == 2)
                {
                    for (int i = validNPCTags[0]; i < validNPCTags[1]; i++)
                    {
                        if (defaultNPCTags.Contains(i))
                        {
                            validNPCTags.Add(i);
                        }
                    }
                }

                int randomNPCTag = generateRandomIntFromList(validNPCTags);
                int randomZhengxie = jsonData.instance.NPCTagDate[randomNPCTag.ToString()][
                    "zhengxie"
                ].I;
                int randomXingge = __instance.getRandomXingGe(randomZhengxie);
                return new Dictionary<string, int>
                {
                    { "RandomNPCTag", randomNPCTag },
                    { "RandomXingge", randomXingge }
                };
            }
            void parseNPCChengHao()
            {
                for (int i = 0; i < jsonData.instance.NPCChengHaoData.Count; i++)
                {
                    int npcType = jsonData.instance.NPCChengHaoData[i]["NPCType"].I;
                    if (!npcChengHaos.ContainsKey(npcType))
                    {
                        npcChengHaos.Add(
                            npcType,
                            new List<JSONObject>() { jsonData.instance.NPCChengHaoData[i] }
                        );
                    }
                    else
                    {
                        List<JSONObject> newNPCTypeChengHaoList = npcChengHaos[npcType];
                        newNPCTypeChengHaoList.Add(jsonData.instance.NPCChengHaoData[i]);
                        npcChengHaos[npcType] = newNPCTypeChengHaoList;
                    }
                }
            }
            Dictionary<string, dynamic> findAndGetNewNPCChengHaoId(int npcType, int npcLevel)
            {
                if (npcChengHaos.ContainsKey(npcType))
                {
                    for (int i = 0; i < npcChengHaos[npcType].Count; i++)
                    {
                        JSONObject chengHao = npcChengHaos[npcType][i];
                        if (npcLevel >= chengHao["Level"].I && npcLevel <= chengHao["Level"].I)
                        {
                            return new Dictionary<string, dynamic>
                            {
                                { "ChengHaoId", chengHao["id"].I },
                                { "ChengHao", chengHao["ChengHao"].Str }
                            };
                        }
                    }
                }
                //418 is 宁州散修
                return new Dictionary<string, dynamic>
                {
                    { "ChengHaoId", 418 },
                    { "ChengHao", "宁州散修" }
                };
            }
            void customCreateNpcs(int idx, int loopIdx)
            {
                JSONObject newNPC = new JSONObject();
                try
                {
                    int leiXingIdx = loopIdx + 1;
                    if (leiXingIdx > jsonData.instance.NPCLeiXingDate.Count)
                    {
                        leiXingIdx = leiXingIdx % jsonData.instance.NPCLeiXingDate.Count;
                    }
                    JSONObject newNPCLeiXingJson = jsonData.instance.NPCLeiXingDate[leiXingIdx];

                    int avatarJsonDataId = jsonData.instance.AvatarJsonData[idx]["id"].I;
                    newNPC.SetField("id", 100000 + loopIdx); // Add 100,000 so there are definitely no conflicts with the original NPCs, hopefully...

                    JSONObject newNPCStatusJson = new JSONObject();
                    newNPCStatusJson.SetField("StatusId", 1);
                    newNPCStatusJson.SetField("StatusTime", 60000);
                    newNPC.SetField("Status", newNPCStatusJson);

                    if (createRandomNamesWhenSpawning.Value)
                    {
                        newNPC.SetField("Name", createRandomName(idx));
                    }
                    else
                    {
                        newNPC.SetField("Name", jsonData.instance.AvatarJsonData[idx]["Name"].Str);
                    }

                    newNPC.SetField("IsTag", false);
                    newNPC.SetField("FirstName", "");
                    newNPC.SetField("face", jsonData.instance.AvatarJsonData[idx]["face"].I);
                    newNPC.SetField(
                        "fightFace",
                        jsonData.instance.AvatarJsonData[idx]["fightFace"].I
                    );
                    newNPC.SetField("isImportant", false);

                    Dictionary<string, int> randomNPCTagAndXingge = getRandomNPCTagAndXingge(
                        idx,
                        new List<int>
                        {
                            newNPCLeiXingJson["NPCTag"][0].I,
                            newNPCLeiXingJson["NPCTag"][1].I
                        }
                    );
                    newNPC.SetField("NPCTag", randomNPCTagAndXingge["RandomNPCTag"]);
                    newNPC.SetField("XingGe", randomNPCTagAndXingge["RandomXingge"]);
                    newNPC.SetField("ActionId", 1);
                    newNPC.SetField("IsKnowPlayer", false);
                    newNPC.SetField("QingFen", 0);
                    newNPC.SetField("CyList", JSONObject.arr);
                    newNPC.SetField("TuPoMiShu", JSONObject.arr);

                    Dictionary<string, dynamic> newNPCChengHao = findAndGetNewNPCChengHaoId(
                        newNPCLeiXingJson["Type"].I,
                        newNPCLeiXingJson["Level"].I
                    );
                    newNPC.SetField("ChengHaoID", newNPCChengHao["ChengHaoId"]);
                    newNPC.SetField("Title", newNPCChengHao["ChengHao"]);
                    newNPC.SetField("GongXian", 0);
                    newNPC.SetField("SexType", newNPCLeiXingJson["SexType"].I);
                    newNPC.SetField("Type", newNPCLeiXingJson["Type"].I);
                    newNPC.SetField("LiuPai", newNPCLeiXingJson["LiuPai"].I);
                    newNPC.SetField("MenPai", newNPCLeiXingJson["MengPai"].I); //到底为什么拼对真的很不懂，你这样我很头痛欸
                    newNPC.SetField(
                        "AvatarType",
                        jsonData.instance.AvatarJsonData[idx]["AvatarType"].I
                    );

                    int newNPCLevel = newNPCLeiXingJson["Level"].I;
                    newNPC.SetField("Level", newNPCLevel);
                    newNPC.SetField("WuDaoValue", 0);
                    newNPC.SetField("WuDaoValueLevel", 0);
                    newNPC.SetField("EWWuDaoDian", 0);
                    newNPC.SetField("shaQi", 0);

                    if (newNPCLevel <= 14)
                    {
                        newNPC.SetField(
                            "NextExp",
                            jsonData.instance.NPCChuShiShuZiDate[newNPCLevel + 1]["xiuwei"].I
                        );
                    }
                    else
                    {
                        newNPC.SetField("NextExp", 0);
                    }

                    newNPC.SetField("equipWeapon", 0);
                    newNPC.SetField("equipClothing", 0);
                    newNPC.SetField("equipRing", 0);
                    newNPC.SetField("equipWeaponPianHao", newNPCLeiXingJson["equipWeapon"]);
                    newNPC.SetField("equipWeapon2PianHao", newNPCLeiXingJson["equipWeapon"]);
                    newNPC.SetField("equipClothingPianHao", newNPCLeiXingJson["equipClothing"]);
                    newNPC.SetField("equipRingPianHao", newNPCLeiXingJson["equipRing"]);
                    newNPC.SetField("equipList", new JSONObject());
                    newNPC.SetField("LingGen", newNPCLeiXingJson["LingGen"]);
                    newNPC.SetField("skills", newNPCLeiXingJson["skills"]);
                    newNPC.SetField("JinDanType", newNPCLeiXingJson["JinDanType"]);
                    newNPC.SetField("staticSkills", newNPCLeiXingJson["staticSkills"]);

                    if (useNPCFactoryInitValuesToSpawn.Value)
                    {
                        JSONObject initValuesJson = jsonData.instance.NPCChuShiShuZiDate[
                            newNPCLevel
                        ];
                        newNPC.SetField(
                            "HP",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["HP"][0].I,
                                    initValuesJson["HP"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "age",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["age"][0].I,
                                    initValuesJson["age"][1].I
                                }
                            )
                        );
                        int newNPCShouyuan = generateRandomIntFromRange(
                            new List<int>()
                            {
                                initValuesJson["shouYuan"][0].I,
                                initValuesJson["shouYuan"][1].I
                            }
                        );
                        if (newNPCShouyuan < minNPCLifeSpan.Value)
                        {
                            newNPC.SetField("shouYuan", minNPCLifeSpan.Value);
                        }
                        else
                        {
                            newNPC.SetField("shouYuan", newNPCShouyuan);
                        }
                        newNPC.SetField(
                            "ziZhi",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["ziZhi"][0].I,
                                    initValuesJson["ziZhi"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "wuXin",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["wuXin"][0].I,
                                    initValuesJson["wuXin"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "dunSu",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["dunSu"][0].I,
                                    initValuesJson["dunSu"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "shengShi",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["shengShi"][0].I,
                                    initValuesJson["shengShi"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "MoneyType",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initValuesJson["MoneyType"][0].I,
                                    initValuesJson["MoneyType"][1].I
                                }
                            )
                        );
                    }
                    else
                    {
                        newNPC.SetField("HP", jsonData.instance.AvatarJsonData[idx]["HP"].I);
                        newNPC.SetField("age", jsonData.instance.AvatarJsonData[idx]["age"].I);
                        if (
                            jsonData.instance.AvatarJsonData[idx]["shouYuan"].I
                            < minNPCLifeSpan.Value
                        )
                        {
                            newNPC.SetField("shouYuan", minNPCLifeSpan.Value);
                        }
                        else
                        {
                            newNPC.SetField(
                                "shouYuan",
                                jsonData.instance.AvatarJsonData[idx]["shouYuan"].I
                            );
                        }
                        newNPC.SetField("ziZhi", jsonData.instance.AvatarJsonData[idx]["ziZhi"].I);
                        newNPC.SetField("wuXin", jsonData.instance.AvatarJsonData[idx]["wuXin"].I);
                        newNPC.SetField("dunSu", jsonData.instance.AvatarJsonData[idx]["dunSu"].I);
                        newNPC.SetField(
                            "shengShi",
                            jsonData.instance.AvatarJsonData[idx]["shengShi"].I
                        );
                        newNPC.SetField(
                            "MoneyType",
                            jsonData.instance.AvatarJsonData[idx]["MoneyType"].I
                        );
                    }

                    newNPC.SetField("IsNeedHelp", false);
                    newNPC.SetField("isTanChaUnlock", false);
                    newNPC.SetField("exp", 15000);
                    newNPC.SetField(
                        "xiuLianSpeed",
                        __instance.getXiuLianSpeed(newNPC["staticSkills"], (float)newNPC["ziZhi"].I)
                    );
                    newNPC.SetField("yuanying", newNPCLeiXingJson["yuanying"].I);
                    newNPC.SetField("IsRefresh", 0);
                    newNPC.SetField("dropType", 0);
                    newNPC.SetField("canjiaPaiMai", newNPCLeiXingJson["canjiaPaiMai"].I);
                    newNPC.SetField("paimaifenzu", newNPCLeiXingJson["paimaifenzu"]);
                    newNPC.SetField("wudaoType", newNPCLeiXingJson["wudaoType"].I);
                    newNPC.SetField(
                        "XinQuType",
                        jsonData.instance.AvatarJsonData[idx]["XinQuType"].I
                    );
                    newNPC.SetField("gudingjiage", 0);
                    newNPC.SetField("sellPercent", 0);
                    newNPC.SetField("useItem", new JSONObject());
                    newNPC.SetField("NoteBook", new JSONObject());
                    __instance.SetNpcWuDao(newNPC["Level"].I, newNPC["wudaoType"].I, newNPC);
                    __instance.UpNpcWuDaoByTag(randomNPCTagAndXingge["RandomNPCTag"], newNPC);

                    //Add it to the JSON!
                    jsonData.instance.AvatarJsonData.SetField(newNPC["id"].Str, newNPC);
                    WriteToShittyLog(newNPC.Print(true));
                }
                catch (Exception ex)
                {
                    WriteToShittyLog(
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
                                    ex.StackTrace.Length - ex.StackTrace.LastIndexOf("\\") - 1
                                ),
                                "\n",
                                newNPC.Print(false),
                                "\n\n"
                            }
                        )
                    );
                }
            }
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_FirstCreateNpcs_Postfix_Old(NPCFactory __instance)
        {
            bool flag = MCSAllNPCsSpawn.enableAllNPCsSpawn.Value;
            if (flag)
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
                    bool flag2 = jsonData.instance.NPCLeiXingDate[i]["NPCTag"].Count == 2;
                    if (flag2)
                    {
                        int i2 = jsonData.instance.NPCLeiXingDate[i]["Level"].I;
                        list[i2].Add(jsonData.instance.NPCLeiXingDate[i].ToString());
                    }
                }
                List<string> list2 = new List<string>();
                List<string> list3 = new List<string>();
                for (int j = 0; j < jsonData.instance.NPCImportantDate.Count; j++)
                {
                    list2.Add(jsonData.instance.NPCImportantDate[j]["id"].I.ToString());
                    list3.Add(
                        ToolsEx.ToCN(
                            jsonData.instance.AvatarJsonData[
                                jsonData.instance.NPCImportantDate[j]["id"].I.ToString()
                            ]["Name"].Str
                        )
                    );
                }
                List<int> list4 = new List<int>();
                List<string> list5 = new List<string>();
                List<string> list6 = new List<string>();
                for (int k = 0; k < jsonData.instance.NPCChengHaoData.Count; k++)
                {
                    list4.Add(jsonData.instance.NPCChengHaoData[k]["id"].I);
                    list5.Add(ToolsEx.ToCN(jsonData.instance.NPCChengHaoData[k]["ChengHao"].Str));
                    list6.Add(jsonData.instance.NPCChengHaoData[k]["NPCType"].I.ToString());
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
                        int i5 = jsonData.instance.AvatarJsonData[l]["AvatarType"].I;
                        bool isValidAvatarType = MCSAllNPCsSpawn.onlyHumans.Value ? i5 == 1 : true;
                        bool passesMinMoney =
                            jsonData.instance.AvatarJsonData[l]["MoneyType"].I
                            >= MCSAllNPCsSpawn.spawnMinMoney.Value;
                        bool passesMaxMoney =
                            jsonData.instance.AvatarJsonData[l]["MoneyType"].I
                            <= MCSAllNPCsSpawn.spawnMaxMoney.Value;
                        bool passesMinCultivation =
                            jsonData.instance.AvatarJsonData[l]["Level"].I
                            >= MCSAllNPCsSpawn.spawnMinCultivationLevel.Value;
                        bool passesMaxCultivation =
                            jsonData.instance.AvatarJsonData[l]["Level"].I
                            <= MCSAllNPCsSpawn.spawnMaxCultivationLevel.Value;
                        bool passesConfigChecks =
                            isValidAvatarType
                            && passesMinMoney
                            && passesMaxMoney
                            && passesMinCultivation
                            && passesMaxCultivation;
                        bool flag5 = (
                            !(npcname != "")
                            || list3.FindIndex((string xxt) => xxt == npcname) <= -1
                        );
                        if (flag5 && passesConfigChecks)
                        {
                            int i4 = jsonData.instance.AvatarJsonData[l]["Level"].I;
                            string npctitle = ToolsEx.ToCN(
                                jsonData.instance.AvatarJsonData[l]["Title"].Str
                            );
                            int num2 = list5.FindIndex((string xxt) => xxt == npctitle);
                            bool flag6 = num2 > -1;
                            int num3;
                            string text2;
                            int num4;
                            if (flag6)
                            {
                                num3 = list4[num2];
                                text2 = list6[num2];
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
                                WriteToShittyLog("flag8");
                                WriteToShittyLog(
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
                                        ToolsEx.ToCN(
                                            jsonData.instance.AvatarJsonData[l]["Title"].Str
                                        )
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
                                            jsonData.instance.NPCChuShiShuZiDate[
                                                (i4 + 1).ToString()
                                            ]["xiuwei"].I
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
                                    jsonobject3.SetField(
                                        "equipWeapon",
                                        jsonobject["equipWeapon"].I
                                    );
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
                                    jsonobject3.SetField(
                                        "equipRingPianHao",
                                        jsonobject["equipRing"]
                                    );
                                    jsonobject3.SetField("yuanying", jsonobject["yuanying"]);
                                    jsonobject3.SetField(
                                        "canjiaPaiMai",
                                        jsonobject["canjiaPaiMai"].I
                                    );
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
                                    __instance.SetNpcWuDao(
                                        i4,
                                        jsonobject["wudaoType"].I,
                                        jsonobject3
                                    );
                                    __instance.UpNpcWuDaoByTag(
                                        jsonobject3["NPCTag"].I,
                                        jsonobject3
                                    );
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
                                    WriteToShittyLog(
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

        public static MCSAllNPCsSpawn Inst;
        public static ConfigEntry<bool> enableAllNPCsSpawn;
        public static ConfigEntry<int> maxSpawnCount;
        public static ConfigEntry<bool> createRandomNamesWhenSpawning;
        public static ConfigEntry<bool> spawnImportantNPCs;
        public static ConfigEntry<bool> useNPCFactoryInitValuesToSpawn;
        public static ConfigEntry<int> minNPCLifeSpan;

        public static ConfigEntry<bool> onlyHumans;
        public static ConfigEntry<int> spawnMinMoney; //Greater than or equals
        public static ConfigEntry<int> spawnMaxMoney; //Less than or equals
        public static ConfigEntry<int> spawnMinCultivationLevel; //Greater than or equals
        public static ConfigEntry<int> spawnMaxCultivationLevel; //Less than or equals
    }
}
