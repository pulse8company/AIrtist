using System;
using UnityEngine;

namespace Airtist.Prototype
{
    [CreateAssetMenu(menuName="AIrtist/Settings links")]
    public sealed class AirtistSettingsConfig : ScriptableObject
    {
        public string privacyPolicyUrl="https://www.pulse8gaming.com/privacy-policy";
        [Tooltip("Leave empty until the official page is published.")]
        public string instagramUrl="", facebookUrl="", supportEmail="";
        [Min(0)] public int socialRewardCoins=50;
        [Tooltip("Only enable locales after the whole game is translated and checked.")]
        public string[] publishedLocaleCodes={"ru"};
        public AudioClip menuClick;
        [Range(0,1)] public float menuClickVolume=.55f;
        public AudioClip paintingComplete;
        [Range(0,1)] public float paintingCompleteVolume=.65f;
        public static AirtistSettingsConfig Current=>Resources.Load<AirtistSettingsConfig>("SettingsConfig");
        public static bool IsWebUrl(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)
            && uri.Scheme==Uri.UriSchemeHttps && !string.IsNullOrEmpty(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);
        public static bool IsSocialUrl(string value,int network)
        {
            if(!IsWebUrl(value))return false;
            string host=new Uri(value).Host, expected=network==0?"instagram.com":"facebook.com";
            return host.Equals(expected,StringComparison.OrdinalIgnoreCase) || host.EndsWith("."+expected,StringComparison.OrdinalIgnoreCase);
        }
    }
}
