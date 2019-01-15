using System;
using UnityEngine;

namespace AdDeals
{
    public class AdDealsWrapperBase
    {
        // 0:Unknown 1:portrait 2:landscape
        public const int UIOrientationUnset = 0;
        public const int UIOrientationPortrait = 1;
        public const int UIOrientationLandscape = 2;

        public const int AdTypeInterstitial = 1;
        public const int AdTypeRewardedVideo = 2;

        public const int UserConsentNotApplicable = -1;
        public const int UserConsentRevoke = 0;
        public const int UserConsentGrant = 1;
    }
}

