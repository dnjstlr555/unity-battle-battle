using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 
using UnityEngine.AI;
using System.Linq;

public class Unit : MonoBehaviour {
	
	//variables visible in the inspector
	public float lives;
	public float damage;
	public string attackTag;
	public GameObject ragdoll;
	public AudioClip attackAudio;
	public AudioClip runAudio;
	public Collider Hitbox;
	public float RandomRange=1;
	public float AttackRange=2f;
	public float AttackCooltime=1f;
	public ParticleSystem DamagedParticle;
	//not visible in the inspector
	[HideInInspector]
	public Transform currentTarget;
	
	[HideInInspector]
	public bool spread;
	
	[HideInInspector] public NavMeshAgent agent;
	private GameObject health;
	private GameObject healthbar;
	
	[HideInInspector] public float startLives;
	[HideInInspector] public bool isPassedInit=false;
	private float defaultStoppingDistance;
	private Animator animator;
	private AudioSource source;
	
	private Vector3 randomTarget;
	private WalkArea area;
	private float lastLives=0;
	private ParticleSystem dustEffect;
	private int maxAlliesPerEnemy;
	
	private GameSystem Academy;
	public bool dead;
	private UnitInspect inspector;
	private System.Random rnd;
	private HitboxScript HitboxComponent;
	private bool isPassedCooltime=true;
	private MLAgents.ResetParameters param;
	public void initRefer(dynamic obj) {
		param=obj.Param;
		float plive, pcool, pdamage;
		lives = (param.TryGetValue("EnemyHP", out plive))?plive:lives;
		damage = (param.TryGetValue("EnemyDamage", out pdamage))?pdamage:damage;
		AttackCooltime = (param.TryGetValue("EnemyAttackCooltime", out pcool))?pcool:AttackCooltime;
	}
	void Start(){
		//if this an archer or enemy, don't use the spread option
		if(GetComponent<Archer>() || this.tag == "Enemy")
			spread = false; //spread the alley
		Academy = GameObject.FindObjectOfType<GameSystem>();
		//print(Academy);
		//get the audio source
		source = GetComponent<AudioSource>();
		maxAlliesPerEnemy = 1;
	
		//find navmesh agent component
		agent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		/*
		//find objects attached to this character
		health = transform.Find("Health").gameObject;
		healthbar = health.transform.Find("Healthbar").gameObject;
		health.SetActive(false);	
		*/
		lastLives=lives;
		//set healtbar value
		//healthbar.GetComponent<Slider>().maxValue = lives;
		startLives = lives;
		Academy.AllInitLives+=lives;
		//get default stopping distance
		defaultStoppingDistance = agent.stoppingDistance;
	
		//if there's a dust effect, find and assign it
		if(transform.Find("dust"))
			dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();
		
		//find the area so the character can walk around
		area = GameObject.FindObjectOfType<WalkArea>();
		inspector = new UnitInspect(Academy);
		inspector.cam=FindObjectOfType<CamController>();
		rnd = new System.Random();
		HitboxComponent=Hitbox.GetComponent<HitboxScript>();
		isPassedInit=true;
		if(!Academy.showanim) animator.enabled=false;
	}
	
	void FixedUpdate(){
		if(lives != lastLives && Academy.showeffects) {
			if(!DamagedParticle.isPlaying) {
				DamagedParticle.Play();
				//DamagedParticle.Simulate(Time.unscaledDeltaTime, true, false);
			}
			lastLives=lives;
		}
		
		//find closest enemy
		//ML:relating to moves
		if(currentTarget!=null) {
			inspector.setScriptsFrom(currentTarget.gameObject);
			if(!inspector.isScriptValid() || inspector.isDead()) {
				currentTarget = findCurrentTarget();
			}
		} else {
			currentTarget = findCurrentTarget();
		}
			
		
		//if character ran out of lives, it should die
		if(lives < 0 && !dead)
			die();
		else{
			if(Vector3.Distance(agent.destination, transform.position)<=agent.stoppingDistance && currentTarget!=null) {
				int sign = rnd.Next(0, 2) * 2 - 1;
				int sign2 = rnd.Next(0, 2) * 2 - 1;
				agent.destination = new Vector3(currentTarget.position.x+(float)gausianRand()*RandomRange*sign, currentTarget.position.y, currentTarget.position.z+(float)gausianRand()*RandomRange*sign2);	
				agent.isStopped = false;
			} else if (currentTarget==null) {
				agent.destination = getRandomPosition(area);
			}
			if(isPassedCooltime) { //&& (rnd.Next(0, 2) * 2 - 1)>0) {
				foreach(GameObject unit in HitboxComponent.GetCollideObjects()) {
					if(unit==null && !ReferenceEquals(unit, null)) {
							HitboxComponent.RemoveObject(unit);
							continue;
					}
					if(unit.CompareTag("Knight")) {
						if(inspector.setScriptsFrom(unit) && !inspector.isDead()) {
							inspector.setLives(inspector.getLives()-(damage));
							if(inspector.getLives()<0) HitboxComponent.RemoveObject(unit);
						} else {
							Debug.LogWarning("Invalid Target Triggered.");
						}
					}
				}
				isPassedCooltime=false;
				StartCoroutine("Cooltime");
			}
		}
		//ML:relating to moves
	}
	IEnumerator Cooltime() {
        yield return new WaitForSeconds(AttackCooltime);
        isPassedCooltime=true;
    }
	//randomly walk around
	public void walkRandomly(){
		//check if the area exists
		if(area != null){
			//set the stopping distance to 2
			if(agent.stoppingDistance > 2)
				agent.stoppingDistance = 2;
			
			//get a random position in the area
			if(randomTarget == Vector3.zero || Vector3.Distance(transform.position, randomTarget) < 3f)
				randomTarget = getRandomPosition(area);
			
			//check if the random target is not equal to vector3.zero
			if(randomTarget != Vector3.zero){
				//stop attacking
				if(animator.GetBool("Attacking")){
					animator.SetBool("Attacking", false);
					
					//play the run audio
					if(source.clip != runAudio){
						source.clip = runAudio;
						source.Play();
					}
				}
				
				//move the agent around
				agent.isStopped = false;
				agent.destination = randomTarget;
			}
		}
	}
	
	public Transform findCurrentTarget(){  
		//find all potential targets (enemies of this character)
		GameObject[] enemies = inspector.getUnitsByTag(attackTag);
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
						if(Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null && !target.GetComponent<AgentScript>().dead){
							//if this enemy is closest to character, set closest distance to distance between character and enemy
							closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
							target = potentialTarget.transform;
						}
					}	
					
					//if it is valid, return this target
					if(target && canAttack(target) && !target.GetComponent<AgentScript>().dead){
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
	private double gausianRand() {
		System.Random rand = new System.Random(); //reuse this if you are generating many
		double u1 = 1.0-rand.NextDouble(); //uniform(0,1] random doubles
		double u2 = 1.0-rand.NextDouble();
		double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
					System.Math.Sin(2.0 * System.Math.PI * u2); //random normal(0,1)

		return randStdNormal;
		
	}
	
	//check if there's not too much allies attacking this same enemy already
	public bool canAttack(Transform target){
		//get the number of allies that are already attacking this enemy
		int numberOfUnitsAttackingThisEnemy = 0;
		
		//foreach ally that's attacking the same enemy, increase the number of allies
		foreach(GameObject ally in inspector.getUnitsByTag(gameObject.tag)){
			if(ally.GetComponent<Unit>().currentTarget == target && !ally.GetComponent<Archer>())
				numberOfUnitsAttackingThisEnemy++;
		}
		
		//check if we may attack this target
		if(numberOfUnitsAttackingThisEnemy < maxAlliesPerEnemy && !target.GetComponent<AgentScript>().dead)
			return true;
		
		//return false if there's too much allies attacking this enemy already
		return false;
	}
	
	//find a random position inside the walk area
	public Vector3 getRandomPosition(WalkArea area){
		Vector3 center = area.RandCenter;
		Vector3 bounds = area.RandArea;
		
		//create a ray using the center and the bounds
		float yRay = center.y + bounds.y/2f;
		
		//get a random position for the ray to start from
		Vector3 rayStart = new Vector3(center.x + Random.Range(-bounds.x/2f, bounds.x/2f), yRay, center.z + Random.Range(-bounds.z/2f, bounds.z/2f));
		
		//store the raycast hit
		RaycastHit hit;
		
		//check if there's terrain underneath
		if(Physics.Raycast(rayStart, -Vector3.up, out hit, bounds.y))
			return hit.point;
		
		//if there's no terrain, return the center
		return Vector3.zero;
	}
	
	public void die(){
		
		
		//create the ragdoll at the current position
		if(!dead) Instantiate(ragdoll, transform.position, transform.rotation);
		
		dead = true;
		inspector.removeFrom(this.gameObject);
		Destroy(gameObject);
		//wait a moment and destroy the original unit
		return;
		
	}
}
