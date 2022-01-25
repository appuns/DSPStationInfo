﻿using BepInEx;
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

//[module: UnverifiableCode]
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPStationInfo
{

    [BepInPlugin("Appun.DSP.plugin.StationInfo", "DSPStationInfo", "0.4.2")]
    [BepInProcess("DSPGAME.exe")]



    public class Main : BaseUnityPlugin
    {
        public static ConfigEntry<bool> ShowStationInfo;
        public static ConfigEntry<int> maxInfo;

        public struct Station
        {
            public Vector2 UIpos;
            public int id;
            public float dist;
            public int maxStorage;
            public float num;
        }

        public static List<Station> stationList = new List<Station>();


        public void Start()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            //configの設定
            ShowStationInfo = Config.Bind("General", "ShowStationInfo", false, "Show Station Information");
            maxInfo = Config.Bind("General", "maxInfo", 20, "Maximum number of station information to be displayed");

            //UI作成
            UI.create();


        }


        //signButtonボタンイベント
        public static void OnSignButtonClick()
        {
            ShowStationInfo.Value = !ShowStationInfo.Value;
            UI.signButton.GetComponent<UIButton>().highlighted = ShowStationInfo.Value;

            if (!ShowStationInfo.Value)
            {
                for (int i = 0; i < maxInfo.Value; i++)
                {
                    UI.tipBox[i].SetActive(false);
                }
            }
        }


        //メインの処理
        public void Update()
        {


            UIStorageWindow storageWindow = UIRoot.instance.uiGame.storageWindow;


            if (!ShowStationInfo.Value)
            {
                return;
            }
            if (UIGame.viewMode != EViewMode.Normal && UIGame.viewMode != EViewMode.Globe && UIGame.viewMode != EViewMode.Build)
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
            UI.stationTip.SetActive(true);

            Vector3 localPosition = GameCamera.main.transform.localPosition;
            Vector3 forward = GameCamera.main.transform.forward;
            StationComponent[] stationPool = localPlanet.factory.transport.stationPool;

            float realRadius = localPlanet.realRadius;    //惑星の半径

            //LogManager.Logger.LogInfo("------------------------------------------------------------------start");


            if (stationPool.Length > 0)
            {
                //LogManager.Logger.LogInfo("------------------------------localPlanet.factory.transport.stationPool.Length : " + stationPool.Length);
                for (int i = 0; i < stationPool.Length; i++)
                {
                    StationComponent station = stationPool[i];
                    if (station != null)
                    {
                        //LogManager.Logger.LogInfo("------------------------------------------------------------station.id : " + stationPool[i].id);
                        Vector3 vector;
                        int maxStorage;
                        //ステーションの種類によって高さ調節
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
                        //LogManager.Logger.LogInfo("------------------------------------------------------------maxStorage : " + maxStorage);

                        Vector3 vector2 = vector - localPosition;
                        float magnitude = vector2.magnitude;
                        float num = Vector3.Dot(forward, vector2);

                        //if (magnitude < 1f || num < 0.5f) //見えてるか？
                        //{
                        //    break;
                        //}
                        //else
                        //{
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
                            if (flag) //見えたら？
                            {
                                Station st = new Station();
                                st.UIpos.x = Mathf.Round(vector3.x);
                                st.UIpos.y = Mathf.Round(vector3.y);
                                st.id = station.id;
                                st.dist = magnitude;
                                st.maxStorage = maxStorage;
                                st.num = num;
                                stationList.Add(st);
                                //LogManager.Logger.LogInfo("------------------------------------------------------------------stationList.Add");
                            }
                        //}
                    }
                }
                //LogManager.Logger.LogInfo("------------------------------------------------------------------stationList created " + stationList.Count);

                //距離順に並べる
                stationList.Sort((a, b) => a.dist.CompareTo(b.dist));
                //LogManager.Logger.LogInfo("------------------------------------------------------------------stationList created " + stationList.Count);
                //11個目以降を削除
                if (stationList.Count > maxInfo.Value)
                {
                    stationList.RemoveRange(maxInfo.Value, stationList.Count - maxInfo.Value);
                }
                //LogManager.Logger.LogInfo("------------------------------------------------------------------sorted and deleted " + stationList.Count);

                for (int i = 0; i < stationList.Count; i++)
                {
                    int k = stationList.Count - i - 1;

                    StationComponent staionComp = stationPool[stationList[k].id];


                    if (staionComp.storage != null)
                    {

                        //LogManager.Logger.LogInfo($"---{i}----{stationList[k].id}--- {stationList[k].UIpos.x} , {stationList[k].UIpos.y} --{stationList[k].dist}---- ");
                        UI.tipBox[i].GetComponent<RectTransform>().anchoredPosition = stationList[k].UIpos;

                        //位置調整以外の処理を間引いて処理軽減
                        //if (Time.frameCount % 10 == 0)
                        //{
                        //枠のサイズ調整
                        UI.tipBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(100, stationList[k].maxStorage * 30 + 10);

                        for (int j = 0; j < stationList[k].maxStorage; j++)
                        {
                            //maxStorageより大きいスロット非アクティブ化
                            if (j < stationList[k].maxStorage)
                            {
                                //maxStorageより大きい場合はデータ書き込み
                                //アイテムが設定してある場合
                                if (staionComp.storage[j].itemId != 0)
                                {
                                    //アイコンの表示
                                    UI.tipIcon[i, j].sprite = LDB.items.Select(staionComp.storage[j].itemId).iconSprite;
                                    UI.tipIcon[i, j].gameObject.SetActive(true);

                                    //テキスト個数の表示
                                    UI.tipCounter[i, j].text = staionComp.storage[j].count.ToString("#,##0");
                                    //UI.tipCounter[i, j].text = stationList[k].num.ToString("F");
                                    //テキスト色の指定示
                                    if ((staionComp.storage[j].count * 10) < staionComp.storage[j].max)
                                    {
                                        UI.tipCounter[i, j].color = new Color(1, 0.45f, 0.2f, 1);
                                    }
                                    else
                                    {
                                        UI.tipCounter[i, j].color = Color.white;
                                    }
                                    UI.tipCounter[i, j].gameObject.SetActive(true);
                                }
                                else
                                {
                                    //アイテムが設定してない場合
                                    UI.tipIcon[i, j].gameObject.SetActive(false);
                                    UI.tipCounter[i, j].text = "-------------";
                                    UI.tipCounter[i, j].color = Color.white;
                                    UI.tipCounter[i, j].gameObject.SetActive(true);
                                }
                            }
                            else
                            {
                                UI.tipIcon[i, j].gameObject.SetActive(false);
                                UI.tipCounter[i, j].gameObject.SetActive(false);
                            }
                            UI.tipBox[i].SetActive(true);

                        }
                        ////距離によって大きさの調整
                        if (stationList[k].dist < 50)
                        {
                            UI.tipBox[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        }
                        else if (stationList[k].dist < 250)
                        {
                            float num2 = (float)(1.75 - stationList[k].dist * 0.005);
                            UI.tipBox[i].transform.localScale = new Vector3(1, 1, 1) * num2;
                        }
                        else
                        {
                            UI.tipBox[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        }
                        //}
                    }
                }
                for (int i = stationList.Count; i < maxInfo.Value; i++)
                {
                    UI.tipBox[i].SetActive(false);
                }
                stationList.Clear();
            }
        }
    }

    public class LogManager
    {
        public static ManualLogSource Logger;
    }

}