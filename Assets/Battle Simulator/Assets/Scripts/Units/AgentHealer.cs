using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentHealer : AgentScript
{   
    public override void AgentAlwaysUpdate() {
        
    }
    public override bool DecideAttack(float? act) {
        return false;
    }
}
