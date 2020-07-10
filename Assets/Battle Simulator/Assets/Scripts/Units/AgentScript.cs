using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Linq;
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
	
	private bool dead;
    Rigidbody rBody;
    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
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
        //space size:6
        if(currentTarget == null && GameObject.FindGameObjectsWithTag(attackTag).Length > 0) currentTarget = findCurrentTarget();
        // Target and Agent positions
        sensor.AddObservation((currentTarget.position!=null)?currentTarget.position:Vector3.zero);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        /*
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
        */
    }
    public float speed = 10;
    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
		if (!dead) {
			if(lives != startLives){
				//only use the healthbar when the character lost some lives
				if(!health.activeSelf)
					health.SetActive(true);
				
				health.transform.LookAt(2 * transform.position - Camera.main.transform.position);
				healthbar.GetComponent<Slider>().value = lives;
			}
		
			//if character ran out of lives, it should die
			if(lives < 0 && !dead)
				StartCoroutine(die());
				//if()
			
			//play dusteffect when running and stop it when the character is not running
			if(dustEffect && animator.GetBool("Attacking") == false && !dustEffect.isPlaying)
				dustEffect.Play();

			if(dustEffect && dustEffect.isPlaying && animator.GetBool("Attacking") == true)
				dustEffect.Stop();
			
			//randomly walk across the battlefield if there's no targets left
			//ML:relating to moves
			//if there are targets, make sure to use the default stopping distance
			if(agent.stoppingDistance != defaultStoppingDistance)
				agent.stoppingDistance = defaultStoppingDistance;
			
			//move the agent around and set its destination to the enemy target
			//agent.isStopped = false;	
			//agent.destination = controlSignal;
			//Vector3 dir = (transform.position - currentTarget.position).normalized;
			//Rigid.MovePosition(dir);
			
			//check if character has reached its target and than rotate towards target and attack it
			if(Vector3.Distance(controlSignal, transform.position) <= agent.stoppingDistance){
				/* 
				Vector3 currentTargetPosition = currentTarget.position;
				currentTargetPosition.y = transform.position.y;
				transform.LookAt(currentTargetPosition);
				animator.SetBool("Attacking", true);
				
				//play the attack audio
				if(source.clip != attackAudio){
					source.clip = attackAudio;
					source.Play();
				}
				
				//apply damage to the enemy
				currentTarget.gameObject.GetComponent<Unit>().lives -= Time.deltaTime * damage;
				*/ 
			}
				
			//if its still traveling to the target, play running animation
			if(animator.GetBool("Attacking") && Vector3.Distance(currentTarget.position, transform.position) > agent.stoppingDistance){
				animator.SetBool("Attacking", false);
				
				//play the running audio
				if(source.clip != runAudio){
					source.clip = runAudio;
					source.Play();
				}
			}
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
		} //20200710 
			
    }
	public IEnumerator die(){
		dead = true;
		
		//create the ragdoll at the current position
		Instantiate(ragdoll, transform.position, transform.rotation);
		
		//wait a moment and destroy the original unit
		yield return new WaitForEndOfFrame();
	}
    public Transform findCurrentTarget(){  
		//find all potential targets (enemies of this character)
		GameObject[] enemies = GameObject.FindGameObjectsWithTag(attackTag);
		Transform target = null;
		
		//if we want this character to communicate with his allies
		if(spread){
			//get all enemies
			List<GameObject> availableEnemies = enemies.ToList();
			int count = 0;
			
			//make sure it doesn't get stuck in an infinite loop
			while(count < 300){
				//for all enemies
				for(int i = 0; i < enemies.Length; i++){
					//distance between character and its nearest enemy
					float closestDistance = Mathf.Infinity;
		
					foreach(GameObject potentialTarget in availableEnemies){
						//check if there are enemies left to attack and check per enemy if its closest to this character
						if(Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null){
							//if this enemy is closest to character, set closest distance to distance between character and enemy
							closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
							target = potentialTarget.transform;
						}
					}	
					
					//if it is valid, return this target
					if(target && canAttack(target)){
						return target;
					}
					else{
						//if it's not, remove it from the list and try again
						availableEnemies.Remove(target.gameObject);
					}
				}
				
				//after checking all enemies, allow one more ally to also attack the same enemy and try again
				maxAlliesPerEnemy++;
				availableEnemies.Clear();
				availableEnemies = enemies.ToList();
			
				count++;
			}
			
			//show a loop error
			Debug.LogError("Infinite loop");
		}
		else{ 
			//if we're using the simple method:
            //find closest target between
			float closestDistance = Mathf.Infinity;
		
			foreach(GameObject potentialTarget in enemies){
				//check if there are enemies left to attack and check per enemy if its closest to this character
				if(Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null){
					//if this enemy is closest to character, set closest distance to distance between character and enemy
					closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
					target = potentialTarget.transform;
				}
			}	
			
			//check if there's a target and return it
			if(target)
				return target;
		}
		
		//otherwise return null
		return null;
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
