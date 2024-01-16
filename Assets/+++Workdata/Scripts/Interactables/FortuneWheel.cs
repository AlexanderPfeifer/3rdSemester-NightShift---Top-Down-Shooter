using UnityEngine;

public class FortuneWheel : MonoBehaviour
{
    [SerializeField] public GameObject ride;
    
    public void DeactivateFortuneWheel()
    {
        gameObject.SetActive(false);
    }
}
