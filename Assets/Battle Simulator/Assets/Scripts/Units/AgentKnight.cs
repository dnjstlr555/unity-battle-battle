using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightRewardSys : Reward {
    public override void RewardAtDie(AgentScript unit) {
        AddReward(-1f);
    }
    public void UpdateAtAttackCheck(AgentKnight unit, bool Attacked) {
        if(Attacked) unit.AttackCount+=1;
    }
    public void RewardAtStep(AgentKnight unit) {
        if(unit.AttackCount>0) {
            AddReward(1f);
        } else {
            AddReward(-0.1f);
        }
        unit.AttackCount=0;
    }
}
public class AgentKnight : AgentScript
{
    private bool isPassedCooltime=true;
    private KnightRewardSys rewardSys = new KnightRewardSys();
    private HitboxScript HitboxComponent;
    public float AttackCooltime=1f;
    public int AttackCount=0;
    public override void InnerRewardAtStep() {
        rewardSys.RewardAtStep(this);
        rewardSys.Apply(this);
    }
    public override void innerInitializeAgent() {
        HitboxComponent=Hitbox[0].GetComponent<HitboxScript>();
    }
    public override bool DecideAttack(float? act) {
		bool attacking=false;
        if(act!=null) {
            if(act>0 && isPassedCooltime) {
                foreach(GameObject unit in HitboxComponent.GetCollideObjects()) {
                    if(unit==null && !ReferenceEquals(unit, null)) {
                        HitboxComponent.RemoveObject(unit);
                        continue;
                    }
                    if(unit.CompareTag("Enemy")) {
                        if(inspector.setScriptsFrom(unit) && !inspector.isDead()) {
                            inspector.setLives(inspector.getLives()-(damage));
                            if(inspector.getLives()<0) HitboxComponent.RemoveObject(unit);
                            attacking=true;
                        } else {
                            Debug.LogWarning("Invalid Target Triggered.");
                        }
                    }
                }
                isPassedCooltime=false;
                StartCoroutine("Cooltime");
            } else {

            }
        }
        rewardSys.UpdateAtAttackCheck(this, attacking);
        return attacking;
	}
    IEnumerator Cooltime() {
        yield return new WaitForSeconds(AttackCooltime);
        isPassedCooltime=true;
    }
    public override void die() {
        rewardSys.RewardAtDie(this);
    }
}
