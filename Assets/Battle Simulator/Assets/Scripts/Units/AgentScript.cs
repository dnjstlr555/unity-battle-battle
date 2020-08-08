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
    public float lives, initLives;
	public float damage;
	public string attackTag;
	public GameObject ragdoll;
	public AudioClip attackAudio;
	public AudioClip runAudio;
	
	//not visible in the inspector
	[HideInInspector]
	public Transform currentTarget;
	
	[HideInInspector]
	
	private NavMeshAgent agent;
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
	const int PlannedObs=GameSystem.initKnightNumber+GameSystem.initEnemyNumber;
	public override void InitializeAgent() {
		sys = GameObject.FindObjectOfType<GameSystem>();
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
				for(int i=0;i<GameSystem.initKnightNumber;i++) {
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
				for(int i=0;i<GameSystem.initEnemyNumber;i++) {
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
					Debug.LogError("Unknown game object tagged as knight and it observated.");
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
	public static Vector2 RadianToVector2(float radian) {
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}
  
	public static Vector2 DegreeToVector2(float degree) {
		return RadianToVector2(degree * Mathf.Deg2Rad);
	}
    public float vector_offset = 10;
    public override void AgentAction(float[] act, string textAction)
    {
		REWARD=-0.01f;
		//print(act[0]+" "+act[1]);
		float angle = act[0]*360f*Mathf.Deg2Rad;
    	float force = Mathf.Clamp(act[1], -1, 1) * vector_offset;
        Vector3 controlSignal = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
		controlSignal.Normalize();
		controlSignal*=force;
    	//print(controlSignal);
		if (!dead && this.enabled) {
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
		
			//if character ran out of lives, it should die
			if(lives < 0) {
				//die();
			}
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
		} else {
			agent.isStopped = true;
			SetReward(-0.03f);
		}
			
    }
	public void AgentDescisionRequest() {
		if(dead || gameObject==null || !sys.battleStarted) return;
		if(Vector3.Distance(agent.destination, transform.position) <= agent.stoppingDistance || agent.destination==null) {
			RequestDecision();
		} else {
			if(!dead && this.enabled) {
				//Moving towards to destination
			} else {
				SetReward(-0.03f);
			}
		}
	}
	public void die() {
		SetReward(-1f);
		Done();
	}
	public override void AgentOnDone() {
		//Theory : if agent still remains after the destroying, Destroy log triggers two times more after episode ends because ending an episode triggers each agent's done function.
		dead=true;
		//create the ragdoll at the current position
		try {
			Instantiate(ragdoll, transform.position, transform.rotation);
			//transform.position = new Vector3(999, 999, 999);
		} catch {
			Debug.LogWarning("Error on placing deadbody. You probably unsigned the ragdoll from editor manually.");
		}
		Destroy(gameObject);
		Destroy(this);
		Debug.Log("destroy!!!");
	}
}
