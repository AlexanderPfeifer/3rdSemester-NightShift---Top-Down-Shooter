using System.Collections;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
    private bool treeShrinking;

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
        
        StartCoroutine(TreeShrinkCoroutine());
    }
}
