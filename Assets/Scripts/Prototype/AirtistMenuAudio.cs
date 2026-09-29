using UnityEngine;

namespace Airtist.Prototype
{
    // Lives on the controller, not a menu: closing a window does not cut off its click.
    public sealed class AirtistMenuAudio : MonoBehaviour
    {
        private AudioSource source;
        private AudioSource celebrationSource;
        private AirtistSettingsConfig config;
        private float lastClick=-10;

        public static void Wire(UnityEngine.UI.Button button)
        {
            if(button==null || button.GetComponent<AirtistArtworkPointer>()!=null)return;
            var owner=button.GetComponentInParent<AirtistLandscapePrototypeController>(true);
            if(owner==null)return;
            var audio=owner.GetComponent<AirtistMenuAudio>();
            if(audio==null)audio=owner.gameObject.AddComponent<AirtistMenuAudio>();
            audio.config=AirtistSettingsConfig.Current;
            // Theme refreshes and gallery re-binding must not stack duplicate sounds.
            button.onClick.RemoveListener(audio.PlayClick);
            button.onClick.AddListener(audio.PlayClick);
        }

        private void PlayClick()
        {
            // Static previews and edit-mode verification are deliberately silent.
            if(!Application.isPlaying || config==null || config.menuClick==null
                || PlayerPrefs.GetInt("AIrtist.Settings.Sound",1)==0 || AudioListener.pause || AudioListener.volume<=0)return;
            if(Time.unscaledTime-lastClick<.045f)return;
            if(source==null)
            {
                var go=new GameObject("MenuClickAudio");go.transform.SetParent(transform,false);
                source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;
                source.spatialBlend=0;source.dopplerLevel=0;source.priority=64;
                source.bypassReverbZones=true;source.volume=1;
            }
            lastClick=Time.unscaledTime;
            source.PlayOneShot(config.menuClick,Mathf.Clamp01(config.menuClickVolume));
        }

        public void PlayPaintingComplete()
        {
            config=AirtistSettingsConfig.Current;
            if(!Application.isPlaying || config==null || config.paintingComplete==null
                || PlayerPrefs.GetInt("AIrtist.Settings.Sound",1)==0 || AudioListener.pause || AudioListener.volume<=0)return;
            if(celebrationSource==null)
            {
                var go=new GameObject("PaintingCompleteAudio");go.transform.SetParent(transform,false);
                celebrationSource=go.AddComponent<AudioSource>();celebrationSource.playOnAwake=false;celebrationSource.loop=false;
                celebrationSource.spatialBlend=0;celebrationSource.dopplerLevel=0;celebrationSource.priority=48;
                celebrationSource.bypassReverbZones=true;celebrationSource.volume=1;
            }
            celebrationSource.PlayOneShot(config.paintingComplete,Mathf.Clamp01(config.paintingCompleteVolume));
        }
    }
}
