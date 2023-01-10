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
        public const string modVersion = "1.0.0.2";

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
                "启动本模组 / Enable this mod"
            );
            MCSAllNPCsSpawn.maxSpawnCount = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "MaxSpawnCount",
                1000,
                "多生成的NPC，测试了一下大概2000、2500以后游戏开始想死 / Controls the max amount of extra NPCs that spawn (Around 2000 seems to be the max before game goes bonkers)"
            );
            MCSAllNPCsSpawn.useRandomNamesWhenSpawning = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "UseRandomNamesWhenSpawning",
                true,
                "NPC是否使用自定义名字列表生成 / Whether to use names from custom name list when spawning NPCs. Otherwise uses game's auto-generated names"
            );
            MCSAllNPCsSpawn.spawnImportantNPCs = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "SpawnImportantNPCs",
                false,
                "是否生成重要NPC，不想路上遇到冒牌货副角就关了吧 / Whether to spawn important NPCs (turn this off usually to avoid disruptions to storyline)"
            );
            MCSAllNPCsSpawn.normalNPCsCanUseImportantNPCSkills = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "NormalNPCsCanUseImportantNPCSkills",
                false,
                "普通NPC是否可以使用重要角色的类型（技能、悟道等）。真的真的真的不建议开，不建议开，不建议开（说三次你要开随便你） / Normal NPCs can use important Character LeiXings (Skills, WuDao, etc.) I really don't recommend you turning this on."
            );
            MCSAllNPCsSpawn.useNPCFactoryInitValuesToSpawn = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "UseNPCFactoryInitValuesToSpawn",
                true,
                "是否使用初始化数值（NPCChuShiShuZiDate.json）生成NPC，关闭就默认用AvatarJsonData.json / Whether to use NPCFactory init values (NPCChuShiShuZiDate.json) to spawn NPCs. If false, NPCs will use values from default AvatarJsonData.json"
            );
            MCSAllNPCsSpawn.minNPCLifeSpan = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "MinNPCLifeSpan",
                120,
                "控制生成NPC最短寿命，游戏里NPC任然有物品还是会给自己延长寿命 / Minimum NPC lifespan when initially spawning"
            );
            MCSAllNPCsSpawn.onlyHumans = base.Config.Bind<bool>(
                "MCSAllNPCsSpawnConfig",
                "onlyHumans",
                true,
                "只生成人类，要妖魔鬼怪的就关了吧 / Only Humans Spawn"
            );
            MCSAllNPCsSpawn.spawnMinMoney = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMinMoney",
                0,
                "控制生成最少富有度 （大于等于） / Limit NPC spawning to NPCs with AT LEAST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Greater than or equals"
            );
            MCSAllNPCsSpawn.spawnMaxMoney = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMaxMoney",
                11,
                "控制生成最大富有度 （小于等于） / Limit NPC spawning to NPCs with AT MOST this wealth level (0-11), NPC entries outside this wealth level won't be spawned -> Less than or equals"
            );
            MCSAllNPCsSpawn.spawnMinCultivationLevel = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMinCultivationLevel",
                1,
                "控制生成最少修为 （大于等于） / Limit NPC spawning to NPCs with AT LEAST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Greater than or equals"
            );
            MCSAllNPCsSpawn.spawnMaxCultivationLevel = base.Config.Bind<int>(
                "MCSAllNPCsSpawnConfig",
                "SpawnMaxCultivationLevel",
                15,
                "控制生成最大修为 （小于等于） / Limit NPC spawning to NPCs with AT MOST this amount of cultivation (0-15), NPC entries outside this cultivation level won't be spawned -> Less than or equals"
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
            JSONObject avatarJsonData = new JSONObject();
            JSONObject npcLeiXingJsonData = new JSONObject();
            getInitialAvatarJsonToLoop();
            if (MCSAllNPCsSpawn.enableAllNPCsSpawn.Value)
            {
                parseNPCLeiXing(normalNPCsCanUseImportantNPCSkills.Value);
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
                WriteToShittyLog("npcLeiXingJsonData has " + npcLeiXingJsonData.Count + " entries");
                for (int i = 0; i < npcLeiXingJsonData.Count; i++)
                {
                    WriteToShittyLog(
                        "npcLeiXingJsonData[" + i.ToString() + "]: " + npcLeiXingJsonData[i].Print()
                    );
                }
                int numNPCsToSpawn = maxSpawnCount.Value;
                int numSpawnedNPCs = 0;
                int loopBreaker = 0;

                while (numSpawnedNPCs < numNPCsToSpawn)
                {
                    for (int i = 0; i < avatarJsonData.Count; i++)
                    {
                        int idx = i;
                        if (i >= avatarJsonData.Count)
                        {
                            WriteToShittyLog(
                                "i (spawn for loop) is bigger than jsonData.instance.AvatarJsonData.Count. Reforming idx, idx was "
                                    + idx.ToString()
                            );
                            idx = idx % avatarJsonData.Count;
                            WriteToShittyLog("idx is now " + idx.ToString());
                        }
                        WriteToShittyLog(
                            "Currently on loop: "
                                + i.ToString()
                                + "\nAlready spawned "
                                + numSpawnedNPCs.ToString()
                                + " / "
                                + numNPCsToSpawn.ToString()
                                + " NPCs\nTrying to get Avatar at index = "
                                + idx.ToString()
                                + ".\nAvatar's ID should be "
                                + avatarJsonData[idx]["id"].I.ToString()
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
                        else
                        {
                            loopBreaker--;
                        }
                        loopBreaker++;
                    }
                    if (loopBreaker > numNPCsToSpawn)
                    {
                        break;
                    }
                }
                WriteToShittyLog(
                    "Finished spawning NPCs! Added " + numSpawnedNPCs.ToString() + " NPCs."
                );
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
                    if (!shouldSpawnImportantNPCs)
                    {
                        if (jsonData.instance.NPCImportantDate.HasField(idx.ToString()))
                        {
                            WriteToShittyLog("Avatar is important NPC, skipping...");
                            return false;
                        }
                    }
                    if (onlyHumans.Value && avatarJsonData[idx]["AvatarType"].I != 1)
                    {
                        WriteToShittyLog("Avatar is not human, skipping...");
                        return false;
                    }
                    int avatarMoneyType = avatarJsonData[idx]["MoneyType"].I;
                    if (
                        avatarMoneyType < spawnMinMoney.Value
                        || avatarMoneyType > spawnMaxMoney.Value
                    )
                    {
                        WriteToShittyLog("Avatar's money type is out of range, skipping...");
                        return false;
                    }
                    int avatarCultivationLevel = avatarJsonData[idx]["Level"].I;
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
            string getRandomName(int genderType, int npcType)
            {
                int randomNum = generateRandomInt(0, Constants.maleNames.Count);
                if (Constants.specialFamilyNames.ContainsKey(npcType))
                {
                    switch (genderType)
                    {
                        case 1:
                            string maleName = Constants.maleNames[randomNum];
                            return Constants.specialFamilyNames[npcType]
                                + maleName.Substring(maleName.Length - 2);
                        case 2:
                            string femaleName = Constants.maleNames[randomNum];
                            return Constants.specialFamilyNames[npcType]
                                + femaleName.Substring(femaleName.Length - 2);
                    }
                }
                switch (genderType)
                {
                    case 1:
                        return Constants.maleNames[randomNum];
                    case 2:
                        return Constants.femaleNames[randomNum];
                }
                return "";
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
                validNPCTags = validNPCTags.Count > 0 ? validNPCTags : defaultNPCTags;

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
            void parseNPCLeiXing(bool disableSpecialLeiXings = true)
            {
                for (int i = 0; i < jsonData.instance.NPCLeiXingDate.Count; i++)
                {
                    JSONObject leiXing = jsonData.instance.NPCLeiXingDate[i];
                    int leiXingId = leiXing["id"].I;
                    bool shouldAddLeiXing = true;
                    if (disableSpecialLeiXings)
                    {
                        foreach (List<int> leiXingRange in Constants.specialLeiXingIdRanges)
                        {
                            if (leiXingRange[0] <= leiXingId && leiXingId <= leiXingRange[1])
                            {
                                shouldAddLeiXing = false;
                                break;
                            }
                        }
                    }
                    if (shouldAddLeiXing)
                    {
                        npcLeiXingJsonData.SetField(leiXingId.ToString(), leiXing);
                    }
                }
            }
            void parseNPCChengHao()
            {
                for (int i = 0; i < jsonData.instance.NPCChengHaoData.Count - 1; i++)
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
            void getInitialAvatarJsonToLoop()
            {
                try
                {
                    for (int i = 0; i < jsonData.instance.AvatarJsonData.Count; i++)
                    {
                        JSONObject avatarJson = jsonData.instance.AvatarJsonData[i];
                        //20,000 because new NPCs start at 20,000
                        if (avatarJson["id"].I < 20000)
                        {
                            avatarJsonData.SetField(avatarJson["id"].I.ToString(), avatarJson);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToShittyLog(ex.ToString());
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
                    // JSONObject newNPCLeiXingJson = jsonData.instance.NPCLeiXingDate.GetField("441"); //Some random level 6 wandering cultivator LeiXing
                    if (idxToUseForLeiXingJson + 1 > npcLeiXingJsonData.Count)
                    {
                        idxToUseForLeiXingJson = idxToUseForLeiXingJson % npcLeiXingJsonData.Count;
                    }
                    JSONObject newNPCLeiXingJson = npcLeiXingJsonData[idxToUseForLeiXingJson];
                    WriteToShittyLog("Leixing to use: " + newNPCLeiXingJson.Print());
                    WriteToShittyLog("AvatarJSON to use: " + avatarJsonData[idx].Print());

                    int avatarJsonDataId = avatarJsonData[idx]["id"].I;
                    newNPC.SetField("id", 50000 + idxToUseForLeiXingJson); // Add 50,000 so there are definitely no conflicts with the original NPCs, hopefully...

                    JSONObject newNPCStatusJson = new JSONObject();
                    newNPCStatusJson.SetField("StatusId", 1);
                    newNPCStatusJson.SetField("StatusTime", 60000);
                    newNPC.SetField("Status", newNPCStatusJson);
                    newNPC.SetField("IsTag", false);
                    newNPC.SetField("SexType", avatarJsonData[idx]["SexType"].I);
                    newNPC.SetField("Type", newNPCLeiXingJson["Type"].I);
                    newNPC.SetField("FirstName", "");
                    if (useRandomNamesWhenSpawning.Value)
                    {
                        newNPC.SetField(
                            "Name",
                            getRandomName(newNPC["SexType"].I, newNPC["Type"].I)
                        );
                    }
                    else
                    {
                        newNPC.SetField("Name", ToolsEx.ToCN(newNPC["Name"].Str));
                    }
                    newNPC.SetField("face", avatarJsonData[idx]["face"].I);
                    newNPC.SetField("fightFace", avatarJsonData[idx]["fightFace"].I);
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
                    newNPC.SetField("IsKnowPlayer", false);
                    newNPC.SetField("QingFen", 0);
                    newNPC.SetField("CyList", JSONObject.arr);
                    newNPC.SetField("TuPoMiShu", JSONObject.arr);
                    Dictionary<string, dynamic> newNPCChengHao = findAndGetNewNPCChengHaoId(
                        newNPCLeiXingJson["Type"].I,
                        newNPCLeiXingJson["Level"].I
                    );
                    newNPC.SetField("Title", newNPCChengHao["ChengHao"]);
                    newNPC.SetField("ChengHaoID", newNPCChengHao["ChengHaoId"]);
                    newNPC.SetField("GongXian", 0);
                    newNPC.SetField("LiuPai", newNPCLeiXingJson["LiuPai"].I);
                    newNPC.SetField("MenPai", newNPCLeiXingJson["MengPai"].I); //到底为什么拼成这样真的很不懂，你这样我很头痛欸
                    newNPC.SetField("AvatarType", avatarJsonData[idx]["AvatarType"].I);
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
                            jsonData.instance.NPCChuShiShuZiDate[newNPCLevel]["xiuwei"].I
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
                    newNPC.SetField("HuaShenLingYu", newNPCLeiXingJson["HuaShenLingYu"].I);
                    newNPC.SetField("staticSkills", newNPCLeiXingJson["staticSkills"]);
                    if (useNPCFactoryInitValuesToSpawn.Value)
                    {
                        JSONObject initVals = jsonData.instance.NPCChuShiShuZiDate.GetField(
                            newNPCLevel.ToString()
                        );
                        newNPC.SetField(
                            "HP",
                            generateRandomIntFromRange(
                                new List<int>() { initVals["HP"][0].I, initVals["HP"][1].I }
                            )
                        );
                        int initAge = generateRandomIntFromRange(
                            new List<int>() { initVals["age"][0].I, initVals["age"][1].I }
                        );
                        if (initAge < newNPCLevel * 5)
                        {
                            initAge = newNPCLevel * 5;
                        }
                        newNPC.SetField("age", initAge);
                        int newNPCShouyuan = generateRandomIntFromRange(
                            new List<int>() { initVals["shouYuan"][0].I, initVals["shouYuan"][1].I }
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
                                new List<int>() { initVals["ziZhi"][0].I, initVals["ziZhi"][1].I }
                            )
                        );
                        newNPC.SetField(
                            "wuXin",
                            generateRandomIntFromRange(
                                new List<int>() { initVals["wuXin"][0].I, initVals["wuXin"][1].I }
                            )
                        );
                        newNPC.SetField(
                            "dunSu",
                            generateRandomIntFromRange(
                                new List<int>() { initVals["dunSu"][0].I, initVals["dunSu"][1].I }
                            )
                        );
                        newNPC.SetField(
                            "shengShi",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initVals["shengShi"][0].I,
                                    initVals["shengShi"][1].I
                                }
                            )
                        );
                        newNPC.SetField(
                            "MoneyType",
                            generateRandomIntFromRange(
                                new List<int>()
                                {
                                    initVals["MoneyType"][0].I,
                                    initVals["MoneyType"][1].I
                                }
                            )
                        );
                    }
                    else
                    {
                        newNPC.SetField("HP", avatarJsonData[idx]["HP"].I);
                        newNPC.SetField("age", avatarJsonData[idx]["age"].I);
                        if (avatarJsonData[idx]["shouYuan"].I < minNPCLifeSpan.Value)
                        {
                            newNPC.SetField("shouYuan", minNPCLifeSpan.Value);
                        }
                        else
                        {
                            newNPC.SetField("shouYuan", avatarJsonData[idx]["shouYuan"].I);
                        }
                        newNPC.SetField("ziZhi", avatarJsonData[idx]["ziZhi"].I);
                        newNPC.SetField("wuXin", avatarJsonData[idx]["wuXin"].I);
                        newNPC.SetField("dunSu", avatarJsonData[idx]["dunSu"].I);
                        newNPC.SetField("shengShi", avatarJsonData[idx]["shengShi"].I);
                        newNPC.SetField("MoneyType", avatarJsonData[idx]["MoneyType"].I);
                    }
                    newNPC.SetField("IsNeedHelp", false);
                    newNPC.SetField("ActionId", 1);
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
                    newNPC.SetField("XinQuType", avatarJsonData[idx]["XinQuType"].I);
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
        public static ConfigEntry<bool> useRandomNamesWhenSpawning;
        public static ConfigEntry<bool> spawnImportantNPCs;
        public static ConfigEntry<bool> normalNPCsCanUseImportantNPCSkills;
        public static ConfigEntry<bool> useNPCFactoryInitValuesToSpawn;
        public static ConfigEntry<int> minNPCLifeSpan;
        public static ConfigEntry<bool> onlyHumans;
        public static ConfigEntry<int> spawnMinMoney; //Greater than or equals
        public static ConfigEntry<int> spawnMaxMoney; //Less than or equals
        public static ConfigEntry<int> spawnMinCultivationLevel; //Greater than or equals
        public static ConfigEntry<int> spawnMaxCultivationLevel; //Less than or equals
    }
}
