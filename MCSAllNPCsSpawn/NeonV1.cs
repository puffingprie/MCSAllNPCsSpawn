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
    // Token: 0x02000002 RID: 2
    [BepInPlugin("Neon_MCS_03", "自用", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(Main), null);
            Main.Inst = this;
            base.StartCoroutine(this.FindGameObject("OkBtn"));
            this.initconfig();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002083 File Offset: 0x00000283
        private IEnumerator FindGameObject(string name)
        {
            GameObject obj;
            do
            {
                obj = ResManager.inst.LoadPrefab("PaiMai/NewPaiMaiUI");
                yield return new WaitForFixedUpdate();
            } while (obj == null);
            this.go = this.GetChild<RectTransform>(obj, name, true);
            bool flag = this.go != null;
            if (flag)
            {
                base.Logger.Log(8, "Load " + name + " has been done");
            }
            yield break;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000209C File Offset: 0x0000029C
        private GameObject GetChild<T>(GameObject gameObject, string name, bool showError = true)
            where T : Component
        {
            foreach (T t in gameObject.GetComponentsInChildren<T>(true))
            {
                bool flag = t.name == name;
                if (flag)
                {
                    return t.gameObject;
                }
            }
            if (showError)
            {
                Debug.LogError("对象" + gameObject.name + "不存在子对象" + name);
            }
            return null;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x0000211C File Offset: 0x0000031C
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DragMag), "Clear")]
        private static bool DragMag_Clear_Prefix(DragMag __instance)
        {
            bool flag = false;
            try
            {
                JiaoYiSlot jiaoYiSlot = (JiaoYiSlot)__instance.DragSlot;
                bool flag2 = !jiaoYiSlot.IsPlayer && !jiaoYiSlot.IsInBag;
                flag = !flag2;
            }
            catch
            {
                flag = false;
            }
            bool result;
            try
            {
                string itemuuid = __instance.DragSlot.Item.Uid;
                string name = __instance.DragSlot.Item.GetName();
                int count = __instance.DragSlot.Item.Count;
                UnityAction<int> unityAction = delegate(int n)
                {
                    Tools.instance.getPlayer().removeItem(itemuuid, n);
                    SingletonMono<TabUIMag>.Instance.TabBag.UpDateSlotList();
                };
                bool flag3 =
                    __instance.ToSlot == null
                    && !flag
                    && __instance.DragSlot.Item.Count >= 1
                    && !__instance.DragSlot.IsIn
                    && __instance.IsDraging;
                if (flag3)
                {
                    bool flag4 = __instance.DragSlot.Item.Count > 1;
                    if (flag4)
                    {
                        USelectNum.Show("需要丢掉 " + name + " x{num}", 1, count, unityAction, null);
                    }
                    else
                    {
                        USelectBox.Show(
                            "需要丢掉该物品吗？",
                            delegate()
                            {
                                Tools.instance.getPlayer().removeItem(itemuuid);
                                SingletonMono<TabUIMag>.Instance.TabBag.UpDateSlotList();
                            },
                            null
                        );
                    }
                }
                result = true;
            }
            catch
            {
                result = true;
            }
            return result;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002278 File Offset: 0x00000478
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CyEmailCell), "Init")]
        private static void CyEmailCell_Init_Patch(
            EmailData emailData,
            bool isDeath,
            CyEmailCell __instance
        )
        {
            int num = Main.Inst.readconfig("cheat26", "enable");
            bool flag = num != 1;
            if (!flag)
            {
                bool flag2 = !isDeath && emailData.PaiMaiInfo != null;
                if (flag2)
                {
                    Avatar avatar = new Avatar();
                    avatar = Tools.instance.getPlayer();
                    bool flag3 =
                        emailData.PaiMaiInfo.EndTime
                        >= Tools.instance.getPlayer().worldTimeMag.getNowTime();
                    if (flag3)
                    {
                        GameObject gameObject = Object.Instantiate<GameObject>(
                            Main.Inst.go,
                            __instance.transform
                        );
                        gameObject.transform.localPosition = new Vector3(0f, -345f);
                        gameObject.transform.localScale = Vector3.one;
                        gameObject.GetComponent<FpBtn>().enabled = false;
                        gameObject
                            .AddComponent<Button>()
                            .onClick.AddListener(
                                delegate()
                                {
                                    bool flag4 =
                                        emailData.PaiMaiInfo.StartTime
                                        > Tools.instance.getPlayer().worldTimeMag.getNowTime();
                                    if (flag4)
                                    {
                                        UIPopTip.Inst.Pop("拍卖时间还没到！", 0);
                                    }
                                    else
                                    {
                                        bool flag5 = Tools.instance.getPlayer().money < 10000UL;
                                        if (flag5)
                                        {
                                            UIPopTip.Inst.Pop("要求10000灵石的服务费用！", 0);
                                        }
                                        else
                                        {
                                            Tools.instance.getPlayer().money -= 10000UL;
                                            GameObejetUtils
                                                .Inst(
                                                    ResManager.inst.LoadPrefab(
                                                        "PaiMai/NewPaiMaiUI"
                                                    ),
                                                    null
                                                )
                                                .GetComponent<NewPaiMaiJoin>()
                                                .Init(
                                                    emailData.PaiMaiInfo.PaiMaiId,
                                                    emailData.npcId
                                                );
                                        }
                                    }
                                }
                            );
                    }
                }
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x0000238C File Offset: 0x0000058C
        [HarmonyPrefix]
        [HarmonyPatch(typeof(taskUI), "Awake")]
        private static bool taskUI_Awake_Prefix()
        {
            int num = Main.Inst.readconfig("cheat16", "enable");
            bool flag = num == 1;
            if (flag)
            {
                USelectBox.Show(
                    "请问需要查看目前失联人员情报吗？",
                    delegate()
                    {
                        string text = "";
                        int num2 = 0;
                        JSONObject npcDeathJson = NpcJieSuanManager.inst.npcDeath.npcDeathJson;
                        bool flag2 = !npcDeathJson.IsNull;
                        if (flag2)
                        {
                            bool flag3 =
                                npcDeathJson.HasField("deathImportantList")
                                && npcDeathJson["deathImportantList"].Count > 0;
                            if (flag3)
                            {
                                text = "\n\n特殊人物失联名单：";
                                for (int i = 0; i < npcDeathJson["deathImportantList"].Count; i++)
                                {
                                    string text2 = npcDeathJson["deathImportantList"][i].ToString();
                                    string text3 = jsonData.instance.AvatarJsonData[text2].HasField(
                                        "Title"
                                    )
                                        ? jsonData.instance.AvatarJsonData[text2]["Title"].Str
                                        : text2;
                                    string text4 = jsonData.instance.AvatarJsonData[text2].HasField(
                                        "Name"
                                    )
                                        ? jsonData.instance.AvatarJsonData[text2]["Name"].Str
                                        : text2;
                                    bool flag4 = text4 == "";
                                    if (flag4)
                                    {
                                        text4 = (
                                            jsonData.instance.AvatarRandomJsonData[text2].HasField(
                                                "Name"
                                            )
                                                ? jsonData.instance.AvatarRandomJsonData[text2][
                                                    "Name"
                                                ].Str
                                                : "神秘人"
                                        );
                                    }
                                    text = string.Concat(
                                        new object[] { text, "<", text3, " ", text4, "> " }
                                    );
                                }
                                text += "\n\n";
                            }
                            int num3 = npcDeathJson.Count - 1;
                            foreach (string text5 in npcDeathJson.keys)
                            {
                                try
                                {
                                    bool flag5 = npcDeathJson[num3].HasField("deathName");
                                    if (flag5)
                                    {
                                        bool flag6 = num2 >= 200;
                                        if (flag6)
                                        {
                                            break;
                                        }
                                        num2++;
                                        bool flag7 = num3 < 0;
                                        if (flag7)
                                        {
                                            break;
                                        }
                                        string str = npcDeathJson[num3]["deathChengHao"].Str;
                                        DateTime dateTime = DateTime.Parse(
                                            npcDeathJson[num3]["deathTime"].Str
                                        );
                                        string text6 = string.Format(
                                            "{0}年{1}月{2}日",
                                            dateTime.Year,
                                            dateTime.Month,
                                            dateTime.Day
                                        );
                                        string str2 = npcDeathJson[num3]["deathName"].Str;
                                        int i2 = npcDeathJson[num3]["deathType"].I;
                                        int num4 = 0;
                                        bool flag8 = npcDeathJson[num3].HasField("killNpcId");
                                        if (flag8)
                                        {
                                            num4 = npcDeathJson[num3]["killNpcId"].I;
                                        }
                                        string text7 = "";
                                        bool flag9 = i2 == 1;
                                        string text8;
                                        if (flag9)
                                        {
                                            text8 = "意外死亡";
                                        }
                                        else
                                        {
                                            bool flag10 = i2 == 2;
                                            if (flag10)
                                            {
                                                text8 = "被你击杀";
                                            }
                                            else
                                            {
                                                bool flag11 = i2 == 3;
                                                if (flag11)
                                                {
                                                    text8 = "游历身死";
                                                }
                                                else
                                                {
                                                    bool flag12 = i2 == 4;
                                                    if (flag12)
                                                    {
                                                        text8 = "海妖攻击";
                                                    }
                                                    else
                                                    {
                                                        bool flag13 = i2 == 5;
                                                        if (flag13)
                                                        {
                                                            text8 = "劫杀成功";
                                                            bool flag14 = num4 > 0;
                                                            if (flag14)
                                                            {
                                                                bool flag15 = npcDeathJson.HasField(
                                                                    num4.ToString()
                                                                );
                                                                if (flag15)
                                                                {
                                                                    text7 =
                                                                        "凶手："
                                                                        + npcDeathJson[
                                                                            num4.ToString()
                                                                        ]["deathName"].Str
                                                                        + "（已死）";
                                                                }
                                                                else
                                                                {
                                                                    text7 =
                                                                        "凶手："
                                                                        + jsonData
                                                                            .instance
                                                                            .AvatarRandomJsonData[
                                                                            num4.ToString()
                                                                        ]["Name"].Str;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                text7 = "凶手：神秘人物";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bool flag16 = i2 == 6;
                                                            if (flag16)
                                                            {
                                                                text8 = "门派任务";
                                                            }
                                                            else
                                                            {
                                                                bool flag17 = i2 == 7;
                                                                if (flag17)
                                                                {
                                                                    text8 = "主城任务";
                                                                }
                                                                else
                                                                {
                                                                    bool flag18 = i2 == 8;
                                                                    if (flag18)
                                                                    {
                                                                        text8 = "炉毁人亡";
                                                                    }
                                                                    else
                                                                    {
                                                                        bool flag19 = i2 == 9;
                                                                        if (flag19)
                                                                        {
                                                                            text8 = "炼器身亡";
                                                                        }
                                                                        else
                                                                        {
                                                                            bool flag20 = i2 == 11;
                                                                            if (flag20)
                                                                            {
                                                                                text8 = "劫杀失败";
                                                                                bool flag21 =
                                                                                    num4 > 0;
                                                                                if (flag21)
                                                                                {
                                                                                    bool flag22 =
                                                                                        npcDeathJson.HasField(
                                                                                            num4.ToString()
                                                                                        );
                                                                                    if (flag22)
                                                                                    {
                                                                                        text7 =
                                                                                            "对象："
                                                                                            + npcDeathJson[
                                                                                                num4.ToString()
                                                                                            ][
                                                                                                "deathName"
                                                                                            ].Str
                                                                                            + "（已死）";
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        text7 =
                                                                                            "对象："
                                                                                            + jsonData
                                                                                                .instance
                                                                                                .AvatarRandomJsonData[
                                                                                                num4.ToString()
                                                                                            ][
                                                                                                "Name"
                                                                                            ].Str;
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    text7 =
                                                                                        "凶手：神秘人物";
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                text8 = "纯属意外";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        text = string.Concat(
                                            new string[]
                                            {
                                                text,
                                                text6,
                                                "\t【",
                                                text8,
                                                "】\t",
                                                str,
                                                "\t",
                                                str2,
                                                "\t\t\t\t ",
                                                text7,
                                                "\n"
                                            }
                                        );
                                    }
                                    num3--;
                                }
                                catch (Exception ex)
                                {
                                    bool flag23 = !File.Exists("d:/diy_list.log");
                                    if (flag23)
                                    {
                                        File.WriteAllText("d:/diy_list.log", "");
                                    }
                                    File.AppendAllText(
                                        "d:/diy_list.log",
                                        string.Concat(
                                            new object[]
                                            {
                                                ex.Message,
                                                "\n",
                                                ex.Source,
                                                "\n",
                                                ex.TargetSite,
                                                "\n",
                                                npcDeathJson.Print(false),
                                                "\n",
                                                ex.StackTrace.Substring(
                                                    ex.StackTrace.LastIndexOf("\\") + 1,
                                                    ex.StackTrace.Length
                                                        - ex.StackTrace.LastIndexOf("\\")
                                                        - 1
                                                ),
                                                "\n\n\n"
                                            }
                                        )
                                    );
                                }
                            }
                        }
                        UBigCheckBox.Show(
                            string.Concat(
                                new string[]
                                {
                                    "                        目前失联人员情报一鉴 （当前显示前"
                                        + num2.ToString()
                                        + "人/最大200人）\n\n",
                                    text
                                }
                            ),
                            null
                        );
                    },
                    null
                );
            }
            return true;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x000023EC File Offset: 0x000005EC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(KillUILogic), "CreateRewardOrderList")]
        private static bool KillUILogic_CreateRewardOrderList_Prefix()
        {
            int num = Main.Inst.readconfig("cheat25", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                int i = 5 - KillManager.Inst.RewardOrderModels.Count;
                RewardOrderFactory rewardOrderFactory =
                    new RewardOrderFactory_PoolService().Get<RewardOrder_OldNpcFactory>();
                foreach (KillFixedNpcData killFixedNpcData in KillFixedNpcData.DataList)
                {
                    RewardOrderModel rewardOrderModel = rewardOrderFactory.Create(
                        killFixedNpcData.id
                    );
                    bool flag2 = KillManager.Inst.AddRewardOrderModel(rewardOrderModel);
                    if (flag2)
                    {
                        i--;
                    }
                }
                bool flag3 = i > 0;
                if (flag3)
                {
                    rewardOrderFactory =
                        new RewardOrderFactory_PoolService().Get<RewardOrder_PlayerFactory>();
                    RewardOrderModel rewardOrderModel2 = rewardOrderFactory.Create(0);
                    bool flag4 = KillManager.Inst.AddRewardOrderModel(rewardOrderModel2);
                    if (flag4)
                    {
                        i--;
                    }
                }
                bool flag5 = i > 0;
                if (flag5)
                {
                    rewardOrderFactory =
                        new RewardOrderFactory_PoolService().Get<RewardOrder_RandomNpcFactory>();
                    while (i > 0)
                    {
                        RewardOrderModel rewardOrderModel3 = rewardOrderFactory.Create(
                            Tools.getRandomInt(1, 3)
                        );
                        bool flag6 = !KillManager.Inst.AddRewardOrderModel(rewardOrderModel3);
                        if (flag6)
                        {
                            Debug.LogError("创建随机Npc悬赏失败");
                            break;
                        }
                        i--;
                    }
                }
                result = true;
            }
            return result;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002548 File Offset: 0x00000748
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RewardOrder_RandomNpcFactory), "GetNpcId")]
        private static bool RewardOrder_RandomNpcFactory_GetNpcId_Prefix(
            int id,
            RewardOrder_RandomNpcFactory __instance,
            ref int __result
        )
        {
            int num = Main.Inst.readconfig("cheat24", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                List<int> liuPai = KillRandomNpcData.DataDict[id].LiuPai;
                List<int> level = KillRandomNpcData.DataDict[id].Level;
                List<int> xingGe = KillRandomNpcData.DataDict[id].XingGe;
                Random random = new Random();
                int num2 = liuPai[random.Next(0, liuPai.Count)];
                int num3 = level[random.Next(0, level.Count)];
                bool flag2 = num3 >= 10;
                if (flag2)
                {
                    num3 = Tools.getRandomInt(10, 15);
                }
                int num4 = xingGe[random.Next(0, xingGe.Count)];
                foreach (JSONObject jsonobject in jsonData.instance.AvatarJsonData.list)
                {
                    bool flag3 =
                        jsonobject["id"].I >= 20000
                        && (!jsonobject.HasField("isImportant") || !jsonobject["isImportant"].b)
                        && !KillManager.Inst.RewardOrderModels.ContainsKey(jsonobject["id"].I)
                        && jsonobject["Level"].I == num3
                        && jsonobject["XingGe"].I == num4
                        && jsonobject["LiuPai"].I == num2;
                    if (flag3)
                    {
                        __result = jsonobject["id"].I;
                        return false;
                    }
                }
                __result = FactoryManager.inst.npcFactory.CreateNpc(num2, num3, num4);
                result = false;
            }
            return result;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x0000274C File Offset: 0x0000094C
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TianJiDaBiManager), "SearchLiuPaiNPC")]
        private static bool TianJiDaBiManager_SearchLiuPaiNPC_Prefix(
            Dictionary<int, List<Vector2Int>> dict,
            int liuPai,
            int jingJie,
            ref List<int> __result
        )
        {
            int num = Main.Inst.readconfig("cheat12", "enable");
            bool flag = num == 1;
            bool result;
            if (flag)
            {
                List<int> list = new List<int>();
                foreach (Vector2Int vector2Int in dict[jingJie + 3])
                {
                    bool flag2 = vector2Int.y == liuPai;
                    if (flag2)
                    {
                        list.Add(vector2Int.x);
                    }
                }
                __result = list;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000027F4 File Offset: 0x000009F4
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NpcJieSuanManager), "RandomNpcAction")]
        private static bool NpcJieSuanManager_RandomNpcAction_Prefix(NpcJieSuanManager __instance)
        {
            try
            {
                __instance.PaiMaiAction();
                __instance.LunDaoAction();
                __instance.npcTeShu.NextJieSha();
                Avatar avatar = new Avatar();
                avatar = Tools.instance.getPlayer();
                __instance.npcMap.RestartMap();
                Dictionary<int, int> dictionary =
                    (Dictionary<int, int>)
                        Traverse
                            .Create(__instance)
                            .Field("NpcActionQuanZhongDictionary")
                            .GetValue();
                bool flag = dictionary.Count < 1;
                if (flag)
                {
                    foreach (string text in jsonData.instance.NPCActionDate.keys)
                    {
                        dictionary.Add(
                            int.Parse(text),
                            jsonData.instance.NPCActionDate[text]["QuanZhong"].I
                        );
                    }
                }
                Traverse
                    .Create(__instance)
                    .Field("NpcActionQuanZhongDictionary")
                    .SetValue(dictionary);
                List<int> list = new List<int>();
                new Dictionary<int, int>();
                JSONObject avatarJsonData = jsonData.instance.AvatarJsonData;
                List<string> keys = jsonData.instance.AvatarJsonData.keys;
                for (int i = 0; i < keys.Count; i++)
                {
                    try
                    {
                        string text2 = keys[i];
                        int num = int.Parse(text2);
                        bool flag2 =
                            num >= 20000
                            && !avatarJsonData[text2].HasField("IsFly")
                            && NPCChengHaoData.DataDict.ContainsKey(
                                avatarJsonData[text2]["ChengHaoID"].I
                            )
                            && avatarJsonData[text2].HasField("Title");
                        if (flag2)
                        {
                            bool flag3 =
                                avatarJsonData[text2].HasField("FlyTime")
                                && __instance.GetNowTime()
                                    >= DateTime.Parse(avatarJsonData[text2]["FlyTime"].Str);
                            if (flag3)
                            {
                                __instance.npcTuPo.NpcFlyToSky(num);
                            }
                            else
                            {
                                try
                                {
                                    Dictionary<int, int> dictionary2 = new Dictionary<int, int>(
                                        dictionary
                                    );
                                    dictionary2 = __instance.getFinallyNpcActionQuanZhongDictionary(
                                        avatarJsonData[text2],
                                        dictionary2
                                    );
                                    int randomActionID = __instance.getRandomActionID(dictionary2);
                                    bool flag4 =
                                        __instance.ActionDictionary.ContainsKey(randomActionID)
                                        && !__instance.npcDeath.NpcYiWaiPanDing(randomActionID, num)
                                        && __instance.npcSetField.AddNpcAge(num, 1);
                                    if (flag4)
                                    {
                                        __instance.npcStatus.ReduceStatusTime(num, 1);
                                        int i2 = avatarJsonData[num.ToString()]["ActionId"].I;
                                        bool flag5 = __instance.NextActionDictionary.ContainsKey(
                                            i2
                                        );
                                        if (flag5)
                                        {
                                            __instance.NextActionDictionary[i2](num);
                                        }
                                        int i3 = avatarJsonData[num.ToString()]["Status"][
                                            "StatusId"
                                        ].I;
                                        bool flag6 =
                                            avatarJsonData[num.ToString()]["Status"]["StatusId"].I
                                            == 20;
                                        if (flag6)
                                        {
                                            __instance.npcTeShu.NpcFriendToDongFu(num);
                                            avatarJsonData[num.ToString()].SetField(
                                                "ActionId",
                                                113
                                            );
                                        }
                                        else
                                        {
                                            bool flag7 =
                                                avatarJsonData[num.ToString()]["Status"][
                                                    "StatusId"
                                                ].I == 21;
                                            if (flag7)
                                            {
                                                __instance.npcTeShu.NpcDaoLuToDongFu(num);
                                                avatarJsonData[num.ToString()].SetField(
                                                    "ActionId",
                                                    114
                                                );
                                            }
                                            else
                                            {
                                                bool flag8 = !Tools.instance
                                                    .getPlayer()
                                                    .ElderTaskMag.GetExecutingTaskNpcIdList()
                                                    .Contains(num);
                                                if (flag8)
                                                {
                                                    bool flag9 =
                                                        avatarJsonData[num.ToString()][
                                                            "isImportant"
                                                        ].b
                                                        && avatarJsonData[num.ToString()].HasField(
                                                            "BindingNpcID"
                                                        );
                                                    if (flag9)
                                                    {
                                                        bool flag10 =
                                                            !__instance.ImprotantNpcActionPanDing(
                                                                num
                                                            );
                                                        if (flag10)
                                                        {
                                                            avatarJsonData[num.ToString()].SetField(
                                                                "ActionId",
                                                                randomActionID
                                                            );
                                                            __instance.ActionDictionary[
                                                                randomActionID
                                                            ](num);
                                                            avatarJsonData[num.ToString()].SetField(
                                                                "IsNeedHelp",
                                                                __instance.IsNeedHelp()
                                                            );
                                                        }
                                                    }
                                                    else
                                                    {
                                                        avatarJsonData[num.ToString()].SetField(
                                                            "ActionId",
                                                            randomActionID
                                                        );
                                                        __instance.ActionDictionary[randomActionID](
                                                            num
                                                        );
                                                        avatarJsonData[num.ToString()].SetField(
                                                            "IsNeedHelp",
                                                            __instance.IsNeedHelp()
                                                        );
                                                    }
                                                    bool flag11 =
                                                        randomActionID == 35 && !list.Contains(num);
                                                    if (flag11)
                                                    {
                                                        list.Add(num);
                                                    }
                                                    __instance.SendMessage(num);
                                                    __instance.SendCy(num);
                                                }
                                            }
                                        }
                                        __instance.GuDingAddExp(num, 1f);
                                        bool flag12 =
                                            avatarJsonData[num.ToString()]["ActionId"].I == 1;
                                        if (flag12)
                                        {
                                            __instance.npcMap.AddNpcToBigMap(num, 1, true);
                                            avatarJsonData[num.ToString()].SetField("ActionId", 33);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    bool flag13 = !File.Exists("d:/diy_randomNpcAction1.log");
                                    if (flag13)
                                    {
                                        File.WriteAllText("d:/diy_randomNpcAction1.log", "");
                                    }
                                    File.AppendAllText(
                                        "d:/diy_randomNpcAction1.log",
                                        string.Concat(
                                            new object[]
                                            {
                                                ex.Message,
                                                "\n",
                                                ex.Source,
                                                "\n",
                                                ex.TargetSite,
                                                "\n",
                                                num,
                                                ex.StackTrace.Substring(
                                                    ex.StackTrace.LastIndexOf("\\") + 1,
                                                    ex.StackTrace.Length
                                                        - ex.StackTrace.LastIndexOf("\\")
                                                        - 1
                                                ),
                                                "\n\n\n"
                                            }
                                        )
                                    );
                                    FactoryManager.inst.npcFactory.InitAutoCreateNpcBackpack(
                                        jsonData.instance.AvatarBackpackJsonData,
                                        num,
                                        null
                                    );
                                    Debug.LogError(string.Format("结算异常{0}", ex));
                                    __instance.IsError = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        bool flag14 = !File.Exists("d:/diy_randomNpcAction.log");
                        if (flag14)
                        {
                            File.WriteAllText("d:/diy_randomNpcAction.log", "");
                        }
                        File.AppendAllText(
                            "d:/diy_randomNpcAction.log",
                            string.Concat(
                                new object[]
                                {
                                    ex2.Message,
                                    "\n",
                                    ex2.Source,
                                    "\n",
                                    ex2.TargetSite,
                                    "\n",
                                    keys[i],
                                    ex2.StackTrace.Substring(
                                        ex2.StackTrace.LastIndexOf("\\") + 1,
                                        ex2.StackTrace.Length - ex2.StackTrace.LastIndexOf("\\") - 1
                                    ),
                                    "\n\n\n"
                                }
                            )
                        );
                    }
                }
                bool flag15 = !avatar.emailDateMag.IsStopAll;
                if (flag15)
                {
                    bool flag16 = __instance.lateEmailList.Count > 0;
                    if (flag16)
                    {
                        foreach (EmailData emailData in __instance.lateEmailList)
                        {
                            avatar.emailDateMag.AddNewEmail(emailData.npcId.ToString(), emailData);
                        }
                        __instance.lateEmailList = new List<EmailData>();
                    }
                    bool flag17 = __instance.lateEmailDict.Keys.Count > 0;
                    if (flag17)
                    {
                        List<int> list2 = new List<int>();
                        foreach (int num2 in __instance.lateEmailDict.Keys)
                        {
                            EmailData emailData2 = __instance.lateEmailDict[num2];
                            bool flag18 = emailData2.RandomTask != null;
                            if (flag18)
                            {
                                DateTime dateTime = DateTime.Parse(emailData2.sendTime);
                                bool flag19 = __instance.GetNowTime() < dateTime;
                                if (flag19)
                                {
                                    continue;
                                }
                                RandomTask randomTask = emailData2.RandomTask;
                                bool flag20 = randomTask.TaskId != 0;
                                if (flag20)
                                {
                                    avatar.StreamData.TaskMag.AddTask(
                                        randomTask.TaskId,
                                        randomTask.TaskType,
                                        randomTask.CyId,
                                        emailData2.npcId,
                                        randomTask.TaskValue,
                                        dateTime
                                    );
                                    bool flag21 =
                                        randomTask.TaskType == 1 && randomTask.LockActionId > 0;
                                    if (flag21)
                                    {
                                        __instance
                                            .GetNpcData(emailData2.npcId)
                                            .SetField("ActionId", randomTask.LockActionId);
                                        __instance
                                            .GetNpcData(emailData2.npcId)
                                            .SetField("LockAction", randomTask.LockActionId);
                                    }
                                }
                                bool flag22 = randomTask.TaskValue != 0;
                                if (flag22)
                                {
                                    GlobalValue.Set(
                                        randomTask.TaskValue,
                                        emailData2.npcId,
                                        "NpcJieSuanManager.RandomNpcAction 传音符相关全局变量A"
                                    );
                                }
                                bool flag23 = randomTask.StaticId.Count > 0;
                                if (flag23)
                                {
                                    for (int j = 0; j < randomTask.StaticId.Count; j++)
                                    {
                                        GlobalValue.Set(
                                            randomTask.StaticId[j],
                                            randomTask.StaticValue[j],
                                            "NpcJieSuanManager.RandomNpcAction 传音符相关全局变量B"
                                        );
                                    }
                                }
                            }
                            list2.Add(num2);
                            avatar.emailDateMag.AddNewEmail(
                                emailData2.npcId.ToString(),
                                emailData2
                            );
                        }
                        foreach (int key in list2)
                        {
                            __instance.lateEmailDict.Remove(key);
                        }
                    }
                }
                Tools.instance.getPlayer().StreamData.TaskMag.CheckHasOut();
                __instance.CheckMenPaiTask();
                Tools.instance.getPlayer().ElderTaskMag.UpdateTaskProcess.CheckHasExecutingTask();
                foreach (int num3 in list)
                {
                    Tools.instance.getPlayer().ElderTaskMag.AddCanAccpetNpcIdList(num3);
                }
                Tools.instance.getPlayer().ElderTaskMag.AllotTask.GetCanAccpetNpcList();
            }
            catch (Exception ex3)
            {
                bool flag24 = !File.Exists("d:/diy_randomNpcAction.log");
                if (flag24)
                {
                    File.WriteAllText("d:/diy_randomNpcAction.log", "");
                }
                File.AppendAllText(
                    "d:/diy_randomNpcAction.log",
                    string.Concat(
                        new object[]
                        {
                            ex3.Message,
                            "\n",
                            ex3.Source,
                            "\n",
                            ex3.TargetSite,
                            "\n",
                            ex3.StackTrace.Substring(
                                ex3.StackTrace.LastIndexOf("\\") + 1,
                                ex3.StackTrace.Length - ex3.StackTrace.LastIndexOf("\\") - 1
                            ),
                            "\n\n\n"
                        }
                    )
                );
            }
            return false;
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00003438 File Offset: 0x00001638
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NpcJieSuanManager), "IsCanChangeEquip")]
        private static bool NpcJieSuanManager_IsCanChangeEquip_Prefix(
            JSONObject npcDate,
            ref int __result
        )
        {
            int i = jsonData.instance.AvatarBackpackJsonData[npcDate["id"].I.ToString()]["money"].I;
            int num = (int)
                jsonData.instance.LianQiWuQiQuality[npcDate["Level"].I.ToString()]["price"];
            int i2 = npcDate["Level"].I;
            int i3 = jsonData.instance.NpcLevelShouYiDate[i2.ToString()]["fabao"].I;
            bool flag = i < num;
            bool result;
            if (flag)
            {
                __result = 0;
                result = false;
            }
            else
            {
                try
                {
                    bool flag2 = !npcDate["equipList"].HasField("Weapon1");
                    if (flag2)
                    {
                        __result = 1;
                        return false;
                    }
                    bool flag3 = npcDate["wuDaoSkillList"].ToList().Contains(2231);
                    if (flag3)
                    {
                        bool flag4 = !npcDate["equipList"].HasField("Weapon2");
                        if (flag4)
                        {
                            __result = 4;
                            return false;
                        }
                        int i4 = npcDate["equipList"]["Weapon1"]["quality"].I;
                        int i5 = npcDate["equipList"]["Weapon2"]["quality"].I;
                        bool flag5 = i3 > i4;
                        if (flag5)
                        {
                            __result = 1;
                            return false;
                        }
                        bool flag6 = i3 > i5;
                        if (flag6)
                        {
                            __result = 4;
                            return false;
                        }
                    }
                    else
                    {
                        int i6 = npcDate["equipList"]["Weapon1"]["quality"].I;
                        bool flag7 = i3 > i6;
                        if (flag7)
                        {
                            __result = 1;
                            return false;
                        }
                    }
                    bool flag8 = !npcDate["equipList"].HasField("Clothing");
                    if (flag8)
                    {
                        __result = 2;
                        return false;
                    }
                    int i7 = npcDate["equipList"]["Clothing"]["quality"].I;
                    bool flag9 = i3 > i7;
                    if (flag9)
                    {
                        __result = 2;
                        return false;
                    }
                    bool flag10 = i2 >= 7;
                    if (flag10)
                    {
                        bool flag11 = !npcDate["equipList"].HasField("Ring");
                        if (flag11)
                        {
                            __result = 3;
                            return false;
                        }
                        int i8 = npcDate["equipList"]["Ring"]["quality"].I;
                        bool flag12 = i3 > i8;
                        if (flag12)
                        {
                            __result = 3;
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    bool flag13 = !File.Exists("d:/diy_IsCanChangeEquip.log");
                    if (flag13)
                    {
                        File.WriteAllText("d:/diy_IsCanChangeEquip.log", "");
                    }
                    File.AppendAllText(
                        "d:/diy_IsCanChangeEquip.log",
                        string.Concat(
                            new object[]
                            {
                                ex.Message,
                                "\n",
                                ex.Source,
                                "\n",
                                ex.TargetSite,
                                "\n",
                                npcDate["id"].ToString(),
                                "\n",
                                ex.StackTrace.Substring(
                                    ex.StackTrace.LastIndexOf("\\") + 1,
                                    ex.StackTrace.Length - ex.StackTrace.LastIndexOf("\\") - 1
                                ),
                                "\n\n\n"
                            }
                        )
                    );
                    jsonData.instance.AvatarJsonData[npcDate["id"].I.ToString()].SetField(
                        "equipList",
                        new JSONObject()
                    );
                    __result = 0;
                    return false;
                }
                __result = 0;
                result = false;
            }
            return result;
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00003860 File Offset: 0x00001A60
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCFactory), "firstCreateNpcs")]
        private static void NPCFactory_firstCreateNpcs_Postfix(NPCFactory __instance)
        {
            int num = Main.Inst.readconfig("cheat27", "enable");
            bool flag = num != 1;
            if (!flag)
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
                        bool flag5 =
                            !(npcname != "")
                            || list3.FindIndex((string xxt) => xxt == npcname) <= -1;
                        if (flag5)
                        {
                            int i4 = jsonData.instance.AvatarJsonData[l]["Level"].I;
                            string npctitle = ToolsEx.ToCN(
                                jsonData.instance.AvatarJsonData[l]["Title"].Str
                            );
                            int i5 = jsonData.instance.AvatarJsonData[l]["AvatarType"].I;
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
                                    bool flag15 = !File.Exists("d:/diy_firstCreateNpcs.log");
                                    if (flag15)
                                    {
                                        File.WriteAllText("d:/diy_firstCreateNpcs.log", "");
                                    }
                                    File.AppendAllText(
                                        "d:/diy_firstCreateNpcs.log",
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

        // Token: 0x0600000D RID: 13 RVA: 0x00004A0C File Offset: 0x00002C0C
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreateNewPlayerFactory), "initAvatarFace")]
        private static bool CreateNewPlayerFactory_initAvatarFace_Prefix(
            int id,
            int index,
            CreateNewPlayerFactory __instance,
            int startIndex = 1
        )
        {
            int num = Main.Inst.readconfig("cheat27", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                JSONObject avatarJsonData = jsonData.instance.AvatarJsonData;
                JSONObject avatarRandomJsonData = jsonData.instance.AvatarRandomJsonData;
                new JSONObject();
                foreach (JSONObject jsonobject in avatarJsonData.list)
                {
                    bool flag2 = jsonobject["id"].I != 1 && jsonobject["id"].I >= startIndex;
                    if (flag2)
                    {
                        int i = jsonobject["id"].I;
                        bool flag3 =
                            !jsonobject.HasField("isImportant") || !jsonobject["isImportant"].b;
                        if (flag3)
                        {
                            bool flag4 =
                                jsonobject.HasField("mybangding") && jsonobject["mybangding"].I > 0;
                            if (flag4)
                            {
                                avatarRandomJsonData.SetField(
                                    string.Concat(jsonobject["id"].I),
                                    avatarRandomJsonData[jsonobject["mybangding"].I.ToString()]
                                );
                            }
                            else
                            {
                                bool flag5 =
                                    jsonobject["id"].I > 100000
                                    && avatarRandomJsonData.HasField(
                                        (jsonobject["id"].I - 100000).ToString()
                                    );
                                if (flag5)
                                {
                                    avatarRandomJsonData.SetField(
                                        string.Concat(jsonobject["id"].I),
                                        avatarRandomJsonData[
                                            (jsonobject["id"].I - 100000).ToString()
                                        ]
                                    );
                                }
                                else
                                {
                                    JSONObject jsonobject2 = jsonData.instance.randomAvatarFace(
                                        jsonobject,
                                        avatarRandomJsonData.HasField(
                                            string.Concat(jsonobject["id"].I)
                                        )
                                            ? avatarRandomJsonData[jsonobject["id"].I.ToString()]
                                            : null
                                    );
                                    avatarRandomJsonData.SetField(
                                        string.Concat(jsonobject["id"].I),
                                        jsonobject2.Copy()
                                    );
                                }
                            }
                        }
                        else
                        {
                            avatarRandomJsonData.SetField(
                                string.Concat(jsonobject["id"].I),
                                avatarRandomJsonData[jsonobject["BindingNpcID"].I.ToString()]
                            );
                        }
                    }
                }
                bool flag6 = avatarRandomJsonData.HasField("1");
                if (flag6)
                {
                    avatarRandomJsonData.SetField("10000", avatarRandomJsonData["1"]);
                }
                Traverse
                    .Create(__instance)
                    .Method("randomAvatarBackpack", Array.Empty<object>())
                    .GetValue();
                Tools.instance.getPlayer();
                foreach (JSONObject jsonobject3 in avatarJsonData.list)
                {
                    int i2 = jsonobject3["id"].I;
                    bool flag7 = i2 >= 20000;
                    if (flag7)
                    {
                        int i3 = jsonobject3["Type"].I;
                        FactoryManager.inst.npcFactory.InitAutoCreateNpcBackpack(
                            jsonData.instance.AvatarBackpackJsonData,
                            i2,
                            null
                        );
                    }
                }
                result = false;
            }
            return result;
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00004DFC File Offset: 0x00002FFC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NpcJieSuanManager), "FixNpcData")]
        private static bool NpcJieSuanManager_FixNpcData_Prefix()
        {
            int num = Main.Inst.readconfig("cheat27", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                JSONObject avatarJsonData = jsonData.instance.AvatarJsonData;
                foreach (string text in avatarJsonData.keys)
                {
                    bool flag2 = int.Parse(text) >= 20000;
                    if (flag2)
                    {
                        bool flag3 = !jsonData.instance.AvatarRandomJsonData.HasField(text);
                        if (flag3)
                        {
                            bool b = avatarJsonData[text]["isImportant"].b;
                            if (b)
                            {
                                jsonData.instance.AvatarRandomJsonData.SetField(
                                    avatarJsonData[text]["id"].I.ToString(),
                                    jsonData.instance.AvatarRandomJsonData[
                                        avatarJsonData[text]["BindingNpcID"].I.ToString()
                                    ]
                                );
                            }
                            else
                            {
                                bool flag4 =
                                    avatarJsonData[text].HasField("mybangding")
                                    && avatarJsonData[text]["mybangding"].I > 0;
                                if (flag4)
                                {
                                    jsonData.instance.AvatarRandomJsonData.SetField(
                                        avatarJsonData[text]["id"].I.ToString(),
                                        jsonData.instance.AvatarRandomJsonData[
                                            avatarJsonData[text]["mybangding"].I.ToString()
                                        ]
                                    );
                                }
                                else
                                {
                                    bool flag5 =
                                        int.Parse(text) > 100000
                                        && avatarJsonData.HasField(
                                            (int.Parse(text) - 100000).ToString()
                                        );
                                    if (flag5)
                                    {
                                        jsonData.instance.AvatarRandomJsonData.SetField(
                                            avatarJsonData[text]["id"].I.ToString(),
                                            jsonData.instance.AvatarRandomJsonData[
                                                (int.Parse(text) - 100000).ToString()
                                            ]
                                        );
                                    }
                                    else
                                    {
                                        JSONObject jsonobject = jsonData.instance.randomAvatarFace(
                                            avatarJsonData[text],
                                            null
                                        );
                                        jsonData.instance.AvatarRandomJsonData.SetField(
                                            string.Concat(avatarJsonData[text]["id"].I),
                                            jsonobject.Copy()
                                        );
                                    }
                                }
                            }
                        }
                        bool flag6 = !jsonData.instance.AvatarBackpackJsonData.HasField(text);
                        if (flag6)
                        {
                            FactoryManager.inst.npcFactory.InitAutoCreateNpcBackpack(
                                jsonData.instance.AvatarBackpackJsonData,
                                avatarJsonData[text]["id"].I,
                                avatarJsonData[text]
                            );
                        }
                    }
                }
                result = false;
            }
            return result;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00005124 File Offset: 0x00003324
        [HarmonyPrefix]
        [HarmonyPatch(typeof(jsonData), "MonstarCreatInterstingType")]
        private static bool jsonData_MonstarCreatInterstingType_Prefix(
            int MonstarID,
            jsonData __instance
        )
        {
            int i = __instance.AvatarJsonData[MonstarID.ToString()]["XinQuType"].I;
            JSONObject jsonobject = __instance.AvatarBackpackJsonData[string.Concat(MonstarID)];
            jsonobject.SetField("XinQuType", new JSONObject(4));
            foreach (KeyValuePair<string, JToken> keyValuePair in __instance.NPCInterestingItem)
            {
                bool flag = (int)keyValuePair.Value["type"] == i;
                if (flag)
                {
                    List<int> list = new List<int>();
                    foreach (JToken jtoken in ((JArray)keyValuePair.Value["xihao"]))
                    {
                        list.Add((int)jtoken);
                    }
                    for (int j = 0; j < (int)keyValuePair.Value["num"]; j++)
                    {
                        bool flag2 = list.Count <= 0;
                        if (flag2)
                        {
                            break;
                        }
                        JSONObject jsonobject2 = new JSONObject(3);
                        int num = list[jsonData.GetRandom() % list.Count];
                        list.Remove(num);
                        jsonobject2.SetField("type", num);
                        jsonobject2.SetField("percent", (int)keyValuePair.Value["percent"]);
                        jsonobject["XinQuType"].Add(jsonobject2);
                    }
                }
            }
            return false;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00005324 File Offset: 0x00003524
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NPCSpeedJieSuan), "DoSpeedJieSuan")]
        private static bool NPCSpeedJieSuan_DoSpeedJieSuan_Prefix(
            NPCSpeedJieSuan __instance,
            int times = 1
        )
        {
            List<int> list = new List<int>();
            Tools.instance.getPlayer().fakeTimes += times;
            foreach (JSONObject jsonobject in jsonData.instance.AvatarJsonData.list)
            {
                bool flag =
                    jsonobject["id"].I >= 20000
                    && !jsonobject.HasField("IsFly")
                    && jsonobject.HasField("Title");
                if (flag)
                {
                    list.Add(jsonobject["id"].I);
                }
            }
            foreach (int num in list)
            {
                try
                {
                    NpcJieSuanManager.inst.npcStatus.ReduceStatusTime(num, times);
                    NpcJieSuanManager.inst.npcSetField.AddNpcMoney(
                        num,
                        (int)(
                            (double)(
                                jsonData.instance.NpcLevelShouYiDate[
                                    jsonData.instance.AvatarJsonData[num.ToString()][
                                        "Level"
                                    ].I.ToString()
                                ]["money"].I * times
                            ) * 0.4
                        )
                    );
                    NpcJieSuanManager.inst.GuDingAddExp(num, (float)times * 1.3f);
                    int i = jsonData.instance.AvatarJsonData[num.ToString()]["NPCTag"].I;
                    bool flag2 =
                        NpcJieSuanManager.inst.JieSuanTimes > 0
                        && NpcJieSuanManager.inst.JieSuanTimes % 12 == 0;
                    if (flag2)
                    {
                        List<int> list2 =
                            (List<int>)
                                Traverse.Create(__instance).Field("lianDanTagList").GetValue();
                        bool flag3 = list2.Contains(i);
                        if (flag3)
                        {
                            NpcJieSuanManager.inst.npcSetField.AddNpcWuDaoExp(num, 21, 200);
                        }
                        list2 =
                            (List<int>)
                                Traverse.Create(__instance).Field("lianQiTagList").GetValue();
                        bool flag4 = list2.Contains(i);
                        if (flag4)
                        {
                            NpcJieSuanManager.inst.npcSetField.AddNpcWuDaoExp(num, 22, 200);
                        }
                        bool flag5 = NpcJieSuanManager.inst.getRandomInt(1, 100) <= 40;
                        if (flag5)
                        {
                            NpcJieSuanManager.inst.npcLiLian.NPCNingZhouYouLi(num);
                        }
                    }
                    default(Main.FunObj_npcid).npcid = num;
                    bool flag6 = (bool)
                        Traverse
                            .Create(__instance)
                            .Method(
                                "IsSpeedCanBigTuPo",
                                new Type[] { typeof(int) },
                                new object[] { num }
                            )
                            .GetValue();
                    bool flag7 = flag6;
                    if (flag7)
                    {
                        Traverse
                            .Create(__instance)
                            .Method(
                                "NpcSpeedBigTuPo",
                                new Type[] { typeof(int) },
                                new object[] { num }
                            )
                            .GetValue();
                    }
                    NpcJieSuanManager.inst.npcSetField.AddNpcAge(num, times);
                }
                catch (Exception ex)
                {
                    bool flag8 = !File.Exists("d:/diy_DoSpeedJieSuan.log");
                    if (flag8)
                    {
                        File.WriteAllText("d:/diy_DoSpeedJieSuan.log", "");
                    }
                    File.AppendAllText(
                        "d:/diy_DoSpeedJieSuan.log",
                        string.Concat(
                            new object[]
                            {
                                ex.Message,
                                "\n",
                                ex.Source,
                                "\n",
                                ex.TargetSite,
                                "\n",
                                num.ToString(),
                                "\n",
                                ex.StackTrace.Substring(
                                    ex.StackTrace.LastIndexOf("\\") + 1,
                                    ex.StackTrace.Length - ex.StackTrace.LastIndexOf("\\") - 1
                                ),
                                "\n\n\n"
                            }
                        )
                    );
                }
            }
            Tools.instance.getPlayer().ElderTaskMag.UpdateTaskProcess.CheckHasExecutingTask(times);
            return false;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000057A0 File Offset: 0x000039A0
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapRandomCompent), "EventRandom")]
        private static bool MapRandomCompent_EventRandom_Prefix(BaseMapCompont __instance)
        {
            int num = Main.Inst.readconfig("cheat23", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Avatar player = Tools.instance.getPlayer();
                bool flag2 = !__instance.CanClick();
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    bool flag3 = WASDMove.Inst != null;
                    if (flag3)
                    {
                        WASDMove.Inst.IsMoved = true;
                    }
                    __instance.fuBenSetClick();
                    __instance.movaAvatar();
                    RandomFuBen component = __instance.transform.parent.GetComponent<RandomFuBen>();
                    List<int> list = new List<int>();
                    List<int> list2 = new List<int>();
                    component.mapMag.getAllMapIndex(2, list, list2);
                    bool flag4 =
                        component.mapMag.mapIndex[list[0], list2[0]] == __instance.NodeIndex;
                    bool flag5 = flag4;
                    if (flag5)
                    {
                        USelectBox.Show(
                            "是否离开当前副本？",
                            delegate()
                            {
                                Tools.instance.getPlayer().randomFuBenMag.OutRandomFuBen();
                            },
                            delegate()
                            {
                                bool flag26 =
                                    AllMapManage.instance != null
                                    && AllMapManage.instance.mapIndex.ContainsKey(
                                        Tools.instance.fubenLastIndex
                                    );
                                if (flag26)
                                {
                                    AllMapManage.instance.mapIndex[
                                        Tools.instance.fubenLastIndex
                                    ].AvatarMoveToThis();
                                }
                            }
                        );
                        result = false;
                    }
                    else
                    {
                        int nowRandomFuBenID = player.NowRandomFuBenID;
                        JObject jobject = (JObject)
                            player.RandomFuBenList[nowRandomFuBenID.ToString()];
                        bool flag6 =
                            jobject.ContainsKey("TaskIndex")
                            && (int)jobject["TaskIndex"] == __instance.NodeIndex;
                        int num2;
                        if (flag6)
                        {
                            num2 = (int)jobject["TaskTalkID"];
                        }
                        else
                        {
                            num2 = -1;
                        }
                        bool flag7 = num2 != -1 && num2 != 0;
                        if (flag7)
                        {
                            Object.Instantiate<GameObject>(
                                Resources.Load<GameObject>("talkPrefab/TalkPrefab/talk" + num2)
                            );
                            int nowRandomFuBenID2 = player.NowRandomFuBenID;
                            JToken jtoken = player.RandomFuBenList[nowRandomFuBenID2.ToString()];
                            jtoken["ShouldReset"] = true;
                            jtoken["TaskTalkID"] = -1;
                            JToken jtoken2 = player.RandomFuBenList[nowRandomFuBenID2.ToString()];
                            foreach (JToken jtoken3 in ((JArray)jtoken2["Award"]))
                            {
                                JObject jobject2 = (JObject)jtoken3;
                                bool flag8 = (int)jobject2["Index"] == __instance.NodeIndex;
                                if (flag8)
                                {
                                    bool flag9 =
                                        (int)jobject2["ID"] != -1
                                        && (int)
                                            jsonData.instance.RandomMapEventList[
                                                ((int)jobject2["ID"]).ToString()
                                            ]["chufaduoci"] == 1;
                                    if (flag9)
                                    {
                                        jobject2["ID"] = -1;
                                        break;
                                    }
                                    break;
                                }
                            }
                            foreach (JToken jtoken4 in ((JArray)jtoken2["Event"]))
                            {
                                JObject jobject3 = (JObject)jtoken4;
                                bool flag10 = (int)jobject3["Index"] == __instance.NodeIndex;
                                if (flag10)
                                {
                                    bool flag11 =
                                        (int)jobject3["ID"] != -1
                                        && (int)
                                            jsonData.instance.RandomMapEventList[
                                                ((int)jobject3["ID"]).ToString()
                                            ]["chufaduoci"] == 1;
                                    if (flag11)
                                    {
                                        jobject3["ID"] = -1;
                                        break;
                                    }
                                    break;
                                }
                            }
                            result = false;
                        }
                        else
                        {
                            int nowRandomFuBenID3 = player.NowRandomFuBenID;
                            JToken jtoken5 = player.RandomFuBenList[nowRandomFuBenID3.ToString()];
                            int num3 = -1;
                            foreach (JToken jtoken6 in ((JArray)jtoken5["Award"]))
                            {
                                JObject jobject4 = (JObject)jtoken6;
                                bool flag12 = (int)jobject4["Index"] == __instance.NodeIndex;
                                if (flag12)
                                {
                                    num3 = (int)jobject4["ID"];
                                }
                            }
                            foreach (JToken jtoken7 in ((JArray)jtoken5["Event"]))
                            {
                                JObject jobject5 = (JObject)jtoken7;
                                bool flag13 = (int)jobject5["Index"] == __instance.NodeIndex;
                                if (flag13)
                                {
                                    num3 = (int)jobject5["ID"];
                                }
                            }
                            bool flag14 = num3 == -1;
                            if (flag14)
                            {
                                result = false;
                            }
                            else
                            {
                                JToken jtoken8 = jsonData.instance.RandomMapEventList[
                                    num3.ToString()
                                ];
                                int num4 = 0;
                                string text = ",0,1,2,5,6,8,";
                                int randomInt;
                                int id;
                                do
                                {
                                    randomInt = Tools.getRandomInt(
                                        0,
                                        _ItemJsonData.DataList.Count - 1
                                    );
                                    id = _ItemJsonData.DataList[randomInt].id;
                                } while (
                                    !text.Contains(
                                        ","
                                            + _ItemJsonData.DataList[randomInt].type.ToString()
                                            + ","
                                    )
                                    || _ItemJsonData.DataList[randomInt].price < 20000
                                );
                                bool flag15 =
                                    ((JArray)jtoken8["valueID"]).Count >= 4
                                    && (int)jtoken8["valueID"][0] == 953
                                    && (int)jtoken8["valueID"][1] == 952
                                    && (int)jtoken8["valueID"][2] == 955
                                    && (int)jtoken8["valueID"][3] == 950
                                    && id > 0;
                                if (flag15)
                                {
                                    bool flag16 =
                                        _ItemJsonData.DataList[randomInt].type == 5
                                        || _ItemJsonData.DataList[randomInt].type == 6
                                        || _ItemJsonData.DataList[randomInt].type == 8;
                                    if (flag16)
                                    {
                                        player.addItem(
                                            id,
                                            Tools.getRandomInt(1, 10),
                                            Tools.CreateItemSeid(id),
                                            true
                                        );
                                    }
                                    jtoken8["value"][0] = id;
                                }
                                string text2 = (string)jtoken8["name"];
                                bool flag17 =
                                    ((JArray)jtoken8["valueID"]).Count == 3
                                    && (int)jtoken8["valueID"][0] == 952
                                    && (int)jtoken8["valueID"][1] == 954
                                    && (int)jtoken8["valueID"][2] == 953
                                    && id > 0
                                    && text2.Contains("藏经阁");
                                if (flag17)
                                {
                                    bool flag18 =
                                        _ItemJsonData.DataList[randomInt].type == 5
                                        || _ItemJsonData.DataList[randomInt].type == 6
                                        || _ItemJsonData.DataList[randomInt].type == 8;
                                    if (flag18)
                                    {
                                        player.addItem(
                                            id,
                                            Tools.getRandomInt(1, 10),
                                            Tools.CreateItemSeid(id),
                                            true
                                        );
                                    }
                                    jtoken8["value"][2] = id;
                                }
                                foreach (JToken jtoken9 in ((JArray)jtoken8["valueID"]))
                                {
                                    GlobalValue.Set(
                                        (int)jtoken9,
                                        (int)jtoken8["value"][num4],
                                        "MapRandomComponent.EventRandom 随机副本事件全局变量"
                                    );
                                    num4++;
                                }
                                bool flag19 =
                                    !(AllMapManage.instance != null)
                                    || !AllMapManage.instance.RandomFlag.ContainsKey(num3);
                                if (flag19)
                                {
                                    string text3 = string.Format(
                                        "talkPrefab/TalkPrefab/talk{0}",
                                        (int)jtoken8["talk"]
                                    );
                                    GameObject gameObject = Resources.Load<GameObject>(text3);
                                    bool flag20 = gameObject != null;
                                    if (flag20)
                                    {
                                        Flowchart componentInChildren = Object
                                            .Instantiate<GameObject>(gameObject)
                                            .GetComponentInChildren<Flowchart>();
                                        bool flag21 = componentInChildren.HasVariable("FBEventID");
                                        if (flag21)
                                        {
                                            componentInChildren.SetIntegerVariable(
                                                "FBEventID",
                                                num3
                                            );
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(text3 + "不存在，无法实例化talk，请检查");
                                    }
                                }
                                int nowRandomFuBenID4 = player.NowRandomFuBenID;
                                JToken jtoken10 = player.RandomFuBenList[
                                    nowRandomFuBenID4.ToString()
                                ];
                                foreach (JToken jtoken11 in ((JArray)jtoken10["Award"]))
                                {
                                    JObject jobject6 = (JObject)jtoken11;
                                    bool flag22 = (int)jobject6["Index"] == __instance.NodeIndex;
                                    if (flag22)
                                    {
                                        bool flag23 =
                                            (int)jobject6["ID"] != -1
                                            && (int)
                                                jsonData.instance.RandomMapEventList[
                                                    ((int)jobject6["ID"]).ToString()
                                                ]["chufaduoci"] == 1;
                                        if (flag23)
                                        {
                                            jobject6["ID"] = -1;
                                            break;
                                        }
                                        break;
                                    }
                                }
                                foreach (JToken jtoken12 in ((JArray)jtoken10["Event"]))
                                {
                                    JObject jobject7 = (JObject)jtoken12;
                                    bool flag24 = (int)jobject7["Index"] == __instance.NodeIndex;
                                    if (flag24)
                                    {
                                        bool flag25 =
                                            (int)jobject7["ID"] != -1
                                            && (int)
                                                jsonData.instance.RandomMapEventList[
                                                    ((int)jobject7["ID"]).ToString()
                                                ]["chufaduoci"] == 1;
                                        if (flag25)
                                        {
                                            jobject7["ID"] = -1;
                                            break;
                                        }
                                        break;
                                    }
                                }
                                result = false;
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00006320 File Offset: 0x00004520
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PaiMaiDataMag), "RandomPaiMaiShopList")]
        private static bool PaiMaiDataMag_RandomPaiMaiShopList_Prefix(
            int id,
            PaiMaiDataMag __instance,
            ref List<int> __result
        )
        {
            List<int> list = new List<int>();
            int num = 0;
            bool flag = PaiMaiBiao.DataDict[id].Type.Count == 0;
            if (flag)
            {
                list.Add(PaiMaiBiao.DataDict[id].guding[0]);
            }
            else
            {
                int count = PaiMaiBiao.DataDict[id].guding.Count;
                int i = PaiMaiBiao.DataDict[id].ItemNum;
                int num2 = 0;
                bool flag2 = count > 0;
                if (flag2)
                {
                    num = PaiMaiBiao.DataDict[id].quanzhong2[0];
                }
                foreach (int num3 in PaiMaiBiao.DataDict[id].quanzhong1)
                {
                    num2 += num3;
                }
                Dictionary<int, List<int>> dictionary =
                    (Dictionary<int, List<int>>)
                        Traverse.Create(__instance).Field("PaiMaiShopQualityDict").GetValue();
                Dictionary<int, List<int>> dictionary2 =
                    (Dictionary<int, List<int>>)
                        Traverse.Create(__instance).Field("PaiMaiShopTypeDict").GetValue();
                List<int> list2 = new List<int>();
                for (int j = 0; j < dictionary[id].Count; j++)
                {
                    list2.Add(j);
                }
                while (i > 0)
                {
                    bool flag3 = count > 0 && i == 1;
                    if (flag3)
                    {
                        bool flag4 = Tools.instance.GetRandomInt(0, num2) <= num;
                        if (flag4)
                        {
                            list.Add(PaiMaiBiao.DataDict[id].guding[0]);
                        }
                        else
                        {
                            bool flag5 = list2.Count > 0;
                            int i2;
                            if (flag5)
                            {
                                int randomInt = Tools.instance.GetRandomInt(0, list2.Count - 1);
                                i2 = FactoryManager.inst.npcFactory.GetRandomItemByShopType(
                                    dictionary2[id][list2[randomInt]],
                                    dictionary[id][list2[randomInt]]
                                )["id"].I;
                                list2.RemoveAt(randomInt);
                            }
                            else
                            {
                                do
                                {
                                    int randomInt2 = Tools.instance.GetRandomInt(
                                        0,
                                        dictionary2[id].Count - 1
                                    );
                                    i2 = FactoryManager.inst.npcFactory.GetRandomItemByShopType(
                                        dictionary2[id][randomInt2],
                                        dictionary[id][randomInt2]
                                    )["id"].I;
                                } while (list.Contains(i2));
                            }
                            list.Add(i2);
                        }
                    }
                    else
                    {
                        bool flag6 = list2.Count > 0;
                        int i3;
                        if (flag6)
                        {
                            int randomInt3 = Tools.instance.GetRandomInt(0, list2.Count - 1);
                            i3 = FactoryManager.inst.npcFactory.GetRandomItemByShopType(
                                dictionary2[id][list2[randomInt3]],
                                dictionary[id][list2[randomInt3]]
                            )["id"].I;
                            list2.RemoveAt(randomInt3);
                        }
                        else
                        {
                            do
                            {
                                int randomInt4 = Tools.instance.GetRandomInt(
                                    0,
                                    dictionary2[id].Count - 1
                                );
                                i3 = FactoryManager.inst.npcFactory.GetRandomItemByShopType(
                                    dictionary2[id][randomInt4],
                                    dictionary[id][randomInt4]
                                )["id"].I;
                            } while (list.Contains(i3));
                        }
                        list.Add(i3);
                    }
                    i--;
                }
            }
            __result = list;
            return false;
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000066F8 File Offset: 0x000048F8
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PaiMaiDataMag), "Init")]
        private static bool PaiMaiDataMag_Init_Prefix(PaiMaiDataMag __instance)
        {
            int num = Main.Inst.readconfig("cheat22", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
                Dictionary<int, List<int>> dictionary2 = new Dictionary<int, List<int>>();
                Traverse.Create(__instance).Field("PaiMaiShopQualityDict").SetValue(dictionary);
                Traverse.Create(__instance).Field("PaiMaiShopTypeDict").SetValue(dictionary2);
                foreach (PaiMaiBiao paiMaiBiao in PaiMaiBiao.DataList)
                {
                    try
                    {
                        bool flag2 = paiMaiBiao.Type.Count > 0;
                        if (flag2)
                        {
                            dictionary2.Add(paiMaiBiao.PaiMaiID, new List<int>());
                            dictionary.Add(paiMaiBiao.PaiMaiID, new List<int>());
                            for (int i = 0; i < paiMaiBiao.Type.Count; i++)
                            {
                                dictionary2[paiMaiBiao.PaiMaiID].Add(paiMaiBiao.Type[i]);
                                dictionary[paiMaiBiao.PaiMaiID].Add(paiMaiBiao.quality[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                        Debug.LogError("初始化拍卖会失败");
                        Debug.LogError(string.Format("拍卖会ID：{0}", paiMaiBiao.PaiMaiID));
                        Debug.LogError(
                            string.Format("商品类型数目：{0},", paiMaiBiao.Type.Count)
                                + string.Format(
                                    "品阶数目：{0},权重数目{1}",
                                    paiMaiBiao.quality.Count,
                                    paiMaiBiao.quanzhong1.Count
                                )
                        );
                    }
                }
                Traverse.Create(__instance).Field("PaiMaiShopQualityDict").SetValue(dictionary);
                Traverse.Create(__instance).Field("PaiMaiShopTypeDict").SetValue(dictionary2);
                foreach (PaiMaiBiao paiMaiBiao2 in PaiMaiBiao.DataList)
                {
                    bool flag3 =
                        __instance.PaiMaiDict.Count < PaiMaiBiao.DataList.Count
                        && !__instance.PaiMaiDict.ContainsKey(paiMaiBiao2.PaiMaiID);
                    if (flag3)
                    {
                        PaiMaiData paiMaiData = new PaiMaiData();
                        paiMaiData.Id = paiMaiBiao2.PaiMaiID;
                        paiMaiData.IsJoined = false;
                        paiMaiData.No = __instance.GetNowPaiMaiJieNum(paiMaiBiao2.PaiMaiID);
                        paiMaiData.ShopList =
                            (List<int>)
                                Traverse
                                    .Create(__instance)
                                    .Method(
                                        "RandomPaiMaiShopList",
                                        new Type[] { typeof(int) },
                                        new object[] { paiMaiData.Id }
                                    )
                                    .GetValue();
                        bool flag4 = paiMaiData.No > 0;
                        if (flag4)
                        {
                            paiMaiData.NextUpdateTime = DateTime
                                .Parse(PaiMaiBiao.DataDict[paiMaiBiao2.PaiMaiID].EndTime)
                                .AddYears((paiMaiData.No - 1) * paiMaiBiao2.circulation)
                                .AddDays(1.0);
                        }
                        else
                        {
                            paiMaiData.No = 1;
                            paiMaiData.NextUpdateTime = DateTime
                                .Parse(PaiMaiBiao.DataDict[paiMaiBiao2.PaiMaiID].EndTime)
                                .AddDays(1.0);
                        }
                        __instance.PaiMaiDict.Add(paiMaiData.Id, paiMaiData);
                    }
                }
                result = false;
            }
            return result;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00006B00 File Offset: 0x00004D00
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NewPaiMaiJoin), "RefreshShop")]
        private static bool NewPaiMaiJoin_RefreshShop_Prefix(NewPaiMaiJoin __instance)
        {
            int num = Main.Inst.readconfig("cheat21", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Traverse.Create(__instance).Field("BaseShopList").SetValue(new List<BaseItem>());
                Avatar player = Tools.instance.getPlayer();
                bool flag2 = player.StreamData.PaiMaiDataMag.PaiMaiDict.Count == 0;
                if (flag2)
                {
                    player.StreamData.PaiMaiDataMag.AuToUpDate();
                }
                else
                {
                    bool isJoined = player.StreamData.PaiMaiDataMag.PaiMaiDict[
                        __instance.PaiMaiId
                    ].IsJoined;
                    if (isJoined)
                    {
                        player.StreamData.PaiMaiDataMag.UpdateById(__instance.PaiMaiId);
                    }
                }
                List<BaseItem> list =
                    (List<BaseItem>)Traverse.Create(__instance).Field("BaseShopList").GetValue();
                foreach (
                    int num2 in player.StreamData.PaiMaiDataMag.PaiMaiDict[
                        __instance.PaiMaiId
                    ].ShopList
                )
                {
                    Transform transform = (Transform)
                        Traverse.Create(__instance).Field("ShopPanel").GetValue();
                    GameObject gameObject = (GameObject)
                        Traverse.Create(__instance).Field("ShopCell").GetValue();
                    PaiMaiSlot component = GameObejetUtils
                        .Inst(gameObject, transform)
                        .GetComponent<PaiMaiSlot>();
                    component.SetSlotData(
                        BaseItem.Create(num2, 1, Tools.getUUID(), Tools.CreateItemSeid(num2))
                    );
                    component.gameObject.SetActive(true);
                    list.Add(component.Item.Clone());
                }
                Traverse.Create(__instance).Field("BaseShopList").SetValue(list);
                PaiMaiSay PaiMaiSay1 = (PaiMaiSay)
                    Traverse.Create(__instance).Field("Say").GetValue();
                USelectBox.Show(
                    "请问需要确认预约换下一轮物资？换预约物资清单需要花费" + 50000.ToString() + "灵石。",
                    delegate()
                    {
                        bool flag3 = (int)Tools.instance.getPlayer().money < 50000;
                        if (flag3)
                        {
                            PaiMaiSay1.SayWord("取消预药费用不够哦，需要" + 50000.ToString() + "灵石。", null, 1f);
                        }
                        else
                        {
                            Tools.instance.getPlayer().money -= 50000UL;
                            PaiMaiSay1.SayWord(
                                "好的，马上为您更新下一批。已扣除5万费用，当前您还剩余"
                                    + Tools.instance.getPlayer().money.ToString()
                                    + "费用。",
                                null,
                                1f
                            );
                            Tools.instance
                                .getPlayer()
                                .StreamData.PaiMaiDataMag.UpdateById(__instance.PaiMaiId, 0);
                            __instance.Invoke("Close", 1f);
                        }
                    },
                    null
                );
                result = false;
            }
            return result;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00006D80 File Offset: 0x00004F80
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AvatarCtr), "AvatarStart")]
        private static bool AvatarCtr_AvatarStart_Prefix()
        {
            int num = Main.Inst.readconfig("cheat20", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Time.timeScale = 100f;
                result = true;
            }
            return result;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00006DC4 File Offset: 0x00004FC4
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RandomFuBenMag), "AddFuBenJsonNode")]
        private static bool RandomFuBenMag_AddFuBenJsonNode_Prefix(
            FuBenMap map,
            List<JToken> EventRandomJson,
            JToken FuBenJson,
            string type,
            List<int> listEventX,
            List<int> listEventY
        )
        {
            int num = Main.Inst.readconfig("cheat19", "enable");
            bool flag = num != 1;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                List<JToken> list = new List<JToken>();
                int[] array = new int[]
                {
                    9002,
                    9006,
                    9007,
                    9008,
                    9009,
                    9013,
                    9014,
                    9015,
                    9016,
                    9001,
                    9004,
                    9003
                };
                foreach (JToken jtoken in EventRandomJson)
                {
                    string text = (string)jtoken["name"];
                    bool flag2 =
                        (
                            (int)jtoken["fenzu"] == 0
                            && Array.IndexOf<int>(array, (int)jtoken["talk"]) != -1
                        ) || text.Contains("有人");
                    if (flag2)
                    {
                        list.Add(jtoken);
                    }
                }
                foreach (JToken item in list)
                {
                    EventRandomJson.Remove(item);
                }
                List<int> list2 = new List<int>();
                int num2 = 0;
                int num3 = 0;
                while (num3 < listEventX.Count && num2 < 1000)
                {
                    num2++;
                    bool flag3 = EventRandomJson.Count <= 0;
                    if (flag3)
                    {
                        Debug.LogError("副本随机事件数量不足");
                        return false;
                    }
                    JToken randomListByPercent = Tools.instance.getRandomListByPercent(
                        EventRandomJson,
                        "percent"
                    );
                    bool flag4 = (int)randomListByPercent["duoci"] == 0;
                    if (flag4)
                    {
                        EventRandomJson.Remove(randomListByPercent);
                    }
                    list2.Add((int)randomListByPercent["id"]);
                    JObject jobject = new JObject();
                    jobject["ID"] = (int)randomListByPercent["id"];
                    jobject["Index"] = map.mapIndex[listEventX[num3], listEventY[num3]];
                    ((JArray)FuBenJson[type]).Add(jobject);
                    num3++;
                }
                result = false;
            }
            return result;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00007024 File Offset: 0x00005224
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmailDataMag), "YaoQingNpcToDongFu")]
        private static bool EmailDataMag_YaoQingNpcToDongFu_Prefix(EmailData emailData, object obj)
        {
            bool flag = PlayerEx.IsDaoLv(emailData.npcId);
            if (flag)
            {
                jsonData.instance.AvatarJsonData[emailData.npcId.ToString()].SetField(
                    "DongFuId",
                    (int)obj
                );
            }
            return true;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00007074 File Offset: 0x00005274
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EndlessSeaMag), "CreateFengBao")]
        private static bool EndlessSeaMag_CreateFengBao_Prefix()
        {
            int num = Main.Inst.readconfig("cheat18", "enable");
            bool flag = num == 1;
            return !flag;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x000070AC File Offset: 0x000052AC
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerEx), "AddSeaTanSuoDu")]
        private static bool PlayerEx_AddSeaTanSuoDu_Prefix(ref int value)
        {
            int num = Main.Inst.readconfig("cheat14", "enable");
            bool flag = num == 1;
            if (flag)
            {
                value *= 2;
            }
            return true;
        }

        // Token: 0x0600001A RID: 26 RVA: 0x000070E4 File Offset: 0x000052E4
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerEx), "AddShengWang")]
        private static bool PlayerEx_AddShengWang_Prefix(ref int add)
        {
            int num = Main.Inst.readconfig("cheat15", "enable");
            bool flag = num == 1;
            if (flag)
            {
                add *= 5;
            }
            return true;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x0000711C File Offset: 0x0000531C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BiaoBaiManager), "CalcBiaoBaiScore")]
        private static void BiaoBaiManager_CalcBiaoBaiScore_Postfix()
        {
            int num = Main.Inst.readconfig("cheat13", "enable");
            bool flag = num == 1;
            if (flag)
            {
                BiaoBaiManager.BiaoBaiScore.OtherTotalScore = 0;
                BiaoBaiManager.BiaoBaiScore.TotalScore = 0;
                BiaoBaiManager.BiaoBaiScore.OtherTotalScore += BiaoBaiManager
                    .BiaoBaiScore
                    .FavorScore;
                BiaoBaiManager.BiaoBaiScore.OtherTotalScore += BiaoBaiManager
                    .BiaoBaiScore
                    .ZhengXieScore;
                BiaoBaiManager.BiaoBaiScore.OtherTotalScore += BiaoBaiManager
                    .BiaoBaiScore
                    .DongFuScore;
                BiaoBaiManager.BiaoBaiScore.TotalScore += BiaoBaiManager
                    .BiaoBaiScore
                    .OtherTotalScore;
                BiaoBaiManager.BiaoBaiScore.TotalScore += BiaoBaiManager.BiaoBaiScore.DaTiScore;
                bool flag2 =
                    BiaoBaiManager.BiaoBaiScore.TotalScore >= 200
                    && UINPCJiaoHu.Inst.NowJiaoHuNPC.ShouYuan < 5000;
                if (flag2)
                {
                    new NpcSetField().AddNpcShouYuan(UINPCJiaoHu.Inst.NowJiaoHuNPC.ID, 5000);
                }
            }
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00007238 File Offset: 0x00005438
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TryinitFungaus), "OnEnter")]
        private static bool TryinitFungaus_OnEnter_Prefix(TryinitFungaus __instance)
        {
            Avatar player = Tools.instance.getPlayer();
            Flowchart flowchart = __instance.GetFlowchart();
            __instance.setHasVariable("ShenShi", player.shengShi, flowchart);
            __instance.setHasVariable("JinJie", (int)player.level, flowchart);
            __instance.setHasVariable("DunSu", player.dunSu, flowchart);
            __instance.setHasVariable("ZiZhi", player.ZiZhi, flowchart);
            __instance.setHasVariable("WuXin", (int)player.wuXin, flowchart);
            __instance.setHasVariable("ShaQi", (int)player.shaQi, flowchart);
            __instance.setHasVariable("MenPai", (int)player.menPai, flowchart);
            __instance.setHasVariable("ChengHao", player.chengHao, flowchart);
            int num = Main.Inst.readconfig("cheat10", "enable");
            bool flag = num == 1;
            if (flag)
            {
                bool flag2 = player.menPai > 0;
                if (flag2)
                {
                    __instance.setHasVariable("Sex", player.Sex, flowchart);
                    int num2 = Main.Inst.readconfig("cheat17", "enable");
                    bool flag3 = num2 == 1;
                    if (flag3)
                    {
                        bool flag4 = false;
                        foreach (SkillItem skillItem in player.equipStaticSkillList)
                        {
                            bool flag5 = skillItem.itemId == 804;
                            if (flag5)
                            {
                                flag4 = true;
                                break;
                            }
                        }
                        bool flag6 = flag4;
                        if (flag6)
                        {
                            bool flag7 =
                                Array.IndexOf<int>(
                                    new int[] { 1, 3, 4, 2, 5, 12, 14, 16, 15, 11 },
                                    player.NowMapIndex
                                ) != -1;
                            if (flag7)
                            {
                                bool flag8 = player.NowMapIndex == 1 || player.NowMapIndex == 12;
                                if (flag8)
                                {
                                    __instance.setHasVariable("MenPai", 1, flowchart);
                                }
                                bool flag9 = player.NowMapIndex == 3 || player.NowMapIndex == 14;
                                if (flag9)
                                {
                                    __instance.setHasVariable("MenPai", 3, flowchart);
                                }
                                bool flag10 = player.NowMapIndex == 4 || player.NowMapIndex == 16;
                                if (flag10)
                                {
                                    __instance.setHasVariable("MenPai", 4, flowchart);
                                }
                                bool flag11 = player.NowMapIndex == 2 || player.NowMapIndex == 15;
                                if (flag11)
                                {
                                    __instance.setHasVariable("MenPai", 5, flowchart);
                                }
                                bool flag12 = player.NowMapIndex == 5 || player.NowMapIndex == 11;
                                if (flag12)
                                {
                                    __instance.setHasVariable("MenPai", 6, flowchart);
                                }
                            }
                            else
                            {
                                __instance.setHasVariable("MenPai", (int)player.menPai, flowchart);
                            }
                        }
                    }
                }
                else
                {
                    __instance.setHasVariable("Sex", 2, flowchart);
                }
            }
            else
            {
                __instance.setHasVariable("Sex", player.Sex, flowchart);
            }
            int num3 = Main.Inst.readconfig("cheat11", "enable");
            bool flag13 = num3 == 1;
            if (flag13)
            {
                bool flag14 = player.NowMapIndex == 21;
                if (flag14)
                {
                    __instance.setHasVariable("JinJie", 11, flowchart);
                }
            }
            __instance.Continue();
            return false;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x0000756C File Offset: 0x0000576C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NPCEx), "ZengLiToNPC")]
        private static void NPCEx_ZengLiToNPC_Postfix(UINPCData npc, item item, int count)
        {
            int num = Main.Inst.readconfig("cheat9", "enable");
            int num2 = Main.Inst.readconfig("cheat9", "effect");
            bool flag = num == 1;
            if (flag)
            {
                bool flag2 = num2 < 1;
                if (flag2)
                {
                    num2 = 1;
                }
                bool flag3 = num2 > 10000;
                if (flag3)
                {
                    num2 = 10000;
                }
                bool flag4 = item.itemID == 5404;
                if (flag4)
                {
                    new NpcSetField().AddNpcShouYuan(npc.ID, num2);
                    UIPopTip.Inst.Pop(
                        string.Concat(
                            new string[] { "你的天命让", npc.Name, "的寿元提升了", num2.ToString(), "年。" }
                        ),
                        1
                    );
                }
            }
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00007638 File Offset: 0x00005838
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Avatar), "AddTime")]
        private static bool Avatar_AddTime_Prefix(Avatar __instance)
        {
            int num = Main.Inst.readconfig("cheat8", "enable");
            bool flag = num == 1;
            if (flag)
            {
                bool flag2 = __instance.NowMapIndex == 25 && Tools.getRandomInt(1, 10) >= 5;
                if (flag2)
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00007690 File Offset: 0x00005890
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapPlayerSeaShow), "SetShiYe")]
        private static void MapPlayerSeaShow_SetShiYe_Postfix(MapPlayerSeaShow __instance)
        {
            int num = Main.Inst.readconfig("cheat5", "enable");
            bool flag = num == 1;
            if (flag)
            {
                __instance.SeaZheZhao.gameObject.SetActive(false);
            }
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000076D0 File Offset: 0x000058D0
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapSeaCompent), "GetAddTimeNum")]
        private static void MapSeaCompent_GetAddTimeNum_Postfix(ref int __result)
        {
            int num = Main.Inst.readconfig("cheat7", "enable");
            int num2 = Main.Inst.readconfig("cheat7", "effect");
            bool flag = num == 1;
            if (flag)
            {
                bool flag2 = num2 < 1;
                if (flag2)
                {
                    num2 = 1;
                }
                __result /= num2;
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00007724 File Offset: 0x00005924
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tools), "CalcLingWuOrTuPoTime")]
        private static void Tools_CalcLingWuOrTuPoTime_Postfix(ref int __result)
        {
            try
            {
                int num = Main.Inst.readconfig("cheat1", "enable");
                int num2 = Main.Inst.readconfig("cheat1", "effect");
                bool flag = num == 1;
                if (flag)
                {
                    bool flag2 = num2 <= 0;
                    if (flag2)
                    {
                        num2 = 1;
                    }
                    __result /= num2;
                    bool flag3 = __result <= 0;
                    if (flag3)
                    {
                        __result = 1;
                    }
                }
            }
            catch
            {
                __result++;
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000077B0 File Offset: 0x000059B0
        [HarmonyPrefix]
        [HarmonyPatch(typeof(JSONObject), "SetField", new Type[] { typeof(string), typeof(int) })]
        private static bool JSONObject_SetField_Prefix(string name, int val, JSONObject __instance)
        {
            int num = Main.Inst.readconfig("cheat4", "enable");
            bool flag = num == 1;
            bool result;
            if (flag)
            {
                bool flag2 = name == "NaiJiu";
                if (flag2)
                {
                    val = 100;
                }
                __instance.SetField(name, JSONObject.Create(val));
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        // Token: 0x06000023 RID: 35 RVA: 0x0000780C File Offset: 0x00005A0C
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIMapSea), "CalcQuickMove")]
        private static void UIMapSea_CalcQuickMove_Postfix(ref SeaQuickMoveData __result)
        {
            int num = Main.Inst.readconfig("cheat6", "enable");
            int num2 = Main.Inst.readconfig("cheat6", "effect");
            bool flag = num == 1;
            if (flag)
            {
                bool flag2 = num2 < 1;
                if (flag2)
                {
                    num2 = 1;
                }
                __result.CostDaySum /= num2;
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00007868 File Offset: 0x00005A68
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WuDaoMag), "CalcGanWuTime")]
        private static void WuDaoMag_CalcGanWuTime_Postfix(ref int __result)
        {
            try
            {
                int num = Main.Inst.readconfig("cheat2", "enable");
                int num2 = Main.Inst.readconfig("cheat2", "effect");
                bool flag = num == 1;
                if (flag)
                {
                    bool flag2 = num2 <= 0;
                    if (flag2)
                    {
                        num2 = 1;
                    }
                    __result /= num2;
                    bool flag3 = __result <= 1;
                    if (flag3)
                    {
                        __result = 2;
                    }
                }
            }
            catch
            {
                __result++;
            }
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000078F4 File Offset: 0x00005AF4
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), "GetRandomLingQiTypes")]
        private static bool RoundManager_GetRandomLingQiTypes_Prefix(Avatar avatar, ref int count)
        {
            bool flag = count <= 0;
            if (flag)
            {
                count = 1;
            }
            Avatar avatar2 = (Avatar)KBEngineApp.app.entities[11];
            int num = Main.Inst.readconfig("cheat3", "enable");
            int num2 = Main.Inst.readconfig("cheat3", "effect");
            bool flag2 = num2 <= 0;
            if (flag2)
            {
                num2 = 1;
            }
            bool flag3 = num == 1 && avatar2.name == "木谷上人";
            if (flag3)
            {
                count += num2;
            }
            return true;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00007994 File Offset: 0x00005B94
        private int readconfig(string s1, string s2)
        {
            int result;
            try
            {
                result = this.testobj["Cheat"][s1][s2].I;
            }
            catch
            {
                result = 1;
            }
            return result;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000079E0 File Offset: 0x00005BE0
        private void initconfig()
        {
            string path = Application.dataPath + "/../新世界MOD配置.txt";
            JSONObject jsonobject = new JSONObject();
            this.teststring = this.teststring.Replace("'", "\"");
            try
            {
                bool flag = !File.Exists(path);
                if (flag)
                {
                    File.WriteAllText(path, this.teststring);
                    jsonobject = new JSONObject(this.teststring, -2, false, false);
                }
                else
                {
                    jsonobject = new JSONObject(File.ReadAllText(path), -2, false, false);
                    bool flag2 = jsonobject == null;
                    if (flag2)
                    {
                        File.WriteAllText(path, this.teststring);
                        jsonobject = new JSONObject(this.teststring, -2, false, false);
                    }
                }
                this.testobj = jsonobject;
                for (int i = 1; i <= 27; i++)
                {
                    int i2 = this.testobj["Cheat"]["cheat" + i.ToString()]["enable"].I;
                }
            }
            catch
            {
                File.WriteAllText(path, this.teststring);
                this.testobj = new JSONObject(this.teststring, -2, false, false);
            }
        }

        // Token: 0x04000001 RID: 1
        private static Main Inst;

        // Token: 0x04000002 RID: 2
        private GameObject go;

        // Token: 0x04000003 RID: 3
        private JSONObject testobj;

        // Token: 0x04000004 RID: 4
        private string teststring =
            "{\r\n\t'Cheat': {\r\n\t\t'cheat1': {\r\n\t\t\t'name': '领悟突破加速',\r\n\t\t\t'tip': '开启后默认时间缩短30倍，enable设为1则作弊开启，设为0则关闭此功能',\r\n\t\t\t'effect': 30,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat2': {\r\n\t\t\t'name': '感悟思绪加速',\r\n\t\t\t'tip': '开启后默认时间缩短100倍，enable设为1则作弊开启，设为0则关闭此功能',\r\n\t\t\t'effect': 100,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat3': {\r\n\t\t\t'name': '突破灵气增加',\r\n\t\t\t'tip': '突破时每次多吸收30点灵气，enable设为1则作弊开启，设为0则关闭此功能',\r\n\t\t\t'effect': 100,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat4': {\r\n\t\t\t'name': '丹炉灵舟耐久锁定',\r\n\t\t\t'tip': '锁定数值100不减，但丹炉如果一次性超过100的消耗还是会爆炸',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat5': {\r\n\t\t\t'name': '海上视野打开',\r\n\t\t\t'tip': '海上全明朗，护眼',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat6': {\r\n\t\t\t'name': '海上快速移动消耗',\r\n\t\t\t'tip': '默认消耗降低20倍',\r\n\t\t\t'effect': 20,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat7': {\r\n\t\t\t'name': '海上WASD移动消耗',\r\n\t\t\t'tip': '默认降低30倍',\r\n\t\t\t'effect': 30,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat8': {\r\n\t\t\t'name': '天魔眼地图延时',\r\n\t\t\t'tip': '方便一次性触发所有剧情和完成更多的杀魔数量',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat9': {\r\n\t\t\t'name': '给丹延寿',\r\n\t\t\t'tip': '给NPC赠送【5品延命丹】可以让NPC增寿500年',\r\n\t\t\t'effect': 500,\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat10': {\r\n\t\t\t'name': '开放男同胞入星河派',\r\n\t\t\t'tip': '无',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat11': {\r\n\t\t\t'name': '（争对你）天机大比不限境界',\r\n\t\t\t'tip': '你可以不限境界的参与',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat12': {\r\n\t\t\t'name': '（争对NPC）天机大比不限境界',\r\n\t\t\t'tip': 'NPC可以不限境界的参与',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat13': {\r\n\t\t\t'name': '更容易的表白',\r\n\t\t\t'tip': 'NPC不嫌弃你的年纪 境界 道侣数量 以及你（她）是否已满18岁',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat14': {\r\n\t\t\t'name': '海上探索度更容易获得',\r\n\t\t\t'tip': '海上探索度2倍',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat15': {\r\n\t\t\t'name': '声望更容易获得',\r\n\t\t\t'tip': '声望获得5倍',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat16': {\r\n\t\t\t'name': '是否开启查看失联人员',\r\n\t\t\t'tip': '是否开启查看失联人员，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat17': {\r\n\t\t\t'name': '天下门派是一家',\r\n\t\t\t'tip': '是否开放全门派可入，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat18': {\r\n\t\t\t'name': '去掉海面风暴',\r\n\t\t\t'tip': '是否去掉海面风暴，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat19': {\r\n\t\t\t'name': '地洞事件过滤',\r\n\t\t\t'tip': '地洞事件过滤掉非物品事件，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat20': {\r\n\t\t\t'name': '拍卖会加速',\r\n\t\t\t'tip': '拍卖会加速，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat21': {\r\n\t\t\t'name': '拍卖会可刷新清单',\r\n\t\t\t'tip': '拍卖会可刷新清单，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat22': {\r\n\t\t\t'name': '拍卖会物品不重复随机',\r\n\t\t\t'tip': '拍卖会物品不重复随机，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat23': {\r\n\t\t\t'name': '地洞探索物品全随机获取',\r\n\t\t\t'tip': '地洞探索物品全随机获取，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat24': {\r\n\t\t\t'name': '风雨楼可悬赏化神',\r\n\t\t\t'tip': '风雨楼可悬赏化神，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat25': {\r\n\t\t\t'name': '随时刷新悬赏榜',\r\n\t\t\t'tip': '随时刷新悬赏榜，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat26': {\r\n\t\t\t'name': '激活传音远程拍卖功能',\r\n\t\t\t'tip': '激活传音远程拍卖功能，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t},\r\n\t\t'cheat27': {\r\n\t\t\t'name': '全NPC开放功能',\r\n\t\t\t'tip': '全NPC开放功能，1开启，0关闭',\r\n\t\t\t'enable': 1\r\n\t\t}\r\n\t}\r\n}";

        // Token: 0x02000004 RID: 4
        private struct FunObj_npcid
        {
            // Token: 0x04000007 RID: 7
            public int npcid;
        }
    }
}
