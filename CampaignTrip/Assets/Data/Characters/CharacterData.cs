using UnityEngine;

#pragma warning disable 0649
[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Data Object/Character")]
public class CharacterData : ScriptableObject
{
    public string FlavorText
    {
        get { return flavorText; }
    }

    public Sprite Sprite
    {
        get { return sprite; }
    }

    public Sprite Icon
    {
        get { return icon; }
    }
    
    public GameObject BattlePrefab
    {
        get { return battlePrefab; }
    }

    [SerializeField] private string flavorText;
	[SerializeField] private Sprite sprite;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject battlePrefab;
}