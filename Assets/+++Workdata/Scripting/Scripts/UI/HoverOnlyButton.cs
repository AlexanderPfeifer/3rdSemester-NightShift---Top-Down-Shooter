using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverOnlyButton : Button
{
    public bool disableClick = true;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!disableClick)
            base.OnPointerClick(eventData);
    }
}
