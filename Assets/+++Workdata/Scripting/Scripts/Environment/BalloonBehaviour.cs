using UnityEngine;

public class BalloonBehaviour : MonoBehaviour
{
    [SerializeField] private ParticleSystem balloonPopParticle;
    [SerializeField] private Vector2Int getCurrencyOnShot = new(2, 4);

    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) 
            return;
        
        Instantiate(balloonPopParticle, transform.position + new Vector3(0, 2, 0), Quaternion.identity).Play();
        int _randomCurrency = Random.Range(getCurrencyOnShot.x, getCurrencyOnShot.y);
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(_randomCurrency, false);
        PlayerBehaviour.Instance.playerCurrency.currencyBackground.gameObject.SetActive(true);
    }
}
