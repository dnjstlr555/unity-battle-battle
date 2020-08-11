using System.Collections;
using System;
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
	public Unit UnitScript {get; set;}
	public AgentScript AgentScript {get; set;}
	public GameSystem sys {get; set;}
	public UnitInspect(GameSystem GameSystem) {
		this.sys=GameSystem;
	}
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
	public string getScriptType() {
		return(AgentScript!=null)?"AgentScript":((UnitScript!=null)?"Unit":null);
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
    public GameObject[] getCurrentUnits() {
		GameObject[] k = getCurrentKnights();
		GameObject[] e = getCurrentEnemys();
		int lk = k.Length;
		int le = e.Length;
		Array.Resize<GameObject>(ref k, lk + le);
		Array.Copy(e, 0, k, lk, le);
        return k;
    }
	public GameObject[] getCurrentKnights() {
        return sys.knightUnits;
    }
	public GameObject[] getCurrentEnemys() {
        return sys.enemyUnits;
	}
	public GameObject[] getInstantiatedKnights() {
		return GameObject.FindGameObjectsWithTag("Knight");
	}
	public GameObject[] getInstantiatedEnemys() {
		return GameObject.FindGameObjectsWithTag("Enemy");
	}
	public GameObject[] getInstantiatedUnits() {
		GameObject[] k = getInstantiatedKnights();
		GameObject[] e = getInstantiatedEnemys();
		int lk = k.Length;
		int le = e.Length;
		Array.Resize<GameObject>(ref k, lk + le);
		Array.Copy(e, 0, k, lk, le);
        return k;
	}
	public GameObject[] getUnitsByTag(String tag) {
		return (tag=="Knight")?getCurrentKnights():((tag=="Enemy")?getCurrentEnemys():null);
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
    public void setDead() {
        if(this.isScriptValid() && !this.isDead()) {
            if(AgentScript) {
                AgentScript.die();
            } else if(UnitScript) {
                UnitScript.die();
            } else {
                Debug.LogError("Couldn't setDead() because the script was valid");
            }
        } else {
            Debug.LogError("Couldn't setDead() because the script was valid");
        }
    }
	public void setInitAgent(MLAgents.Brain brain) {
		if(this.isScriptValid()) {
			AgentScript.GiveBrain(brain);
			//AgentScript.AgentReset();
		}
	}
	public void AgentDescisionRequest() {
		//unused
		if(this.isScriptValid() && this.getScriptType()=="AgentScript") {
			AgentScript.AgentDescisionRequest();
		}
	}
    public void AgentDone() {
        if(this.isScriptValid() && this.getScriptType()=="AgentScript") {
            AgentScript.Done();
        }
    }
	public void AgentAlwaysUpdate() {
		if(this.isScriptValid() && this.getScriptType()=="AgentScript") {
            AgentScript.AgentAlwaysUpdateInternal();
			AgentScript.AgentAlwaysUpdate();
        }
	}
	public float AvgLives(GameObject[] objs) {
		float sum=0;
		foreach(GameObject obj in objs) {
			if(setScriptsFrom(obj)) {
				if(!isDead()) sum+=getLives();
			}
		}
		return sum/objs.Length;
	}
	public void removeFrom(GameObject unit) {
		if(sys==null) throw new Exception("Use of removeFrom without assigning GameSystem to unitinspect");
		List<GameObject> a=new List<GameObject>{};
		if(unit.tag=="Knight") {
			a.AddRange(sys.knightUnits);
		} else if (unit.tag=="Enemy") {
			a.AddRange(sys.enemyUnits);
		} else {
			Debug.LogError("Unknown unit passed to removeFrom");
			return;
		}
		a.Remove(unit);
		sys.knightUnits=(unit.tag=="Knight")?a.ToArray():sys.knightUnits;
		sys.enemyUnits=(unit.tag=="Enemy")?a.ToArray():sys.enemyUnits;
	}
	
}
