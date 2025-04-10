using UnityEngine;

public class BalloonBehaviour : MonoBehaviour
{
    [SerializeField] private ParticleSystem balloonPopParticle;

    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) 
            return;
        
        Instantiate(balloonPopParticle, transform.position + new Vector3(0, 2, 0), Quaternion.identity).Play();
    }
}
