using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentHealer : AgentScript
{   
    private UnitInspect inspector=new UnitInspect();
    public override void AgentAlwaysUpdate() {
        
    }
    public override bool DecideAttack(float? act) {
        return false;
    }
}
