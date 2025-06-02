using UnityEngine;

public class TopCarouselEnemyDetection : MonoBehaviour
{
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer sr;
    [SerializeField] private Color alphaOnEnemyDetection;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    { 
        Collider2D[] _results = new Collider2D[10];
        int _count = capsuleCollider.Overlap(new ContactFilter2D().NoFilter(), _results);
        
        sr.color = _count > 1 ? alphaOnEnemyDetection : Color.white;
    }
}
