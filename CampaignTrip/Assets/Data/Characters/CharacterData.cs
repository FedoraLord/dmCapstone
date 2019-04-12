using UnityEngine;
using static BattlePlayerBase;

#pragma warning disable 0649
[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Data Object/Character")]
public class CharacterData : ScriptableObject
{
    public CharacterType Type
    {
        get { return type; }
    }

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

    [SerializeField] private CharacterType type;
    [SerializeField] private string flavorText;
	[SerializeField] private Sprite sprite;
    [SerializeField] private Sprite icon;
}