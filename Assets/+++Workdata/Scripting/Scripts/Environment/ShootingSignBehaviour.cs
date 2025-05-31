using System.Collections;
using UnityEngine;

public class ShootingSignBehaviour : MonoBehaviour
{
    [HideInInspector] public bool canGetHit = true;
    [SerializeField] private float shrinkYSizeOnShot = .6f;

    public IEnumerator SnapDownOnHit(bool shotSign)
    {
        canGetHit = false;

        float _scale = transform.localScale.x;

        transform.localScale = new Vector3(_scale, shrinkYSizeOnShot, _scale);

        if (shotSign)
        {
            TutorialManager.Instance.AddAndCheckShotSigns();
        }
        else
        {
            TutorialManager.Instance.AddAndCheckHitSigns();
        }
        
        yield return new WaitForSeconds(1);
        
        transform.localScale = new Vector3(_scale, _scale, _scale);

        canGetHit = true;
    }
}
