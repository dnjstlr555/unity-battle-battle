using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentHealer : AgentScript
{   
    private int checkedNumber=1;
    private float totalHealed=0;
    public override void AgentAlwaysUpdate() {
        if(SpecialReward==-2f) {
            checkedNumber=1;
            totalHealed=0;
        }
        SpecialReward=totalHealed/checkedNumber;
    }
    public override bool DecideAttack(float? act) {
        bool attacking=false;
        if(act!=null) {
            if(act>=0) {
                float minDistance=Mathf.Infinity;
				GameObject minUnit=sys.EmptyUnit;
                foreach(GameObject knight in inspector.getCurrentKnights()) {
                    if(knight==this.gameObject) continue;
                    if(inspector.setScriptsFrom(knight) && !inspector.isDead()) {
                        float distanceToTarget = Vector3.Distance(this.transform.localPosition, knight.transform.localPosition);
                        if(distanceToTarget<= Deprecated_AttackRange) {
                            minUnit=(distanceToTarget<minDistance)?knight:minUnit;
                            minDistance=(distanceToTarget<minDistance)?distanceToTarget:minDistance;
                        }
                    }
                }
                if(minUnit.CompareTag("Enemy") || minUnit.CompareTag("Knight")) {
                    Vector3 currentTargetPosition = minUnit.transform.position;
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);
                    if(inspector.setScriptsFrom(minUnit)) {
                        inspector.setLives(inspector.getLives()+(Time.deltaTime * damage));
                        attacking=true;
                    } else {
                        Debug.LogError("Invalid unit targetted.");
                    }   
                }
            }
        }
        return attacking;
    }
}
