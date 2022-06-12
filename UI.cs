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
    class UI : MonoBehaviour
    {

        public static GameObject[] tipBox = new GameObject[Main.maxInfo.Value];
        public static Text[,] tipCounter = new Text[Main.maxInfo.Value, Main.maxKinds];
        public static Image[,] tipIcon = new Image[Main.maxInfo.Value, Main.maxKinds];

        public static GameObject stationTip;
        public static GameObject tipPrefab;
        public static GameObject signButton;

        public static void create()
        {

            //LogManager.Logger.LogInfo("------------------------------------------------------------------Main.maxInfo.Value : " + Main.maxInfo.Value);
            //LogManager.Logger.LogInfo("------------------------------------------------------------------Main.maxKinds : " + Main.maxKinds);


            //LogManager.Logger.LogInfo("------------------------------------------------------------------start create");

            stationTip = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks"), GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs").transform) as GameObject;
            stationTip.name = "stationTip";
            Destroy(stationTip.GetComponent<UIVeinDetail>());

            //ボタンの作成
            signButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/detail-func-group/dfunc-1"), GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/detail-func-group").transform) as GameObject;
            signButton.name = "signButton";
            signButton.transform.localPosition = GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/detail-func-group/dfunc-4").transform.localPosition;
            //signButton.transform.localPosition = new Vector3(-155, 163, 0);
            signButton.GetComponent<UIButton>().tips.tipTitle = "Station Contents".Translate(); // "ステーション情報";
            signButton.GetComponent<UIButton>().tips.tipText = "Click to turn ON / OFF real-time contents of stations".Translate(); //"ステーションのストレージ情報を表示/非表示します。";
            GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/detail-func-group/dfunc-4").SetActive(false);

            //ボタンイベントの作成
            signButton.GetComponent<UIButton>().button.onClick.AddListener(new UnityAction(Main.OnSignButtonClick));


            //tipPrefab
            tipPrefab = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks/vein-tip-prefab"), stationTip.transform) as GameObject;
            tipPrefab.name = "tipPrefab";
            tipPrefab.GetComponent<Image>().sprite = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Key Tips/tip-prefab").GetComponent<Image>().sprite;
            tipPrefab.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);  //new Color(0.23f, 0.45f, 0.7f, 0.2f);
            tipPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 160);
            tipPrefab.GetComponent<Image>().enabled = true;
            //tipPrefab.transform.Find("info-text").GetComponent<Shadow>().enabled = true;
            tipPrefab.transform.localPosition = new Vector3(200, 800, 0);
            tipPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            Destroy(tipPrefab.GetComponent<UIVeinDetailNode>());
            tipPrefab.SetActive(false);

            //表示欄の作成
            for (int i = 0; i < Main.maxKinds; i++)
            {
                InstantiateTip(i);
            }
            tipPrefab.transform.Find("info-text").gameObject.SetActive(false);
            tipPrefab.transform.Find("icon").gameObject.SetActive(false);

            //tipの作成
            for (int i = 0; i < Main.maxInfo.Value; i++)
            {
                tipBox[i] = Instantiate(tipPrefab, stationTip.transform);
                for (int j = 0; j < Main.maxKinds; j++)

                {
                    tipCounter[i, j] = tipBox[i].transform.Find("countText" + j).GetComponent<Text>();
                    tipIcon[i, j] = tipBox[i].transform.Find("icon" + j).GetComponent<Image>();
                }
            }
            //LogManager.Logger.LogInfo("------------------------------------------------------------------created");

        }

        public static void InstantiateTip(int i)
        {
            GameObject valueText = GameObject.Find("UI Root/Overlay Canvas/In Game/Planet & Star Details/planet-detail-ui/res-group/res-entry/value-text").gameObject;

            GameObject text = Instantiate(tipPrefab.transform.Find("info-text").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
            text.name = "countText" + i;

            text.GetComponent<Text>().fontSize = 20;
            text.GetComponent<Text>().font = valueText.GetComponent<Text>().font;
            text.GetComponent<Text>().text = "99999";
            text.GetComponent<Text>().alignment = TextAnchor.MiddleRight;

            text.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 30);
            text.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            text.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            text.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            text.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -5 - 30 * i, 0);
            Destroy(text.GetComponent<Shadow>());

            GameObject icon = Instantiate(tipPrefab.transform.Find("icon").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
            icon.name = "icon" + i;
            icon.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            icon.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            icon.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            icon.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(5, -5 - 30 * i, 0);
        }

    }

}
