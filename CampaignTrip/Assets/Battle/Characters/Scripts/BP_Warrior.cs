using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class BP_Warrior : BattlePlayerBase
{
    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //    Warrior = this;
    //}

    public override void Ability3(BattleActorBase target)
    {
        CmdProtect(target.gameObject);
    }
    
    [Command]
    private void CmdProtect(GameObject target)
    {
        RpcProtect(target);
    }

    [ClientRpc]
    private void RpcProtect(GameObject target)
    {
        BattlePlayerBase player = target.GetComponent<BattlePlayerBase>();
        
    }
}
#pragma warning restore CS0618, 0649