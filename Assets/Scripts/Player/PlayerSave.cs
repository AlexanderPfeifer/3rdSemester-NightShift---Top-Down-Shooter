
[System.Serializable]
public class PlayerSave
{
    public float[] position;
    
    public PlayerSave (Player player)
    {
        position = new float[3];
        var playerPosition = player.transform.position;
        position[0] = playerPosition.x;
        position[1] = playerPosition.y;
        position[2] = playerPosition.z;
    }
}
