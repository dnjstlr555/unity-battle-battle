using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightRewardSys : Reward {
    public override void RewardAtDie(AgentScript Unit) {
        AddReward(-1f);
    }
    public override void RewardAtStep(AgentScript Unit) {

    }
}
public class AgentKnight : AgentScript
{
    private double totalDamaged=1;
    private double totalChecked=1;
    private KnightRewardSys rewardSys = new KnightRewardSys();
    private HitboxScript HitboxComponent;
    public override void InnerRewardAtStep() {
        rewardSys.Apply(this);
    }
    public override void innerInitializeAgent() {
        HitboxComponent=Hitbox[0].GetComponent<HitboxScript>();
    }
    public override bool DecideAttack(float? act) {
		bool attacking=false;
        totalChecked+=Time.deltaTime;
        if(act!=null) {
            if(act>=0) {
                foreach(GameObject unit in HitboxComponent.GetCollideObjects()) {
                    if(unit==null && !ReferenceEquals(unit, null)) {
                        HitboxComponent.RemoveObject(unit);
                        continue;
                    }
                    if(unit.CompareTag("Enemy")) {
                        if(inspector.setScriptsFrom(unit) && !inspector.isDead()) {
                            inspector.setLives(inspector.getLives()-(Time.deltaTime * damage));
                            if(inspector.getLives()<0) HitboxComponent.RemoveObject(unit);
                            attacking=true;
                        } else {
                            Debug.LogWarning("Invalid Target Triggered.");
                        }
                    }
                }
            } else {

            }
        }
        return attacking;
	}
    public override void AgentAlwaysUpdate() {
    }
    public override void die() {
        rewardSys.RewardAtDie(this);
    }
}
