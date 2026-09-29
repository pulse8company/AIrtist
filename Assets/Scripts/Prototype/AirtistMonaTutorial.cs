using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed partial class AirtistLandscapePrototypeController
    {
        private int tutorialStage=-1,tutorialVersion;
        private RectTransform tutorialPanel;
        private TMP_Text tutorialTitle,tutorialText,tutorialNextText;
        private UnityEngine.UI.Button tutorialNext,tutorialZoom;
        private int renderedTutorialStage=-2;
        private bool TutorialRunning => tutorialStage>=0 && selectedChapter==0 && galleryOpen;
        private static readonly string[] TutorialTitles={
            "Твоё первое расследование","1 · Проверка","2 · Пометка","3 · Обзор",
            "4 · Область","5 · Точно","6 · Убрать","7 · Видео","Теперь твоя очередь"
        };
        private static readonly string[] TutorialTexts={
            "На «Моне Лизе» четыре дорисовки. Разберём инструменты на трёх заметных предметах, а четвёртый ты найдёшь самостоятельно.\n\nВо время обучения время и запасы не расходуются.",
            "Выбери «Проверка» и коснись лишней детали на лице. В обычной игре каждое нажатие расходует энергию и проверку — даже промах.",
            "Выбери «Пометка» и коснись любого места на картине. Это временная заметка, а не находка. Повторное касание убирает её; энергия не тратится.",
            "Выбери «Обзор» и увеличь картину: двумя пальцами, колесом мыши или кнопкой ниже. Здесь касания не проверяют предметы. Нажми «ОК», чтобы идти дальше.",
            "Нажми кисть «Область». Она выделяет примерную зону, но не находит предмет за тебя. Сейчас попробуем бесплатно.",
            "Нажми кисть «Точно» или кнопку «Проверка», затем коснись выделенных часов на руке. Учебная проверка бесплатна. В обычной игре кисть расходует подсказки.",
            "Нажми кисть «Убрать»: она сама исключит один предмет из поиска. Учебное применение не расходует подсказки.",
            "Крайняя справа кисть «Видео» позволяет получить случайный бонус за просмотр рекламы.\n\nВ обучении ролик не запускается и бонус не выдаётся. Нажми «ОК», чтобы продолжить.",
            "Инструменты знакомы! На картине осталась ещё одна, менее заметная дорисовка. Ищи её самостоятельно: увеличивай изображение и сравнивай детали.\n\nПосле старта действуют обычные лимиты и стоимость проверок."
        };

        private void BuildTutorial()
        {
            if(gameplayScreen==null || AirtistApprovedTheme.Current==null)return;
            var root=(RectTransform)gameplayScreen.transform;
            tutorialPanel=CreatePanel(root,"MonaTutorial",AirtistApprovedTheme.Paper,Anchor.Stretch,Vector2.zero,Vector2.zero);
            AirtistApprovedTheme.Rect(tutorialPanel,.735f,.12f,.25f,.52f);
            AirtistApprovedTheme.Current.Surface(tutorialPanel.GetComponent<UnityEngine.UI.Image>(),AirtistApprovedTheme.Paper);
            tutorialPanel.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            tutorialTitle=IntroLabel(tutorialPanel,"TutorialTitle","",.065f,.04f,.87f,.11f,26);
            tutorialText=IntroLabel(tutorialPanel,"TutorialText","",.075f,.18f,.85f,.58f,22);
            tutorialText.alignment=TextAlignmentOptions.TopLeft;
            tutorialNext=IntroButton(tutorialPanel,"TutorialNext","Начать",.07f,.82f,.86f,.13f,TutorialNextStep);
            tutorialNextText=GetButtonLabel(tutorialNext);
            AirtistApprovedTheme.Current.Button(tutorialNext,AirtistApprovedTheme.Sage);
            AirtistApprovedTheme.Rect(tutorialNextText.rectTransform,.20f,.08f,.76f,.84f);
            // Two graphic strokes keep the confirmation icon independent of font glyph coverage.
            var checkRoot=new GameObject("OkIcon",typeof(RectTransform)).GetComponent<RectTransform>();
            checkRoot.SetParent(tutorialNext.transform,false);
            AirtistApprovedTheme.Rect(checkRoot,.075f,.25f,.09f,.50f);
            AddTutorialCheckStroke(checkRoot,new Vector2(.28f,.36f),new Vector2(.42f,.16f),-45);
            AddTutorialCheckStroke(checkRoot,new Vector2(.62f,.54f),new Vector2(.80f,.16f),45);
            tutorialZoom=IntroButton(tutorialPanel,"TutorialZoom","Приблизить",.07f,.66f,.86f,.12f,()=>{
                if(TutorialRunning && tutorialStage==3 && overviewMode)galleryPanZoom?.ZoomIn();
            });
            tutorialPanel.gameObject.SetActive(false);
        }

        private static void AddTutorialCheckStroke(RectTransform parent,Vector2 center,Vector2 size,float angle)
        {
            var stroke=new GameObject("Stroke",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=(RectTransform)stroke.transform;rect.SetParent(parent,false);
            rect.anchorMin=center-size*.5f;rect.anchorMax=center+size*.5f;
            rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localRotation=Quaternion.Euler(0,0,angle);
            var image=stroke.GetComponent<UnityEngine.UI.Image>();image.color=AirtistApprovedTheme.Ink;image.raycastTarget=false;
        }

        private void BeginMonaTutorial()
        {
            TickAttemptClock();selectedChapter=0;tutorialStage=0;renderedTutorialStage=-2;
            if(chapterCollected[0] && IsChapterComplete(0))
            {
                // Explicit re-entry through the story starts a replay, keeping collection,
                // best stars and one-time reward history intact.
                ReplayRestoration();
            }
            else
            {
                if(attempts[0]!=null && attempts[0].Exhausted)attempts[0]=null;
                OpenChapter(0);
            }
            SelectWorkingTool(false);galleryPanZoom?.ResetView();
            SaveProgress();RefreshTutorial();
        }
        private void SetTutorialStage(int stage)
        {
            tutorialStage=stage;renderedTutorialStage=-2;
            if(stage==3)galleryPanZoom?.ResetView();
            if(stage==4){ClearTemporaryMarks();SelectWorkingTool(false);galleryPanZoom?.ResetView();}
            SaveProgress();RefreshTutorial();
        }
        private void TutorialNextStep()
        {
            if(!TutorialRunning)return;
            // Confirmation advances the explanation, never awarding skipped findings or bonuses.
            if(tutorialStage>=8)FinishMonaTutorial();
            else SetTutorialStage(tutorialStage+1);
        }
        private void FinishMonaTutorial()
        {
            tutorialStage=-1;tutorialVersion=1;
            areaHintTargets[0]=exactHintTargets[0]=0;ClearTemporaryMarks();
            galleryPanZoom?.ResetView();SelectWorkingTool(false);
            clockStamp=Time.realtimeSinceStartupAsDouble;
            SaveProgress();UpdateProgressLabels();RefreshTutorial();
            if(galleryFeedback!=null)galleryFeedback.text="Найди оставшиеся дорисовки. Картина готова только после всех четырёх.";
        }
        private bool TutorialAllowsArtwork(int artifact)
        {
            return tutorialStage==1 && !markingMode && artifact==0
                || tutorialStage==2 && markingMode
                || tutorialStage==5 && !markingMode && artifact==1 && exactHintTargets[0]==2;
        }
        private void TutorialObjectFound(int artifact)
        {
            if(!TutorialRunning)return;
            if(IsChapterComplete(0)){FinishMonaTutorial();return;}
            if(tutorialStage==1 && artifact==0)SetTutorialStage(2);
            else if(tutorialStage==5 && artifact==1)SetTutorialStage(6);
            else if(tutorialStage==6 && artifact==2)SetTutorialStage(7);
        }
        private void UseTutorialBrush(int kind)
        {
            if(!TutorialRunning || appPaused || appUnfocused || adPending || SettingsOpen)return;
            if(tutorialStage==4 && kind==0)
            {
                areaHintTargets[0]=artifactFound[0][1]?0:2;
                SelectWorkingTool(false);
                var point=HintPoint(1);
                point=new Vector2((Mathf.Floor(point.x*3)+.5f)/3,(Mathf.Floor(point.y*3)+.5f)/3);
                // Do not inherit the player's zoom from the navigation lesson.
                galleryPanZoom?.ResetView();
                galleryPanZoom?.FocusOn(point);UpdateArtifactTargets(false,true);SetTutorialStage(5);
            }
            else if(tutorialStage==5 && kind==1)
            {
                if(artifactFound[0][1]){SetTutorialStage(6);return;}
                exactHintTargets[0]=2;SelectWorkingTool(false);galleryPanZoom?.FocusOn(HintPoint(1));
                SaveProgress();UpdateArtifactTargets(false,true);
            }
            else if(tutorialStage==6 && kind==2)
            {
                if(artifactFound[0][2])SetTutorialStage(7);
                else CompleteAssistedObject(2,false);
                galleryPanZoom?.ResetView();
            }
            else if(tutorialStage==7 && kind==3)SetTutorialStage(8);
        }
        private void RefreshTutorial()
        {
            if(tutorialPanel==null)return;
            bool visible=TutorialRunning;
            tutorialPanel.gameObject.SetActive(visible);
            RefreshTutorialGuidance(visible);
            if(!visible)return;
            if(renderedTutorialStage!=tutorialStage)
            {
                renderedTutorialStage=tutorialStage;
                // Also recover the overview when resuming a saved tutorial at this step.
                if(tutorialStage==4)galleryPanZoom?.ResetView();
                tutorialTitle.text=TutorialTitles[tutorialStage];tutorialText.text=TutorialTexts[tutorialStage];
                // Reserve a separate row for the zoom action rather than covering the copy.
                AirtistApprovedTheme.Rect(tutorialText.rectTransform,.075f,.18f,.85f,tutorialStage==3?.44f:.58f);
                if(tutorialStage==1 && artifactFound[0][0])tutorialText.text="Этот предмет уже найден. Нажми «ОК», чтобы познакомиться с остальными инструментами.";
                if(tutorialStage==8 && FoundArtifactCount(0)!=3)
                    tutorialText.text="Инструменты знакомы! Найди оставшиеся дорисовки самостоятельно. Увеличивай изображение и сравнивай детали.\n\nПропущенные учебные находки не засчитываются. После старта действуют обычные лимиты и стоимость проверок.";
                tutorialNextText.text=tutorialStage==8?"ОК · искать самому":"ОК · дальше";
            }
            tutorialNext.gameObject.SetActive(true);
            tutorialNext.interactable=true;
            tutorialZoom.gameObject.SetActive(tutorialStage==3);
            tutorialZoom.interactable=overviewMode;
            if(attemptHud!=null){attemptHud.timeLabel.text="Обучение · пауза";attemptHud.endPanel.SetActive(false);}
            RefreshTutorialTools();
        }
        private void RefreshTutorialTools()
        {
            if(gameplayScreen==null)return;
            gameplayScreen.mainTool.interactable=tutorialStage==1 || tutorialStage==5;
            gameplayScreen.markTool.interactable=tutorialStage==2;
            gameplayScreen.zoomTool.interactable=tutorialStage==3;
            gameplayScreen.help.interactable=false;
            for(int i=0;i<4;i++)
            {
                gameplayScreen.brushes[i].interactable=tutorialStage==i+4;
                if(gameplayScreen.brushCosts!=null && i<gameplayScreen.brushCosts.Length)
                    gameplayScreen.brushCosts[i].text=AirtistApprovedTheme.Current!=null
                        ? (i==3?"Обучение":"Бесплатно") : (i==3?"i":"0");
            }
        }
    }
}
