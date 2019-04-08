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

    private void Start()
    {
        gameObject.SetActive(Started);
    }

    protected override void Initialize()
    {
        gameObject.SetActive(false);
    }

    public void Begin()
    {
        Started = true;
        gameObject.SetActive(true);
        BattleController.Instance.battleCam.ZoomOut(OnZoomCompleted);
        base.Initialize();
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
            phase = numPhases - 1 - (int)(Health / hpStepSize);
            if (phase < 0 || phase > 2)
                Debug.Log("nononnononon");
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
