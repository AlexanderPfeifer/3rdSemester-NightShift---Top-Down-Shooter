using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllButtonsConfiguration : Singleton<AllButtonsConfiguration>
{
    public CanvasGroup inGameUICanvasGroup;

    private void Start()
    {
        foreach (Button _button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            AddHoverEvent(_button.gameObject);
        }
    }

    public void AddHoverEvent(GameObject buttonObject)
    {
        if (!TryGetComponent(out EventTrigger _eventTrigger))
        {
            _eventTrigger = buttonObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry _entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        _entry.callback.AddListener(_ => { OnHover(buttonObject); });
        _eventTrigger.triggers.Add(_entry);
    }

    private void OnHover(GameObject buttonObject)
    {
        EventSystem.current.SetSelectedGameObject(buttonObject);
    }
}
