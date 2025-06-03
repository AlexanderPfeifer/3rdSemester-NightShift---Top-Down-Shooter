using UnityEngine;

public class SizeHighlight : MonoBehaviour
{
    [SerializeField] private float frequency = 1f;
    [SerializeField] private float maxScaleFactor = 2f;

    private Vector3 originalScale;
    private float timer;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime * frequency * 2f * Mathf.PI; 
        float _scaleFactor = Mathf.Lerp(1f, maxScaleFactor, (Mathf.Sin(timer) + 1f) / 2f); 
        transform.localScale = originalScale * _scaleFactor;
    }
}
