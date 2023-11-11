using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using File = UnityEngine.Windows.File;

public static class SaveSystem
{
    public static void SavePlayer(Player player)
    {
        var formatter = new BinaryFormatter();

        SetFile();
        var stream = new FileStream(SetFile(), FileMode.Create);

        var save = new PlayerSave(player);
        
        formatter.Serialize(stream, save);
        stream.Close();
    }

    public static PlayerSave LoadPlayer()
    {
        SetFile();
        
        if (File.Exists(SetFile()))
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(SetFile(), FileMode.Open);
            
            var save = formatter.Deserialize(stream) as PlayerSave;
            stream.Close();
            
            return save;
        }
        else
        {
            Debug.LogError("Save file not found in " + SetFile());
            return null;
        }
    }

    private static string SetFile()
    { 
        var path = Application.persistentDataPath + "/player.clown";
        return path;
    }
}
