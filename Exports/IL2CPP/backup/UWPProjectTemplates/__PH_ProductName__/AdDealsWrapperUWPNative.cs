using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Newtonsoft.Json;
using AdDealsUniversalSDKW81;
using AdDealsUniversalSDKW81.Views.UserControls;
using AdDealsUniversalSDKW81.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

using UUBridge;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace AdDeals
{
    class AdDealsWrapperUWPNative
    {
        private static Panel rootPanel = null;

        static public void HandleEvent(String json)
        {
            RunInUWPUIThread(() =>
            {
                InvokeParam param = InvokeParam.Parse(json);
                Log(param.ToString());

                switch (param.function)
                {
                    case "Init":
                        {
                            InitAdDeals(param.paramlist[0].asString(), param.paramlist[1].asString());
                            break;
                        }
                    case "SetConsent":
                        {
                            SetConsent(param.paramlist[0].asInt());
                            break;
                        }
                    case "IsAvailable":
                        {
                            IsAvailable(param.paramlist[0].asInt(), param.paramlist[1].asInt());
                            break;
                        }
                    case "CacheAd":
                        {
                            CacheAd(param.paramlist[0].asInt(), param.paramlist[1].asString(), param.paramlist[2].asInt());
                            break;
                        }
                    case "ShowAd":
                        {
                            ShowAd(param.paramlist[0].asInt(), param.paramlist[1].asString(), param.paramlist[2].asInt());
                            break;
                        }
                    default:
                        {
                            Debug.WriteLine("ERROR! AdDealsWrapperUWPNative unknow function:%s", param.function);
                            break;
                        }
                }
            });
        }

        private static void Log(params System.Object[] msgs)
        {
            foreach (var m in msgs)
            {
                Debug.WriteLine(m);
            }
        }

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
         
        private static void RunInUWPUIThread(DispatchedHandler h)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, h);
            //UnityEngine.WSA.Application.InvokeOnUIThread(handler, false);
        }

        private static void SendToIL2CPP(string json)
        {
            var unityBridge = BridgeBootstrapper.GetUnityBridge();
            if (null == unityBridge)
            {
                Log("ERROR! unityBridge is null");
                return;
            }
            unityBridge.Send(json);
        }

        private static void InitAdDeals(String appKey, String appSecret)
        {
            Log("AdDeals InitAdDeals");

            AdManager.InitSDKSuccess += AdManager_InitSDKSuccess;
            AdManager.InitSDKFailed += AdManager_InitSDKFailed;
            AdManager.UpdateConsentFailed += AdManager_ConsentFailed;
            AdManager.UpdateConsentSuccess += AdManager_ConsentSuccess;
            AdManager.AppDownloadSourceDetected += AdManager_AppDownloadSourceDetected;
            AdManager.AppSessionSourceDetected += AdManager_AppSessionSourceDetected;

            if (null == rootPanel)
            {
                rootPanel = getRootPanel();
            }
            if (null == rootPanel)
            {
                Log("ERROR! cannot get root panel");
                return;
            }
            AdManager.InitSDK(rootPanel, appKey, appSecret);
        }

        private static void SetConsent(int consent)
        {
            AdManager.SetConsent((AdManager.PrivacyPolicyConsent)consent);
        }

        private static void IsAvailable(int adType, int uiOrientation)
        {
            AdManager.AdKind adKind = (AdManager.AdKind)adType;
            Task<AdDealsPopupAd> task = AdManager.GetPopupAd(AdDealsWrapperUWPNative.rootPanel, adKind);
            task.Wait();
            AdDealsPopupAd cachePopupAd = task.Result;
            if (AdManager.AdKind.REWARDEDVIDEOAD != adKind)
            {
                InvokeParam ip = new InvokeParam();
                ip.function = "AdAvailableEvent";
                ip.paramlist.Add(new Param(adType));
                ip.paramlist.Add(new Param(cachePopupAd.IsCachedAdAvailable()));
                SendToIL2CPP(ip.ToString());
            }
            else
            {
                Task<bool> taskAvailable = cachePopupAd.IsVideoAvailable();
                taskAvailable.ContinueWith((t) =>
                {
                    InvokeParam ip = new InvokeParam();
                    ip.function = "AdAvailableEvent";
                    ip.paramlist.Add(new Param(adType));
                    ip.paramlist.Add(new Param(t.Result));
                    SendToIL2CPP(ip.ToString());
                });
            }
        }

        public static void CacheAd(int adType, string placementID, int uiOrientation)
        {
            AdManager.AdKind adKind = (AdManager.AdKind)adType;
            Task<AdDealsPopupAd> cachePopupAdTask = AdManager.GetPopupAd(AdDealsWrapperUWPNative.rootPanel, adKind);
            cachePopupAdTask.Wait();

            AdDealsPopupAd cachePopupAd = cachePopupAdTask.Result;
            cachePopupAd.CacheAdSuccess += CacheAdSuccess_Event;                             // OPTIONAL. This is triggered when an ad is cached successfully.
            cachePopupAd.CacheAdFailed += CacheAdFailed_Event;                               // OPTIONAL. This is triggered when an ad could not be cached.
            cachePopupAd.MinDelayBtwAdsNotReached += MinDelayBtwAdsNotReached_Event;         // OPTIONAL. This is triggered when you try to call more than 1 ad in a very short period of time (less than 3 sec).
            cachePopupAd.SDKNotInitialized += SDKNotInitialized_Event;                       // OPTIONAL. This is triggered when youn try to load an ad without initilizing the SDK.

            if (0 == placementID.Length)
            {
                cachePopupAd.CacheAd();
            }
            else
            {
                cachePopupAd.CacheAd(placementID);
            }
        }

        public static void ShowAd(int adType, string placementID, int uiOrientation)
        {
            AdManager.AdKind adKind = (AdManager.AdKind)adType;
            Task<AdDealsPopupAd> showAdTask = AdManager.GetPopupAd(AdDealsWrapperUWPNative.rootPanel, adKind);
            showAdTask.Wait();
            AdDealsPopupAd showAd = showAdTask.Result;

            showAd.AdClosed += AdClosed_Tap;                                           // OPTIONAL. This is triggered when the popup ad is closed.
            showAd.AdClicked += AdClicked_Tap;                                         // OPTIONAL. This is triggered when an ad is clicked by end user.
            showAd.ShowAdFailed += ShowAdFailed_Event;                                 // OPTIONAL. This is triggered when no ad is available or an issue occurs (slow network connection...)
            showAd.ShowAdSuccess += ShowAdSucess_Event;                                // OPTIONAL. This is triggered when an ad is displayed to end user.
            showAd.MinDelayBtwAdsNotReached += MinDelayBtwAdsNotReached_Event;         // OPTIONAL. This is triggered when you try to call more than 1 ad in a very short period of time (less than 3 sec).
            showAd.SDKNotInitialized += SDKNotInitialized_Event;                       // OPTIONAL. This is triggered when youn try to load an ad without initilizing the SDK.
            showAd.VideoRewardGranted += ShowAdVideoRewardGranted_Event;               // REQUIRED FOR REWARDED VIDEOS If you want to notify the end user that a video view has been completed.

            if (0 == placementID.Length)
            {
                showAd.ShowAd();
            }
            else
            {
                showAd.ShowAd(placementID);
            }
        }

        #region Optional events for exclusive offers/campaigns (AdDeals)
        private static void AdManager_InitSDKSuccess(object sender, EventArgs e)
        {
            Log("AdDeals: SDK has been successfully initialized.");
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerInitSDKSuccess";
            SendToIL2CPP(ip.ToString());
        }
        private static void AdManager_InitSDKFailed(object sender, AdDealsUniversalSDKW81.Models.DetailedEventArgs e)
        {
            Log("AdDeals: SDK could not be initialized:" + e.Details);
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerInitSDKFailed";
            ip.paramlist.Add(new Param(e.Details));
            SendToIL2CPP(ip.ToString());
        }
        private static void AdManager_ConsentSuccess(object sender, EventArgs e)
        {
            Log("AdDeals: Consent successfully updated.");
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerConsentSuccess";
            ip.paramlist.Add(new Param(""));
            SendToIL2CPP(ip.ToString());
        }
        private static void AdManager_ConsentFailed(object sender, AdDealsUniversalSDKW81.Models.DetailedEventArgs e)
        {
            Log("AdDeals: Consent update failed:" + e.Details);
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerConsentFailed";
            ip.paramlist.Add(new Param(e.Details));
            SendToIL2CPP(ip.ToString());
        }
        private static void AdManager_AppDownloadSourceDetected(object sender, EventArgs e)
        {
            if ((AdManager.AppDownloadSource)sender == AdManager.AppDownloadSource.ADDEALS)
            {
                Log("AdDeals: This app WAS download from AdDeals links/campaigns");
            }
            else
            {
                Log("AdDeals: This app WAS NOT downloaded from AdDeals links/campaigns!");
            }
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerAppDownloadSourceDetected";
            SendToIL2CPP(ip.ToString());
        }
        private static void AdManager_AppSessionSourceDetected(object sender, EventArgs e)
        {
            Log("AdDeals: Tells whether or not the user opened up the app after clicking on an AdDeals campaign and provides AdDeals campaign information to display special/exclusive offer.");
            InvokeParam ip = new InvokeParam();
            ip.function = "AdManagerAppSessionSourceDetected";
            SendToIL2CPP(ip.ToString());
        }
        #endregion

        #region AdDeals Ad event
        private static void SDKNotInitialized_Event(object sender, EventArgs e)
        {
            Log("You need to call Init() prior showing ads or caching them.");
            InvokeParam ip = new InvokeParam();
            ip.function = "SDKNotInitializedEvent";
            SendToIL2CPP(ip.ToString());
        }

        private static void ShowAdVideoRewardGranted_Event(object sender, EventArgs e)
        {
            Log("Thanks for watching, you received 1000 coins!");
            InvokeParam ip = new InvokeParam();
            ip.function = "ShowAdVideoRewardGrantedEvent";
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals detected that an ad has been displayed.
        private static void ShowAdSucess_Event(object sender, EventArgs e)
        {
            Log("A campaign ad (single app or web ad) has been displayed.");
            InvokeParam ip = new InvokeParam();
            ip.function = "ShowAdSucessEvent";
            SendToIL2CPP(ip.ToString());
            UnityPlayer.AppCallbacks.Instance.UnityPause(1);
        }

        // OPTIONAL - Delegated event when AdDeals Ad could not display any ad to end users.
        private static void ShowAdFailed_Event(object sender, DetailedEventArgs e)
        {
            Log("No ad available for this user or there is some issue (network access...).To increase your fill rate worldwide (up to 100%), go to your AdDeals account and create an interstitial campaign.");
            InvokeParam ip = new InvokeParam();
            ip.function = "ShowAdFailedEvent";
            ip.paramlist.Add(new Param(e.Details));
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals detected that an ad has been displayed.
        private static void CacheAdSuccess_Event(object sender, EventArgs e)
        {
            Log("This ad could be cached successfully.");
            InvokeParam ip = new InvokeParam();
            ip.function = "CacheAdSuccessEvent";
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals Ad could not display any ad to end users.
        private static void CacheAdFailed_Event(object sender, DetailedEventArgs e)
        {
            Log("This ad could not be cached. However, only one can be cached until display so you may call: IsCachedAdAvailable() prior caching a new one.");
            InvokeParam ip = new InvokeParam();
            ip.function = "CacheAdFailedEvent";
            ip.paramlist.Add(new Param(e.Details));
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals ad does not return any ad to end users.
        private static void MinDelayBtwAdsNotReached_Event(object sender, EventArgs e)
        {
            Log("Delay between two ads is not reached. The minimal time between two ad calls is 10 seconds. You can set a higher number if you wish.");
            InvokeParam ip = new InvokeParam();
            ip.function = "MinDelayBtwAdsNotReachedEvent";
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals ad is closed by end user.
        private static void AdClosed_Tap(object sender, EventArgs e)
        {
            UnityPlayer.AppCallbacks.Instance.UnityPause(0);
            Log("The user closed AdDeals Ad.");
            InvokeParam ip = new InvokeParam();
            ip.function = "AdClosedTap";
            SendToIL2CPP(ip.ToString());
        }

        // OPTIONAL - Delegated event when AdDeals ad is closed by end user.
        private static void AdClicked_Tap(object sender, EventArgs e)
        {
            Log("The user clicked on AdDeals Ad.");
            InvokeParam ip = new InvokeParam();
            ip.function = "AdClickedTap";
            SendToIL2CPP(ip.ToString());
        }
        #endregion

    }

    public class ParamConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(Param) == objectType;
        }

        private bool IsNumber(object value)
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = reader.Value as string;
            if (null != s)
            {
                return new Param(s);
            }
            else if (IsNumber(reader.Value))
            {
                return new Param(Convert.ToInt32(reader.Value));
            }
            else if (reader.Value is bool)
            {
                return new Param((bool)reader.Value);
            }
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var p = value as Param;
            if (0 == p.getValType())
            {
                writer.WriteValue(p.asInt());
            }
            else if (1 == p.getValType())
            {
                writer.WriteValue(p.asString());
            }
            else if (2 == p.getValType())
            {
                writer.WriteValue(p.asBoolean());
            }
        }
    }

    [JsonConverter(typeof(ParamConverter))]
    public class Param
    {
        private int intVal;
        private string stringVal;
        private bool boolVal;
        private int valType; // 0:int 1:string 2:boolean


        public Param(int i)
        {
            intVal = i;
            valType = 0;
        }

        public Param(string s)
        {
            stringVal = s;
            valType = 1;
        }

        public Param(bool b)
        {
            boolVal = b;
            valType = 2;
        }

        public string asString()
        {
            switch(valType)
            {
                case 0:
                    {
                        return intVal.ToString();
                    }
                case 1:
                    {
                        return stringVal;
                    }
                case 2:
                    {
                        if (boolVal)
                        {
                            return "true";
                        }
                        else
                        {
                            return "false";
                        }
                    }
            }
            return "";
        }

        public int asInt()
        {
            switch (valType)
            {
                case 0:
                    {
                        return intVal;
                    }
                case 1:
                    {
                        int i = 0;

                        try
                        {
                            i = int.Parse(stringVal);
                        }
                        catch (Exception e)
                        {
                            i = 0;
                        }

                        return i;
                    }
                case 2:
                    {
                        if (boolVal)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
            }
            return 0;
        }

        public bool asBoolean()
        {
            switch (valType)
            {
                case 0:
                    {
                        if (0 == intVal)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case 1:
                    {
                        if (null == stringVal || 0 == stringVal.Length)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case 2:
                    {
                        return boolVal;
                    }
            }
            return false;
        }

        public int getValType()
        {
            return valType;
        }

    }

    public class InvokeParam
    {
        public InvokeParam()
        {
            function = "";
            paramlist = new List<Param>();
        }

        public static InvokeParam Parse(string json)
        {
            var ip = JsonConvert.DeserializeObject<InvokeParam>(json);
            return ip;
        }

        //public void parseJson(string json)
        //{
        //    JObject jo = (JObject)JsonConvert.DeserializeObject(json);
        //    function = jo["f"].ToString();
        //    var ja = jo["p"].ToList<JToken>();
        //    foreach (var jt in ja)
        //    {
        //        if (jt.GetType() == typeof(string))
        //        {
        //            paramlist.Add(new Param(jt.Value<string>()));
        //        }
        //        else if (jt.GetType() == typeof(int))
        //        {
        //            paramlist.Add(new Param(jt.Value<int>()));
        //        }
        //    }
        //}

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        [JsonProperty("f")]
        public string function;

        [JsonProperty("p")]
        public List<Param> paramlist;
    }
}
