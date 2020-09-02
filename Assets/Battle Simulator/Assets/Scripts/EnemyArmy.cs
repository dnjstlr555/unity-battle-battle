using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmy : MonoBehaviour {
	
	public bool IsEnemyAllPlaced = false;
	//not visible in the inspector
	private LevelData levelData;
	private GameSystem characterPlacement;
	private int level;
	private UnitInspect inspector;
	public void Academy_Initialize() {
		print("Initializing Enemy System");
		characterPlacement = GameObject.FindObjectOfType<GameSystem>();

		levelData = Resources.Load("Level data") as LevelData;
		level = PlayerPrefs.GetInt("level");
		inspector=new UnitInspect(characterPlacement);
	}
	///<summary>Spawning enemy army</summary>
	public void Academy_Start () {
		print("Enemy army spawning");
		IsEnemyAllPlaced = false;
		//spawn enemies if this level exists
		if(level < levelData.levels.Count)
			spawnEnemies(level);
	}
	
	void spawnEnemies(int levelIndex){
		//get the gridsize and the space in between grid cells
		int levelGridSize = levelData.levels[levelIndex].gridSize;
		int sizeGrid = 2;
		
		//find the 3d start position for the grid
		Vector3 startPosition = new Vector3(transform.position.x + ((float)sizeGrid * ((float)levelGridSize/2f)), 100, transform.position.z + ((float)sizeGrid * ((float)levelGridSize/2f)));
		int currentPosition = 0;
		
		//for each position in the grid
		for(int x = 0; x < levelGridSize; x++){
			for(int z = 0; z < levelGridSize; z++){
				//get the 3d position and the unit for that position
				Vector3 position = new Vector3(startPosition.x - ((float)x * sizeGrid + Random.Range(-5.0f, 5.0f)), startPosition.y, startPosition.z - ((float)z * sizeGrid + Random.Range(-5.0f, 5.0f)));
				GameObject unit = levelData.levels[levelIndex].units[currentPosition];
				
				if(unit != null){
					//if there is a unit/character, spawn it and wait a moment for the spawn effect
					spawnNew(position, unit);
					//yield return new WaitForSeconds(levelData.spawnDelay);
				}
				
				//increase the current position index
				currentPosition++;
			}
		}
		IsEnemyAllPlaced=true;
	}
	public bool IsPlaced() {
		return IsEnemyAllPlaced;
	}
	//spawn a new enemy
	public void spawnNew(Vector3 position, GameObject unit){
		//store the raycast hit
		RaycastHit hit;
		
		if(Physics.Raycast(position, -Vector3.up, out hit)){
			//if the raycast hits a terrain, spawn a unit at the hit point
			GameObject newUnit = Instantiate(unit, hit.point, Quaternion.Euler(0, 90, 0));
			inspector.addFrom(newUnit);
		} else {
			Debug.LogWarning("Couldn't spawn enemy");
		}
	}
	public void initEnemies(){
		foreach(GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy")){
			Destroy(enemyUnit);
		}
	}
}
