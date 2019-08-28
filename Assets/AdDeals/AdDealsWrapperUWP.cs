#if ENABLE_WINMD_SUPPORT

using System;
using UnityEngine;

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;

using UUBridge;

namespace AdDeals
{

    class UnityBridge : IUnityBridge
    {
        public void Send(string json)
        {
            AdDealsWrapperUWP.HandleEvent(json);
        }
    }

    public class AdDealsWrapperUWP : AdDealsWrapperBase
    {
        // disable EventHandler not used waring when unity compile
        public delegate void AdAvailableHandler(int adType, int uiOrientation, bool available);
        public delegate void AdEventHandler();
        public delegate void AdEventStringHandler(string error);
        public static event AdAvailableHandler AdAvailableEvent;
        public static event AdEventHandler SDKNotInitializedEvent;
        public static event AdEventHandler ShowAdVideoRewardedGrantedEvent;
        public static event AdEventHandler ShowAdSuccessEvent;
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
            var bridge = BridgeBootstrapper.GetUWPBridge();
            //bridge = BridgeBootstrapper.GetUnityBridge();
            if (null == bridge)
            {
                Debug.Log("ERROR! UWPBridge is null");
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
            bridge.Send(obj.ToString());
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
            BridgeBootstrapper.SetUnityBridge(new UnityBridge());

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
        public static void IsCachedAdAvailable(int adType, int uiOrientation)
        {
            Invoke("IsCachedAdAvailable", adType, uiOrientation);
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
                        AdDealsWrapperUWP.AdManagerInitSDKSuccess.Invoke();
                        break;
                    }
                    case "AdManagerInitSDKFailed":
                    {
                        AdDealsWrapperUWP.AdManagerInitSDKFailed.Invoke(objP[0].str);
                        break;
                    }
                    case "AdManagerConsentSuccess":
                    {
                        AdDealsWrapperUWP.AdManagerConsentSuccess.Invoke();
                        break;
                    }
                    case "AdManagerConsentFailed":
                    {
                        AdDealsWrapperUWP.AdManagerConsentFailed.Invoke(objP[0].str);
                        break;
                    }
                    case "AdManagerAppDownloadSourceDetected":
                    {
                        AdDealsWrapperUWP.AdManagerAppDownloadSourceDetected.Invoke();
                        break;
                    }
                    case "AdManagerAppSessionSourceDetected":
                    {
                        AdDealsWrapperUWP.AdManagerAppSessionSourceDetected.Invoke();
                        break;
                    }
                    case "SDKNotInitializedEvent":
                    {
                        AdDealsWrapperUWP.SDKNotInitializedEvent.Invoke();
                        break;
                    }
                    case "ShowAdVideoRewardedGrantedEvent":
                    {
                        AdDealsWrapperUWP.ShowAdVideoRewardedGrantedEvent.Invoke();
                        break;
                    }
                    case "ShowAdSuccessEvent":
                    {
                        AdDealsWrapperUWP.ShowAdSuccessEvent.Invoke();
                        break;
                    }
                    case "ShowAdFailedEvent":
                    {
                        AdDealsWrapperUWP.ShowAdFailedEvent.Invoke(objP[0].str);
                        break;
                    }
                    case "CacheAdSuccessEvent":
                    {
                        AdDealsWrapperUWP.CacheAdSuccessEvent.Invoke();
                        break;
                    }
                    case "CacheAdFailedEvent":
                    {
                        AdDealsWrapperUWP.CacheAdFailedEvent.Invoke(objP[0].str);
                        break;
                    }
                    case "MinDelayBtwAdsNotReachedEvent":
                    {
                        AdDealsWrapperUWP.MinDelayBtwAdsNotReachedEvent.Invoke();
                        break;
                    }
                    case "AdClosedTap":
                    {
                        AdDealsWrapperUWP.AdClosedTap.Invoke();
                        break;
                    }
                    case "AdClickedTap":
                    {
                        AdDealsWrapperUWP.AdClickedTap.Invoke();
                        break;
                    }
                    case "AdAvailableEvent":
                    {
                        AdDealsWrapperUWP.AdAvailableEvent.Invoke((int)(objP[0].n), (int)(objP[1].n), objP[2].b);
                        break;
                    }
                    default:
                    {
                        Debug.Log("Unknown function name:" + f);
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
