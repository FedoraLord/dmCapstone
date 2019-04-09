using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649
public class BattleCamera : MonoBehaviour
{
    public Camera Cam { get { return cam; } }
    public List<Transform> EnemySpawnPoints { get { return enemySpawnPoints; } }
    public List<Transform> PlayerSpawnPoints { get { return playerSpawnPoints; } }
    public Transform BossSpawnPoint { get { return bossSpawnPoint; } }

    [SerializeField] private Camera cam;
    [SerializeField] private List<Transform> enemySpawnPoints;
    [SerializeField] private List<Transform> playerSpawnPoints;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform wrapper;
    [SerializeField] private float zoomInSize;
    [SerializeField] private float zoomOutSize;
    [SerializeField] private Transform zoomInPos;
    [SerializeField] private Transform zoomOutPos;
    
    public void ZoomOut(Action callback = null)
    {
        StartCoroutine(Zoom(zoomOutSize, zoomOutPos, callback));
    }

    public void ZoomIn(Action callback = null)
    {
        StartCoroutine(Zoom(zoomInSize, zoomInPos, callback));
    }

    private IEnumerator Zoom(float targetSize, Transform targetPosition, Action callback = null)
    {
        float animTime = 0;
        float totalAnimTime = 1f;
        float startSize = cam.orthographicSize;
        float endSize = targetSize;
        Vector3 startPos = wrapper.position;
        Vector3 endPos = targetPosition.position;
        
        while (animTime < 1)
        {
            wrapper.transform.position = Vector3.Lerp(startPos, endPos, animTime);
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, animTime);

            foreach (BattlePlayerBase p in BattlePlayerBase.players)
                p.HealthBar.UpdatePosition();

            foreach (EnemyBase enemy in BattleController.Instance.aliveEnemies)
                enemy.HealthBar.UpdatePosition();

            yield return new WaitForEndOfFrame();
            animTime += Time.deltaTime / totalAnimTime;
        }

        callback?.Invoke();
    }
}
