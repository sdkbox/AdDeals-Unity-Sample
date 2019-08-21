using System;
using UnityEngine;

namespace AdDeals
{
#if ENABLE_WINMD_SUPPORT
    public class AdDealsWrapper : AdDealsWrapperUWP
#elif UNITY_ANDROID
    public class AdDealsWrapper : AdDealsWrapperAndroid
#elif UNITY_IOS
    public class AdDealsWrapper : AdDealsWrapperIOS
#else
    public class AdDealsWrapper : AdDealsWrapperDummy
#endif
    {
    }
}

