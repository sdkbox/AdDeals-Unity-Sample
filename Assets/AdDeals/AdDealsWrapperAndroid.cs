#if UNITY_ANDROID

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace AdDeals
{

    public class AdDealsWrapperAndroid : AdDealsWrapperBase
    {
        public delegate void AdAvailableHandler(int adType, bool available);
        public delegate void AdEventHandler();
        public delegate void AdEventStringHandler(string error);
        public delegate void AdEventIntStringHandler(int adType, string error);

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

        private static String WRAPPER_CLASS = "com.addeals.unity.AdDealsWrapper";
        public static void Init(String appKey, String appSecret)
        {
#if !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass(WRAPPER_CLASS))
            {
                jc.CallStatic("initSDK", appKey, appSecret);
            }
#endif
        }

        public static void SetConsent(int consent)
        {
#if !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass(WRAPPER_CLASS))
            {
                jc.CallStatic("setConsent", AdDealsWrapperAndroid.transToAndroidConsent(consent));
            }
#endif
        }

        public static void IsAvailable(int adType, int uiOrientation)
        {
#if !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass(WRAPPER_CLASS))
            {
                bool b = jc.CallStatic<bool>("isCacheAdAvailable", adType, AdDealsWrapperAndroid.transToAndroidOrientation(uiOrientation));
                AdDealsWrapperAndroid.AdAvailableEvent.Invoke(adType, b);
            }
#endif
        }

        public static void CacheAd(int adKind, string placementID, int uiOrientation)
        {
#if !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass(WRAPPER_CLASS))
            {
                jc.CallStatic("cacheAd", adKind, placementID, AdDealsWrapperAndroid.transToAndroidOrientation(uiOrientation));
            }
#endif
        }

        public static void ShowAd(int adKind, string placementID, int uiOrientation)
        {
#if !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass(WRAPPER_CLASS))
            {
                jc.CallStatic("showAd", adKind, placementID, AdDealsWrapperAndroid.transToAndroidOrientation(uiOrientation));
            }
#endif
        }


        public static void NotifyInitSDKSuccess(string s)
        {
            AdDealsWrapperAndroid.AdManagerInitSDKSuccess.Invoke();
        }

        public static void NotifyInitSDKFail(string error)
        {
            AdDealsWrapperAndroid.AdManagerInitSDKFailed.Invoke(error);
        }

        public static void NotifyNotInitializedSDK(string error)
        {
            AdDealsWrapperAndroid.SDKNotInitializedEvent.Invoke();
        }

        public static void NotifyUpdateConsentSuccess(string s)
        {
            AdDealsWrapperAndroid.AdManagerConsentSuccess.Invoke();
        }

        public static void NotifyUpdateConsentFail(string error)
        {
            AdDealsWrapperAndroid.AdManagerConsentFailed.Invoke(error);
        }

        public static void NotifyCacheInterstitialAdSuccess(string orientation)
        {
            AdDealsWrapperAndroid.CacheAdSuccessEvent.Invoke();
        }

        public static void NotifyCacheInterstitialAdFailed(string error)
        {
            AdDealsWrapperAndroid.CacheAdFailedEvent.Invoke(error);
        }

        public static void NotifyShowInterstitialAdSuccess(string s)
        {
            AdDealsWrapperAndroid.ShowAdSucessEvent.Invoke();
        }

        public static void NotifyShowInterstitialAdFailed(string error)
        {
            AdDealsWrapperAndroid.ShowAdFailedEvent.Invoke(error);
        }

        public static void NotifyMinDelayBtwInterstitialAdsNotReached(string s)
        {
            AdDealsWrapperAndroid.MinDelayBtwAdsNotReachedEvent.Invoke();
        }

        public static void NotifyInterstitialAdClosed(string s)
        {
            AdDealsWrapperAndroid.AdClosedTap.Invoke();
        }

        public static void NotifyInterstitialAdClicked(string s)
        {
            AdDealsWrapperAndroid.AdClickedTap.Invoke();
        }

        public static void NotifyCacheVideoAdSuccess(string orientation)
        {
            AdDealsWrapperAndroid.CacheAdSuccessEvent.Invoke();
        }

        public static void NotifyCacheVideoAdFailed(string error)
        {
            AdDealsWrapperAndroid.CacheAdFailedEvent.Invoke(error);
        }

        public static void NotifyShowVideoAdSuccess(string s)
        {
            AdDealsWrapperAndroid.ShowAdSucessEvent.Invoke();
        }

        public static void NotifyShowVideoAdFailed(string error)
        {
            AdDealsWrapperAndroid.ShowAdFailedEvent.Invoke(error);
        }

        public static void NotifyMinDelayBtwVideoAdsNotReached(string s)
        {
            AdDealsWrapperAndroid.MinDelayBtwAdsNotReachedEvent.Invoke();
        }

        public static void NotifyVideoAdClosed(string s)
        {
            AdDealsWrapperAndroid.AdClosedTap.Invoke();
        }

        public static void NotifyVideoAdClicked(string s)
        {
            AdDealsWrapperAndroid.AdClickedTap.Invoke();
        }

        public static void NotifyVideoRewardGranted(string s)
        {
            AdDealsWrapperAndroid.ShowAdVideoRewardGrantedEvent.Invoke();
        }

        private static int transToAndroidOrientation(int i)
        {
            //Android just have two orientation, 0:Portrait, 1:Landscape 
            if (i < AdDealsWrapperAndroid.UIOrientationLandscapeRight)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private static int transToAndroidConsent(int i)
        {
            switch (i)
            {
                case AdDealsWrapperBase.UserConsentNotApplicable:
                {
                    // NOT_ELIGIBLE
                    return 2;
                }
                case AdDealsWrapperBase.UserConsentRevoke:
                {
                    // DISAGREE
                    return 1;
                }
                case AdDealsWrapperBase.UserConsentGrant:
                {
                    // APPROVE
                    return 0;
                }
                case AdDealsWrapperBase.UserConsentNotSet:
                default:
                {
                    //NOT_SET
                    return 3;
                }
            }
        }

    }
}

#endif
