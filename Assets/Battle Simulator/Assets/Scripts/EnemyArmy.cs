﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmy : MonoBehaviour {
	
	public bool IsEnemyAllPlaced = false;
	//not visible in the inspector
	private LevelData levelData;
	private GameSystem characterPlacement;
	private int level;
	private UnitInspect inspector;
	private System.Random rnd = new System.Random();
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
				Vector3 position = new Vector3(startPosition.x - ((float)x * sizeGrid), startPosition.y, startPosition.z - ((float)z * sizeGrid));
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
		Vector3 pos=position;
		RaycastHit hit;
		bool isInstantiated=false;
		for(int i=0;i<6;i++) {
			if(Physics.Raycast(pos, -Vector3.up, out hit)){
				Debug.Log($"Spawn Point:{hit.point}/{hit.collider.gameObject.tag}");
				if(hit.collider.gameObject.CompareTag("Battle ground")) {
					//if the raycast hits a terrain, spawn a unit at the hit point
					GameObject newUnit = Instantiate(unit, hit.point, Quaternion.Euler(0, 90, 0));
					inspector.addFrom(newUnit);
					isInstantiated=true;
					break;
				} else {
					int sign = rnd.Next(0, 2) * 2 - 1;
					int sign2 = rnd.Next(0, 2) * 2 - 1;
					pos.x+=2.4f*sign;
					pos.z+=2.4f*sign2;
				}
			}
		}
		if(!isInstantiated) {
			Debug.LogError("Couldn't instantiated enemy");
		}
	}
	public void initEnemies(){
		foreach(GameObject enemyUnit in GameObject.FindGameObjectsWithTag("Enemy")){
			Destroy(enemyUnit);
		}
	}
}
