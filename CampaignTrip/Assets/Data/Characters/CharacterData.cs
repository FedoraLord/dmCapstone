using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Data Object/Character")]
public class CharacterData : ScriptableObject
{
	public string flavorText;
	public Sprite sprite;
    public Sprite icon;
	public GameObject characterPrefab;
}