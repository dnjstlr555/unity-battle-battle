using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.AI;

//public UnitInspect class located at GameSystem.cs

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
	public float AttackRange=2f;
	
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
	
	[HideInInspector] public bool dead;
	[HideInInspector] public GameSystem sys;
	private Vector3 initPosition;
	[HideInInspector] public UnitInspect inspector;
	private int PlannedObs;
	[HideInInspector] public float lastLives=0;
	[HideInInspector] public float SpecialReward=-2;
	[HideInInspector] public bool onceDone=false;
	[HideInInspector] public bool needAction=false;
	[HideInInspector] public DebugInfo DebugInner;
	[HideInInspector] public int test=0;
	private float lastTime;
	private bool LastAction=false;
	public override void InitializeAgent() {
		innerInitializeAgent();
		sys = GameObject.FindObjectOfType<GameSystem>();
		PlannedObs = sys.initKnightNumber + sys.initEnemyNumber;
		source = GetComponent<AudioSource>();
		agent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		health = transform.Find("Health").gameObject;
		healthbar = health.transform.Find("Healthbar").gameObject;
		health.SetActive(false);	
		healthbar.GetComponent<Slider>().maxValue = lives;
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
					Debug.LogError("Unknown game object tagged as knight and it observated.");
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
		} else if (LastAction) {
		}
		else {
			Debug.LogWarning($"{this.GetInstanceID()}:agent remained after {((dead)?"dead":((IsDone())?"done":"battle ended"))}");
		}
		test=0;
    }
	public virtual void InnerRewardAtStep() {
	}
	public void AgentAlwaysUpdateInternal() {
		//Always attack check, update health bar and minus reward when it recieves damage, also animation
		if(!dead && !this.onceDone) {
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
			if(lives != lastLives) {
				lastLives=lives;
			}
			if(agent.stoppingDistance != defaultStoppingDistance)
				agent.stoppingDistance = defaultStoppingDistance;

			bool attacking = DecideAttack(1);
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
                        if(distanceToTarget<= AttackRange) {
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
		inspector.printOnPanel($"{this.gameObject.GetInstanceID()}:Dead");
		try {
			if(!dead) {
				Instantiate(ragdoll, transform.position, transform.rotation);
			}
		} catch {
			Debug.LogWarning("Error on placing deadbody. You probably unsigned the ragdoll from editor manually.");
		}
		LastAction=true;
		dead=true;
		this.agent.isStopped=true;
		agent.enabled = false;
		this.gameObject.transform.position=new Vector3(-999,-999,-999);
		RequestDecision();
		inspector.removeFrom(this.gameObject);
	}
	public override void AgentOnDone() {
		//if agent still remains after the destroying, Destroy log triggers two times more after episode ends because ending an episode triggers each agent's done function.
		Destroy(gameObject);
		Debug.Log("Agent Done");
	}
}
