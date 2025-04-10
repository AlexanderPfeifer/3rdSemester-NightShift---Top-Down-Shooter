using System.Collections;
using UnityEngine;

public class ShootingSignBehaviour : MonoBehaviour
{
    [HideInInspector] public bool canGetHit = true;
    [SerializeField] private float shrinkYSizeOnShot = .6f;

    public IEnumerator SnapDownOnHit()
    {
        canGetHit = false;

        float _scale = transform.localScale.x;

        transform.localScale = new Vector3(_scale, shrinkYSizeOnShot, _scale);
        
        TutorialManager.Instance.AddAndCheckShotSigns();
        
        yield return new WaitForSeconds(1);
        
        transform.localScale = new Vector3(_scale, _scale, _scale);

        canGetHit = true;
    }
}
