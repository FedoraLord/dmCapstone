﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBug : EnemyBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnMinigameFailed()
    {
        base.OnMinigameFailed();

        this.basicDamage += 20;
    }
}
