using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveFileManager
{
    //this gives us the folder path we want to save the files to
    private static string SaveFolderPath => Path.Combine(Application.persistentDataPath, "NightShiftSaveState");
    
    //this takes the folder path and adds the file name to it.
    private static string GetFilePath(string fileName) => Path.Combine(SaveFolderPath, $"{fileName}.json");

    /// <summary>
    /// We call this to save the actual data to a json file.
    /// </summary>
    /// <param name="fileName">The name under which we want to save the file</param>
    /// <param name="data">The actual object that we want to save</param>
    /// <typeparam name="T">"The type of serialized object we want to be saved"</typeparam>
    /// <returns>If saving works, returns true. If anything fails, this returns false</returns>
    public static void TrySaveData<T>(string fileName, T data)
    {
        var _path = GetFilePath(fileName);
        
        try 
        {
            if(!Directory.Exists(SaveFolderPath))
            {	
                Directory.CreateDirectory(SaveFolderPath);
            }
            
            if (File.Exists(_path))
            {
                //if the file already exists, we delete it, so that we can create it anew
                File.Delete(_path);
            }
            
            //we create the new file
            using FileStream _stream = File.Create(_path);
            _stream.Close();

            //then we fill the file with the created json text
            string _jsonConvertedData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_path, _jsonConvertedData);
        }
        catch (Exception _e)
        {
            Debug.LogError($"Data cannot be saved due to: {_e.Message} {_e.StackTrace}");
        }
    }
    
    /// <summary>
    /// We call this to load a json file and turn it back into an object we can work with.
    /// </summary>
    /// <param name="fileName">the filename we attempt to load</param>
    /// <param name="data">the data we will give out once the file is loaded and converted</param>
    /// <typeparam name="T">The type of object we want to turn the json file into</typeparam>
    /// <returns>If the file can be loaded, we return true, otherwise we return false</returns>
    public static bool TryLoadData<T>(string fileName, out T data)
    {
        var _path = GetFilePath(fileName);
        /*
            We have to create a default data, so that in case the loading goes wrong, 
            we can give out anything. Without this, c# would not compile.
         */
        data = default;

        if (!File.Exists(_path))
        {
            Debug.LogWarning($"File cannot be loaded at \"{_path}\".");
            return false;
        }

        try
        {
            //we then read the file and convert it into the object type we try to load
            data = JsonConvert.DeserializeObject<T>(File.ReadAllText(_path));
            return true;
        }
        catch (Exception _e)
        {
            Debug.LogError($"Data cannot be loaded due to: {_e.Message} {_e.StackTrace}");
            return false;
        }
    }
    
    /// <returns>An array of strings representing all save file names.</returns>
    public static string[] GetAllSaveFileNames()
    {
        var _info = new DirectoryInfo(SaveFolderPath);
        var _fileInfo = _info.GetFiles();

        return (from _file in _fileInfo where _file.Name.EndsWith(".json") 
            select _file.Name.Replace(".json", "")).ToArray();
    }

    public static void DeleteSaveState(string saveState)
    {
        var _path = GetFilePath(saveState);
        
        try 
        {
            if(!Directory.Exists(SaveFolderPath))
            {	
                return;
            }
            
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch (Exception _e)
        {
            Debug.LogError($"Data cannot be saved due to: {_e.Message} {_e.StackTrace}");
        }
    }
}
