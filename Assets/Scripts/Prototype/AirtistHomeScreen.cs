using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>Interactive Home with independently laid out artwork and controls.</summary>
    public sealed class AirtistHomeScreen : MonoBehaviour
    {
        // Gift, Map, Collection, Store, Profile, Continue, Daily reward, World map.
        [SerializeField] private UnityEngine.UI.Button[] buttons;
        [SerializeField] private GameObject claimedBadge;
        [SerializeField] private UnityEngine.Localization.Components.LocalizeStringEvent claimCaption;

        public UnityEngine.UI.Button[] Buttons => buttons;

        public void ApplyFlexiblePresentation(Sprite panelSprite, Sprite backdrop)
        {
            if(transform.Find("FlexibleRoutePanel")!=null) return;
            var root=(RectTransform)transform;
            // Retain the approved portrait as a separate, undistorted illustration. The old
            // background tiles cannot be stretched independently without visible seams.
            Texture portraitTexture=null;
            foreach(Transform child in transform)
            {
                if(!child.name.StartsWith("Artwork_",StringComparison.Ordinal)) continue;
                var image=child.GetComponent<UnityEngine.UI.Image>();
                if(image!=null && image.sprite!=null) portraitTexture=image.sprite.texture;
                child.gameObject.SetActive(false);
            }
            if(portraitTexture!=null)
            {
                var slot=new GameObject("FlexiblePortraitSlot",typeof(RectTransform)).GetComponent<RectTransform>();
                slot.SetParent(root,false); Place(slot,.63f,.255f,.91f,.89f);
                var portrait=new GameObject("AmeliePortrait",typeof(RectTransform),typeof(UnityEngine.UI.RawImage),typeof(UnityEngine.UI.AspectRatioFitter));
                portrait.transform.SetParent(slot,false);
                var raw=portrait.GetComponent<UnityEngine.UI.RawImage>();
                raw.texture=portraitTexture; raw.raycastTarget=false;
                raw.uvRect=new Rect(1045f/1672,226f/941,474f/1672,628f/941);
                var fit=portrait.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                fit.aspectMode=UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio=474f/628;
                foreach(string labelName in new[]{"Text_home.character.name","Text_home.character.role"})
                {
                    var label=transform.Find(labelName) as RectTransform;
                    if(label==null) continue;
                    var min=label.anchorMin; var max=label.anchorMax;
                    label.SetParent(portrait.transform,false);
                    Place(label,(min.x*1672-1045)/474,(min.y*941-226)/628,
                        (max.x*1672-1045)/474,(max.y*941-226)/628);
                }
                slot.SetAsFirstSibling();
            }
            AddPanel("FlexibleRoutePanel",new Color(.99f,.94f,.81f,.97f),.045f,.105f,.59f,.45f);
            AddPanel("FlexibleTitlePanel",new Color(1f,.97f,.88f,.82f),.09f,.565f,.535f,.87f);
            if(backdrop!=null)
            {
                var thumb=AddPanel("FlexibleRouteThumbnail",Color.white,.063f,.255f,.213f,.38f);
                thumb.sprite=backdrop; thumb.type=UnityEngine.UI.Image.Type.Simple; thumb.preserveAspect=true;
                thumb.transform.SetSiblingIndex(transform.Find("FlexibleRoutePanel").GetSiblingIndex()+1);
            }
            // Keep original Button instances and localization components: actions and keys survive.
            Surface(5,new Color(.79f,.35f,.25f));
            Surface(6,new Color(.96f,.77f,.43f));
            buttons[7].gameObject.SetActive(false);
            var mapCaption=transform.Find("Text_home.map.open");
            if(mapCaption!=null) mapCaption.gameObject.SetActive(false);
            Place((RectTransform)buttons[5].transform,.065f,.13f,.325f,.235f);
            Place((RectTransform)buttons[6].transform,.34f,.13f,.575f,.235f);
            RestyleCaption("Text_home.continue",.075f,.145f,.315f,.22f,new Color(1f,.97f,.89f));
            RestyleCaption("Text_home.daily.description",.35f,.195f,.565f,.225f,new Color(.22f,.25f,.23f));
            RestyleCaption("Text_home.daily.claim",.35f,.14f,.565f,.195f,new Color(.12f,.23f,.25f));
            void RestyleCaption(string name,float x0,float y0,float x1,float y1,Color color)
            {
                var text=transform.Find(name)?.GetComponent<TMP_Text>();
                if(text==null) return;
                Place(text.rectTransform,x0,y0,x1,y1); text.color=color; text.alignment=TextAlignmentOptions.Center;
            }
            void Surface(int index,Color color)
            {
                var image=buttons[index].GetComponent<UnityEngine.UI.Image>();
                image.sprite=panelSprite; image.type=UnityEngine.UI.Image.Type.Sliced; image.color=color;
                image.raycastPadding=Vector4.zero;
                var outline=image.GetComponent<UnityEngine.UI.Outline>() ?? image.gameObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor=new Color(.42f,.28f,.17f,.35f); outline.effectDistance=new Vector2(1,-1);
            }
            UnityEngine.UI.Image AddPanel(string name,Color color,float x0,float y0,float x1,float y1)
            {
                var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));
                go.transform.SetParent(root,false); Place((RectTransform)go.transform,x0,y0,x1,y1);
                go.transform.SetAsFirstSibling();
                var image=go.GetComponent<UnityEngine.UI.Image>(); image.sprite=panelSprite;
                image.type=UnityEngine.UI.Image.Type.Sliced; image.color=color; image.raycastTarget=false;
                return image;
            }
        }

        private static void Place(RectTransform rect,float x0,float y0,float x1,float y1)
        {
            rect.anchorMin=new Vector2(x0,y0); rect.anchorMax=new Vector2(x1,y1);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }

        public void Configure(UnityEngine.UI.Button[] controls, GameObject badge, UnityEngine.Localization.Components.LocalizeStringEvent caption = null)
        {
            buttons = controls;
            claimedBadge = badge;
            claimCaption = caption;
        }

        public void Bind(Action[] actions)
        {
            if (buttons == null || actions.Length != buttons.Length)
                throw new InvalidOperationException("Home requires one action for each illustrated button.");

            for (int i = 0; i < buttons.Length; i++)
            {
                Action action = actions[i];
                buttons[i].onClick.AddListener(() => action());
            }
        }

        public void SetBonusClaimed(bool claimed)
        {
            buttons[0].interactable = !claimed;
            buttons[6].interactable = !claimed;
            if (claimedBadge != null) claimedBadge.SetActive(claimed);
            if (claimCaption != null)
            {
                string key = claimed ? "home.daily.claimed" : "home.daily.claim";
                if (claimCaption.StringReference.TableEntryReference.Key != key)
                {
                    claimCaption.StringReference.TableEntryReference = key;
                    claimCaption.RefreshString();
                }
            }
        }
    }
}
