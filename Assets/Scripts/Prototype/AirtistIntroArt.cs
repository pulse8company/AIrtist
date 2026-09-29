using UnityEngine;

namespace Airtist.Prototype
{
    // Artwork and text are independent: portraits/panels are views into approved art,
    // while every caption, choice and navigation control is an editable UI object.
    [CreateAssetMenu(menuName="AIrtist/Introduction art")]
    public sealed class AirtistIntroArt : ScriptableObject
    {
        public Texture2D portraits;
        public Texture2D comic;
        public Sprite[] heroSprites;
        public Sprite Hero(int hero)
        {
            hero=Mathf.Clamp(hero,0,2);
            if(heroSprites!=null && hero<heroSprites.Length && heroSprites[hero]!=null)return heroSprites[hero];
            return hero==0 && AirtistApprovedTheme.Current!=null?AirtistApprovedTheme.Current.amelie:null;
        }
        public Rect Portrait(int hero) => new Rect(.190f + Mathf.Clamp(hero,0,2)*.219f,.325f,.18f,.45f);
        public Rect Frame(int hero,int frame)
        {
            // The approved storyboard has deliberately unequal column widths.
            float[] left={.007f,.304f,.532f,.757f},width={.287f,.219f,.215f,.234f};
            float[] top={.010f,.322f,.633f},height={.298f,.298f,.312f};
            hero=Mathf.Clamp(hero,0,2);frame=Mathf.Clamp(frame,0,3);
            return new Rect(left[frame],1f-top[hero]-height[hero],width[frame],height[hero]);
        }
    }
}
