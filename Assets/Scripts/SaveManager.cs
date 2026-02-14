using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "pet_data.json");

    public static void Save(PetData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Data saved to: {SavePath}");
    }

    public static PetData Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PetData>(json);
        }
        
        Debug.Log("No save file found, creating new data.");
        return new PetData();
    }
}
