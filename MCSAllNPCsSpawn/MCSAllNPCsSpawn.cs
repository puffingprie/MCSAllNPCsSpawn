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
                1000,
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
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {
                parseNPCChengHao();
                WriteToShittyLog("npcChengHaos has " + npcChengHaos.Count + " entries");
                foreach (var pair in npcChengHaos)
                {
                    WriteToShittyLog($"ChengHao with key (npcType) {pair.Key}: {pair.Value}");
                    WriteToShittyLog("Getting list content...");
                    foreach (var item in pair.Value)
                    {
                        WriteToShittyLog(item.Print());
                    }
                }
                int numNPCsToSpawn = maxSpawnCount.Value;
                int numSpawnedNPCs = 0;

                while (numSpawnedNPCs < numNPCsToSpawn)
                {
                    for (int i = 0; i < jsonData.instance.AvatarJsonData.Count; i++)
                    {
                        int idx = i;
                        if (i >= jsonData.instance.AvatarJsonData.Count)
                        {
                            WriteToShittyLog(
                                "i (spawn for loop) is bigger than jsonData.instance.AvatarJsonData.Count. Reforming idx, idx was "
                                    + idx.ToString()
                            );
                            idx = idx % jsonData.instance.AvatarJsonData.Count;
                            WriteToShittyLog("idx is now " + idx.ToString());
                        }
                        WriteToShittyLog(
                            "Currently on loop: "
                                + i.ToString()
                                + "\nAlready spawned "
                                + numSpawnedNPCs.ToString()
                                + " NPCs\nTrying to get Avatar at index = "
                                + idx.ToString()
                                + ".\nAvatar's ID should be "
                                + jsonData.instance.AvatarJsonData[idx]["id"].I.ToString()
                        );
                        if (getAvatarValidity(idx, spawnImportantNPCs.Value))
                        {
                            JSONObject coolNewnPC = customCreateNpcs(idx, numSpawnedNPCs);
                            try
                            {
                                WriteToShittyLog("Trying to add NPC to AvatarJsonData...");
                                jsonData.instance.AvatarJsonData.SetField(
                                    (50000 + numSpawnedNPCs).ToString(),
                                    coolNewnPC
                                );
                                if (
                                    jsonData.instance.AvatarJsonData.HasField(
                                        (50000 + numSpawnedNPCs).ToString()
                                    )
                                )
                                {
                                    WriteToShittyLog("Successfully added NPC to AvatarJsonData!");
                                    WriteToShittyLog(coolNewnPC.Print());
                                    numSpawnedNPCs++;
                                }
                                else
                                {
                                    WriteToShittyLog("Failed to add NPC to AvatarJsonData...");
                                }
                                if (numSpawnedNPCs >= numNPCsToSpawn)
                                {
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                WriteToShittyLog("Error creating NPC: " + e.ToString());
                                WriteToShittyLog(coolNewnPC.Print());
                            }
                        }
                    }
                }

                if (jsonData.instance.AvatarJsonData.HasField("8881"))
                {
                    WriteToShittyLog("AvatarJsonData has 8881 entry, logging...");
                    WriteToShittyLog(jsonData.instance.AvatarJsonData["8881"].Print());
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
                        WriteToShittyLog("Avatar is important NPC, skipping...");
                        return false;
                    }
                    if (
                        onlyHumans.Value
                        && jsonData.instance.AvatarJsonData[idx]["AvatarType"].I != 1
                    )
                    {
                        WriteToShittyLog("Avatar is not human, skipping...");
                        return false;
                    }
                    int avatarMoneyType = jsonData.instance.AvatarJsonData[idx]["MoneyType"].I;
                    if (
                        avatarMoneyType < spawnMinMoney.Value
                        || avatarMoneyType > spawnMaxMoney.Value
                    )
                    {
                        WriteToShittyLog("Avatar's money type is out of range, skipping...");
                        return false;
                    }
                    int avatarCultivationLevel = jsonData.instance.AvatarJsonData[idx]["Level"].I;
                    if (
                        avatarCultivationLevel < spawnMinCultivationLevel.Value
                        || avatarCultivationLevel > spawnMaxCultivationLevel.Value
                    )
                    {
                        WriteToShittyLog("Avatar's cultivation level is out of range, skipping...");
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
                    if (
                        !npcChengHaos.ContainsKey(npcType)
                        && jsonData.instance.NPCChengHaoData[i]["id"].I < 500
                    ) //<500 is a shitty solution to try and not pick up Chenghaos from mods but idc
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
                        if (
                            chengHao["Level"].Count >= 2
                            && npcLevel >= chengHao["Level"][0].I
                            && npcLevel <= chengHao["Level"][1].I
                        )
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
            JSONObject customCreateNpcs(int idx, int idxToUseForLeiXingJson)
            {
                JSONObject newNPC = new JSONObject();
                try
                {
                    // WriteToShittyLog("1");
                    int leiXingIdx = idxToUseForLeiXingJson + 1;
                    if (leiXingIdx > jsonData.instance.NPCLeiXingDate.Count)
                    {
                        leiXingIdx = leiXingIdx % jsonData.instance.NPCLeiXingDate.Count;
                    }
                    JSONObject newNPCLeiXingJson = jsonData.instance.NPCLeiXingDate[leiXingIdx];
                    WriteToShittyLog("Leixing to use: " + newNPCLeiXingJson.Print());
                    WriteToShittyLog(
                        "AvatarJSON to use: " + jsonData.instance.AvatarJsonData[idx].Print()
                    );

                    int avatarJsonDataId = jsonData.instance.AvatarJsonData[idx]["id"].I;
                    newNPC.SetField("id", 50000 + idxToUseForLeiXingJson); // Add 50,000 so there are definitely no conflicts with the original NPCs, hopefully...

                    JSONObject newNPCStatusJson = new JSONObject();
                    newNPCStatusJson.SetField("StatusId", 1);
                    newNPCStatusJson.SetField("StatusTime", 60000);
                    newNPC.SetField("Status", newNPCStatusJson);
                    // WriteToShittyLog("2");
                    if (createRandomNamesWhenSpawning.Value)
                    {
                        newNPC.SetField("Name", createRandomName(idx));
                    }
                    else
                    {
                        newNPC.SetField(
                            "Name",
                            ToolsEx.ToCN(jsonData.instance.AvatarJsonData[idx]["Name"].Str)
                        );
                    }

                    newNPC.SetField("IsTag", false);
                    newNPC.SetField("FirstName", "");
                    newNPC.SetField("face", jsonData.instance.AvatarJsonData[idx]["face"].I);
                    newNPC.SetField(
                        "fightFace",
                        jsonData.instance.AvatarJsonData[idx]["fightFace"].I
                    );
                    newNPC.SetField("isImportant", false);
                    // WriteToShittyLog("3");
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
                    newNPC.SetField("IsKnowPlayer", false);
                    newNPC.SetField("QingFen", 0);
                    newNPC.SetField("CyList", JSONObject.arr);
                    newNPC.SetField("TuPoMiShu", JSONObject.arr);
                    // WriteToShittyLog("4");
                    Dictionary<string, dynamic> newNPCChengHao = findAndGetNewNPCChengHaoId(
                        newNPCLeiXingJson["Type"].I,
                        newNPCLeiXingJson["Level"].I
                    );
                    newNPC.SetField("Title", newNPCChengHao["ChengHao"]);
                    newNPC.SetField("ChengHaoID", newNPCChengHao["ChengHaoId"]);
                    newNPC.SetField("GongXian", 0);
                    newNPC.SetField("SexType", jsonData.instance.AvatarJsonData[idx]["SexType"].I);
                    newNPC.SetField("Type", newNPCLeiXingJson["Type"].I);
                    newNPC.SetField("LiuPai", newNPCLeiXingJson["LiuPai"].I);
                    newNPC.SetField("MenPai", newNPCLeiXingJson["MengPai"].I); //到底为什么拼对真的很不懂，你这样我很头痛欸
                    newNPC.SetField(
                        "AvatarType",
                        jsonData.instance.AvatarJsonData[idx]["AvatarType"].I
                    );
                    // WriteToShittyLog("5");
                    int newNPCLevel = newNPCLeiXingJson["Level"].I;
                    newNPC.SetField("Level", newNPCLevel);
                    newNPC.SetField("WuDaoValue", 0);
                    newNPC.SetField("WuDaoValueLevel", 0);
                    newNPC.SetField("EWWuDaoDian", 0);
                    newNPC.SetField("shaQi", 0);
                    // WriteToShittyLog("6");
                    if (newNPCLevel <= 14)
                    {
                        newNPC.SetField(
                            "NextExp",
                            jsonData.instance.NPCChuShiShuZiDate[newNPCLevel]["xiuwei"].I
                        );
                    }
                    else
                    {
                        newNPC.SetField("NextExp", 0);
                    }
                    // WriteToShittyLog("7");
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
                    newNPC.SetField("HuaShenLingYu", newNPCLeiXingJson["HuaShenLingYu"].I);
                    newNPC.SetField("staticSkills", newNPCLeiXingJson["staticSkills"]);
                    // WriteToShittyLog("8");
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
                    // WriteToShittyLog("9");
                    newNPC.SetField("IsNeedHelp", false);
                    newNPC.SetField("ActionId", 1);
                    newNPC.SetField("isTanChaUnlock", false);
                    newNPC.SetField("exp", 15000);
                    newNPC.SetField(
                        "xiuLianSpeed",
                        __instance.getXiuLianSpeed(newNPC["staticSkills"], (float)newNPC["ziZhi"].I)
                    );
                    WriteToShittyLog("10");
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
                    WriteToShittyLog("11");
                    newNPC.SetField("gudingjiage", 0);
                    newNPC.SetField("sellPercent", 0);
                    newNPC.SetField("useItem", new JSONObject());
                    newNPC.SetField("NoteBook", new JSONObject());
                    __instance.SetNpcWuDao(newNPC["Level"].I, newNPC["wudaoType"].I, newNPC);
                    __instance.UpNpcWuDaoByTag(randomNPCTagAndXingge["RandomNPCTag"], newNPC);

                    //Add it to the JSON!
                    // jsonData.instance.AvatarJsonData.SetField(newNPC["id"].Str, newNPC);
                    // WriteToShittyLog(newNPC.Print());
                    return newNPC;
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
                                newNPC.Print(false)
                            }
                        )
                    );
                }
                return null;
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
