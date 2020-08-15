using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentKnight : AgentScript
{
    private double totalDamaged=1;
    private double totalChecked=1;
    private bool prevDamaged=false;
    private float prevDamage=1;
    private bool nowDamaged=false;
    public override bool DecideAttack(float? act) {
		//act will indicates special ability
		bool attacking=false;
        totalChecked+=Time.deltaTime;
        if(act!=null) {
            if(act>=0) {
				//placeholder, since new gameobject makes useless game object and it slows entire game
                float minDistance=Mathf.Infinity;
				GameObject minUnit=sys.EmptyUnit;
                nowDamaged=false;
                foreach(GameObject enemy in inspector.getCurrentEnemys()) {
                    if(enemy==this.gameObject) continue;
                    if(inspector.setScriptsFrom(enemy) && !inspector.isDead()) {
                        float distanceToTarget = Vector3.Distance(this.transform.localPosition, enemy.transform.localPosition);
                        if(distanceToTarget<= agent.stoppingDistance) {
                            minUnit=(distanceToTarget<minDistance)?enemy:minUnit;
                            minDistance=(distanceToTarget<minDistance)?distanceToTarget:minDistance;
                        }
                    }
                }
                if(minUnit.CompareTag("Enemy") || minUnit.CompareTag("Knight")) {
                    Vector3 currentTargetPosition = minUnit.transform.position;
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);
                    if(inspector.setScriptsFrom(minUnit)) {
                        inspector.setLives(inspector.getLives()-(Time.deltaTime * damage));
                        nowDamaged=true;
                        attacking=true;
                        totalDamaged+=(prevDamaged)?Time.deltaTime*damage+prevDamage:Time.deltaTime*damage;
                        if(inspector.getLives()<0) {
                            print("Damaged opponent dead");
                            inspector.printOnPanel($"{this.gameObject.GetInstanceID()}:Reward 0.5");
                            AddReward(0.5f);
                        }
                    } else {
                        Debug.LogError("Invalid unit targetted.");
                    }   
                }
                prevDamage=(nowDamaged)?Time.deltaTime*damage*prevDamage:1;
                prevDamaged=nowDamaged;
            } else {

            }
        }
        return attacking;
	}
    public override void AgentAlwaysUpdate() {
        DebugSetPreReward((float)totalDamaged-1, (float)(DebugInfo.Sigmoid(totalDamaged-1.8f)-0.9d));
    }
    public override void die() {
        print($"Before dead reward:{(float)(DebugInfo.Sigmoid(totalDamaged-1.8f)-0.9d)}");
		SetReward((float)(DebugInfo.Sigmoid(totalDamaged-1.8f)-0.9d));
        inspector.cam.printOnPanel($"{this.gameObject.GetInstanceID()}:Reward {(float)(DebugInfo.Sigmoid(totalDamaged-1.8f)-0.9d)}");
        Done();
    }
}
