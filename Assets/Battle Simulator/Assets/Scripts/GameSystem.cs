using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;

//troop class so we can build different troops/characters
[System.Serializable]


public class GameSystem : MonoBehaviour {
	
	//variables visible in the inspector
	[Header("Objects:")]
	public Animator leftPanelAnimator;
	public Animator buttonsAnimator;
	public Animator gamePanel;
	public Animator cameraAnimator;
	
	[Space(5)]
	public GameObject characterPanel;
	public GameObject button;
	public GameObject indicator;
	public GameObject characterStatsPanel;
	public GameObject topDownMapPanel;
	public GameObject gridCell;
	
	[Space(5)]
	public Image statsButton;
	public Image topDownButton;
	public Image battleIndicator;
	
	[Space(5)]
	public Text statsName;
	public Text statsDamage;
	public Text statsHealth;
	public Text statsRange;
	public Text statsSpeed;
	public Text coinsText;
	public Text levelInfo;
	public Text infoPanel;
	public Text DebugOutput;
	
	[Space(5)]
	public Dropdown speedSetting;
	public int initEnemyNumber, initKnightNumber; 
	//public GameObject indicator;
	
	[Space(5)]

	[Header("Troops:")]
	public List<Troop> troops;
	public MLAgents.Brain brain;

	private int selected;
	private GameObject currentDemoCharacter;
	private int rotation = -90;
	///<summary>
	///Indicates whether the battle started or not. True If battle started<br/>
	///If it false, Academy_Update() of GameSystem.cs, Sticky feature(requires all units to be valid) of CamController.cs, AcademyStep() of MyAcademy.cs, CollectObservations(), AgentAction(), AgentDescisionRequest() of AgentScript.cs will be disabled.
	///</summary>
	[HideInInspector] public bool battleStarted;
	[HideInInspector] public int enemyNumber, knightNumber;
	[HideInInspector] public bool FirstWarnUseOfAttackFb=false;
	[HideInInspector] public float Timer=0;
	[HideInInspector] public GameObject EmptyUnit;
	[HideInInspector] public GameObject[] knightUnits;
	[HideInInspector] public GameObject[] enemyUnits;
	[HideInInspector] public float AllInitLives, AllDamaged;
	private bool erasing;
	private LevelData levelData;
	private bool characterStats;
	private Vector3 gridCenter;
	private int gridSize;
	private UnitInspect inspector;
	private CamController cam;
	private DebugInfo DebugInner;
	private MyAcademy AcademyInner;

	///<summary>Initializing system varables.</summary>
	public void Academy_Initialize() {
		print("Initializing Game System");
		levelData = Resources.Load("Level data") as LevelData;
		//double the grid size so it's always even
		gridSize = levelData.gridSize * 2;
		gridCenter = GameObject.FindObjectOfType<EnemyArmy>().gameObject.transform.position;
		//get the grid center by taking the opposite of the the enemy army position
		gridCenter = new Vector3(-gridCenter.x, gridCenter.y, gridCenter.z);

		//if the level exists, show some level info, else load the end screen
		if(PlayerPrefs.GetInt("level") >= levelData.levels.Count){
			Debug.LogError("Invalid level data, Load default level - " + PlayerPrefs.GetInt("level"));
			PlayerPrefs.SetInt("level", 0);
		}
		else{
			levelInfo.text = "Level " + (PlayerPrefs.GetInt("level") + 1) + " - " + levelData.levels[PlayerPrefs.GetInt("level")].scene;
		}
		characterStats = true;
		switchPanelContent(false);
		characterStatsPanel.SetActive(false);
		//enables minimap
		topDownMapPanel.SetActive(true);
		leftPanelAnimator.SetBool("hide instant", true);

		EmptyUnit=new GameObject();
		EmptyUnit.transform.parent = this.gameObject.transform;
		inspector = new UnitInspect(this);
		cam=FindObjectOfType<CamController>();
		inspector.cam=cam;
		DebugInner= new DebugInfo(inspector);
		AcademyInner = FindObjectOfType<MyAcademy>();
	}
	///<summary>Initializes knightNumber and enemyNumber, battleStarted</summary>
	public void Academy_Awake() {
		knightNumber=0;
		enemyNumber=0;
		battleStarted=false;
		print($"after awake:{knightUnits.Length}/{enemyUnits.Length}");
		foreach(GameObject d in enemyUnits) {
			print(d);
		}
		DebugInner.printReset();
	}
	///<summary>Spawn the agents</summary>
	public void Academy_Start() {
		Academy_Spawn();
	}
	public void Academy_Spawn() {
		print("Spawning Agents");
		//placeAgent(new Vector3(3.0f,0.0f,-12.7f),4);
		//placeAgent(new Vector3(3.2f,0,-5.7f),4);
		placeAgent(new Vector3(3.0f,0.0f,2.9f),4);
		//placeAgent(new Vector3(3.0f,0.0f,9.7f),4);
	}
	public void placeAgent(Vector3 position, int select) {
		if(canPlace(position, false)){
			GameObject AgentObj = Instantiate(troops[select].deployableTroops, position, Quaternion.identity);
			AgentScript unit = AgentObj.GetComponent<AgentScript>();
			updateRotation(unit.gameObject);
			inspector.addFrom(AgentObj);
		} else {
			Debug.LogWarning("Couldn't spawn agent for unknown reason");
		}
	}
	///<summary>set battleStarted to be True</summary>
	public void startBattle(){
		if(!FindObjectOfType<EnemyArmy>().IsPlaced()) {
			print("Coudln't start battle beacuse the enemy didn't spawn");
			return;
		}
		//knightUnits=GameObject.FindGameObjectsWithTag("Knight");
		//enemyUnits=GameObject.FindGameObjectsWithTag("Enemy");
		print($"Start:{knightUnits.Length}/{enemyUnits.Length}");
		//show the new UI
		cam.printOnPanel($"Episode:{AcademyInner.GetEpisodeCount()}");
		StartCoroutine(battleUI());
		battleStarted = true;

		//Hardcoded rewarding algorithm, need to be adjusted
		AllInitLives=0;
	}
	///<summary>Check battle started and updates units to be set dead in certain condition. also updates number and battle indicator, debug infos</summary>
	public void Academy_Update() {
		if(battleStarted){
			knightNumber=0;
			enemyNumber=0;
			GameObject[] Units = inspector.getCurrentUnits();
			foreach(GameObject unit in Units) {
				if(inspector.setScriptsFrom(unit) && !inspector.isDead() && inspector.getLives()<0 && !inspector.isDoneInProgress()) {
					inspector.setDead();
				}
			}
			GameObject[] UpdatedUnits = inspector.getCurrentUnits();
			for(int i=0;i<UpdatedUnits.Length;i++) {
				if(inspector.setScriptsFrom(UpdatedUnits[i]) && !inspector.isDead()) {
					knightNumber+=(inspector.getTag()=="Knight")?1:0;
					enemyNumber+=(inspector.getTag()=="Enemy")?1:0;
				}
			}
			//get the current battle status to show in the indicator
			float fill = BattleStatus();
			
			//change the indicator fill based on the status
			if(battleIndicator.fillAmount < fill){
				battleIndicator.fillAmount += Time.deltaTime * 0.1f;
				
				if(battleIndicator.fillAmount >= fill)
					battleIndicator.fillAmount = fill;
			}
			else if(battleIndicator.fillAmount > fill){
				battleIndicator.fillAmount -= Time.deltaTime * 0.1f;
				
				if(battleIndicator.fillAmount <= fill)
					battleIndicator.fillAmount = fill;
			}
		}
		else if(knightNumber == 0){
			battleIndicator.fillAmount -= Time.deltaTime * 0.5f;
		}
		else if(enemyNumber == 0){
			battleIndicator.fillAmount += Time.deltaTime * 0.5f;
		}
		/*
		GameObject nowUnit=cam.getStickyUnit();
		if(inspector.setScriptsFrom(nowUnit)) {
			DebugInner.setFromUnit(nowUnit);
		}
		
		infoPanel.text = $@"Id:{inspector.InnerObject.GetInstanceID()}
Hp:{DebugInner.initialLives}/{DebugInner.currentLives}
Default Damage:{DebugInner.defaultDammage}
Attack Tag:{DebugInner.attackTag}
Range:{DebugInner.range}
Unit Type:{DebugInner.unitType}

Total Damage:{DebugInner.totalDamage}
Pre Reward:{DebugInner.preReward}

Knight Num:{knightNumber}
Enemy Num:{enemyNumber}
Episode:{AcademyInner.GetEpisodeCount()}
		";
		
	*/
	}
	
	
	//check if the position is within the 3D grid
	bool withinGrid(Vector3 position){
		//if we're not using any grid, it's inside the grid by default
		if(!levelData.grid)
			return true;
			
		
		//else, compare the position to the grid
		if(position.x > gridCenter.x + gridSize || position.x < gridCenter.x - gridSize || position.z < gridCenter.z - gridSize || position.z > gridCenter.z + gridSize)
			return false;
		
		return true;
	}
	
	//calculate the battle status by comparing the number of enemies vs the number of allies
	float BattleStatus(){
		int knightsLeft = this.knightNumber;
		int enemiesLeft = this.enemyNumber;
		int total = knightsLeft + enemiesLeft;
		
		return (float)knightsLeft/(float)total;
	}
	
	//place a new unit
	
	//check if the character can be placed at this position
	bool canPlace(Vector3 position, bool placingGridCell){
		//check if there's units too close to the current position
		if(unitsInRange(position) != null || !withinGrid(position)) //troops[selected].troopCosts > coins ||
			return false;
		
		//check if we're within the maximum place range
		if(!placingGridCell && (EventSystem.current.IsPointerOverGameObject() || Vector3.Distance(Camera.main.transform.position, position) > levelData.placeRange))
			return false;
			
		return true;
	}
	
	//translate a 3d position to a 2d grid index
	int positionToGridIndex(Vector3 position){
		position = new Vector3(Mathf.RoundToInt(position.x) - gridCenter.x, position.y, Mathf.RoundToInt(position.z)  - gridCenter.z);
		int index = 0;
		index += (int)(Mathf.Abs(-(gridSize - 1) - position.z)/2);
		index += (int)(gridSize * Mathf.Abs(-(gridSize - 1) - position.x)/2);
		return index;
	}
	
	//translate a 2d grid index to a 3d position
	Vector3 gridIndexToPosition(int index){
		int x = (index % gridSize) * 2;
		int z = (int)(index/gridSize) * 2;
		Vector3 position = gridCenter + new Vector3(-(gridSize - 1) + z, 100, -(gridSize - 1) + x);
		
		RaycastHit hit;
		if(Physics.Raycast(position, -Vector3.up, out hit))
			return hit.point;
		
		return Vector3.zero;
	}
	
	//get the index in the troops list for this unit
	int unitIndex(GameObject unit){
		for(int i = 0; i < troops.Count; i++){
			if(troops[i].deployableTroops.name == unit.name.Substring(0, unit.name.Length - 7))
				return i;
		}
		
		return 0;
	}
	
	//get all units in range of a certain position
	public GameObject unitsInRange(Vector3 position){
		//store the units in an array
		GameObject[] allUnits = inspector.getInstantiatedUnits();
		
		//foreach unit, check if it's in range and return as soon as one of them is
		foreach(GameObject unit in allUnits){
			if(Vector3.Distance(unit.transform.position, position) < levelData.checkRange && unit.gameObject != currentDemoCharacter)
				return unit;
		}
		
		//after checking all units, return null
		return null;
	}
	
	//hide or show the panel on the left
	public void showHideLeftPanel(){
		leftPanelAnimator.SetBool("hide panel", !leftPanelAnimator.GetBool("hide panel"));
		if(!leftPanelAnimator.GetBool("hide panel"))
			changeDemo();
	}
	
	IEnumerator addCharacterButtons(){
		//for all troops...
		for(int i = 0; i < troops.Count; i++){
			//add a button to the list of buttons
			GameObject newButton = Instantiate(button);
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			rectTransform.SetParent(characterPanel.transform, false);
			
			//set button outline
			newButton.GetComponent<Outline>().effectColor = levelData.buttonHighlight;
			
			//set the correct button sprite
			newButton.gameObject.GetComponent<Image>().sprite = troops[i].buttonImage;
			
			//only enable outline for the first button
			if(i == 0){
				newButton.GetComponent<Outline>().enabled = true;
			}
			else{
				newButton.GetComponent<Outline>().enabled = false;	
			}
			
			//set button name to its position in the list
			newButton.transform.name = "" + i;
			
			newButton.GetComponentInChildren<Text>().text = "" + troops[i].troopCosts;
			
			//this is the new button
			troops[i].button = newButton;
			
			//wait to create the button spawn effect
			yield return new WaitForSeconds(levelData.buttonEffectTime/(float)troops.Count);
		}
	}
	
	public void selectTroop(int index){
		print(index);
		//remove all outlines and set the current button outline visible
		for(int i = 0; i < troops.Count; i++){
			troops[i].button.GetComponent<Outline>().enabled = false;	
		}
		troops[index].button.GetComponent<Outline>().enabled = true;
		
		//update the selected unit
		selected = index;
		
		//stop erasing
		erasing = false;
		
		
		//change the character statistics
		setStats(index);
	}
	
	public void changeDemo(){
		//if there is one, remove the current demo
		if(currentDemoCharacter)
			Destroy(currentDemoCharacter);
		
		//create a new demo and name and tag it
		currentDemoCharacter = Instantiate(troops[selected].deployableTroops);
		currentDemoCharacter.name = "demo";
		currentDemoCharacter.tag = "Untagged";
		
		//disable the new demo so it doesn't move around using the navmesh
		disableUnit(currentDemoCharacter);
		
		//change the demo colors
		foreach(Renderer renderer in currentDemoCharacter.GetComponentsInChildren<Renderer>()){
			foreach(Material material in renderer.materials){
				material.shader = Shader.Find("Unlit/UnlitAlphaWithFade");
				float colorStrength = (material.color.r + material.color.g + material.color.b)/3f;
				material.color = new Color(material.color.r, material.color.g, material.color.b, levelData.demoCharacterAlpha * colorStrength);
			}
		}
		
		//create the demo tile and parent it to the demo character
		GameObject tile = newTile(levelData.tileColor);
		tile.transform.SetParent(currentDemoCharacter.transform, false);
		
		//update the demo rotation
		updateRotation(currentDemoCharacter);
	}
	
	//change all the unit stats in the character panel
	public void setStats(int index){
		GameObject troop = troops[index].deployableTroops;
		Unit unit = troop.GetComponent<Unit>();
		statsName.text = troop.name;
		statsDamage.text = unit.damage + "";
		statsHealth.text = unit.lives + "";
		statsRange.text = troop.GetComponent<NavMeshAgent>().stoppingDistance + "";
		statsSpeed.text = troop.GetComponent<NavMeshAgent>().speed + "";
	}
	
	//create new demo tile
	public GameObject newTile(Color color){
		//instantiate the tile and scale it
		GameObject tile = Instantiate(indicator);
		tile.transform.localScale = new Vector3(2, 0.1f, 2);
		
		//change the look of the tile
		foreach(Renderer renderer in tile.GetComponentsInChildren<Renderer>()){
			renderer.material.shader = Shader.Find("Unlit/UnlitAlphaWithFade");
			renderer.material.color = color;
		}
		
		//if we're erasing, destroy the demo character
		if(erasing)
			Destroy(tile.transform.GetChild(0).gameObject);
		
		return tile;
	}
	
	//update the character placement rotation
	public void updateRotation(GameObject unit){
		Vector3 characterRotation = unit.transform.localEulerAngles;
		unit.transform.localEulerAngles = new Vector3(characterRotation.x, rotation, characterRotation.z);
	}
	
	//using raycasting, get the mouse position compared to the terrain
	public Vector3 getPosition(){
		//initialize a ray and a hit object
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		//check if there's terrain below the current mouse position
		if(Physics.Raycast(ray, out hit) && hit.collider != null && !EventSystem.current.IsPointerOverGameObject() && hit.collider.gameObject.tag == "Battle ground"){
			//enable the demo character if there's a valid position
			if(!currentDemoCharacter.activeSelf)
				currentDemoCharacter.SetActive(true);
			
			//normally, return the hit point
			if(!Input.GetKey(levelData.snappingKey) && !levelData.grid){
				return hit.point;
			}
			else{
				//if we're using snapping, change the position so it snaps in place
				Vector3 pos = hit.point;
				pos -= Vector3.one;
				pos /= 2f;
				pos = new Vector3(Mathf.Round(pos.x), pos.y, Mathf.Round(pos.z));
				pos *= 2f;
				pos += Vector3.one;
				return pos;
			}
		}
		else if(currentDemoCharacter.activeSelf){
			//don't show the character if it didn't find a position on the terrain
			currentDemoCharacter.SetActive(false);
		}
		
		//if there's no position, return vector3.zero
		return Vector3.zero;
	}
	
	public void disableUnit(GameObject unit){
		if(unit) {
			inspector.setScriptsFrom(unit);
			//disable the navmesh agent component
			unit.GetComponent<NavMeshAgent>().enabled = false;
			unit.GetComponent<Collider>().enabled = false;

			//if this is an archer, disable the archer functionality
			if(inspector.isScriptValid()) {
				inspector.setEnable(false);
				//inspector.setSpread(levelData.spreadUnits);
			}
			if(unit.GetComponent<Archer>())
				unit.GetComponent<Archer>().enabled = false;
			/*
			foreach (MonoBehaviour component in unit.GetComponents<MonoBehaviour>()) {
				if(component.type == "CustomBehaviour") {
					print("haha");
					this.enabled = false;
				}
			}
			*/
			//disable the health object
			unit.transform.Find("Health").gameObject.SetActive(false);	
			
			//disable any particles
			foreach(ParticleSystem particles in unit.GetComponentsInChildren<ParticleSystem>()){
				particles.gameObject.SetActive(false);
			}
			
			//make sure it's playing an idle animation
			foreach(Animator animator in unit.GetComponentsInChildren<Animator>()){
				animator.SetBool("Start", false);
			}
		}
	}
	
	public void enableUnit(GameObject unit){
		//enable all the components
		unit.GetComponent<NavMeshAgent>().enabled = true;
		unit.GetComponent<Collider>().enabled = true;
		unit.GetComponent<AudioSource>().Play();
		if(inspector.setScriptsFrom(unit)) {
			inspector.setEnable(true);
		}
		if(inspector.getScriptType()=="AgentScript") {
			inspector.setInitAgent(brain);
		}
		if(unit.GetComponent<Archer>())
			unit.GetComponent<Archer>().enabled = true;
		
		unit.transform.Find("Health").gameObject.SetActive(true);	
		
		//show particles
		foreach(ParticleSystem particles in unit.GetComponentsInChildren<ParticleSystem>()){
			particles.gameObject.SetActive(true);
		}
		
		//start the animators
		foreach(Animator animator in unit.GetComponentsInChildren<Animator>()){
			animator.SetBool("Start", true);
		}
	}
	
	public void reloadScene(){
		//reload the current scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	public void menu(){
		//load the menu scene
		SceneManager.LoadScene(0);
	}
	
	//change the UI in the bottom of the left panel
	public void switchPanelContent(bool statsButtonActive){
		if(statsButtonActive && !characterStats){
			statsButton.color = Color.white;
			topDownButton.color = levelData.selectedPanelColor;
			
			statsButton.gameObject.GetComponentInChildren<Text>().color = levelData.selectedPanelColor;
			topDownButton.gameObject.GetComponentInChildren<Text>().color = Color.white;
		}
		else if(!statsButtonActive && characterStats){
			statsButton.color = levelData.selectedPanelColor;
			topDownButton.color = Color.white;
			
			statsButton.gameObject.GetComponentInChildren<Text>().color = Color.white;
			topDownButton.gameObject.GetComponentInChildren<Text>().color = levelData.selectedPanelColor;
		}
		else{
			return;
		}
		
		characterStats = !characterStats;
		
		//set the panels active if they should be active and turn them off if they should not be active
		characterStatsPanel.SetActive(!characterStatsPanel.activeSelf);
		topDownMapPanel.SetActive(!topDownMapPanel.activeSelf);
	}
	

	public void setSpeed(){
		//change the timescale based on the selected setting
		switch(speedSetting.value){
			case 0: Time.timeScale = 0; break;
			case 1: Time.timeScale = 0.5f; break;
			case 2: Time.timeScale = 1; break;
			case 3: Time.timeScale = 1.5f; break;
			case 4: Time.timeScale = 2; break;
		}
		
		//stop audio if the timescale is 0
		if(Time.timeScale == 0){
			AudioListener.volume = 0;
		}
		else{
			AudioListener.volume = 1;
		}
	}
	
	IEnumerator battleUI(){
		//hide the grid
		leftPanelAnimator.gameObject.SetActive(false);
		buttonsAnimator.gameObject.SetActive(false);
		foreach(GameObject UI in GameObject.FindGameObjectsWithTag("Editory")) {
			UI.SetActive(false);
		}

		//wait a moment and remove the panels
		yield return new WaitForSeconds(0.5f);
		//show the game panel
		gamePanel.SetBool("show", true);
	}
	private void Update() {
		Timer+=Time.deltaTime;
	}
}
