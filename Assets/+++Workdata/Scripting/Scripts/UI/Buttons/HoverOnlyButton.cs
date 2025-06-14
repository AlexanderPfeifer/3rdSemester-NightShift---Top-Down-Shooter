using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverOnlyButton : Button
{
    public bool disableClick;
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!disableClick)
            base.OnPointerClick(eventData);
    }
}
