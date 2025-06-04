using UnityEngine;

public class BalloonCartBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask bulletLayer;
    private int balloonCount = 3; 
    private Animator anim;
    [SerializeField] private ParticleSystem balloonPopParticle;
    [SerializeField] private Vector2Int getCurrencyOnShot = new(2, 4);

    private void Start()
    {
        anim = GetComponent<Animator>();
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
            InGameUIManager.Instance.currencyUI.GetCurrencyText().gameObject.SetActive(true);
        }
    }

    public void ResetBalloons()
    {
        balloonCount = 3;
        anim.SetInteger("BalloonCount", balloonCount);
    }
}
