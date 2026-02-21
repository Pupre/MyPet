using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string GetSavePath(string id) => Path.Combine(Application.persistentDataPath, $"pet_data_{id}.json");

    public static void Save(PetData data)
    {
        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(data.petID);
        File.WriteAllText(path, json);
        Debug.Log($"Data saved to: {path}");
    }

    public static PetData Load(string id)
    {
        string path = GetSavePath(id);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PetData>(json);
        }
        
        Debug.Log($"No save file found for ID {id}, creating new data.");
        PetData newData = new PetData();
        newData.petID = id;
        return newData;
    }
}
