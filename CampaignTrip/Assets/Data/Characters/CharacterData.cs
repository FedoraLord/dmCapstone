using UnityEngine;

#pragma warning disable 0649 
[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Data Object/Character")]
public class CharacterData : ScriptableObject
{
    public enum Character { Warrior, Rogue, Alchemist, Mage };

    public Character CharacterName
    {
        get { return characterName; }
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

    public GameObject CharacterPrefab
    {
        get
        {
            if (characterPrefab == null)
                GetTheFuckingPrefab();
            return characterPrefab;
        }
    }

    [SerializeField] private Character characterName;
    [SerializeField] private string flavorText;
	[SerializeField] private Sprite sprite;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject characterPrefab;

    private void GetTheFuckingPrefab()
    {
#if UNITY_EDITOR
        characterPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("Assets/Battle/Characters/BP_{0}.prefab", characterName.ToString()));
#endif
    }
}
#pragma warning restore 0649