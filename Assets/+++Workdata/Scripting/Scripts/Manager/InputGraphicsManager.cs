using UnityEngine;

public class InputGraphicsManager : Singleton<InputGraphicsManager>
{
    [SerializeField] private GameObject[] mouseGraphics;
    [SerializeField] private GameObject[] controllerGraphics;
    [SerializeField] private GameObject chalkObjects;

    public void SetInputGraphics(bool mouseIsLastUsedDevice)
    {
        if (mouseIsLastUsedDevice)
        {
            foreach (var graphic in mouseGraphics)
            {
                graphic.SetActive(true);
            }

            foreach (var graphic in controllerGraphics)
            {
                graphic.SetActive(false);
            }
        }
        else
        {
            foreach (var graphic in controllerGraphics)
            {
                graphic.SetActive(true);
            }

            foreach (var graphic in mouseGraphics)
            {
                graphic.SetActive(false);
            }
        }
    }

    public void RemoveAllChalkSigns()
    {
        chalkObjects.SetActive(false);
    }
}
