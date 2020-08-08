using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class MyAcademy : Academy
{
    private GameSystem sys;
    private EnemyArmy enemysys;
    private UnitInspect inspector;
    public override void InitializeAcademy() {
        sys=FindObjectOfType<GameSystem>();
        enemysys=FindObjectOfType<EnemyArmy>();
        inspector=new UnitInspect();
    }
    public override void AcademyReset() {
        if(this.firstAcademyReset) {
            
        }

    }

    public override void AcademyStep() {
        
    }
}
