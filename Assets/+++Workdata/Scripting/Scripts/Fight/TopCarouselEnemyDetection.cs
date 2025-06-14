using UnityEngine;

public class TopCarouselEnemyDetection : MonoBehaviour
{
    [SerializeField] private Color alphaOnEnemyDetection;
    [SerializeField] private LayerMask enemyLayer;
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    { 
        Collider2D[] _results = new Collider2D[10];
        ContactFilter2D _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        int _count = capsuleCollider.Overlap(_filter, _results);
        
        sr.color = _count >= 1 ? alphaOnEnemyDetection : Color.white;
    }
}
