using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour {
	
	//reference to the character placement script so it can select a troop on click
	CharacterPlacement characterPlacer;
	
	void Start () {
		//find the character placement script
		characterPlacer = GameObject.FindObjectOfType<CharacterPlacement>();
	}
	
	public void Select(){
		//select the correct troop/unit via the character placement script
		if(characterPlacer != null)
			characterPlacer.selectTroop(int.Parse(transform.name)); 
	}
}
