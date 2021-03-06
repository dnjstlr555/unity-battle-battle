﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class AcademyReward : Reward {
    public override void RewardAtEpisodeEnds(UnitInspect inspector, GameSystem sys) {
        float AllDamaged=(inspector.getCurrentEnemys().Length<=0)?0:inspector.AvgLives(inspector.getCurrentEnemys());
        //Rewarding globally
        foreach(GameObject knight in inspector.getInstantiatedKnights()) {
            inspector.setScriptsFrom(knight);
            if(inspector.isScriptValid() && inspector.getScriptType()=="AgentScript") {
                BagReset();
                AddReward((1-((AllDamaged)/sys.AllInitLives))*2f);
                Apply(inspector.AgentScript);
            }
        }
        /*
        //Rewarding reamined units
        foreach(GameObject knight in inspector.getCurrentKnights()) {
            inspector.AgentAddRewardDircetly(1f);
        }
        */
    }
}
public class MyAcademy : Academy
{
    private GameSystem sys;
    private EnemyArmy enemysys;
    private UnitInspect inspector;
    private bool once=false;
    private bool onceInStep=false;
    private AcademyReward rewardSys=new AcademyReward();
    public override void InitializeAcademy() {
        //Monitor.SetActive(true);

        sys=FindObjectOfType<GameSystem>();
        //enemysys=FindObjectOfType<EnemyArmy>();
        inspector=new UnitInspect(sys);
        inspector.cam=FindObjectOfType<CamController>();

        sys.Academy_Initialize();
        //enemysys.Academy_Initialize();
    }
    public override void AcademyReset() {
        if(once) {
            EndEpisode();
        } else {
            once=true;
        }
        print("Resetting Academy");
        sys.Academy_Awake(); 
        //enemysys.Academy_Start();
        sys.Academy_Start();
        sys.startBattle(); //Asign knights, enemys
        onceInStep=false;
    }
    void EndEpisode() {
        GameObject[] Remained=inspector.getCurrentUnits();
        foreach(GameObject unit in Remained) {
            if(inspector.setScriptsFrom(unit)) {
                if(inspector.getScriptType()=="Unit") {
                    inspector.removeFrom(unit);
                    Destroy(unit);
                } else if(inspector.getScriptType()=="AgentScript") {
                    Debug.LogWarning("Agent Remained after episode ends");
                }
            }
        }
        sys.battleStarted=false;
        System.Array.Resize(ref sys.knightUnits,0);
        System.Array.Resize(ref sys.enemyUnits,0);
        foreach(GameObject unit in inspector.getInstantiatedUnits()) {
            Destroy(unit);
        }
        foreach(GameObject corpe in GameObject.FindGameObjectsWithTag("Ragdoll")) {
            corpe.GetComponent<DeleteParticles>().DestroyMe();
        }
    }
    public override void AcademyStep() {
        if(!IsDone() && sys.battleStarted) {
            GameObject[] units=inspector.getCurrentUnits();
            foreach(GameObject unit in units) {
                if(inspector.setScriptsFrom(unit)) {
                    if(inspector.getScriptType()=="AgentScript" && !inspector.isDead()) {
                        inspector.AgentDescisionRequest();
                        inspector.AgentAlwaysUpdate();
                    }
                }
            }
            //Enviromental Upadte
            sys.Academy_Update();
            if((sys.knightNumber<=0 || sys.enemyNumber<=0) && !onceInStep) {
                //On End
                rewardSys.RewardAtEpisodeEnds(inspector, sys);
                onceInStep=true;
                Done();
            }
        } else if(IsDone()) {
            int knightlen = inspector.getCurrentKnights().Length;
            int enemylen = inspector.getCurrentEnemys().Length;
            string endstring = (knightlen>0 && enemylen<=0)?"Knight win":(enemylen>0 && knightlen<=0)?"Enemy win":(enemylen<=0 && knightlen<=0)?"Draw":(enemylen>0 && knightlen>0)?"Step Max reached":"Unknown state";
            Debug.Log($"{endstring} KnightAvgHP:{inspector.AvgLives(inspector.getCurrentKnights())} / EnemyAvgHP:{inspector.AvgLives(inspector.getCurrentEnemys())}");
        }
    }
}
