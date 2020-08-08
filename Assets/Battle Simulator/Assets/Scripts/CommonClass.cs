using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Troop{
    ///Troops class for setting a troop
	public GameObject deployableTroops;
	public int troopCosts;
	public Sprite buttonImage;
	
	[HideInInspector]
	public GameObject button;
}

public class UnitInspect {
    ///Unit Inspector
	public Unit UnitScript;
	public AgentScript AgentScript;
	public int lives=0;
	public bool isScriptValid() {
		return (AgentScript!=null || UnitScript!=null);
	}
	public string getType() {
		return(AgentScript!=null)?"AgentScript":((UnitScript!=null)?"Unit":null);
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
		try {
			UnitScript = (obj.GetComponent<Unit>()!=null) ? obj.GetComponent<Unit>() : null;
			AgentScript = (obj.GetComponent<AgentScript>()!=null) ? obj.GetComponent<AgentScript>() : null;
		} catch {
			return false;
		}
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
	public void setLives(float hp) {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				AgentScript.lives=hp;
			} else if(UnitScript && !AgentScript) {
				UnitScript.lives=hp;
			}
		}
	}
	public void setInitAgent(MLAgents.Brain brain) {
		if(this.isScriptValid()) {
			AgentScript.GiveBrain(brain);
			//AgentScript.AgentReset();
		}
	}
	public void AgentDescisionRequest() {
		if(this.isScriptValid()) {
			AgentScript.AgentDescisionRequest();
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
