using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdDealsAndroidListener : MonoBehaviour {

#if UNITY_ANDROID

	public void initSDKSuccess(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyInitSDKSuccess(s);
	}

	public void initSDKFail(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyInitSDKFail(error);
	}

	public void notInitializedSDK(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyNotInitializedSDK(error);
	}

	public void updateConsentSuccess(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyUpdateConsentSuccess(s);
	}

	public void updateConsentFail(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyUpdateConsentFail(error);
	}

	public void cacheInterstitialAdSuccess(string orientation)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyCacheInterstitialAdSuccess(orientation);
	}

	public void cacheInterstitialAdFailed(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyCacheInterstitialAdFailed(error);
	}

	public void showInterstitialAdSuccess(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyShowInterstitialAdSuccess(s);
	}

	public void showInterstitialAdFailed(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyShowInterstitialAdFailed(error);
	}

	public void minDelayBtwInterstitialAdsNotReached(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyMinDelayBtwInterstitialAdsNotReached(s);
	}

	public void interstitialAdClosed(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyInterstitialAdClosed(s);
	}

	public void interstitialAdClicked(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyInterstitialAdClicked(s);
	}

	public void cacheVideoAdSuccess(string orientation)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyCacheVideoAdSuccess(orientation);
	}

	public void cacheVideoAdFailed(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyCacheVideoAdFailed(error);
	}

	public void showVideoAdSuccess(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyShowVideoAdSuccess(s);
	}

	public void showVideoAdFailed(string error)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyShowVideoAdFailed(error);
	}

	public void minDelayBtwVideoAdsNotReached(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyMinDelayBtwVideoAdsNotReached(s);
	}

	public void videoAdClosed(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyVideoAdClosed(s);
	}

	public void videoAdClicked(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyVideoAdClicked(s);
	}

	public void videoRewardGranted(string s)
	{
		AdDeals.AdDealsWrapperAndroid.NotifyVideoRewardGranted(s);
	}

#endif

}
