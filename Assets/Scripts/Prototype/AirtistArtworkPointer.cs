using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Airtist.Prototype
{
    public sealed class AirtistArtworkPointer : MonoBehaviour, IPointerClickHandler
    {
        public Action<PointerEventData> clicked;
        public void OnPointerClick(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left || e.dragging) return;
            float threshold=EventSystem.current!=null ? EventSystem.current.pixelDragThreshold : 10;
            if((e.position-e.pressPosition).sqrMagnitude>threshold*threshold) return;
            clicked?.Invoke(e);
        }
    }
}
