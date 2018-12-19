using System;
using UnityEngine;

namespace AdDeals
{
    public class AdDealsWrapperBase
    {
        // 0:Unknown 1:portrait 2:portraitUpsideDown 3:LandscapeRight 4:LandscapeLeft
        public const int UIOrientationUnknown = 0;
        public const int UIOrientationPortrait = 1;
        public const int UIOrientationLandscape = 2;

        public const int AdTypeInterstitial = 1;
        public const int AdTypeRewardVideo = 2;

        // iOS:AdDealsUserConsentNotApplicable:-1 Android:NOT_ELIGIBLE:2
        public const int UserConsentNotApplicable = -1;
        // iOS:AdDealsUserConsentRevoke:0 Android:DISAGREE:1
        public const int UserConsentRevoke = 0;
        // iOS:AdDealsUserConsentGrant:1 Android:APPROVE:0
        public const int UserConsentGrant = 1;
        // iOS:AdDealsUserConsentNotApplicable:-1 Android:NOT_SET:3
        public const int UserConsentNotSet = 2;
    }
}

