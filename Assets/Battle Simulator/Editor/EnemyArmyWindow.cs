using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class EnemyArmyWindow : EditorWindow {
	
	public int levelIndex;
	
	private EnemyArmyLevel enemyArmyLevel;
	private List<Object> enemies;
	private int selectedEnemy;
	private Vector2 scrollPos;
	private Vector2 windowPos;
	private GUISkin skin;
	private LevelData levelData;
	
	void OnEnable(){
		levelData = Resources.Load("Level data") as LevelData;
		levelIndex = levelData.armyToEdit;
		enemyArmyLevel = levelData.levels[levelIndex];
		
		enemies = Resources.LoadAll("Enemies").ToList();
		skin = (GUISkin)Resources.Load("Enemy army editor skin") as GUISkin;
	}

    void OnGUI(){
		windowPos = EditorGUILayout.BeginScrollView(windowPos, false, false);
		GUILayout.BeginHorizontal();
		GUILayout.BeginHorizontal("Box");
		GUILayout.Label("Enemy army configuration for level " + (levelIndex + 1), EditorStyles.largeLabel);
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("Grid size", GUILayout.Height(28))){
			GridSizePopup window = ScriptableObject.CreateInstance<GridSizePopup>();
			window.enemyArmyLevel = enemyArmyLevel;
			window.position = new Rect(position.x + position.width + 10, position.y, 250, 150);
			window.ShowPopup();
		}
		GUILayout.EndHorizontal();
		
		if(enemyArmyLevel.gridSize == 0){
			EditorGUILayout.HelpBox("Please choose a grid size for this enemy army (with a bigger grid you can add more units)", MessageType.Info);
			return;
		}
		
		GUI.color = new Color(0, 0, 0, 0.2f);
		GUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("PLAYER ARMY SIDE", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		int index = 0;
		
		for(int i = 0; i < enemyArmyLevel.gridSize; i++){
			GUILayout.BeginHorizontal();
			
			for(int I = 0; I < enemyArmyLevel.gridSize; I++){
				if(enemyArmyLevel.units[index] == null){
					if(GUILayout.Button("", skin.box, GUILayout.Height((position.height - 170 - (3 * (enemyArmyLevel.gridSize - 1)))/(float)enemyArmyLevel.gridSize))){
						if(selectedEnemy != -1){
							enemyArmyLevel.units[index] = (GameObject)enemies[selectedEnemy] as GameObject;
						}
						else{
							enemyArmyLevel.units[index] = null;
						}
					}
				}
				else{
					Texture2D preview = null;
					
					if(!levelData.customImages){
						preview = AssetPreview.GetAssetPreview(enemyArmyLevel.units[index]);
					}
					else{
						preview = levelData.customEnemyImages[enemies.IndexOf(enemyArmyLevel.units[index])];
					}
					
					if(GUILayout.Button(preview, skin.GetStyle("image"), GUILayout.Width((position.width - (5 * (enemyArmyLevel.gridSize + 1)))/enemyArmyLevel.gridSize), GUILayout.Height((position.height - 170 - (3 * (enemyArmyLevel.gridSize - 1)))/(float)enemyArmyLevel.gridSize))){
						if(selectedEnemy != -1){
							enemyArmyLevel.units[index] = (GameObject)enemies[selectedEnemy] as GameObject;
						}
						else{
							enemyArmyLevel.units[index] = null;
						}
					}
				}
				
				index++;
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		
		GUILayout.BeginHorizontal();
		GUI.color = new Color(0, 0, 0, 0.2f);
		GUILayout.BeginVertical("Box", GUILayout.Width(160));
		GUI.color = Color.white;
		
		GUILayout.Space(4);
		GUILayout.Label("Enemy count: " + enemyCount(), EditorStyles.largeLabel);
		
		GUILayout.Space(10);
		GUILayout.Label("Player coins:");
		enemyArmyLevel.playerCoins = EditorGUILayout.IntField(enemyArmyLevel.playerCoins);
		GUILayout.EndVertical();
		
		GUI.color = new Color(0, 0, 0, 0.2f);
		GUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		GUILayout.BeginHorizontal();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, false, skin.horizontalScrollbar, GUIStyle.none, GUIStyle.none, GUILayout.Width(position.width - 400), GUILayout.Height(70));
		GUILayout.BeginHorizontal();
		
		int eraseButtonSize = 45;
			
		if(selectedEnemy == -1)
			eraseButtonSize = 50;
			
		if(GUILayout.Button("", skin.GetStyle("eraser"), GUILayout.Width(eraseButtonSize), GUILayout.Height(eraseButtonSize)))
			selectedEnemy = -1;
		
		GUILayout.Space(2);
		
		for(int j = 0; j < enemies.Count; j++){
			Texture2D enemyTexture = null;
			
			if(!levelData.customImages){
				enemyTexture = AssetPreview.GetAssetPreview(enemies[j]);
			}
			else{
				enemyTexture = levelData.customEnemyImages[j];
			}
			
			if(selectedEnemy == j){
				GUI.color = Color.white;
			}
			else{
				GUI.color = new Color(1, 1, 1, 0.5f);
			}
			
			int size = 45;
			
			if(selectedEnemy == j)
				size = 50;
			
			if(GUILayout.Button(enemyTexture, GUIStyle.none, GUILayout.Width(size), GUILayout.Height(size)))
				selectedEnemy = j;
			
			GUILayout.Space(5);
		}
		GUI.color = Color.white;
		
		GUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
		
		GUILayout.BeginVertical();
		GUILayout.Space(-0.1f);
		GUI.color = new Color(1, 1, 1, 0.25f);
		GUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		GUILayout.Label("Selected enemy:");
		
		if(selectedEnemy != -1){
			GameObject enemy = (GameObject)enemies[selectedEnemy] as GameObject;
			GUILayout.Label(enemy.name, EditorStyles.largeLabel);
		}
		else{
			GUILayout.Label("Erasing", EditorStyles.largeLabel);
		}
		
		GUILayout.Space(25);
		GUILayout.EndVertical();
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		
		EditorGUILayout.EndScrollView();
		
		EditorUtility.SetDirty(levelData);
    }
	
	int enemyCount(){
		int units = 0;
		
		for(int i = 0; i < enemyArmyLevel.units.Count; i++){
			if(enemyArmyLevel.units[i] != null)
				units++;
		}
		
		return units;
	}
}

public class GridSizePopup : EditorWindow{
	
	public EnemyArmyLevel enemyArmyLevel;
	
	private int size;

    void OnGUI(){
		GUI.color = new Color(0, 0.6f, 0.9f, 0.3f);
		GUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		GUILayout.Space(5);
		GUILayout.Label("New grid size:");
		size = EditorGUILayout.IntField(size);
		
		if(enemyArmyLevel.gridSize != 0)
			EditorGUILayout.HelpBox("Changing the army grid size will remove the current army", MessageType.Warning);
		
        GUILayout.Space(10);
        if(GUILayout.Button("Set grid size")) 
			Apply();
		
		if(GUILayout.Button("Cancel"))
			this.Close();
		
		GUILayout.EndVertical();
    }
	
	public void Apply(){
		if(enemyArmyLevel.units != null)
			enemyArmyLevel.units.Clear();
		
		enemyArmyLevel.units = new List<GameObject>();
		
		for(int i = 0; i < (size * size); i++){
			enemyArmyLevel.units.Add(null);
		}
		
		enemyArmyLevel.gridSize = size;
		this.Close();
	}
}
