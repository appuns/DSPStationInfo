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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPStationInfo
{

    [BepInPlugin("Appun.DSP.plugin.StationInfo", "DSPStationInfo", "0.4.1")]
    [BepInProcess("DSPGAME.exe")]



    public class Main : BaseUnityPlugin
    {
        public static ConfigEntry<bool> ShowStationInfo;
        //public static ConfigEntry<int> maxCount;
        public static int maxCount = 10;



        //public static bool showSignButton = true;

        public static int count;


        public void Start()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            //configの設定
            ShowStationInfo = Config.Bind("General", "ShowStationInfo", false, "Show Station Information");
            //maxCount = Config.Bind("General", "maxCount", 200, "Inventory Column Count");

            UI.create();
        }


        //signButtonボタンイベント
        public static void OnSignButtonClick()
        {
            ShowStationInfo.Value = !ShowStationInfo.Value;
            UI.signButton.GetComponent<UIButton>().highlighted = ShowStationInfo.Value;

            if (!ShowStationInfo.Value)
            {
                for (int i = 0; i < maxCount; i++)
                {
                    UI.tip[i].SetActive(false);
                }
            }
        }


        [HarmonyPatch(typeof(UIGameMenu), "OnButton1Click")]
        public static class UIGameMenu_OnButton1Click_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                UI.signButton.transform.Find("icon").GetComponent<Image>().sprite = LDB.techs.Select(1604).iconSprite;
                if (ShowStationInfo.Value)
                {
                    UI.signButton.GetComponent<UIButton>().highlighted = true;
                }
                else
                {
                    UI.signButton.GetComponent<UIButton>().highlighted = false;
                }
            }
        }

        public void Update()
        {
            UIStorageWindow storageWindow = UIRoot.instance.uiGame.storageWindow;


            if (!ShowStationInfo.Value)
            {
                return;
            }
            if (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Globe || UIGame.viewMode == EViewMode.Build)
            {
                UI.stationTip.SetActive(true);
            }
            else
            {
                UI.stationTip.SetActive(false);
                return;
            }
            if (GameMain.data == null)
            {
                return;
            }
            PlanetData localPlanet = GameMain.data.localPlanet;
            if (localPlanet == null)
            {
                return;
            }
            if (localPlanet.factory == null)
            {
                return;
            }
            var count = 0;

            Vector3 localPosition = GameCamera.main.transform.localPosition;
            Vector3 forward = GameCamera.main.transform.forward;

            float realRadius = localPlanet.realRadius;    //高さ

            LogManager.Logger.LogInfo("------------------------------------------------------------------start");


            if (localPlanet.factory.transport.stationPool.Length > 0)
            {
                LogManager.Logger.LogInfo("------------------------------localPlanet.factory.transport.stationPool.Length : " + localPlanet.factory.transport.stationPool.Length);
                foreach (StationComponent station in localPlanet.factory.transport.stationPool)
                {
                    //LogManager.Logger.LogInfo("count : " + count);
                    if (count == maxCount)
                    {
                        maxCount++;
                        GameObject tiptmp = Instantiate(UI.tipPrefab, UI.stationTip.transform) as GameObject;

                        //UI.tip.Concat(new GameObject[] { UI.tiptmp }).ToArray();
                        Array.Resize<GameObject>(ref UI.tip, maxCount);
                        //LogManager.Logger.LogInfo("Array.Resize : " + UI.tip.Length);
                        UI.tip[maxCount - 1] = Instantiate(UI.tipPrefab, UI.stationTip.transform) as GameObject;

                    }



                    if (station != null)
                    {
                        if (station.storage != null)
                        {
                            Vector3 vector;
                            int maxStorage;
                            if (station.isVeinCollector)
                            {
                                vector = localPlanet.factory.entityPool[station.entityId].pos.normalized * (realRadius + 10f);
                                maxStorage = 1;
                            }
                            else if (station.isCollector)
                            {
                                vector = localPlanet.factory.entityPool[station.entityId].pos.normalized * (realRadius + 35f);
                                maxStorage = 2;
                            }
                            else if (station.isStellar)
                            {
                                vector = localPlanet.factory.entityPool[station.entityId].pos.normalized * (realRadius + 20f);
                                maxStorage = 5;
                            }
                            else
                            {
                                vector = localPlanet.factory.entityPool[station.entityId].pos.normalized * (realRadius + 15f);
                                maxStorage = 3;
                            }

                            Vector3 vector2 = vector - localPosition;
                            float magnitude = vector2.magnitude;
                            float num = Vector3.Dot(forward, vector2);

                            if (magnitude < 1f || num < 1f)
                            {
                                UI.tip[count].SetActive(false);
                            }
                            else
                            {
                                Vector2 vector3;
                                bool flag = UIRoot.ScreenPointIntoRect(GameCamera.main.WorldToScreenPoint(vector), UI.stationTip.GetComponent<RectTransform>(), out vector3);
                                if (Mathf.Abs(vector3.x) > 8000f)
                                {
                                    flag = false;
                                }
                                if (Mathf.Abs(vector3.y) > 8000f)
                                {
                                    flag = false;
                                }
                                RCHCPU rchcpu;
                                if (Phys.RayCastSphere(localPosition, vector2 / magnitude, magnitude, Vector3.zero, realRadius, out rchcpu))
                                {
                                    flag = false;
                                }
                                if (flag)
                                {


                                    vector3.x = Mathf.Round(vector3.x);
                                    vector3.y = Mathf.Round(vector3.y);

                                    UI.tip[count].GetComponent<RectTransform>().anchoredPosition = vector3;

                                    LogManager.Logger.LogInfo("station : " + station.id);
                                    LogManager.Logger.LogInfo("count : " + count);



                                    for (int j = 0; j < maxStorage; j++)
                                    {
                                        if (station.storage[j].itemId != 0)
                                        {
                                            //枠のサイズ調整
                                            UI.tip[count].GetComponent<RectTransform>().sizeDelta = new Vector2(100, maxStorage * 30 + 10 );

                                            //アイコンの表示
                                            UI.tip[count].transform.Find("icon" + j).GetComponent<Image>().sprite = LDB.items.Select(station.storage[j].itemId).iconSprite;
                                            UI.tip[count].transform.Find("icon" + j).gameObject.SetActive(true);

                                            //個数の表示
                                            UI.tip[count].transform.Find("countText" + j).GetComponent<Text>().text = station.storage[j].count.ToString("#,##0");

                                            //色の指定示
                                            if ((station.storage[j].count * 10) < station.storage[j].max)
                                            {
                                                UI.tip[count].transform.Find("countText" + j).GetComponent<Text>().color = new Color(1, 0.45f, 0.2f, 1);
                                            }
                                            else
                                            {
                                                UI.tip[count].transform.Find("countText" + j).GetComponent<Text>().color = Color.white;
                                            }
                                            UI.tip[count].transform.Find("countText" + j).gameObject.SetActive(true);
                                            UI.tip[count].SetActive(true);
                                        }
                                        else
                                        {
                                            UI.tip[count].transform.Find("icon" + j).gameObject.SetActive(false);
                                            UI.tip[count].transform.Find("countText" + j).GetComponent<Text>().text = "-------------";
                                            UI.tip[count].transform.Find("countText" + j).GetComponent<Text>().color = Color.white;
                                            UI.tip[count].transform.Find("countText" + j).gameObject.SetActive(true);
                                        }
                                        //サイズの調整
                                        //if (UIGame.viewMode == EViewMode.Normal)
                                        //{
                                        if (magnitude < 50)
                                        {
                                            UI.tip[count].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                                        }
                                        else if (magnitude < 250)
                                        {
                                            float num2 = (float)(1.75 - magnitude * 0.005);
                                            UI.tip[count].transform.localScale = new Vector3(1, 1, 1) * num2;
                                        }
                                        else
                                        {
                                            UI.tip[count].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                        }
                                        //} else 
                                        //{
                                        //    UI.tip[count].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                        //}

                                    }
                                    //LogManager.Logger.LogInfo("count++ : " + count);

                                    for (int j = maxStorage; j < 5; j++)
                                    {
                                        UI.tip[count].transform.Find("icon" + j).gameObject.SetActive(false);
                                        UI.tip[count].transform.Find("countText" + j).gameObject.SetActive(false);
                                    }
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
            for (int i = count; i < Main.maxCount; i++)
            {
                UI.tip[i].SetActive(false);
            }
        }
    }

    public class LogManager
    {
        public static ManualLogSource Logger;
    }

}