using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingSlime : Boss
{
    public List<BossPhase> phaseGroups = new List<BossPhase>();

    private int turnsUntilSpawn;
    
    public override IEnumerator ExecuteTurn()
    {
        yield return base.ExecuteTurn();

        turnsUntilSpawn--;
        if (turnsUntilSpawn <= 0)
        {
            BossPhase phaseData = phaseGroups[CurrentPhase];
            BattleController.Instance.SpawnEnemies(phaseData.spawnGroups, phaseData.numEnemies);
            turnsUntilSpawn = numPhases - CurrentPhase + 1;
            yield return new WaitForSeconds(1);
        }

        BattleController.Instance.StartPlayerPhase();
    }
}
