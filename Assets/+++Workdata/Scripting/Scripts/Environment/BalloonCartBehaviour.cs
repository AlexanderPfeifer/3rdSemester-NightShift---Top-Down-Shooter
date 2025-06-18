using UnityEngine;
using Random = UnityEngine.Random;

public class BalloonCartBehaviour : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private LayerMask bulletLayer;
    [SerializeField] private ParticleSystem balloonPopParticle;
    [SerializeField] private Vector2Int getCurrencyOnShot = new(2, 4);

    [Header("Animation")]
    private int balloonCount; 
    private Animator anim;

    private void Start()
    {
        balloonCount = 3;
        anim = GetComponent<Animator>();
        anim.SetInteger("BalloonCount", balloonCount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((bulletLayer & (1 << other.gameObject.layer)) != 0 && balloonCount > 0)
        {
            balloonCount--;
            anim.SetInteger("BalloonCount", balloonCount);
            Instantiate(balloonPopParticle, GetComponent<CircleCollider2D>().bounds.center, Quaternion.identity).Play();
            int _randomCurrency = Random.Range(getCurrencyOnShot.x, getCurrencyOnShot.y);
            PlayerBehaviour.Instance.playerCurrency.AddCurrency(_randomCurrency, false);
            PlayerBehaviour.Instance.playerCurrency.currencyBackground.gameObject.SetActive(true);
        }
    }

    public void ResetBalloons()
    {
        balloonCount = 3;
        anim.SetInteger("BalloonCount", balloonCount);
    }
}
