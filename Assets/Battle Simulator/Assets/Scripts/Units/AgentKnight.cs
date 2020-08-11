﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentKnight : AgentScript
{
    public override bool DecideAttack(float? act) {
		//act will indicates special ability
		bool attacking=false;
        if(act!=null) {
            if(act>=0) {
				//placeholder, since new gameobject makes useless game object and it slows entire game
                float minDistance=Mathf.Infinity;
				GameObject minUnit=sys.EmptyUnit;
                foreach(GameObject enemy in inspector.getCurrentEnemys()) {
                    if(inspector.setScriptsFrom(enemy) && !inspector.isDead()) {
                        float distanceToTarget = Vector3.Distance(this.transform.localPosition, enemy.transform.localPosition);
                        if(distanceToTarget<= agent.stoppingDistance) {
                            minUnit=(distanceToTarget<minDistance)?enemy:minUnit;
                            minDistance=(distanceToTarget<minDistance)?distanceToTarget:minDistance;
                        }
                    }
                }
                if(minUnit.CompareTag("Enemy") || minUnit.CompareTag("Knight")) {
                    Vector3 currentTargetPosition = minUnit.transform.position;
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);
                    if(inspector.setScriptsFrom(minUnit)) {
                        inspector.setLives(inspector.getLives()-(Time.deltaTime * damage));
                        attacking=true;
                        if(inspector.getLives()<0) {
                            print("Damaged opponent dead");
                            AddReward(1f);
                        } else {
                            AddReward(0.5f);
                        }
                    } else {
                        Debug.LogError("Invalid unit targetted.");
                    }
                    
                }
            } else {

            }
        }
        return attacking;
	}
}