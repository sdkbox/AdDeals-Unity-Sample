using System;
using UnityEngine;

namespace AdDeals
{
#if ENABLE_WINMD_SUPPORT && ENABLE_IL2CPP
    public class AdDealsWrapper : AdDealsWrapperUWPIL2CPP
#elif ENABLE_ADDEALS_UWP
    public class AdDealsWrapper : AdDealsWrapperUWPNet
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

