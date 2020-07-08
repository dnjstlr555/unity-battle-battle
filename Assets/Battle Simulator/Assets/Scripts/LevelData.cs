using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//enemy army class that holds variables for the enemies of each level
[System.Serializable]
public class EnemyArmyLevel{
	public int gridSize;
	public List<GameObject> units;
	public int playerCoins;
	public string scene;
}

public class LevelData : ScriptableObject {

	[HideInInspector]
	public List<EnemyArmyLevel> levels;
	[HideInInspector]
	public int armyToEdit;
	[HideInInspector]
	public List<Texture2D> customEnemyImages;
	
	//editor variables
	public bool customImages;
	
	[Space(10)]
	public float spawnDelay;
	public bool grid;
	
	[Space(5)]
	public Color buttonHighlight;
	public Color tileColor;
	public Color invalidPosition;
	public Color removeColor;
	public Color eraseButtonColor;
	public Color selectedPanelColor;
	public Color borderColor;
	
	[Space(5)]
	public float demoCharacterAlpha;
	public float buttonEffectTime;
	public float checkRange;
	
	[Space(5)]
	public int rotationStep;
	public int placeRange;
	public int gridSize;
	
	[Space(5)]
	public KeyCode snappingKey;
	
	[Space(5)]
	public bool spreadUnits;
}
