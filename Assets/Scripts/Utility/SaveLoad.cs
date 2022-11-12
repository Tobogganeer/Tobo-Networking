using System;
using UnityEngine;
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;

public static class SaveLoad
{
    /// <summary>
    /// Saves the <paramref name="objToSave"/> in the specified <paramref name="saveType"/> named <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="objToSave"></param>
    public static void Save(string fileName, object objToSave, bool persistentDirectory, SaveType saveType = SaveType.Json)
    {
        if (objToSave == null) return;

        switch (saveType)
        {
            case SaveType.Json:
                SaveJson(fileName, objToSave, persistentDirectory);
                break;
            case SaveType.Binary:
                SaveBinary(fileName, objToSave, persistentDirectory);
                break;
        }
    }

    /// /// <summary>
    /// Saves the <paramref name="objToSave"/> as a human-readable json file named <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="objToSave"></param>
    public static void SaveJson(string fileName, object objToSave, bool persistentDirectory)
    {
        if (objToSave == null) return;
        string jsonObj = JsonUtility.ToJson(objToSave, true);

        if (!persistentDirectory)
            File.WriteAllText(Path.Combine(Application.dataPath, fileName), jsonObj);
        else
            File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), jsonObj);
    }

    /// /// <summary>
    /// Saves the <paramref name="objToSave"/> as a non human-readable binary file named <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="objToSave"></param>
    public static void SaveBinary(string fileName, object objToSave, bool persistentDirectory)
    {
        if (objToSave == null) return;
        string jsonObj = JsonUtility.ToJson(objToSave, false);
        string base64Obj = Base64Encode(jsonObj);
        //File.WriteAllText(Path.Combine(Application.dataPath, fileName), base64Obj);

        if (!persistentDirectory)
            File.WriteAllText(Path.Combine(Application.dataPath, fileName), base64Obj);
        else
            File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), base64Obj);

        //if (objToSave == null) return;
        //
        //FileStream stream = File.OpenWrite(Path.Combine(Application.dataPath, fileName));
        //BinaryWriter writer = new BinaryWriter(stream);
        //
        //string jsonObj = JsonUtility.ToJson(objToSave, false);
        //// BinaryFormatter is not safe anymore, just going to use binary json lol
        //
        //try
        //{
        //    writer.Write(jsonObj);
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"Error caught while trying to save {objToSave.GetType().Name}! " + ex);
        //}
        //finally
        //{
        //    stream.Close();
        //    writer.Dispose();
        //}
    }


    /// <summary>
    /// Reads the <typeparamref name="T"/> object stored in the <paramref name="saveType"/> file named <paramref name="fileName"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T Read<T>(string fileName, bool persistentDirectory, SaveType saveType = SaveType.Json)
    {
        if (!File.Exists(Path.Combine(Application.dataPath, fileName))) return default;

        switch (saveType)
        {
            case SaveType.Json:
                return ReadJson<T>(fileName, persistentDirectory);
            case SaveType.Binary:
                return ReadBinary<T>(fileName, persistentDirectory);
        }

        return default;
    }

    /// <summary>
    /// Reads the <typeparamref name="T"/> object stored in the json file named <paramref name="fileName"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T ReadJson<T>(string fileName, bool persistentDirectory)
    {
        if (persistentDirectory && !File.Exists(Path.Combine(Application.persistentDataPath, fileName))) return default;
        if (!persistentDirectory && !File.Exists(Path.Combine(Application.dataPath, fileName))) return default;

        try
        {
            string jsonObj;

            if (persistentDirectory)
                jsonObj = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            else
                jsonObj = File.ReadAllText(Path.Combine(Application.dataPath, fileName));
            return JsonUtility.FromJson<T>(jsonObj);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error trying to read {typeof(T).Name}: " + ex);
        }

        return default;
    }

    /// <summary>
    /// Reads the <typeparamref name="T"/> object stored in the binary file named <paramref name="fileName"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T ReadBinary<T>(string fileName, bool persistentDirectory)
    {
        if (persistentDirectory && !File.Exists(Path.Combine(Application.persistentDataPath, fileName))) return default;
        if (!persistentDirectory && !File.Exists(Path.Combine(Application.dataPath, fileName))) return default;

        try
        {
            string base64Obj;

            if (persistentDirectory)
                base64Obj = File.ReadAllText(Path.Combine(Application.persistentDataPath, fileName));
            else
                base64Obj = File.ReadAllText(Path.Combine(Application.dataPath, fileName));
            //string base64Obj = File.ReadAllText(Path.Combine(Application.dataPath, fileName));
            return JsonUtility.FromJson<T>(Base64Decode(base64Obj));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error trying to read {typeof(T).Name}: " + ex);
        }

        return default;

        //FileStream stream = File.OpenRead(Path.Combine(Application.dataPath, fileName));
        //BinaryReader reader = new BinaryReader(stream);
        //
        //try
        //{
        //    string jsonObj = reader.ReadString();
        //    return JsonUtility.FromJson<T>(jsonObj);
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"Error caught while trying to read file! " + ex);
        //}
        //finally
        //{
        //    stream.Close();
        //    reader.Dispose();
        //}
        //
        //return default;
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}

public enum SaveType
{
    Json,
    Binary
}
