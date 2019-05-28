#if ENABLE_WINMD_SUPPORT && ENABLE_IL2CPP

using System;
using UnityEngine;

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;

using IL2CPPToDotNetBridge;

namespace AdDeals
{

    class IL2CPPBridge : IIL2CPPBridge
    {
        public void Send(string json)
        {
            AdDealsWrapperUWPIL2CPP.HandleEvent(json);
        }
    }

    public class AdDealsWrapperUWPIL2CPP : AdDealsWrapperBase
    {
        // disable EventHandler not used waring when unity compile
        public delegate void AdAvailableHandler(int adType, bool available);
        public delegate void AdEventHandler();
        public delegate void AdEventStringHandler(string error);
        public static event AdAvailableHandler AdAvailableEvent;
        public static event AdEventHandler SDKNotInitializedEvent;
        public static event AdEventHandler ShowAdVideoRewardGrantedEvent;
        public static event AdEventHandler ShowAdSucessEvent;
        public static event AdEventStringHandler ShowAdFailedEvent;
        public static event AdEventHandler CacheAdSuccessEvent;
        public static event AdEventStringHandler CacheAdFailedEvent;
        public static event AdEventHandler MinDelayBtwAdsNotReachedEvent;
        public static event AdEventHandler AdClosedTap;
        public static event AdEventHandler AdClickedTap;
        public static event AdEventHandler AdManagerInitSDKSuccess;
        public static event AdEventStringHandler AdManagerInitSDKFailed;
        public static event AdEventHandler AdManagerConsentSuccess;
        public static event AdEventStringHandler AdManagerConsentFailed;
        public static event AdEventHandler AdManagerAppDownloadSourceDetected;
        public static event AdEventHandler AdManagerAppSessionSourceDetected;

        private static bool hasInit = false;

        private static void Invoke(string f, params object[] arr)
        {
            var net = BridgeBootstrapper.GetDotNetBridge();
            //net = BridgeBootstrapper.GetIL2CPPBridge();
            if (null == net)
            {
                Debug.Log("ERROR! DotNetBridge is null");
                return;
            }
            JSONObject objP = new JSONObject(JSONObject.Type.ARRAY);
            foreach (var item in arr)
            {
                string s = item as string;
                if (null != s)
                {
                    objP.Add(s);
                }
                else if (IsNumber(item))
                {
                    objP.Add(Convert.ToInt32(item));
                }
                else if (item is bool)
                {
                    objP.Add(Convert.ToInt32((bool)item));
                }
            }
            JSONObject obj = new JSONObject(JSONObject.Type.OBJECT);
            obj.AddField("f", f);
            obj.AddField("p", objP);
            net.Send(obj.ToString());
        }



        /// <summary>
        /// <para> init AdDeals SDK</para>
        /// </summary>
        /// <param name="appID">appID</param>
        /// <param name="appKey">appKey</param>
        public static void Init(String appID, String appKey) {
            if (hasInit)
            {
                RunInUnityMainThread(() =>
                {
                    AdManagerInitSDKFailed.Invoke("AdDeals just need init once");
                });
                return;
            }
            BridgeBootstrapper.SetIL2CPPBridge(new IL2CPPBridge());

            Invoke("Init", appID, appKey);
            hasInit = true;
        }

        /// <summary>
        /// <para> set privacy policy consent</para>
        /// </summary>
        /// <param name="consent">consent, -1(NOT_APPLICABLE), 0(REVOKE), 1(GRANT)</param>
        public static void SetConsent(int consent)
        {
            Invoke("SetConsent", consent);
        }

        /// <summary>
        /// <para> check ad available</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void IsAvailable(int adType, int uiOrientation)
        {
            Invoke("IsAvailable", adType, uiOrientation);
        }

        /// <summary>
        /// <para> cache ad</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="placementID">in most cases just leave it ""</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void CacheAd(int adType, string placementID, int uiOrientation)
        {
            Invoke("CacheAd", adType, placementID, uiOrientation);
        }

        /// <summary>
        /// <para> show ad</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="placementID">in most cases just leave it ""</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void ShowAd(int adType, string placementID, int uiOrientation)
        {
            Invoke("ShowAd", adType, placementID, uiOrientation);
        }

        private static void RunInUnityMainThread(System.Action action)
        {
            UnityMainThreadDispatcher dispatcher = UnityMainThreadDispatcher.Instance();
            if (null == dispatcher)
            {
                Debug.Log("UnityMainThreadDispatcher is null, please add UnityMainThreadDispatcher.prefab to your scene");
                return;
            }
            dispatcher.Enqueue(action);
        }

        public static void HandleEvent(string json)
        {
            RunInUnityMainThread(() =>
            {
                JSONObject obj = new JSONObject(json);
                string f = obj["f"].str;
                var objP = obj["p"].list;
                switch (f)
                {
                    case "AdManagerInitSDKSuccess":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerInitSDKSuccess.Invoke();
                        break;
                    }
                    case "AdManagerInitSDKFailed":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerInitSDKFailed.Invoke(objP[0].str);
                        break;
                    }
                    case "AdManagerConsentSuccess":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerConsentSuccess.Invoke();
                        break;
                    }
                    case "AdManagerConsentFailed":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerConsentFailed.Invoke(objP[0].str);
                        break;
                    }
                    case "AdManagerAppDownloadSourceDetected":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerAppDownloadSourceDetected.Invoke();
                        break;
                    }
                    case "AdManagerAppSessionSourceDetected":
                    {
                        AdDealsWrapperUWPIL2CPP.AdManagerAppSessionSourceDetected.Invoke();
                        break;
                    }
                    case "SDKNotInitializedEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.SDKNotInitializedEvent.Invoke();
                        break;
                    }
                    case "ShowAdVideoRewardGrantedEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.ShowAdVideoRewardGrantedEvent.Invoke();
                        break;
                    }
                    case "ShowAdSucessEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.ShowAdSucessEvent.Invoke();
                        break;
                    }
                    case "ShowAdFailedEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.ShowAdFailedEvent.Invoke(objP[0].str);
                        break;
                    }
                    case "CacheAdSuccessEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.CacheAdSuccessEvent.Invoke();
                        break;
                    }
                    case "CacheAdFailedEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.CacheAdFailedEvent.Invoke(objP[0].str);
                        break;
                    }
                    case "MinDelayBtwAdsNotReachedEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.MinDelayBtwAdsNotReachedEvent.Invoke();
                        break;
                    }
                    case "AdClosedTap":
                    {
                        AdDealsWrapperUWPIL2CPP.AdClosedTap.Invoke();
                        break;
                    }
                    case "AdClickedTap":
                    {
                        AdDealsWrapperUWPIL2CPP.AdClickedTap.Invoke();
                        break;
                    }
                    case "AdAvailableEvent":
                    {
                        AdDealsWrapperUWPIL2CPP.AdAvailableEvent.Invoke((int)(objP[0].n), objP[1].b);
                        break;
                    }
                    default:
                    {
                        Debug.Log("Unknow function name:" + f);
                        break;
                    }
                }
            });
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

    }
}

#endif
