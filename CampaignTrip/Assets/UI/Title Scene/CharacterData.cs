using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
	public string flavorText;
	public Sprite icon;
}