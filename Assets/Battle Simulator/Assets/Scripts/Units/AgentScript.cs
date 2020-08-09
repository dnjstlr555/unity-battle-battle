using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Linq;

//public UnitInspect class located at GameSystem.cs

public class AgentScript : Agent
{
    public float lives;
	public float damage;
	public string attackTag;
	public GameObject ragdoll;
	public AudioClip attackAudio;
	public AudioClip runAudio;
	
	[HideInInspector]
	public NavMeshAgent agent;
	private GameObject health;
	private GameObject healthbar;
	
	[HideInInspector]
	private float startLives;
	private float defaultStoppingDistance;
	public Rigidbody Rigid;
	private Animator animator;
	private AudioSource source;
	
	private WalkArea area;
	
	private ParticleSystem dustEffect;
	
	public bool dead;

    public Transform Target;
	private float REWARD;
	private GameSystem sys;
	private Vector3 initPosition;
	private UnitInspect inspector;
	private int PlannedObs;
	private float lastLives=0;
	private float lastAct=-1;
	private UnitInspect potentialEnemy;
	public override void InitializeAgent() {
		sys = GameObject.FindObjectOfType<GameSystem>();
		PlannedObs = sys.initKnightNumber + sys.initEnemyNumber;
		source = GetComponent<AudioSource>();
		agent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		health = transform.Find("Health").gameObject;
		healthbar = health.transform.Find("Healthbar").gameObject;
		health.SetActive(false);	
		healthbar.GetComponent<Slider>().maxValue = lives;
		startLives = lives;
		//get default stopping distance
		defaultStoppingDistance = agent.stoppingDistance;
		//if there's a dust effect, find and assign it
		if(transform.Find("dust"))
			dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();
		
		//find the area so the character can walk around
		area = GameObject.FindObjectOfType<WalkArea>();
		inspector = new UnitInspect();
		lastLives=lives;
		potentialEnemy = new UnitInspect();
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
				GameObject[] Knight = GameObject.FindGameObjectsWithTag("Knight");
				GameObject[] Enemy = GameObject.FindGameObjectsWithTag("Enemy");
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
    }
    public float vector_offset = 10;
    public override void AgentAction(float[] act, string textAction)
    {
		if (!dead && this.enabled) {
			float angle = act[0]*360f*Mathf.Deg2Rad;
			float force = Mathf.Clamp(act[1], -1, 1);
			Vector3 controlSignal = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
			controlSignal.Normalize();
			controlSignal *= force*vector_offset;

			agent.isStopped = false;	
			agent.destination = transform.position + controlSignal;
			DecideAttack(act[2]);
			lastAct=act[2];
			AddReward(-0.3f);
		} else {
			Debug.LogWarning("Agent remained after dead.");
			agent.isStopped = true;
			SetReward(-0.03f);
		}
			
    }
	public void AgentAlwaysUpdateInternal() {
		//Always attack check, update health bar and minus reward when it recieves damage, also animation
		if(!dead && this.enabled) {
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
			if(lives != lastLives) {
				if(lives<lastLives) {
					//damaged
					AddReward(-0.3f);
				} else {
					//healed
				}
				lastLives=lives;
			}
			if(dustEffect && animator.GetBool("Attacking") == false && !dustEffect.isPlaying)
				dustEffect.Play();

			if(dustEffect && dustEffect.isPlaying && animator.GetBool("Attacking") == true)
				dustEffect.Stop();
			if(agent.stoppingDistance != defaultStoppingDistance)
				agent.stoppingDistance = defaultStoppingDistance;

			bool attacking = DecideAttack(lastAct);
			Monitor.Log("IsAttacking", (attacking)?1:0,this.transform);
			if(animator.GetBool("Attacking") && !attacking){
				animator.SetBool("Attacking", false);
				
				//play the running audio
				if(source.clip != runAudio){
					source.clip = runAudio;
					source.Play();
				}
			}
			if(!animator.GetBool("Attacking") && attacking) {
				animator.SetBool("Attacking", true);
				//play the attack audio
				if(source.clip != attackAudio){
					source.clip = attackAudio;
					source.Play();
				}
			}
			AddReward(-0.3f);
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
                GameObject minUnit=new GameObject();
                foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
                    inspector.setScriptsFrom(enemy);
                    if(!inspector.isDead()) {
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
                        attacking=true;
                        if(inspector.getLives()<0) {
                            print("Damaged opponent dead");
                            AddReward(1f);
                        } else {
                            AddReward(0.5f);
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
		if(Vector3.Distance(agent.destination, transform.position) <= agent.stoppingDistance || agent.destination==null) {
			RequestDecision();
		} else {
			if(!dead && this.enabled) {
				//Moving towards to destination
			} else {
				Debug.LogWarning("Agent Descision Request triggered after its death");
				SetReward(-0.03f);
			}
		}
	}
	public void die() {
		SetReward(-1f);
		Done();
	}
	public override void AgentOnDone() {
		//if agent still remains after the destroying, Destroy log triggers two times more after episode ends because ending an episode triggers each agent's done function.
		dead=true;
		//create the ragdoll at the current position
		try {
			Instantiate(ragdoll, transform.position, transform.rotation);
		} catch {
			Debug.LogWarning("Error on placing deadbody. You probably unsigned the ragdoll from editor manually.");
		}
		Destroy(gameObject);
		Destroy(this);
		Debug.Log("Agent Destroyed");
	}
}
