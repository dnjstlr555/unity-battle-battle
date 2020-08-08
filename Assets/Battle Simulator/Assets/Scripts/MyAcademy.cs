﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class MyAcademy : Academy
{
    private GameSystem sys;
    private EnemyArmy enemysys;
    private UnitInspect inspector;
    private bool firstResetPassed = false;
    public override void InitializeAcademy() {
        sys=FindObjectOfType<GameSystem>();
        enemysys=FindObjectOfType<EnemyArmy>();
        inspector=new UnitInspect();

        sys.Academy_Initialize();
        enemysys.Academy_Initialize();
    }
    public override void AcademyReset() {
        print("Resetting Academy");
        if(firstResetPassed) {
            List<GameObject> units=inspector.getCurrentUnits();
            foreach(GameObject unit in units) {
                if(inspector.setScriptsFrom(unit)) {
                    if(inspector.getType()=="Unit") {
                        Destroy(unit);
                        Debug.Log("an unit remained. destroying");
                    } else if(inspector.getType()=="AgentScript") {
                        Debug.LogError("an Agent unit remained after episode finishes.");
                    }
                }
            }
            enemysys.initEnemies();
        }
        firstResetPassed=true;
        sys.Academy_Awake();
        sys.Academy_Start();
        enemysys.Academy_Start();
        sys.startBattle();
    }

    public override void AcademyStep() {
        if(!IsDone()) {
            List<GameObject> units=inspector.getCurrentUnits();
            foreach(GameObject unit in units) {
                if(inspector.setScriptsFrom(unit)) {
                    if(inspector.getType()=="AgentScript" && !inspector.isDead()) {
                        inspector.AgentDescisionRequest();
                    }
                }
            }
            //Enviromental Upadte
            sys.Academy_Update();
            if(sys.battleStarted && (sys.knightNumber<=0 || sys.enemyNumber<=0)) {
                print("Episode Ended");
                EndEpisode();
            }
        }
    }
    void EndEpisode() {
        sys.battleStarted=false;
        foreach(GameObject corpe in GameObject.FindGameObjectsWithTag("Ragdoll")) {
            corpe.GetComponent<DeleteParticles>().DestroyMe();
        }
        Done();
    }
}