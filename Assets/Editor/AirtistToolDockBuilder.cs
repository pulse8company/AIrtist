using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Airtist.Prototype.Editor
{
    public static class AirtistToolDockBuilder
    {
        public static string Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            var root=PrefabUtility.LoadPrefabContents(AirtistGameplayBuilder.Path);
            try
            {
                var view=root.GetComponent<AirtistGameplayScreen>();
                var buttons=new[]{view.mainTool,view.markTool,view.zoomTool,view.brushes[0],view.brushes[1],view.brushes[2],view.brushes[3]};
                string[] names={"Проверка","Пометка","Обзор","Область","Точно","Убрать","Реклама"};
                float[] starts={.202f,.301f,.400f,.516f,.616f,.716f,.816f};
                Color[] palette={new Color(.73f,.84f,.80f),new Color(.99f,.96f,.88f),new Color(.99f,.96f,.88f),
                    new Color(.96f,.83f,.56f),new Color(.76f,.86f,.81f),new Color(.94f,.76f,.65f),new Color(.84f,.80f,.91f)};
                view.brushCosts=new TMP_Text[4];
                for(int i=0;i<buttons.Length;i++)
                {
                    var button=buttons[i];
                    Place((RectTransform)button.transform,starts[i],.011f,starts[i]+.091f,.119f);
                    button.image.sprite=null;button.image.type=UnityEngine.UI.Image.Type.Simple;button.image.color=palette[i];
                    button.image.raycastPadding=Vector4.zero;
                    foreach(var raw in button.GetComponentsInChildren<UnityEngine.UI.RawImage>(true)) raw.gameObject.SetActive(false);
                    var label=button.GetComponentInChildren<TextMeshProUGUI>();
                    label.text=names[i];label.color=new Color(.08f,.21f,.23f);label.alignment=TextAlignmentOptions.Center;
                    Place(label.rectTransform,.03f,.06f,.97f,.34f);
                    var scaling=label.GetComponent<AirtistScaledLabel>() ?? label.gameObject.AddComponent<AirtistScaledLabel>();
                    scaling.artboard=(RectTransform)root.transform;scaling.designFontSize=19;
                    var iconTransform=button.transform.Find("ToolIcon");
                    var icon=iconTransform!=null?iconTransform.GetComponent<AirtistToolIcon>():null;
                    if(icon==null)
                    {
                        var go=new GameObject("ToolIcon",typeof(RectTransform),typeof(CanvasRenderer),typeof(AirtistToolIcon));
                        go.transform.SetParent(button.transform,false);icon=go.GetComponent<AirtistToolIcon>();
                    }
                    icon.kind=i;icon.color=new Color(.18f,.33f,.33f);icon.raycastTarget=false;
                    Place(icon.rectTransform,.18f,.37f,.66f,.94f);
                    var outline=button.GetComponent<UnityEngine.UI.Outline>() ?? button.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor=new Color(.35f,.30f,.23f,.22f);outline.effectDistance=new Vector2(1,-1);
                    if(i>=3)
                    {
                        button.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();
                        var existing=button.transform.Find("Cost");
                        TMP_Text cost=existing!=null?existing.GetComponent<TMP_Text>():null;
                        if(cost==null)
                        {
                            var go=new GameObject("Cost",typeof(RectTransform),typeof(TextMeshProUGUI));
                            go.transform.SetParent(button.transform,false);cost=go.GetComponent<TextMeshProUGUI>();cost.font=label.font;
                        }
                        Place(cost.rectTransform,.59f,.57f,.98f,.89f);
                        cost.text=i==6?"Видео":(i-2)+" / 3";cost.color=new Color(.18f,.27f,.27f);cost.raycastTarget=false;
                        cost.alignment=TextAlignmentOptions.Center;
                        var scale=cost.GetComponent<AirtistScaledLabel>() ?? cost.gameObject.AddComponent<AirtistScaledLabel>();
                        scale.artboard=(RectTransform)root.transform;scale.designFontSize=15;
                        view.brushCosts[i-3]=cost;
                    }
                }
                Place((RectTransform)view.help.transform,.928f,.025f,.984f,.104f);
                view.help.image.sprite=null;view.help.image.color=new Color(.99f,.96f,.88f);view.help.image.raycastPadding=Vector4.zero;
                Place(view.title.rectTransform,.012f,.052f,.185f,.116f);
                Place(view.progress.rectTransform,.012f,.014f,.185f,.049f);
                PrefabUtility.SaveAsPrefabAsset(root,AirtistGameplayBuilder.Path);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
            return "Saved 3 working tools, 4 bonus brushes, vector icons and editable cost labels. No Play/build.";
        }
        private static void Place(RectTransform rect,float x0,float y0,float x1,float y1)
        {rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
