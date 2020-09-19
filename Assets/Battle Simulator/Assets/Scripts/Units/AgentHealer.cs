using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerReward : Reward {
    public override void RewardAtDie(AgentScript unit) {
        AddReward(-1f);
    }
    public void UpdateAtAttackCheck(AgentHealer unit, bool Attacked) {
        if(Attacked) unit.AttackCount+=1;
    }
    public void RewardAtStep(AgentHealer unit) {
        if(unit.AttackCount>0) {
            AddReward(1f);
        } else {
            AddReward(-0.1f);
        }
        unit.AttackCount=0;
    }
}
public class AgentHealer : AgentScript
{   
    public ParticleSystem HealingParticle;
    public float HealingCooltime=1f;
    [HideInInspector] public int AttackCount=0;
    private HealerReward rewardSys = new HealerReward();
    private HitboxScript HitboxComponent;
    private bool isPassedCooltime=true;
    public override void initplus(dynamic obj)
    {
        HealingParticle=obj.HealingParticle;
        HealingCooltime=obj.HealingCooltime;

        MLAgents.ResetParameters param;
        param=obj.Param;
        float plive, pcool, pdamage;
		lives = (param.TryGetValue("HealerHP", out plive))?plive:lives;
		damage = (param.TryGetValue("HealerHealingAmount", out pdamage))?pdamage:damage;
		HealingCooltime = (param.TryGetValue("HealerHealingCooltime", out pcool))?pcool:HealingCooltime;
    }
    public override void innerInitializeAgent()
    {
        HitboxComponent=Hitbox[0].GetComponent<HitboxScript>();
        HealingParticle.Stop();
    }
    public override void InnerRewardAtStep() {
        rewardSys.RewardAtStep(this);
        rewardSys.Apply(this);
    }
    public override void AgentAlwaysUpdate() {
        if(AttackCount<=0 && sys.showeffects) HealingParticle.Stop();
    }
    public override bool DecideAttack(float? act) {
        bool attacking=false;
        if(act!=null) {
            if(act>=0) {
                foreach(GameObject unit in HitboxComponent.GetCollideObjects()) {
                    if(unit==null && !ReferenceEquals(unit, null)) {
                        HitboxComponent.RemoveObject(unit);
                        continue;
                    }
                    if(unit.CompareTag("Knight")) {
                        if(inspector.setScriptsFrom(unit) && !inspector.isDead()) {
                            inspector.setLives(inspector.getLives()+(damage));
                            if(inspector.getLives()<0) HitboxComponent.RemoveObject(unit);
                            attacking=true;
                            if(sys.showeffects) HealingParticle.Play();
                        } else {
                            Debug.LogWarning("Invalid Target Triggered.");
                        }
                    }
                }
                isPassedCooltime=false;
                StartCoroutine("Cooltime");
            }
        }
        rewardSys.UpdateAtAttackCheck(this, attacking);
        return attacking;
    }
    IEnumerator Cooltime() {
        yield return new WaitForSeconds(HealingCooltime);
        isPassedCooltime=true;
    }
    public override void die() {
        rewardSys.RewardAtDie(this);
    }
}
