using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    // Geographic anchors never move. Only overlapping markers are grouped at overview zoom.
    public sealed class AirtistMuseumMapLayout : MonoBehaviour
    {
        private sealed class Marker
        {
            public RectTransform root, tile;
            public TMP_Text caption, count;
            public GameObject badge;
            public string title;
            public Action open;
            public int group, members;
        }
        private readonly List<Marker> markers=new List<Marker>();
        private AirtistWorldMapScreen screen;
        public void Configure(AirtistWorldMapScreen view) => screen=view;
        public void Register(RectTransform root, RectTransform tile, UnityEngine.UI.Button button,
            TMP_Text caption, TMP_Text count, GameObject badge, string title, Action open)
        {
            var marker=new Marker{root=root,tile=tile,caption=caption,count=count,badge=badge,title=title,open=open};
            markers.Add(marker);
            button.onClick.AddListener(()=>
            {
                if(marker.members<=1){marker.open();return;}
                Vector2 center=Vector2.zero;int members=0;
                foreach(var other in markers)if(other.group==marker.group){center+=other.root.anchorMin;members++;}
                screen.FocusOn(center/Mathf.Max(1,members),Mathf.Min(6,screen.Zoom*2));
                Refresh();
            });
        }
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if(screen==null || screen.Viewport==null || markers.Count==0)return;
            float scale=Mathf.Clamp(screen.Viewport.rect.width/1672f,.4f,2f);
            for(int i=0;i<markers.Count;i++){markers[i].group=i;markers[i].members=0;}
            for(int i=0;i<markers.Count;i++)for(int j=i+1;j<markers.Count;j++)
            {
                Vector2 a=screen.Viewport.InverseTransformPoint(markers[i].root.position);
                Vector2 b=screen.Viewport.InverseTransformPoint(markers[j].root.position);
                if(Mathf.Abs(a.x-b.x)>=172*scale || Mathf.Abs(a.y-b.y)>=114*scale)continue;
                int from=markers[j].group,to=markers[i].group;
                if(from==to)continue;
                for(int k=0;k<markers.Count;k++)if(markers[k].group==from)markers[k].group=to;
            }
            for(int i=0;i<markers.Count;i++)markers[markers[i].group].members++;
            for(int i=0;i<markers.Count;i++)
            {
                var marker=markers[i];bool leader=marker.group==i;
                marker.tile.gameObject.SetActive(leader);
                if(!leader)continue;
                marker.tile.localScale=Vector3.one*(scale/Mathf.Max(1,screen.Zoom));
                marker.tile.anchoredPosition=new Vector2(0,-18)*(scale/Mathf.Max(1,screen.Zoom));
                marker.caption.text=marker.members>1?"Музеи рядом":marker.title;
                marker.badge.SetActive(marker.members>1);
                marker.count.text=marker.members.ToString();
            }
        }
    }
}
