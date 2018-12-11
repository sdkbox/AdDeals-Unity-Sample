using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;


public class Test : MonoBehaviour {

    public UnityEngine.UI.Text logText;
    private string logBuffer = "Log:";
    private const int MAX_LOG_LINE = 3;

    private int adType = AdDeals.AdDealsWrapper.AdTypeInterstitial; // 1:FULLSCREENPOPUPAD  2:REWARDEDVIDEOAD

    private int uiOrientation = AdDeals.AdDealsWrapper.UIOrientationPortrait; // 0:Unknown 1:portrait 2:portraitUpsideDown 3:LandscapeRight 4:LandscapeLeft

    // Use this for initialization
    void Start () {
        AdDeals.AdDealsWrapper.AdAvailableEvent += AdDealsEvtAdAvailable;
        AdDeals.AdDealsWrapper.SDKNotInitializedEvent += AdDealsEvtSDKNotInitializedEvent;
        AdDeals.AdDealsWrapper.ShowAdVideoRewardGrantedEvent += AdDealsEvtShowAdVideoRewardGrantedEvent;
        AdDeals.AdDealsWrapper.ShowAdSucessEvent += AdDealsEvtShowAdSucessEvent;
        AdDeals.AdDealsWrapper.ShowAdFailedEvent += AdDealsEvtShowAdFailedEvent;
        AdDeals.AdDealsWrapper.CacheAdSuccessEvent += AdDealsEvtCacheAdSuccessEvent;
        AdDeals.AdDealsWrapper.CacheAdFailedEvent += AdDealsEvtCacheAdFailedEvent;
        AdDeals.AdDealsWrapper.MinDelayBtwAdsNotReachedEvent += AdDealsEvtMinDelayBtwAdsNotReachedEvent;
        AdDeals.AdDealsWrapper.AdClosedTap += AdDealsEvtAdClosedTap;
        AdDeals.AdDealsWrapper.AdClickedTap += AdDealsEvtAdClickedTap;
        AdDeals.AdDealsWrapper.AdManagerInitSDKSuccess += AdManagerInitSDKSuccess;
        AdDeals.AdDealsWrapper.AdManagerInitSDKFailed += AdManagerInitSDKFailed;
        AdDeals.AdDealsWrapper.AdManagerConsentSuccess += AdManagerConsentSuccess;
        AdDeals.AdDealsWrapper.AdManagerConsentFailed += AdManagerConsentFailed;
        AdDeals.AdDealsWrapper.AdManagerAppDownloadSourceDetected += AdManagerAppDownloadSourceDetected;
        AdDeals.AdDealsWrapper.AdManagerAppSessionSourceDetected += AdManagerAppSessionSourceDetected;

#if ENABLE_ADDEALS_UWP
        AdDeals.AdDealsWrapper.Init("3301", "DWXIUHBW0KF7");
#elif UNITY_ANDROID
        AdDeals.AdDealsWrapper.Init("3181", "69LBY2CITGTJ");
#elif UNITY_IOS
        AdDeals.AdDealsWrapper.Init("3303", "ROPEVXS137S3");
#else
        AdDeals.AdDealsWrapper.Init("dummy", "dummy");
#endif
    }

    public void onButtonChange() {
        adType ++;
        if (adType > 2) {
            adType = 1;
        }
        log("change adType to " + adType);
    }

	public void onButtonCache() {
        log("AdDeals to cache...");
        AdDeals.AdDealsWrapper.CacheAdByType(adType, "", uiOrientation);
    }

    public void onButtonCheckAvailable()
    {
        log("AdDeals to check ad avaialbe...");
        AdDeals.AdDealsWrapper.IsAvailable(adType, uiOrientation);
    }

    public void onButtonShow() {
        log("AdDeals to show...");
        AdDeals.AdDealsWrapper.ShowPopupAd(adType, "", uiOrientation);
    }

    private void AdDealsEvtAdAvailable(int adType, bool available)
    {
        log("AdDealsEvtAdAvailable " + adType + " " + available);
    }

    private void AdDealsEvtSDKNotInitializedEvent()
    {
        log("AdDealsEvtSDKNotInitializedEvent");
    }

    private void AdDealsEvtShowAdVideoRewardGrantedEvent()
    {
        log("AdDealsEvtShowAdVideoRewardGrantedEvent");
    }
    private void AdDealsEvtShowAdSucessEvent()
    {
        log("AdDealsEvtShowAdSucessEvent");
    }
    private void AdDealsEvtShowAdFailedEvent(string error)
    {
        log("AdDealsEvtShowAdFailedEvent:" + error);
    }
    private void AdDealsEvtCacheAdSuccessEvent()
    {
        log("AdDealsEvtCacheAdSuccessEvent");
    }
    private void AdDealsEvtCacheAdFailedEvent(string error)
    {
        log("AdDealsEvtCacheAdFailedEvent:" + error);
    }
    private void AdDealsEvtMinDelayBtwAdsNotReachedEvent()
    {
        log("AdDealsEvtMinDelayBtwAdsNotReachedEvent");
    }
    private void AdDealsEvtAdClosedTap()
    {
        log("AdDealsEvtAdClosedTap");
    }
    private void AdDealsEvtAdClickedTap()
    {
        log("AdDealsEvtAdClickedTap");
    }
    private void AdManagerInitSDKSuccess() {
        log("AdManagerInitSDKSuccess");

        //SetConsent after init sdk success
        AdDeals.AdDealsWrapper.SetConsent(AdDeals.AdDealsWrapper.UserConsentGrant);
    }
    private void AdManagerInitSDKFailed(string error) {
        log("AdManagerInitSDKFailed:" + error);
    }
    private void AdManagerConsentSuccess() {
        log("AdManagerConsentSuccess");
    }
    private void AdManagerConsentFailed(string error) {
        log("AdManagerConsentFailed:" + error);
    }
    private void AdManagerAppDownloadSourceDetected() {
        log("AdManagerAppDownloadSourceDetected");
    }
    private void AdManagerAppSessionSourceDetected() {
        log("AdManagerAppSessionSourceDetected");
    }

    private void log(string s)
    {
        Debug.Log(s);

        String newLine = "\n"; // System.Environment.NewLine;
        logBuffer += newLine;
        logBuffer += s;
        int numLines = logBuffer.Split(newLine.ToCharArray()).Length;
        if (numLines > MAX_LOG_LINE)
        {
            string[] lines = logBuffer.Split(newLine.ToCharArray()).Skip(numLines - MAX_LOG_LINE).ToArray();
            logBuffer = string.Join(newLine, lines);
        }

        if (logText)
        {
            logText.text = logBuffer;
        }
    }

    // Update is called once per frame
    void Update () {
    }
}
