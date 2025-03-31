using UnityEngine;

public class SpriteSortingManager : MonoBehaviour
{
    private void Start()
    {
        SpriteRenderer[] _allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        foreach (var _sprite in _allSprites)
        {
            if (_sprite.GetComponent<MovingSpritesSorting>() == null) 
            {
                _sprite.sortingOrder = Mathf.RoundToInt(-_sprite.transform.position.y * 100);
            }
        }
    }
}
