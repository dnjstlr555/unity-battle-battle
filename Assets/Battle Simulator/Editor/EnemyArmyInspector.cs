using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(LevelData))]
public class EnemyArmyInspector : Editor{
	
	int toolbarSelected = 0;
	LevelData levelData;
	EnemyArmyWindow window;
	Object[] enemies;
	ReorderableList levelList;

	int activeElement;
	
	void OnEnable(){
		levelData = target as LevelData;
		enemies = Resources.LoadAll("Enemies");
		
		if(levelData.customEnemyImages == null)
			levelData.customEnemyImages = new List<Texture2D>();
		
		levelList = new ReorderableList(serializedObject, serializedObject.FindProperty("levels"), true, true, true, true);
		
		levelList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			EditorGUI.LabelField(new Rect(rect.x, rect.y + 2, Screen.width, EditorGUIUtility.singleLineHeight), "Level " + (index + 1) + ": " + levelData.levels[index].scene, EditorStyles.largeLabel);
			
			if(isActive){
				if(activeElement != index && window != null){
					window.Close();
					levelData.armyToEdit = index;
					window = (EnemyArmyWindow)EditorWindow.GetWindow(typeof(EnemyArmyWindow), true, "Enemy army");
					window.minSize = new Vector2(470, 630);
					window.Show();
				}
					
				activeElement = index;
			}
		};	
	
		levelList.elementHeightCallback = (index) => { 
			return EditorGUIUtility.singleLineHeight + 15;
		};
		
		levelList.onAddCallback = (ReorderableList l) => {  
			if(levelData.levels.Count > 0){
				levelData.levels.Add(new EnemyArmyLevel {scene = levelData.levels[levelData.levels.Count - 1].scene});
			}
			else{
				levelData.levels.Add(new EnemyArmyLevel {scene = null});
			}
		};
	
		levelList.onRemoveCallback = (ReorderableList l) => {  
			if(EditorUtility.DisplayDialog("Remove level", "Are you sure you want to remove this level?", "Yes", "No")){
				ReorderableList.defaultBehaviours.DoRemoveButton(l);
				
				if(window != null)
					window.Close();
			}
		};
	
		levelList.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, "Levels (" + levelData.levels.Count + ")");
		};
	}
	
	public override void OnInspectorGUI(){	
	GUILayout.Space(10);
	toolbarSelected = GUILayout.Toolbar(toolbarSelected, new string[] {"Settings", "Levels"}, GUILayout.Height(25));
	
	while(levelData.customEnemyImages.Count != enemies.Length){
		if(levelData.customEnemyImages.Count < enemies.Length){
			levelData.customEnemyImages.Add(null);
		}
		else{
			levelData.customEnemyImages.RemoveAt(levelData.customEnemyImages.Count - 1);
		}
	}
	
	if(toolbarSelected == 0){
		
		if(GUILayout.Button("Delete playerprefs") && EditorUtility.DisplayDialog("Delete PlayerPrefs", "Are you sure you want to delete playerprefs?", "Yes", "Cancel"))
			PlayerPrefs.DeleteAll();
		
		GUILayout.Label("Enemies:", EditorStyles.boldLabel);
		for(int i = 0; i < enemies.Length; i++){
			GUILayout.BeginHorizontal();
			GUILayout.Label("- " + enemies[i].name);
			
			if(levelData.customImages)
				levelData.customEnemyImages[i] = EditorGUILayout.ObjectField(levelData.customEnemyImages[i], typeof(Texture2D), false) as Texture2D;
			
			GUILayout.EndHorizontal();
		}
		
		GUI.color = Color.white;
		
		GUILayout.Space(10);
		DrawDefaultInspector();
	}
	else if(toolbarSelected == 1){
		if(levelData.levels.Count > 0){
			GUILayout.Space(10);
			
			if(GUILayout.Button("Clear all", EditorStyles.toolbarButton) && EditorUtility.DisplayDialog("Clear all levels", "Are you sure you want to clear all levels?", "Yes", "No"))
				levelData.levels.Clear();
		}
		
		serializedObject.Update();
		levelList.DoLayoutList();
		serializedObject.ApplyModifiedProperties();
		
		if(levelData.levels.Count > 0){
			GUILayout.Space(10);
			GUI.color = new Color(0, 0, 0, 0.1f);
			GUILayout.BeginVertical("Box");
			GUI.color = Color.white;
		
			GUILayout.Label("Level " + (activeElement + 1) + " settings", EditorStyles.largeLabel);
			GUILayout.Space(5);
		
			if(activeElement < levelData.levels.Count)
				levelData.levels[activeElement].scene = EditorGUILayout.TextField("Scene", levelData.levels[activeElement].scene);
		
			if(GUILayout.Button("Edit army", GUILayout.Height(25))){
				if(window != null)
					window.Close();
		
				levelData.armyToEdit = activeElement;
				window = (EnemyArmyWindow)EditorWindow.GetWindow(typeof(EnemyArmyWindow), true, "Enemy army");
				window.minSize = new Vector2(470, 630);
				window.Show();
			}
		
			GUILayout.EndVertical();
		}
	}
	
    serializedObject.ApplyModifiedProperties();
	Undo.RecordObject(levelData, "change in enemy army inspector");
	EditorUtility.SetDirty(levelData);
	}
	
	[MenuItem("Edit/Level data")]
    private static void SelectLevelData(){
        Selection.activeObject = Resources.Load("Level data") as LevelData;
    }
}
