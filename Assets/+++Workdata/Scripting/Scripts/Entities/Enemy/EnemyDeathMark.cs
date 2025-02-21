using UnityEngine;

public class EnemyDeathMark : MonoBehaviour
{
    private float alphaValue = 1;
    private Color deathMarkColor;

    private void Start() => deathMarkColor = GetComponent<SpriteRenderer>().color;

    private void Update()
    {
        alphaValue -= Time.deltaTime / 2;
        
        deathMarkColor = new Color(deathMarkColor.r, deathMarkColor.g, deathMarkColor.b, alphaValue);
        
        if (deathMarkColor.a == 0)
        {
            Destroy(gameObject);
        }
    }
}
