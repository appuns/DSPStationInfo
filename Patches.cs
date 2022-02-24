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

    }
}
