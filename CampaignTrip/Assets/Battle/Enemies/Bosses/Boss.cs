using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public abstract class Boss : EnemyBase
{
    public int CurrentPhase { get; protected set; } = -1;
    
    public int numPhases;
    
    public void BeginBossFight()
    {
        StartCoroutine(ZoomOutCamera());
    }

    private IEnumerator ZoomOutCamera()
    {
        yield return new WaitForSeconds(0.1f);
        Action callback = (isServer ? () => StartCoroutine(ExecuteTurn()) : default(Action));
        BattleController.Instance.battleCam.ZoomOut(callback);
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
