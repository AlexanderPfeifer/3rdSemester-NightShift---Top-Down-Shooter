using UnityEngine;

public class FortuneWheel : MonoBehaviour
{
    [SerializeField] public GameObject ride;
    
    //Because it's placed multiple times, it was easier to deactivate the fortune wheel from itself
    public void DeactivateFortuneWheel()
    {
        gameObject.SetActive(false);
    }
}
