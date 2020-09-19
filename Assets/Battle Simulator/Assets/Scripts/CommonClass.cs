using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

[System.Serializable]
public class Troop{
    ///Troops class for setting a troop
	public GameObject deployableTroops;
	public int troopCosts;
	public Sprite buttonImage;
	
	[HideInInspector]
	public GameObject button;
}

public class Reward {
    Queue<float> bag=new Queue<float>();
    public Reward() {
    }
    public void BagReset() {
        bag.Clear();
    }
    public void AddReward(float r) {
        bag.Enqueue(r);
    }
    public void Apply(AgentScript Agent) {
        float allReward=0;
        foreach(float i in bag.ToArray()) {
            allReward+=i;
        }
        Agent.AddReward(allReward);
        BagReset();
    }
    public virtual void RewardAtDie(AgentScript unit) {
    }
    public virtual void RewardAtEpisodeEnds(UnitInspect inspector, GameSystem sys) {
    }
}
public class DebugInfo{
	private UnitInspect i;
	public float initialLives, currentLives, defaultDammage, range, totalDamage, preReward=-1;
	public string attackTag, unitType="None";
	public bool? needAction;
	private ConcurrentQueue<String> prints = new ConcurrentQueue<string>();
	public DebugInfo(UnitInspect inspector) {
		this.i=inspector;
	}
	/// <summary>set Debuginfo from given unit</summary>
	/// <param name="unit">lolsbarry</param>
	public void setFromUnit(GameObject unit) {
		if(i.setScriptsFrom(unit)) {
			initialLives=i.getInitialLives();
			currentLives=i.getLives();
			defaultDammage=i.getDamage();
			attackTag=i.getAttackTag();
			range=i.getRange();
			unitType=i.getScriptType();
			//needAction=i.getNeedAction();
		}
	}
	public static double Sigmoid(double value) {
		double k = Exp(value);
		return k / (1.0f + k);
	}
	public static double Exp(double val) {  
		long tmp = (long) (1512775 * val + 1072632447);  
		return BitConverter.Int64BitsToDouble(tmp << 32);  
	}
}

public class UnitInspect {
    ///Unit Inspector
	public Unit UnitScript {get; set;}
	public AgentScript AgentScript {get; set;}
	public GameObject InnerObject;
	public GameSystem sys {get; set;}
	public CamController cam {get;set;}
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
	public bool isDoneInProgress() {
		if(this.isScriptValid() && getScriptType()=="AgentScript") {
			return AgentScript.onceDone;
		} else {
			return false;
		}
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
	public float getDamage() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.damage;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.damage;
			}
		}
		return -1;
	}
	public float getRange() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.agent.stoppingDistance;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.agent.stoppingDistance;
			}
		}
		return -1f;
	}
	public string getAttackTag() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.attackTag;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.attackTag;
			}
		}
		return "None";
	}
	public bool? getNeedAction() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.needAction;
			}
		}
		return null;
	}
	public float getInitialLives() {
		if(this.isScriptValid()) {
			if(getInitPassed()) {
				if(AgentScript && !UnitScript) {
					return AgentScript.startLives;
				} else if(UnitScript && !AgentScript) {
					return UnitScript.startLives;
				}
			} else {
				if(AgentScript && !UnitScript) {
					return AgentScript.lives;
				} else if(UnitScript && !AgentScript) {
					return UnitScript.lives;
				}
			}
			
		}
		return -1;
	}
	public bool getInitPassed() {
		if(this.isScriptValid()) {
			if(AgentScript && !UnitScript) {
				return AgentScript.isPassedInit;
			} else if(UnitScript && !AgentScript) {
				return UnitScript.isPassedInit;
			}
		}
		return false;
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
	public string getTag() {
		return InnerObject.tag;
	}
	public bool setScriptsFrom(GameObject obj) {
		try {
			UnitScript = (obj.GetComponent<Unit>()!=null) ? obj.GetComponent<Unit>() : null;
			AgentScript = (obj.GetComponent<AgentScript>()!=null) ? obj.GetComponent<AgentScript>() : null;
			InnerObject = obj;
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
				setOnceDone();
				AgentScript.die();
				AgentScript.innerDie();
            } else if(UnitScript) {
                UnitScript.die();
            } else {
                Debug.LogError("Couldn't setDead() because the script was valid");
            }
        } else {
            Debug.LogError("Couldn't setDead() because the script was valid");
        }
    }
	public void setOnceDone() {
		if(this.isScriptValid() && !this.isDead()) {
			if(AgentScript) {
				AgentScript.onceDone=true;
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
		if(this.isScriptValid() && this.getScriptType()=="AgentScript" && !this.isDoneInProgress()) {
            AgentScript.AgentAlwaysUpdateInternal();
			AgentScript.AgentAlwaysUpdate();
        }
	}
	public void AgentSetRewardDirectly(float reward) {
		if(this.isScriptValid() && this.getScriptType()=="AgentScript") {
            AgentScript.SetReward(reward);
        }
	}
	public void AgentAddRewardDirectly(float reward) {
		if(this.isScriptValid() && this.getScriptType()=="AgentScript") {
            AgentScript.AddReward(reward);
        }
	}
	public float AvgLives(GameObject[] objs) {
		float sum=0;
		int cnt=0;
		foreach(GameObject obj in objs) {
			if(setScriptsFrom(obj)) {
				if(!isDead()) {
					sum+=getLives();
					cnt++;
				}
			}
		}
		return (cnt>0)?(float)Math.Round(sum/cnt,3):0;
	}
	public float AvgInitialLives(GameObject[] objs) {
		float sum=0;
		int cnt=0;
		foreach(GameObject obj in objs) {
			if(setScriptsFrom(obj)) {
				if(!isDead()) {
					sum+=getInitialLives();
					cnt++;
				}
			}
		}
		return sum/cnt;
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
	public void addFrom(GameObject unit) {
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
		a.Add(unit);
		sys.knightUnits=(unit.tag=="Knight")?a.ToArray():sys.knightUnits;
		sys.enemyUnits=(unit.tag=="Enemy")?a.ToArray():sys.enemyUnits;
	}
}
