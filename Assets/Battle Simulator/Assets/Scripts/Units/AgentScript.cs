using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Linq;

class UnitInspect {
	public Unit UnitScript;
	public AgentScript AgentScript;
	public int lives=0;
	public bool isScriptValid() {
		return (AgentScript!=null || UnitScript!=null);
	}
	public bool setScriptsFrom(GameObject obj) {
		UnitScript = (obj.GetComponent<Unit>()!=null) ? obj.GetComponent<Unit>() : null;
		AgentScript = (obj.GetComponent<AgentScript>()!=null) ? obj.GetComponent<AgentScript>() : null;
		return isScriptValid();
	}
	public float getLives() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.lives;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.lives;
			}
		}
		return -1;
	}
	public void setLives(float hp) {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				AgentScript.lives=hp;
			} else if(UnitScript && !AgentScript) {
				UnitScript.lives=hp;
			}
		}
	}
	public bool isDead() {
		///Returns true when the unit is dead or valid, otherwise return false
		if(this.isScriptValid()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.dead;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.dead;
			}
		}
		return true;
	}
}
public class AgentScript : Agent
{
    public float lives;
	public float damage;
	public string attackTag;
	public GameObject ragdoll;
	public AudioClip attackAudio;
	public AudioClip runAudio;
	
	//not visible in the inspector
	[HideInInspector]
	public Transform currentTarget;
	
	[HideInInspector]
	public bool spread;
	
	private NavMeshAgent agent;
	private GameObject health;
	private GameObject healthbar;
	
	[HideInInspector]
	private float startLives;
	private float defaultStoppingDistance;
	public Rigidbody Rigid;
	private Animator animator;
	private AudioSource source;
	
	private Vector3 randomTarget;
	private WalkArea area;
	
	private ParticleSystem dustEffect;
	private int maxAlliesPerEnemy;
	
	public bool dead;

    public Transform Target;
	private float REWARD;
	private GameSystem sys;
    public override void OnEpisodeBegin() {
		//if()
		print("NewEpisode");
        spread=false;
		if(GetComponent<Archer>() || this.tag == "Enemy")
			spread = false; //spread the alley
		
		//get the audio source
		source = GetComponent<AudioSource>();
		maxAlliesPerEnemy = 1;
	
		//find navmesh agent component
		agent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		Rigid = this.GetComponent<Rigidbody>();

		//find objects attached to this character
		health = transform.Find("Health").gameObject;
		healthbar = health.transform.Find("Healthbar").gameObject;
		health.SetActive(false);	
	
		//set healtbar value
		healthbar.GetComponent<Slider>().maxValue = lives;
		startLives = lives;
		//get default stopping distance
		defaultStoppingDistance = agent.stoppingDistance;
	
		//if there's a dust effect, find and assign it
		if(transform.Find("dust"))
			dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();
		
		//find the area so the character can walk around
		area = GameObject.FindObjectOfType<WalkArea>();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
		sys = GameObject.FindObjectOfType<GameSystem>();
		if(sys!=null) {
			if(!sys.battleStarted) {
				for(int i=0;i<sys.placedUnits.Count;i++) {
					sensor.AddObservation(0);
				}
			} else {
				UnitInspect inspector = new UnitInspect();
				sensor.AddObservation(transform.localPosition.x);
				sensor.AddObservation(transform.localPosition.z);
				GameObject[] Knight = GameObject.FindGameObjectsWithTag("Knight");
				GameObject[] Enemy = GameObject.FindGameObjectsWithTag("Enemy");
				for(int i=0;i<sys.initKnightNumber;i++) {
					if(!Knight[i]) {
						sensor.AddObservation(0);
						continue;
					}
					if(Knight[i]==this.gameObject) continue;
					if(inspector.setScriptsFrom(Knight[i])) { //returns true when it's valid
						if(!inspector.isDead()) {
							sensor.AddObservation(Knight[i].transform.localPosition.x);
							sensor.AddObservation(Knight[i].transform.localPosition.z);
						} else {
							sensor.AddObservation(0);
							sensor.AddObservation(0);
						}
					} else {
						sensor.AddObservation(0);
						continue;
					}
				}
				for(int i=0;i<sys.initEnemyNumber;i++) {
					if(Enemy[i]==this.gameObject || !Knight[i]) continue;
					if(inspector.setScriptsFrom(Enemy[i])) { //returns true when it's valid
						if(!inspector.isDead()) {
							sensor.AddObservation(Enemy[i].transform.localPosition.x);
							sensor.AddObservation(Enemy[i].transform.localPosition.z);
						} else {
							sensor.AddObservation(-9999999);
							sensor.AddObservation(-9999999);
						}
					}
				}
			}
		}
    }
	public static Vector2 RadianToVector2(float radian) {
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}
  
	public static Vector2 DegreeToVector2(float degree) {
		return RadianToVector2(degree * Mathf.Deg2Rad);
	}
    public float vector_offset = 10;
    public override void OnActionReceived(float[] act)
    {
		REWARD=-0.01f;
		//print(act[0]+" "+act[1]);
		float angle = act[0]*360f*Mathf.Deg2Rad;
    	float force = Mathf.Clamp(act[1], -1, 1) * vector_offset;
        Vector3 controlSignal = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
		controlSignal.Normalize();
		controlSignal*=force;
    	//print(controlSignal);
		if (!dead) {
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
		
			//if character ran out of lives, it should die
			if(lives < 0)
				StartCoroutine(die());
				//if()
			if(dustEffect && animator.GetBool("Attacking") == false && !dustEffect.isPlaying)
				dustEffect.Play();

			if(dustEffect && dustEffect.isPlaying && animator.GetBool("Attacking") == true)
				dustEffect.Stop();
			if(agent.stoppingDistance != defaultStoppingDistance)
				agent.stoppingDistance = defaultStoppingDistance;
			
			//move the agent around and set its destination to the enemy target
			agent.isStopped = false;	
			agent.destination = transform.position + controlSignal;
			
			//check if character has reached its target and than rotate towards target and attack it
			UnitInspect potentialEnemy = new UnitInspect();
			float maxDistance=Mathf.Infinity;
			bool attacking=false;
			foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
				potentialEnemy.setScriptsFrom(enemy);
				if(!potentialEnemy.isDead()) {
					float distanceToTarget = Vector3.Distance(this.transform.localPosition, enemy.transform.localPosition);
					if(distanceToTarget<= agent.stoppingDistance) {
						maxDistance=(distanceToTarget>maxDistance)?distanceToTarget:maxDistance;

						Vector3 currentTargetPosition = enemy.transform.position;
						currentTargetPosition.y = transform.position.y;
						transform.LookAt(currentTargetPosition);
						animator.SetBool("Attacking", true);
						
						//play the attack audio
						if(source.clip != attackAudio){
							source.clip = attackAudio;
							source.Play();
						}
						
						potentialEnemy.setLives(potentialEnemy.getLives()-Time.deltaTime * damage);
						attacking=true;
						if(potentialEnemy.getLives()<0) {
							print("dead!!");
							REWARD+=2;
						} else {
							REWARD+=1;
						}
					}
				}
			}
				
			//if its still traveling to the target, play running animation
			if(animator.GetBool("Attacking") && !attacking){
				animator.SetBool("Attacking", false);
				
				//play the running audio
				if(source.clip != runAudio){
					source.clip = runAudio;
					source.Play();
				}
			}
			SetReward(REWARD);
		/*
			// Rewards
			float distanceToTarget = Vector3.Distance(this.transform.localPosition, findCurrentTarget().localPosition);

			// Reached target
			if (distanceToTarget < 1.42f)
			{
				SetReward(1.0f);
				EndEpisode();
			}
			// Fell off platform
			*/
		} else {
			agent.isStopped = true;
			SetReward(-0.03f);
		}
			
    }
	public IEnumerator die() {
		dead = true;
		SetReward(-1f);
		//create the ragdoll at the current position
		Instantiate(ragdoll, transform.position, transform.rotation);
		transform.position = new Vector3(999, 999, 999);
		//wait a moment and destroy the original unit
		yield return new WaitForEndOfFrame();
	}
	public bool canAttack(Transform target){ //check too much targetting on one instance
		//get the number of allies that are already attacking this enemy
		int numberOfUnitsAttackingThisEnemy = 0;
		
		//foreach ally that's attacking the same enemy, increase the number of allies
		foreach(GameObject ally in GameObject.FindGameObjectsWithTag(gameObject.tag)){
			if(ally.GetComponent<Unit>().currentTarget == target && !ally.GetComponent<Archer>())
				numberOfUnitsAttackingThisEnemy++;
		}
		
		//check if we may attack this target
		if(numberOfUnitsAttackingThisEnemy < maxAlliesPerEnemy)
			return true;
		
		//return false if there's too much allies attacking this enemy already
		return false;
	}
	
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
	
}
