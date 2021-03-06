﻿using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Dynamic;
public class AgentReward : Reward {
	//Example, not working
	public override void RewardAtDie(AgentScript Unit) {
        AddReward(-1f);
    }
}

public class AgentScript : Agent
{
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
	
	[HideInInspector]
	public NavMeshAgent agent;
	private GameObject health;
	private GameObject healthbar;
	

	[HideInInspector] public float startLives=0;
	private float defaultStoppingDistance;
	private Animator animator;
	private AudioSource source;
	
	private WalkArea area;
	
	private ParticleSystem dustEffect;
	
	[HideInInspector] public bool dead=false;
	[HideInInspector] public GameSystem sys;
	private Vector3 initPosition;
	[HideInInspector] public UnitInspect inspector;
	private int PlannedObs;
	[HideInInspector] public float lastLives=0;
	[HideInInspector] public float SpecialReward=-2;
	[HideInInspector] public bool onceDone=false;
	[HideInInspector] public bool needAction=false;
	[HideInInspector] public bool isPassedInit=false;
	[HideInInspector] public DebugInfo DebugInner;
	[HideInInspector] public int test=0;
	private float lastTime;
	private float lastAction=0;
	private void Awake() {
		this.agentParameters = new AgentParameters();
		this.agentParameters.onDemandDecision=true;
		this.agentParameters.resetOnDone=false;
		this.agentParameters.maxStep=0;
	}
	public void initRefer(dynamic obj) {
		Debug.Log("Greetings from refer");
		GiveBrain(obj.AgnetBrain);
		lives=obj.lives;
		damage=obj.damage;
		attackTag=obj.attackTag;
		ragdoll=obj.ragdoll;
		Hitbox=obj.Hitbox;
		attackAudio=obj.attackAudio;
		runAudio=obj.runAudio;
		maxStopSeconds=obj.maxStopSeconds;
		VectorMagnitude=obj.VectorMagnitude;
		DeprecatedAttackRange=obj.DeprecatedAttackRange;
		DamagedParticle=obj.DamagedParticle;
		HealedParticle=obj.HealedParticle;
		float mag;
		VectorMagnitude=(obj.Param.TryGetValue("AngularMoveMagnitude", out mag))?mag:VectorMagnitude;
		Debug.Log("Set parameters passed");
		initplus(obj);
		innerInitializeAgent();

		sys = GameObject.FindObjectOfType<GameSystem>();
		PlannedObs = sys.initKnightNumber + sys.initEnemyNumber;
		source = GetComponent<AudioSource>();
		agent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		/*
		health = transform.Find("Health").gameObject;
		healthbar = health.transform.Find("Healthbar").gameObject;
		health.SetActive(false);	
		healthbar.GetComponent<Slider>().maxValue = lives;
		*/
		//get default stopping distance
		defaultStoppingDistance = agent.stoppingDistance;
		//if there's a dust effect, find and assign it
		if(transform.Find("dust"))
			dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();
		
		//find the area so the character can walk around
		area = GameObject.FindObjectOfType<WalkArea>();
		inspector = new UnitInspect(sys);
		lastLives=lives;
		startLives=lives;
		lastTime=-maxStopSeconds;
		inspector.cam=FindObjectOfType<CamController>();
		DebugInner=new DebugInfo(inspector);
		isPassedInit=true;
	}
	public virtual void initplus(dynamic obj) {
	}
	public override void InitializeAgent() {
		if(!sys.showanim) animator.enabled=false;
	}
	public virtual void innerInitializeAgent() {
	}
    public override void AgentReset() {
		print("NewEpisode");
    }
    public override void CollectObservations()
    {
		if(sys!=null) {
			if(!sys.battleStarted) {
				Debug.LogWarning("Observation triggered before battle started. sending zero observation");
				for(int i=0;i<PlannedObs;i++) {
					AddVectorObs(0);
					AddVectorObs(0);
				}
			} else {
				GameObject[] Knight = inspector.getCurrentKnights();
				GameObject[] Enemy = inspector.getCurrentEnemys();
				for(int i=0;i<sys.initKnightNumber;i++) {
					if(i >= Knight.Length || Knight?[i] == null) {
						AddVectorObs(0);
						AddVectorObs(0);
						continue;
					}
					if(inspector.setScriptsFrom(Knight[i])) { //returns true when it's valid
						if(!inspector.isDead()) {
							// When alive
							AddVectorObs(Knight[i].transform.localPosition.x);
							AddVectorObs(Knight[i].transform.localPosition.z);
						} else {
							// When dead
							AddVectorObs(0);
							AddVectorObs(0);
						}
						continue;
					}
					//When script is not valid but it existed as game object
					Debug.LogWarning("Unknown game object tagged as knight and it observated.");
					AddVectorObs(0);
					AddVectorObs(0);
					continue;
				}
				for(int i=0;i<sys.initEnemyNumber;i++) {
					if(i >= Enemy.Length || Enemy?[i] == null) {
						AddVectorObs(0);
						AddVectorObs(0);
						continue;
					}
					if(inspector.setScriptsFrom(Enemy[i])) { //returns true when it's valid
						if(!inspector.isDead()) {
							AddVectorObs(Enemy[i].transform.localPosition.x);
							AddVectorObs(Enemy[i].transform.localPosition.z);
						} else {
							AddVectorObs(0);
							AddVectorObs(0);
						}
						continue;
					}
					Debug.LogError("Unknown game object tagged as enemy and it observated.");
					AddVectorObs(0);
					AddVectorObs(0);
					continue;
				}
			}
		} else {
			Debug.LogError("Observation triggered before even game system instantiate");
 			for(int i=0;i<PlannedObs;i++) { //placeholder
				 AddVectorObs(0);
				 AddVectorObs(0);
			}
		}
		SetTextObs($"{inspector.AvgLives(inspector.getCurrentKnights())}");
    }
    public override void AgentAction(float[] act, string textAction)
    {
		needAction=false;
		//Debug.Log($"{this.gameObject.GetInstanceID()} : {-0.03f+test*0.01f} {((test!=0)?"LOL":"")}");
		if (!dead && !IsDone() && sys.battleStarted) {
			float angle = act[0]*360f*Mathf.Deg2Rad;
			float force = 1f; //Mathf.Clamp(act[1], 0.1f, 1)-0.1f;
			lastAction=act[1];

			if(force==0f) lastTime=sys.Timer;
			Vector3 controlSignal = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
			controlSignal.Normalize();
			controlSignal *= force*VectorMagnitude;

			agent.isStopped = false;	
			agent.destination = transform.position + controlSignal;

			if(SpecialReward>=-1) {
				SetReward(SpecialReward);
				SpecialReward=-2;
			}
			InnerRewardAtStep();
		} else if(dead) {

		} else {
			Debug.LogWarning($"{this.GetInstanceID()}:agent remained after {((dead)?"dead":((IsDone())?"done":"battle ended"))}");
		}
		test=0;
    }
	public virtual void InnerRewardAtStep() {
	}
	public void AgentAlwaysUpdateInternal() {
		//Always attack check, update health bar and minus reward when it recieves damage, also animation
		if(!dead && !this.onceDone) {
			/*
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
			*/
			if(lives != lastLives) {
				if(lives<lastLives) {
					if(!DamagedParticle.isPlaying && sys.showeffects) {
						DamagedParticle.Play();
						//DamagedParticle.Simulate(Time.unscaledDeltaTime, true, false);
					}
				}
				lastLives=lives;
			}
			if(lives>startLives) lives=startLives;
			if(agent.stoppingDistance != defaultStoppingDistance)
				agent.stoppingDistance = defaultStoppingDistance;

			bool attacking = DecideAttack(lastAction);
			if(sys.showanim) {
				if(attacking) {
					animator.SetBool("Attacking", true);
				} else if(animator.GetBool("Attacking")) {
					animator.SetBool("Attacking", false);
				}
			}
		}
	}
	public virtual void AgentAlwaysUpdate() {
		//use it when we need to reward additional situation 
	}
	public virtual bool DecideAttack(float? act) {
		//act will indicates special ability
		if(!sys.FirstWarnUseOfAttackFb) {
			Debug.LogWarning("No attack method implemented, use fallback method");
			sys.FirstWarnUseOfAttackFb=true;
		}
		bool attacking=false;
        if(act!=null) {
            if(act>=0) {
				float minDistance=Mathf.Infinity;
				GameObject minUnit=sys.EmptyUnit;
                foreach(GameObject enemy in inspector.getCurrentEnemys()) {
                    if(enemy==this.gameObject) continue;
                    if(inspector.setScriptsFrom(enemy) && !inspector.isDead()) {
                        float distanceToTarget = Vector3.Distance(this.transform.localPosition, enemy.transform.localPosition);
                        if(distanceToTarget<= DeprecatedAttackRange) {
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
                        attacking=true;
                        test+=1;
                        if(inspector.getLives()<0) {
                            print("Damaged opponent dead");
                            //inspector.printOnPanel($"{this.gameObject.GetInstanceID()}:Reward 0.5");
                            //AddReward(0.5f);
                        }
                    } else {
                        Debug.LogError("Invalid unit targetted.");
                    }   
                }
            } else {

            }
        }
        return attacking;
	}
	public void AgentDescisionRequest() {
		if(dead || gameObject==null || !sys.battleStarted) return;
		if((Vector3.Distance(agent.destination, transform.position) <= agent.stoppingDistance && sys.Timer>=lastTime+maxStopSeconds) || agent.destination==null) {
			needAction=true;
			RequestDecision();
		} else {
			if(!dead && this.enabled) {
				//Moving towards to destination
			} else {
				Debug.LogWarning("Agent Descision Request triggered after its death");
			}
		}
	}
	public virtual void die() {
	}
	public void innerDie() {
		try {
			if(!dead) {
				Instantiate(ragdoll, transform.position, transform.rotation);
			}
		} catch {
			Debug.LogWarning("Error on placing deadbody. You probably unsigned the ragdoll from editor manually.");
		}
		dead=true;
		//Destroy(DamagedParticle.gameObject);
		agent.isStopped=true;
		agent.enabled = false;
		gameObject.transform.position=new Vector3(-999,-999,-999);
		RequestDecision();
		inspector.removeFrom(this.gameObject);
	}
	public override void AgentOnDone() {
		//if agent still remains after the destroying, Destroy log triggers two times more after episode ends because ending an episode triggers each agent's done function.
		Destroy(gameObject);
		Debug.Log("Agent Done");
	}
}
