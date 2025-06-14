using System.Collections;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject needleFallingParticles;
    private bool treeShrinking;
    private ParticleSystem spawnedNeedlesParticles;

    private IEnumerator TreeShrinkCoroutine()
    {
        treeShrinking = true;

        Vector3 _originalScale = transform.localScale;

        transform.localScale = new Vector3(_originalScale.x, _originalScale.y * 0.75f, _originalScale.z);

        yield return new WaitForSeconds(0.2f);

        transform.localScale = _originalScale;

        treeShrinking = false;
    }

    public void TreeShrink()
    {
        if(treeShrinking)
            return;
        
        if (spawnedNeedlesParticles == null && needleFallingParticles != null)
        {
            spawnedNeedlesParticles = Instantiate(needleFallingParticles, 
                new Vector3(transform.position.x, transform.position.y + .45f, transform.position.z), needleFallingParticles.transform.rotation, transform).GetComponent<ParticleSystem>();
        }
        
        spawnedNeedlesParticles.Play();

        StartCoroutine(TreeShrinkCoroutine());
    }
}
