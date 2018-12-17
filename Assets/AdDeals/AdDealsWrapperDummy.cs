#if UNITY_IOS
#elif ENABLE_ADDEALS_UWP
#else

using System;
using UnityEngine;

namespace AdDeals
{

    public class AdDealsWrapperDummy : AdDealsWrapperBase
    {
        // disable EventHandler not used waring when unity compile
#if ENABLE_ADDEALS
#else
#pragma warning disable 67
#endif
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
#if ENABLE_ADDEALS
#else
#pragma warning restore 67
#endif

        private static String DUMMY_NOTE = "Dummy on this platform, please run on UWP with BuildType XAML with Unity C# Project";

        public static void Init(String appKey, String appSecret)
        {
            Debug.Log(DUMMY_NOTE);
            AdManagerInitSDKFailed.Invoke(DUMMY_NOTE);
        }

        public static void SetConsent(int consent)
        {
            Debug.Log(DUMMY_NOTE);
        }

        public static void IsAvailable(int adType, int uiOrientation)
        {
            Debug.Log(DUMMY_NOTE);
            AdAvailableEvent.Invoke(adType, false);
        }

        public static void CacheAd(int adType, string placementID, int uiOrientation)
        {
            Debug.Log(DUMMY_NOTE);
        }

        public static void ShowAd(int adType, string placementID, int uiOrientation)
        {
            Debug.Log(DUMMY_NOTE);
            ShowAdFailedEvent.Invoke(DUMMY_NOTE);
        }

    }
}

#endif
