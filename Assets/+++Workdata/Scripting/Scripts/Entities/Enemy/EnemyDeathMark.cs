using UnityEngine;

public class EnemyDeathMark : MonoBehaviour
{
    private float alphaValue = 1;

    //When death mark is spawned, it slowly disappears again and then destroys itself
    private void Update()
    {
        alphaValue -= Time.deltaTime / 2;
        
        GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, 
            GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b,
            alphaValue);
        
        if (GetComponent<SpriteRenderer>().color.a == 0)
        {
            Destroy(gameObject);
        }
    }
}
