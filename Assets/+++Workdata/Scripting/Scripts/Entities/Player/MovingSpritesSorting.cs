using UnityEngine;

public class MovingSpritesSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
        UpdateSortingOrder();
    }

    private void Update()
    {
        if (transform.position != lastPosition) // Only update if moved
        {
            UpdateSortingOrder();
            lastPosition = transform.position;
        }
    }

    public void UpdateSortingOrder()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
