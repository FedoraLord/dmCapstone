using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public abstract class Boss : EnemyBase
{
    public bool Started { get; protected set; }
    //public bool PhaseChangedThisTurn { get; private set; }
    public int CurrentPhase { get; protected set; } = -1;
    
    public int numPhases;
    public SpriteRenderer sr;
    
    protected override void Initialize()
    {
        sr.enabled = false;
        BattleController.Instance.SetSpawnPosition(this);
    }
    
    [ClientRpc]
    public void RpcBegin()
    {
        Started = true;
        sr.enabled = true;
        ZoomToBoss();
        base.Initialize();
    }
    
    private void ZoomToBoss()
    {
        Action callback = null;
        if (isServer)
            callback = OnZoomCompleted;
        BattleController.Instance.battleCam.ZoomOut(OnZoomCompleted);
    }

    [Server]
    private void OnZoomCompleted()
    {
        StartCoroutine(ExecuteTurn());
    }

    public virtual IEnumerator ExecuteTurn()
    {
        int phase = 0;
        if (Health < BattleStats.MaxHealth)
        {
            float hpStepSize = (float)BattleStats.MaxHealth / numPhases;
            phase = numPhases - 1 - (int)(Health / hpStepSize);
        }

        CurrentPhase = phase;
        if (CurrentPhase != phase)
            yield return OnPhaseChanged();
    }

    protected virtual IEnumerator OnPhaseChanged()
    {
        yield return null;
    }
}

[Serializable]
public class BossPhase
{
    public int numEnemies;
    public SpawnGroup spawnGroups;
}
