using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.GUILayout;
using UnityEngine.Rendering;
using Steamworks;
using rail;
using xiaoye97;

namespace DSPStationInfo
{


    [HarmonyPatch]
    class Patches
    {


        [HarmonyPostfix,HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static void VFPreload_InvokeOnLoadWorkEnded_Patch()
        {
            //ギガステーションからの情報取得

            if (Harmony.HasAnyPatches("org.kremnev8.plugin.GigaStationsUpdated"))
            {
                Main.gigaStationEnable = true;
                Main.gigaPLSMaxStorages = LDB.items.Select(2110).prefabDesc.stationMaxItemKinds;
                Main.gigaILSMaxStorages = LDB.items.Select(2111).prefabDesc.stationMaxItemKinds;
                Main.maxKinds = Math.Max(Main.gigaPLSMaxStorages, Main.gigaILSMaxStorages);
                LogManager.Logger.LogInfo("GigaStation.plsMaxStorages : " + Main.gigaPLSMaxStorages);
                LogManager.Logger.LogInfo("GigaStation.ilsMaxStorages : " + Main.gigaILSMaxStorages);

            }
            else
            {
                LogManager.Logger.LogInfo("No GigaStation");
            }

            //UI作成
            UI.create();

        }

        //public static bool isDismantling = false;

        [HarmonyPostfix,HarmonyPatch(typeof(UIGameMenu), "OnButton1Click")]
        public static void UIGameMenu_OnButton1Click_PostPatch()
        {
            UI.signButton.transform.Find("icon").GetComponent<Image>().sprite = LDB.techs.Select(1604).iconSprite;
            if (Main.ShowStationInfo.Value)
            {
                UI.signButton.GetComponent<UIButton>().highlighted = true;
            }
            else
            {
                UI.signButton.GetComponent<UIButton>().highlighted = false;
            }
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "DoDismantleObject")]
        //public static void PlayerAction_DoDismantleObject_PrePatch()
        //{
        //    isDismantling = true;

        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), "DoDismantleObject")]
        //public static void PlayerAction_DoDismantleObject_PostPatch()
        //{
        //    isDismantling = false;

        //}

    }
}
