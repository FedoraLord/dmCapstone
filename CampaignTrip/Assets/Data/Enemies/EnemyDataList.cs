using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyBase;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewEnemyList", menuName = "Data Object/EnemyList")]
public class EnemyDataList : ScriptableObject
{
    public string path = "Assets/Battle/Enemies";
    public List<GameObject> list;

    public GameObject GetPrefab(EnemyType type)
    {
        foreach (GameObject item in list)
        {
            EnemyBase enemy = item.GetComponent<EnemyBase>();
            if (enemy.enemyType == type)
                return item;
        }
        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyDataList))]
public class EnemyDataListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EnemyDataList data = target as EnemyDataList;
        if (GUILayout.Button("Update"))
        {
            data.list = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { data.path });

            foreach (string id in guids)
            {
                GameObject prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(id)) as GameObject;
                if (prefab.GetComponent<EnemyBase>())
                    data.list.Add(prefab);
            }
        }

        base.OnInspectorGUI();
    }

    private void OnDisable()
    {
        List<GameObject> list = (target as EnemyDataList).list;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
                i--;
            }
        }
    }
}
#endif