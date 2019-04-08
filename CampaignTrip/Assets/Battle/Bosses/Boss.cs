using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Boss : EnemyBase
{
    public bool Started { get; protected set; }
    //public bool PhaseChangedThisTurn { get; private set; }
    public int CurrentPhase { get; protected set; } = -1;
    
    public int numPhases;
    
    public void Begin()
    {
        Started = true;
        BattleCamera cam = BattleController.Instance.battleCam;
        BattleController.Instance.aliveEnemies.Add(this);
        cam.ZoomOut(OnZoomCompleted);
        transform.parent = cam.BossSpawnPoint;
        transform.localPosition = Vector3.zero;
    }

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
            phase = numPhases - (int)(Health / hpStepSize);
        }

        CurrentPhase = phase;
        //PhaseChangedThisTurn = (CurrentPhase != phase);
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
