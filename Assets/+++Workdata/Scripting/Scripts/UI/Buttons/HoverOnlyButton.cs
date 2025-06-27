using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverOnlyButton : Button
{
    public bool disableClick;
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (disableClick)
            return;

        base.OnPointerClick(eventData);
    }

    //For controller
    public override void OnSubmit(BaseEventData eventData)
    {
        if (disableClick)
            return;

        base.OnSubmit(eventData);
    }
}
