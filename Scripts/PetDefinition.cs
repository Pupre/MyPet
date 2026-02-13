using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPetDefinition", menuName = "MyPet/Pet Definition")]
public class PetDefinition : ScriptableObject
{
    public string speciesName;
    public List<PetStageInfo> stages = new List<PetStageInfo>();

    public PetStageInfo GetStageInfo(int stage)
    {
        return stages.Find(s => s.stageNumber == stage);
    }
}

[System.Serializable]
public class PetStageInfo
{
    public int stageNumber;
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;
    public float baseScale = 1.0f;
}
