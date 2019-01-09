using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;


public class Test : MonoBehaviour {

    public UnityEngine.UI.Text logText;
    private string logBuffer = "Log:";
    private const int MAX_LOG_LINE = 3;
    private string placementID = "";
    private int uiOrientation = AdDeals.AdDealsWrapper.UIOrientationPortrait; // 0:Unknown 1:portrait 2:landscape

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

    public void onButtonCacheVideo() {
        log("to cache video");
        AdDeals.AdDealsWrapper.CacheAd(AdDeals.AdDealsWrapper.AdTypeRewardedVideo, placementID, uiOrientation);
    }
    public void onButtonShowVideo() {
        log("to show video");
        AdDeals.AdDealsWrapper.ShowAd(AdDeals.AdDealsWrapper.AdTypeRewardedVideo, placementID, uiOrientation);
    }
    public void onButtonCacheInterstitial() {
        log("to cache interstitial");
        AdDeals.AdDealsWrapper.CacheAd(AdDeals.AdDealsWrapper.AdTypeInterstitial, placementID, AdDeals.AdDealsWrapper.UIOrientationUnset);
    }
    public void onButtonShowInterstitial() {
        log("to show interstitial");
        AdDeals.AdDealsWrapper.ShowAd(AdDeals.AdDealsWrapper.AdTypeInterstitial, placementID, AdDeals.AdDealsWrapper.UIOrientationUnset);
    }
    public void onButtonSetPlacementID() {
/*
 * Windows: AppID #3301 => AdPlacementIDs: 03301001 / 03301002
 * Android: AppID #3181 => AdPlacementIDs: 03181001 / 03181002
 * iOS: AppID #3303 => AdPlacementIDs: 03303001 / 03303002
 *
 * placementID is an advanced feature and in most cases you can just leave it “”. In case you want to use placementIDs you should contact addeals@ahead-solutions.com
 *
 */

#if ENABLE_ADDEALS_UWP
        placementID = "03301001";
#elif UNITY_ANDROID
        placementID = "03181001";
#elif UNITY_IOS
        placementID = "03303001";
#else
        placementID = "";
#endif

        log("placementID set to:" + placementID);
    }
    public void onButtonSetConsentGrant() {
        //SetConsent after init sdk success
        AdDeals.AdDealsWrapper.SetConsent(AdDeals.AdDealsWrapper.UserConsentGrant);
    }
    public void onButtonSetConsentRevoke() {
        //SetConsent after init sdk success
        AdDeals.AdDealsWrapper.SetConsent(AdDeals.AdDealsWrapper.UserConsentRevoke);
    }
    public void onButtonSetConsentNonEU() {
        //SetConsent after init sdk success
        AdDeals.AdDealsWrapper.SetConsent(AdDeals.AdDealsWrapper.UserConsentNotApplicable);
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
