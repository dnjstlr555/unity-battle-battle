using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Dynamic;

public class AgentPreset : MonoBehaviour
{
	public Brain AgnetBrain;
	public string AgentType="AgentScript";
    public float lives;
	public float damage;
	public string attackTag;
	public GameObject ragdoll;
	public Collider[] Hitbox;
	public AudioClip attackAudio;
	public AudioClip runAudio;
	public float maxStopSeconds;
	public float VectorMagnitude = 10;
	//public float AttackCooltime=3f;
	public float DeprecatedAttackRange=2f;
	public ParticleSystem DamagedParticle;
	public ParticleSystem HealedParticle;
	public ParticleSystem HealingParticle;
	public float AttackCooltime=1f; // knight
	public float HealingCooltime=1f; //healer
	public dynamic refer(int space, MLAgents.ResetParameters param) {
		dynamic obj = new ExpandoObject();
		if(AgentType=="Unit") {
			obj.Param=param;
			return obj;
		}
		AgnetBrain.brainParameters.vectorObservationSize=space;
		obj.AgnetBrain=AgnetBrain;
		obj.AgentType=AgentType;
		obj.lives=lives;
		obj.damage=damage;
		obj.attackTag=attackTag;
		obj.ragdoll=ragdoll;
		obj.Hitbox=Hitbox;
		obj.attackAudio=attackAudio;
		obj.runAudio=runAudio;
		obj.maxStopSeconds=maxStopSeconds;
		obj.VectorMagnitude=VectorMagnitude;
		obj.DeprecatedAttackRange=DeprecatedAttackRange;
		obj.DamagedParticle=DamagedParticle;
		obj.HealedParticle=HealedParticle;
		obj.HealingParticle=HealingParticle;
		obj.AttackCooltime=AttackCooltime;
		obj.HealingCooltime=HealingCooltime;
		obj.Param=param;
		return obj;
	}
}
