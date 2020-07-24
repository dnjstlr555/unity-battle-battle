using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

//troop class so we can build different troops/characters
[System.Serializable]
public class Troop{
	public GameObject deployableTroops;
	public int troopCosts;
	public Sprite buttonImage;
	
	[HideInInspector]
	public GameObject button;
}

public class UnitInspect {
	public Unit UnitScript;
	public AgentScript AgentScript;
	public int lives=0;
	public bool isScriptValid() {
		return (AgentScript!=null || UnitScript!=null);
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
	public bool setScriptsFrom(GameObject obj) {
		UnitScript = (obj.GetComponent<Unit>()!=null) ? obj.GetComponent<Unit>() : null;
		AgentScript = (obj.GetComponent<AgentScript>()!=null) ? obj.GetComponent<AgentScript>() : null;
		return isScriptValid();
	}
	public bool setEnable(bool t) {
		if(this.isScriptValid()) {
			if(AgentScript && !UnitScript) {
				AgentScript.enabled=t;
				return true;
			} else if(UnitScript && !AgentScript) {
				UnitScript.enabled=t;
				return true;
			} else {
				//what?
			}
			return false;
		} else {
			return false;
		}
	}
	public bool setSpread(bool t) {
		if(this.isScriptValid()) {
			if(AgentScript && !UnitScript) {
				AgentScript.spread=t;
				return true;
			} else if(UnitScript && !AgentScript) {
				UnitScript.spread=t;
				return true;
			} else {
				//what?
			}
			return false;
		} else {
			return false;
		}
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
}


public class GameSystem : MonoBehaviour {
	
	//variables visible in the inspector
	[Header("Objects:")]
	public Animator leftPanelAnimator;
	public Animator endPanel;
	public Animator buttonsAnimator;
	public Animator gamePanel;
	public Animator grid;
	public Animator transition;
	public Animator cameraAnimator;
	
	[Space(5)]
	public GameObject characterPanel;
	public GameObject button;
	public GameObject indicator;
	public GameObject characterStatsPanel;
	public GameObject topDownMapPanel;
	public GameObject gridCell;
	public GameObject gridButton;
	
	[Space(5)]
	public Image eraseButton;
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
	public Text gridButtonText;
	
	[Space(5)]
	public Dropdown speedSetting;
	
	[Space(5)]
	public Transform gridPanel;
	public Transform gridArrow;
	
	[Header("Troops:")]
	public List<Troop> troops;
	public int enemyNumber, knightNumber;
	public const int initEnemyNumber=1, initKnightNumber=2; //need to be implemented
	//not visible in the inspector
	private int selected;
	private GameObject currentDemoCharacter;
	private int rotation = -90;
	public List<GameObject> placedUnits = new List<GameObject>();
	
	private bool erasing;
	private Color eraseStartColor;
	private int coins;
	public bool battleStarted;
	private bool erasingUsingKey;
	private LevelData levelData;
	private bool characterStats;
	private Vector3 gridCenter;
	private GameObject border;
	
	private bool mobile;
	private int gridSize;
	private bool EpisodeEnded=false;
	private bool IsEditingMode=false;
	private UnitInspect inspector = new UnitInspect();
	void Awake(){
		knightNumber=0;
		enemyNumber=0;
		//get the level data object and check if we're using mobile controls
		levelData = Resources.Load("Level data") as LevelData;
		mobile = (GameObject.FindObjectOfType<CamJoystick>() != null);
		EpisodeEnded=false;
		if(mobile){
			//if the game has mobile controls, enable the grid, update the button text and don't show the erase button since it doesn't work with the 2D grid
			levelData.grid = true;
			gridButtonText.text = "3D view";
			grid.SetBool("show", true);
			eraseButton.gameObject.SetActive(false);
		}
		
		//double the grid size so it's always even
		gridSize = levelData.gridSize * 2;
		
		//get the grid center by taking the opposite of the the enemy army position
		gridCenter = GameObject.FindObjectOfType<EnemyArmy>().gameObject.transform.position;
		gridCenter = new Vector3(-gridCenter.x, gridCenter.y, gridCenter.z);
		
		//if we're using the grid, create a 3D border and a 2D grid
		if(levelData.grid){
			createBorder();
			initializeGrid();
		}
		else{
			gridButton.SetActive(false);
		}
		
		//if the level exists, show some level info, else load the end screen
		if(PlayerPrefs.GetInt("level") >= levelData.levels.Count){
			//SceneManager.LoadScene("End screen");
			print("Invalid level data, Load default level - " + PlayerPrefs.GetInt("level"));
			PlayerPrefs.SetInt("level", 0);
		}
		else{
			levelInfo.text = "Level " + (PlayerPrefs.GetInt("level") + 1) + " - " + levelData.levels[PlayerPrefs.GetInt("level")].scene;
		}
	}
	
	//create the 3d border for grid mode
	void createBorder(){
		//get the border start position
		Vector3 borderStart = gridCenter + new Vector3(-gridSize, 100, -gridSize);
		//store the current border position (to use during the loop)
		Vector3 current = borderStart;
		
		//create a new gameobject to store the border
		border = new GameObject();
		border.transform.position = gridCenter;
		border.name = "3D grid Border";
		
		//loop through both axis
		for(int z = 0; z <= gridSize; z++){
			for(int x = 0; x <= gridSize; x++){
				//get the edge of the square to place the border
				if(z == 0 || z == gridSize || x == 0 || x == gridSize){
					//store the hit
					RaycastHit hit;
					
					//if there's a terrain at this position..
					if(Physics.Raycast(current, -Vector3.up, out hit)){
						//create a new border object
						GameObject borderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
						//parent it to the main border object and position it correctly
						borderPoint.transform.SetParent(border.transform, false);
						borderPoint.transform.position = hit.point;
						//remove the collider and change the border material
						Destroy(borderPoint.GetComponent<Collider>());
						Material mat = borderPoint.GetComponent<Renderer>().material;
						mat.shader = Shader.Find("Unlit/UnlitAlphaWithFade");
						mat.color = levelData.borderColor;
						
						if((z == 0 || z == gridSize) && (x == 0 || x == gridSize)){
							//square object for the corners
							borderPoint.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
						}
						else{
							//rectangle for the sides
							if(z == 0 || z == gridSize){
								borderPoint.transform.localScale = new Vector3(0.7f, 0.2f, 0.1f);
							}
							else{
								borderPoint.transform.localScale = new Vector3(0.1f, 0.2f, 0.7f);
							}
						}
					}
				}
				
				//change the current border position on the x axis
				current = new Vector3(current.x + 2, current.y, current.z);
			}
			
			//change the z axis for the current border position
			current = new Vector3(borderStart.x, current.y, current.z + 2);
		}
	}
	
	//create the 2d grid
	void initializeGrid(){
		//find the grid layout group component
		GridLayoutGroup gridGroup = gridPanel.GetComponent<GridLayoutGroup>();
		
		//calculate the spacing and cell size based on the grid size
		gridGroup.cellSize = new Vector2(400f/gridSize, 400f/gridSize);
		gridGroup.spacing = new Vector2(2.6f/(gridSize * 1.05f) * 20f, 2.6f/(gridSize * 1.1f) * 20f);
		
		//loop through all cells
		for(int i = 0; i < (gridSize * gridSize); i++){
			//create the cell and parent it to the grid
			GameObject cell = Instantiate(gridCell);
			cell.transform.SetParent(gridPanel, false);
			cell.transform.GetChild(0).gameObject.SetActive(false);
			
			//set the cell name to its index
			cell.transform.name = "" + i;
			
			//add a onclick function to the cell
			cell.GetComponent<Button>().onClick.AddListener(
			() => { 
				gridClick(int.Parse(cell.transform.name), cell); 
			}
			);
		}
		
		//place the red arrow at the bottom of the grid hierarchy so it doesn't change the place index
		gridArrow.SetSiblingIndex(gridSize * gridSize);
	}
	
	void Start(){
		//get the erase button color and store it
		eraseStartColor = eraseButton.color;
		characterStats = true;
		switchPanelContent(false);
		
		characterStatsPanel.SetActive(false);
		topDownMapPanel.SetActive(true);
		if(IsEditingMode) {
			//show the character buttons in the left panel
			StartCoroutine(addCharacterButtons());
			
			//get the coins for this level and show them
			//coins = levelData.levels[PlayerPrefs.GetInt("level")].playerCoins;
			//coinsText.text = coins + "";
			
			//initialize some boolean values
			
			//setStats(0);
			selected=4;
			/*
			placeUnit(new Vector3(3.2f,0,-9.7f), false);
			placeUnit(new Vector3(3.0f,0.0f,2.9f), false);
			startBattle();
			*/
		} else {
			//placeholder
			//instant transition will be.
			leftPanelAnimator.SetBool("hide instant", true);
			selected=4;
			placeUnit(new Vector3(3.2f,0,-9.7f), false);
			placeUnit(new Vector3(3.0f,0.0f,2.9f), false);
			startBattle();
		}
	}

	void Update(){
		//if the battle has started
		if(battleStarted){
			knightNumber=0;
			enemyNumber=0;

			GameObject[] Knights = GameObject.FindGameObjectsWithTag("Knight");
			GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
			for(int i=0;i<Knights.Length;i++) {
				if(inspector.setScriptsFrom(Knights[i]) && !inspector.isDead()) {
					knightNumber+=1;
				} else {
					//What are you?
				}
			}
			for(int i=0;i<Enemies.Length;i++) {
				if(inspector.setScriptsFrom(Enemies[i]) && !inspector.isDead()) {
					enemyNumber+=1;
				} else {
					//What are you?
				}
			}
			if(knightNumber <= 0){
				if(IsEditingMode) {
					endPanel.SetTrigger("defeat");
				}
				print("HI!!!");
				endGame();
			}
			else if(enemyNumber <= 0){
				if(IsEditingMode) {
					endPanel.SetTrigger("victory");
					
				}
				//PlayerPrefs.SetInt("level" + (PlayerPrefs.GetInt("level") + 1), 1); //won flag for each levels
				//PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1); //next level
				endGame();
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
		else if(GameObject.FindGameObjectsWithTag("Knight").Length == 0){
			battleIndicator.fillAmount -= Time.deltaTime * 0.5f;
		}
		else if(GameObject.FindGameObjectsWithTag("Enemy").Length == 0){
			battleIndicator.fillAmount += Time.deltaTime * 0.5f;
		}
		
		//don't update the preview character on mobile devices since it uses the 2d grid	
		//remove the demo character when hiding the left character panel
		//clear: mobile, battleStarted, activeSelf	
		if(mobile || battleStarted || !leftPanelAnimator.gameObject.activeSelf){
			if(currentDemoCharacter)
				Destroy(currentDemoCharacter);
			//return so it will not use the demo
			return;
		}
		if(leftPanelAnimator.GetBool("hide panel")) {
			//placeholder
		}
		
		
		//check for the x key to erase characters
		if((Input.GetKeyDown("x") && !erasing) || (Input.GetKeyUp("x") && erasingUsingKey)){
			erasingUsingKey = !erasingUsingKey;
			erasingMode();
		}
		
		//if there is a demo character on the battlefield
		if(currentDemoCharacter){
			//get the position of the mouse relative to the terrain
			Vector3 position = getPosition();
			
			//move the demo with the mouse 
			currentDemoCharacter.transform.position = position;
			
			//if we're not currently erasing characters
			if(!erasing){	
				//use right mouse button to rotate the character			
				if(Input.GetMouseButtonDown(1)){
					rotation += levelData.rotationStep;
					updateRotation(currentDemoCharacter);
				}
				
				//place a unit when the left mouse button is down
				if(Input.GetMouseButton(0) && position.x > 0) {
					placeUnit(position, false);
					print(position);
				}
				//get a color for the demo character and change it based on the validity of the current mouse position
				Color color = Color.white;
				if(unitsInRange(position) != null || position.x < 0 || Vector3.Distance(Camera.main.transform.position, position) > levelData.placeRange || !withinGrid(position)){ //|| troops[selected].troopCosts > coins
					color = levelData.invalidPosition;
				}
				else{
					color = levelData.tileColor;
				}
				
				//change the indicator color
				foreach(Renderer renderer in currentDemoCharacter.transform.Find("Indicator(Clone)").GetComponentsInChildren<Renderer>()){
					renderer.material.color = color;
				}
			}
			else if(Input.GetMouseButton(0) || erasingUsingKey){
				//if we're erasing, check for left mouse button to erase units/characters
				eraseUnit(position, false, false);
			}
			
			//if the demo character is not playing idle animations, make sure to play idle animations on all of its animators
			if(currentDemoCharacter.activeSelf && currentDemoCharacter.GetComponent<Animator>() && currentDemoCharacter.GetComponent<Animator>().GetBool("Start") != false){
				foreach(Animator animator in currentDemoCharacter.GetComponentsInChildren<Animator>()){
					animator.SetBool("Start", false);
				}
			}
		}
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
		int knightsLeft = GameObject.FindGameObjectsWithTag("Knight").Length;
		int enemiesLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
		int total = knightsLeft + enemiesLeft;
		
		return (float)knightsLeft/(float)total;
	}
	
	//change erasing mode
	public void erasingMode(){
		erasing = !erasing;
		
		if(erasing){
			//if we're erasing, don't display a character
			if(currentDemoCharacter)
				Destroy(currentDemoCharacter); 
			
			//instead of the character, just show the red tile
			currentDemoCharacter = newTile(levelData.removeColor);
			eraseButton.color = levelData.eraseButtonColor;
		}
		else{
			//if we're not erasing anymore, create a new demo character
			changeDemo();
			eraseButton.color = eraseStartColor;
		}
	}
	
	//place a new unit
	public void placeUnit(Vector3 position, bool placingGridCell){
		//check if the position is valid
		if(canPlace(position, placingGridCell)){
			//create a new unit/character and prevent it from moving
			GameObject unit = Instantiate(troops[selected].deployableTroops, position, Quaternion.identity);
			disableUnit(unit);
			
			//set the correct rotation
			updateRotation(unit);
			
			//add it to the list of placed units
			placedUnits.Add(unit);
			
			//decrease the number of coins left
			coins -= troops[selected].troopCosts;
			coinsText.text = coins + "";
			
			//if we're using the grid system, update the grid by enabling the cell that corresponds with this position
			if(levelData.grid){
				GameObject cell = gridPanel.transform.GetChild(positionToGridIndex(position)).GetChild(0).gameObject;
				cell.SetActive(true);
				cell.GetComponent<Image>().sprite = troops[selected].buttonImage;
			}
		}
	}
	
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
	
	//called when you click anywhere in the grid
	public void gridClick(int clickedIndex, GameObject cell){
		//get the 3d position of this click
		Vector3 position = gridIndexToPosition(clickedIndex);
		//if there's a unit already, remove it. Else, add a new one
		if(cell.transform.GetChild(0).gameObject.activeSelf){
			eraseUnit(position, false, true);
		}
		else{
			placeUnit(position, true);
		}
	}
	
	public void eraseUnit(Vector3 position, bool clearing, bool erasingGridCell){
		//get the unit to erase
		GameObject unit = unitsInRange(position);
		
		//check if the unit exists and if it's not an enemy
		if(unit != null && unit.name.Length - 7 > 0 && unit.gameObject.tag != "Enemy" && (!EventSystem.current.IsPointerOverGameObject() || clearing || erasingGridCell)){
			if(!clearing)
				placedUnits.Remove(unit);
			
			//remove the unit
			Destroy(unit);
			
			//give the player back his coins
			coins += troops[unitIndex(unit)].troopCosts;
			coinsText.text = coins + "";
			
			//if we're using the grid, clear this cell
			if(levelData.grid){
				GameObject cell = gridPanel.transform.GetChild(positionToGridIndex(position)).GetChild(0).gameObject;
				cell.SetActive(false);
			}
		}
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
		Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
		
		//foreach unit, check if it's in range and return as soon as one of them is
		foreach(Unit unit in allUnits){
			if(Vector3.Distance(unit.gameObject.transform.position, position) < levelData.checkRange && unit.gameObject != currentDemoCharacter)
				return unit.gameObject;
		}
		
		//after checking all units, return null
		return null;
	}
	
	//hide or show the panel on the left
	public void showHideLeftPanel(){
		leftPanelAnimator.SetBool("hide panel", !leftPanelAnimator.GetBool("hide panel"));
		
		if(!mobile){
			if(!leftPanelAnimator.GetBool("hide panel"))
				changeDemo();
		}
		else{
			grid.SetBool("show", !leftPanelAnimator.GetBool("hide panel"));
		}
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
		
		//update the demo character
		if(!mobile)
			changeDemo();
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
		
		//update the demo character
		if(!mobile)
			changeDemo();
		
		eraseButton.color = Color.white;
		
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
				inspector.setSpread(levelData.spreadUnits);
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
	
	public void openLevel(){
		//wait for the fade transition to end
		//transition.SetTrigger("fade");
		
		//check if the next level exist and load it if it does
		if(PlayerPrefs.GetInt("level") < levelData.levels.Count){
			SceneManager.LoadScene(levelData.levels[PlayerPrefs.GetInt("level")].scene);
		}
		else{
			print("Tried to load invalid level in openlevel()" + PlayerPrefs.GetInt("level"));
			//SceneManager.LoadScene("End screen");
		}
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
	
	public void clear(){	
		//go through all units on the battlefield	
		for(int i = 0; i < placedUnits.Count; i++){
			eraseUnit(placedUnits[i].transform.position, true, false);
		}
		
		//clear the unit list and shake the camera
		placedUnits.Clear();
		cameraAnimator.SetTrigger("shake");
	}
	
	public void startBattle(){
		//enable all units so they start fighting
		/*
		initEnemyNumber=GameObject.FindGameObjectsWithTag("Enemy").Length;
		initKnightNumber=GameObject.FindGameObjectsWithTag("Knight").Length;
		*/
		print(initEnemyNumber+" "+initKnightNumber);
		foreach(GameObject ally in placedUnits){
			enableUnit(ally);
		}
		if(FindObjectOfType<EnemyArmy>().IsPlaced()) {
			FindObjectOfType<EnemyArmy>().startEnemies();
		} else {
			print("Coudln't start battle beacuse the enemy didn't spawn");
			return;
		}
		//show the new UI
		StartCoroutine(battleUI());
		battleStarted = true;
		
		//destroy the border object
		if(border != null)
			Destroy(border);
	}
	
	public void endGame(){
		//end the battle
		battleStarted = false;
		gamePanel.SetBool("show", false);
		//do something
		if(!IsEditingMode) {
			EpisodeEnded=true;
			openLevel();
		}
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
	
	public void showGrid(){
		//show or hide the grid and change the button text
		if(!mobile){
			if(gridButtonText.text == "Grid Layout"){
				gridButtonText.text = "Default 3D Layout";
			}
			else{
				gridButtonText.text = "Grid Layout";
			}
		
			grid.SetBool("show", !grid.GetBool("show"));
		}
		else{
			showHideLeftPanel();
		}
	}
	
	IEnumerator battleUI(){
		//hide the character panel
		/*
		if(!leftPanelAnimator.GetBool("hide panel"))
			if(!leftPanelAnimator.GetBool("hide instant") && !IsEditingMode)
				leftPanelAnimator.SetBool("hide instant", true);
			else
				leftPanelAnimator.SetBool("hide panel", true);
		*/
		//hide the grid
		grid.SetBool("show", false);
		leftPanelAnimator.gameObject.SetActive(false);
		if(IsEditingMode) {
			buttonsAnimator.SetBool("hide", true);
		} else {
			buttonsAnimator.gameObject.SetActive(false);
			foreach(GameObject UI in GameObject.FindGameObjectsWithTag("Editory")) {
				UI.SetActive(false);
			}
		}

		//wait a moment and remove the panels
		yield return new WaitForSeconds(0.5f);
		//show the game panel
		gamePanel.SetBool("show", true);
	}
}
