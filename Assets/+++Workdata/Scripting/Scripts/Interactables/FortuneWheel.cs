using UnityEngine;

public class FortuneWheel : MonoBehaviour
{
    [SerializeField] public GameObject ride;
    
    //I set the fortune wheel to false when it got interacted with, because it's placed multiple times, it was easier to 
    //deactivate the fortune wheel from itself by searching for it in overlap circle
    public void DeactivateFortuneWheel()
    {
        gameObject.SetActive(false);
    }
}
