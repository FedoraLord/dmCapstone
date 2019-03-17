using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewEnemyList", menuName = "Data Object/EnemyList")]
public class EnemyDataList : ScriptableObject
{
    public List<EnemyPrefab> EnemyPrefabList;

    public GameObject GetPrefab(EnemyPrefab.EnemyType type)
    {
        EnemyPrefab result = EnemyPrefabList.Find(x => x.Type == type);
        if (result == null)
            return null;
        return result.Prefab;
    }
}

[Serializable]
public class EnemyPrefab
{
    public GameObject Prefab;
    public EnemyType Type;

    public enum EnemyType { None, Slime, Wolf, KingSlime }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyDataList))]
public class EnemyDataListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Unfinished entries are automatically removed.");
        EnemyDataList data = target as EnemyDataList;
        List<EnemyPrefab> list = data.EnemyPrefabList;

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.MaxWidth(30));
                GameObject prefab = (GameObject)EditorGUILayout.ObjectField(list[i].Prefab, typeof(GameObject), false);
                EnemyPrefab.EnemyType type = (EnemyPrefab.EnemyType)EditorGUILayout.EnumPopup(list[i].Type);

                if (prefab != list[i].Prefab)
                {
                    //changed prefab
                    if (prefab != null && list.Find(x => x.Prefab == prefab) != null)
                    {
                        Debug.LogWarningFormat(string.Format("Prefab '{0}' is already registered on this list.", prefab.name));
                    }
                    else
                    {
                        list[i].Prefab = prefab;
                    }
                }
                else if (type != list[i].Type)
                {
                    //changed type
                    if (list.Find(x => x.Type == type) != null)
                    {
                        Debug.LogWarningFormat(string.Format("Type '{0}' is already registered on this list.", type));
                    }
                    else
                    {
                        list[i].Type = type;
                    }
                }

                if (GUILayout.Button("Remove"))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add"))
        {
            data.EnemyPrefabList.Add(new EnemyPrefab());
        }
    }

    private void OnDisable()
    {
        List<EnemyPrefab> list = (target as EnemyDataList).EnemyPrefabList;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Prefab == null || list[i].Type == EnemyPrefab.EnemyType.None)
            {
                list.RemoveAt(i);
                i--;
            }
        }
    }
}
#endif