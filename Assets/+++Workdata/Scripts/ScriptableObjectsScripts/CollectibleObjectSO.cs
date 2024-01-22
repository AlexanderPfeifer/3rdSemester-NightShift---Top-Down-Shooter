using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleSO", menuName = "CollectibleSO/Create new ColletibleSO", order = 0)]
public class CollectibleObjectSO : ScriptableObject
{
    //Scriptable Object for every unique collectible
    public Sprite icon;
    public string header;
    public string content;
}
