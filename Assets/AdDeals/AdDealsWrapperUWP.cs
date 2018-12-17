#if ENABLE_ADDEALS_UWP

using System;
using UnityEngine;

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;
using AdDealsUniversalSDKW81;
using AdDealsUniversalSDKW81.Views.UserControls;
using AdDealsUniversalSDKW81.Models;

namespace AdDeals
{

    public class AdDealsWrapperUWP : AdDealsWrapperBase
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

        private static Panel rootPanel = null;
        private static bool hasInit = false;

        public static void SetRootPanel(Windows.UI.Xaml.Controls.Panel panel)
        {
            //System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
            AdDealsWrapperUWP.rootPanel = panel;
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

            RunInUWPUIThread(() =>
            {
                Panel panel = rootPanel;
                if (null == panel)
                {
                    panel = getRootPanel();
                    rootPanel = panel;
                }
                if (null == panel)
                {
                    RunInUnityMainThread(() =>
                    {
                        AdManagerInitSDKFailed.Invoke("rooPanel is null");
                    });
                    return;
                }
                AdDealsWrapperUWP.InitAdDeals(panel, appID, appKey);
                hasInit = true;
            });
        }

        /// <summary>
        /// <para> set privacy policy consent</para>
        /// </summary>
        /// <param name="consent">consent, -1(NOT_APPLICABLE), 0(REVOKE), 1(GRANT)</param>
        public static void SetConsent(int consent)
        {
            if (AdDealsWrapperBase.UserConsentNotSet == consent)
            {
                consent = AdDealsWrapperBase.UserConsentRevoke;
            }
            RunInUWPUIThread(() =>
            {
                AdManager.SetConsent((AdManager.PrivacyPolicyConsent)consent);
            });
        }

        /// <summary>
        /// <para> check ad available</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void IsAvailable(int adType, int uiOrientation)
        {
            RunInUWPUIThread(() =>
            {
                AdManager.AdKind adKind = (AdManager.AdKind)adType;
                Task<AdDealsPopupAd> task = AdManager.GetPopupAd(AdDealsWrapperUWP.rootPanel, adKind);
                task.Wait();
                AdDealsPopupAd cachePopupAd = task.Result;
                if (AdManager.AdKind.REWARDEDVIDEOAD != adKind)
                {
                    RunInUnityMainThread(() =>
                    {
                        AdDealsWrapperUWP.AdAvailableEvent.Invoke(adType, cachePopupAd.IsCachedAdAvailable());
                    });
                }
                else
                {
                    Task<bool> taskAvailable = cachePopupAd.IsVideoAvailable();
                    taskAvailable.ContinueWith((t) =>
                    {
                        RunInUnityMainThread(() =>
                        {
                            AdDealsWrapperUWP.AdAvailableEvent.Invoke(adType, t.Result);
                        });
                    });
                }
            });
        }

        /// <summary>
        /// <para> cache ad</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="placementID">in most cases just leave it ""</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void CacheAd(int adType, string placementID, int uiOrientation)
        {
            RunInUWPUIThread(() =>
            {
                AdManager.AdKind adKind = (AdManager.AdKind)adType;
                Task<AdDealsPopupAd> cachePopupAdTask = AdManager.GetPopupAd(AdDealsWrapperUWP.rootPanel, adKind);
                cachePopupAdTask.Wait();

                AdDealsPopupAd cachePopupAd = cachePopupAdTask.Result;
                cachePopupAd.CacheAdSuccess += CacheAdSuccess_Event;                             // OPTIONAL. This is triggered when an ad is cached successfully.
                cachePopupAd.CacheAdFailed += CacheAdFailed_Event;                               // OPTIONAL. This is triggered when an ad could not be cached.
                cachePopupAd.MinDelayBtwAdsNotReached += MinDelayBtwAdsNotReached_Event;         // OPTIONAL. This is triggered when you try to call more than 1 ad in a very short period of time (less than 3 sec).
                cachePopupAd.SDKNotInitialized += SDKNotInitialized_Event;                       // OPTIONAL. This is triggered when youn try to load an ad without initilizing the SDK.

                cachePopupAd.CacheAd();      // CACHING IS OPTIONAL. Should be only called to display ad instantly, for instance after loading an animation. 
             });
        }

        /// <summary>
        /// <para> show ad</para>
        /// </summary>
        /// <param name="adType">adType, 0(WALLAD), 1(FULLSCREENPOPUPAD), 2(REWARDEDVIDEOAD)</param>
        /// <param name="placementID">in most cases just leave it ""</param>
        /// <param name="uiOrientation">invalid on UWP platform</param>
        public static void ShowAd(int adType, string placementID, int uiOrientation)
        {
            RunInUWPUIThread(() =>
            {
                AdManager.AdKind adKind = (AdManager.AdKind)adType;
                Task<AdDealsPopupAd> showAdTask = AdManager.GetPopupAd(AdDealsWrapperUWP.rootPanel, adKind);
                showAdTask.Wait();
                AdDealsPopupAd showAd = showAdTask.Result;

                showAd.AdClosed += AdClosed_Tap;                                           // OPTIONAL. This is triggered when the popup ad is closed.
                showAd.AdClicked += AdClicked_Tap;                                         // OPTIONAL. This is triggered when an ad is clicked by end user.
                showAd.ShowAdFailed += ShowAdFailed_Event;                                 // OPTIONAL. This is triggered when no ad is available or an issue occurs (slow network connection...)
                showAd.ShowAdSuccess += ShowAdSucess_Event;                                // OPTIONAL. This is triggered when an ad is displayed to end user.
                showAd.MinDelayBtwAdsNotReached += MinDelayBtwAdsNotReached_Event;         // OPTIONAL. This is triggered when you try to call more than 1 ad in a very short period of time (less than 3 sec).
                showAd.SDKNotInitialized += SDKNotInitialized_Event;                       // OPTIONAL. This is triggered when youn try to load an ad without initilizing the SDK.
                showAd.VideoRewardGranted += ShowAdVideoRewardGranted_Event;               // REQUIRED FOR REWARDED VIDEOS If you want to notify the end user that a video view has been completed.

                showAd.ShowAd();
            });
        }

        private static void RunInUWPUIThread(UnityEngine.WSA.AppCallbackItem handler)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(handler, false);
        }

        //private static void RunInUWPUIThread(Windows.UI.Core.DispatchedHandler handler)
        //{
        //    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, handler);
        //}

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

        private static void InitAdDeals(Panel panel, String appKey, String appSecret)
        {
            Debug.Log("AdDeals InitAdDeals");

            AdManager.InitSDKSuccess += AdManager_InitSDKSuccess;
            AdManager.InitSDKFailed += AdManager_InitSDKFailed;
            AdManager.UpdateConsentFailed += AdManager_ConsentFailed;
            AdManager.UpdateConsentSuccess += AdManager_ConsentSuccess;
            AdManager.AppDownloadSourceDetected += AdManager_AppDownloadSourceDetected;
            AdManager.AppSessionSourceDetected += AdManager_AppSessionSourceDetected;

            AdManager.InitSDK(panel, appKey, appSecret);
        }

        #region Optional events for exclusive offers/campaigns (AdDeals)
        private static void AdManager_InitSDKSuccess(object sender, EventArgs e)
        {
            Debug.Log("AdDeals: SDK has been successfully initialized.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerInitSDKSuccess.Invoke();
            });
        }
        private static void AdManager_InitSDKFailed(object sender, AdDealsUniversalSDKW81.Models.DetailedEventArgs e)
        {
            Debug.Log("AdDeals: SDK could not be initialized:" + e.Details);
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerInitSDKFailed.Invoke(e.Details);
            });
        }
        private static void AdManager_ConsentSuccess(object sender, EventArgs e)
        {
            Debug.Log("AdDeals: Consent successfully updated.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerConsentSuccess.Invoke();
            });
        }
        private static void AdManager_ConsentFailed(object sender, AdDealsUniversalSDKW81.Models.DetailedEventArgs e)
        {
            Debug.Log("AdDeals: Consent update failed:" + e.Details);
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerConsentFailed.Invoke(e.Details);
            });
        }
        private static void AdManager_AppDownloadSourceDetected(object sender, EventArgs e)
        {
            if ((AdManager.AppDownloadSource)sender == AdManager.AppDownloadSource.ADDEALS)
            {
                Debug.Log("AdDeals: This app WAS download from AdDeals links/campaigns");
            }
            else
            {
                Debug.Log("AdDeals: This app WAS NOT downloaded from AdDeals links/campaigns!");
            }
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerAppDownloadSourceDetected.Invoke();
            });
        }
        private static void AdManager_AppSessionSourceDetected(object sender, EventArgs e)
        {
            Debug.Log("AdDeals: Tells whether or not the user opened up the app after clicking on an AdDeals campaign and provides AdDeals campaign information to display special/exclusive offer.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdManagerAppSessionSourceDetected.Invoke();
            });
        }
        #endregion

        #region AdDeals Ad event
        private static void SDKNotInitialized_Event(object sender, EventArgs e)
        {
            Debug.Log("You need to call Init() prior showing ads or caching them.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.SDKNotInitializedEvent.Invoke();
            });
        }

        private static void ShowAdVideoRewardGranted_Event(object sender, EventArgs e)
        {
            Debug.Log("Thanks for watching, you received 1000 coins!");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.ShowAdVideoRewardGrantedEvent.Invoke();
            });
        }

        // OPTIONAL - Delegated event when AdDeals detected that an ad has been displayed.
        private static void ShowAdSucess_Event(object sender, EventArgs e)
        {
            Debug.Log("A campaign ad (single app or web ad) has been displayed.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.ShowAdSucessEvent.Invoke();
            });
        }

        // OPTIONAL - Delegated event when AdDeals Ad could not display any ad to end users.
        private static void ShowAdFailed_Event(object sender, DetailedEventArgs e)
        {
            Debug.Log("No ad available for this user or there is some issue (network access...).To increase your fill rate worldwide (up to 100%), go to your AdDeals account and create an interstitial campaign.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.ShowAdFailedEvent.Invoke(e.Details);
            });
        }

        // OPTIONAL - Delegated event when AdDeals detected that an ad has been displayed.
        private static void CacheAdSuccess_Event(object sender, EventArgs e)
        {
            Debug.Log("This ad could be cached successfully.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.CacheAdSuccessEvent.Invoke();
            });
        }

        // OPTIONAL - Delegated event when AdDeals Ad could not display any ad to end users.
        private static void CacheAdFailed_Event(object sender, DetailedEventArgs e)
        {
            Debug.Log("This ad could not be cached. However, only one can be cached until display so you may call: IsCachedAdAvailable() prior caching a new one.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.CacheAdFailedEvent.Invoke(e.Details);
            });
        }

        // OPTIONAL - Delegated event when AdDeals ad does not return any ad to end users.
        private static void MinDelayBtwAdsNotReached_Event(object sender, EventArgs e)
        {
            Debug.Log("Delay between two ads is not reached. The minimal time between two ad calls is 10 seconds. You can set a higher number if you wish.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.MinDelayBtwAdsNotReachedEvent.Invoke();
            });
        }

        // OPTIONAL - Delegated event when AdDeals ad is closed by end user.
        private static void AdClosed_Tap(object sender, EventArgs e)
        {
            Debug.Log("The user closed AdDeals Ad.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdClosedTap.Invoke();
            });
        }

        // OPTIONAL - Delegated event when AdDeals ad is closed by end user.
        private static void AdClicked_Tap(object sender, EventArgs e)
        {
            Debug.Log("The user clicked on AdDeals Ad.");
            RunInUnityMainThread(() =>
            {
                AdDealsWrapperUWP.AdClickedTap.Invoke();
            });
        }
        #endregion


        private static Panel getRootPanel()
        {
            if (null == Window.Current)
            {
                return null;
            }
            Frame frame = Window.Current.Content as Frame;
            if (null == frame)
            {
                return null;
            }
            Page page = frame.Content as Page;
            if (null == page)
            {
                return null;
            }

            return page.Content as Panel;
        }

    }
}

#endif
