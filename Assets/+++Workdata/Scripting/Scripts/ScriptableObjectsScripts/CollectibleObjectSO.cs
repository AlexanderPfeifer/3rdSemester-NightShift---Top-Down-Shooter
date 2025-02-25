using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleSO", menuName = "CollectibleSO/Create new CollectibleSO", order = 0)]
public class CollectibleObjectSO : ScriptableObject
{
    public Sprite icon;
    public string header;
    public string content;
}
