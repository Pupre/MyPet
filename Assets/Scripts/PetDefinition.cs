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
public class PetAction
{
    public string actionName;
    public Texture2D spriteSheet;
    public int frameCount = 4;
}

[System.Serializable]
public class PetStageInfo
{
    public int stageNumber;
    
    [Header("3D Visuals")]
    public GameObject modelPrefab;
    public RuntimeAnimatorController animatorController;
    
    [Header("2D Visuals (Optional)")]
    public bool is2D = true;
    public Texture2D idleSpriteSheet;
    public Texture2D moveSpriteSheet;
    public List<PetAction> specialActions = new List<PetAction>(); // 특수 동작 리스트
    public int frameCount = 4; // 기본 4프레임
    
    [Header("Common")]
    public float baseScale = 1.0f;
}
