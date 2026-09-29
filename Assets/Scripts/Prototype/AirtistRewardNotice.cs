using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistRewardNotice : MonoBehaviour
    {
        private float until;
        public void Show(){until=Time.unscaledTime+6;gameObject.SetActive(true);}
        private void Update(){if(Time.unscaledTime>=until)gameObject.SetActive(false);}
    }

    public sealed partial class AirtistLandscapePrototypeController
    {
        private AirtistRewardNotice brushRewardNotice;
        private TMPro.TMP_Text brushRewardNoticeText;
        private void ShowBrushRewardNotice(string message)
        {
            if(AirtistApprovedTheme.Current==null)return;
            if(brushRewardNotice==null)
            {
                var panel=CreatePanel((RectTransform)transform,"BrushRewardNotice",AirtistApprovedTheme.Paper,Anchor.Stretch,Vector2.zero,Vector2.zero);
                AirtistApprovedTheme.Rect(panel,.29f,.11f,.42f,.13f);
                AirtistApprovedTheme.Current.Surface(panel.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
                panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
                brushRewardNoticeText=IntroLabel(panel,"Reward","",.05f,.12f,.90f,.76f,27);
                brushRewardNoticeText.raycastTarget=false;
                brushRewardNotice=panel.gameObject.AddComponent<AirtistRewardNotice>();
            }
            brushRewardNoticeText.text=message;
            brushRewardNotice.transform.SetAsLastSibling();
            brushRewardNotice.Show();
        }
    }
}
