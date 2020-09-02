using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class MyAcademy : Academy
{
    private GameSystem sys;
    private EnemyArmy enemysys;
    private UnitInspect inspector;
    private bool once=false;
    private bool onceInStep=false;
    public override void InitializeAcademy() {
        //Monitor.SetActive(true);

        sys=FindObjectOfType<GameSystem>();
        enemysys=FindObjectOfType<EnemyArmy>();
        inspector=new UnitInspect(sys);
        inspector.cam=FindObjectOfType<CamController>();

        sys.Academy_Initialize();
        enemysys.Academy_Initialize();
    }
    public override void AcademyReset() {
        if(once) {
            EndEpisode();
        }
        once=true;
        print("Resetting Academy");
        sys.Academy_Awake(); 
        enemysys.Academy_Start();
        sys.Academy_Start();
        sys.startBattle(); //Asign knights, enemys
        onceInStep=false;
    }
    void EndEpisode() {
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
                inspector.printOnPanel($"Knight {((inspector.getCurrentKnights().Length<=0)?"Lose":"Win")} / Knight:{inspector.AvgLives(inspector.getCurrentKnights())} / Enemy:{inspector.AvgLives(inspector.getCurrentEnemys())}");
                float AllDamaged=(inspector.getCurrentEnemys().Length<=0)?0:inspector.AvgLives(inspector.getCurrentEnemys());
                //Rewarding globally
                foreach(GameObject knight in inspector.getInstantiatedKnights()) {
                    if(inspector.setScriptsFrom(knight) && inspector.getScriptType()=="AgentScript") {
                        inspector.AgentAddRewardDirectly(1-((AllDamaged-1.8f)/sys.AllInitLives));
                        Debug.Log($"{AllDamaged}/{sys.AllInitLives}");
                    }
                }
                //Rewarding remained agents
                GameObject[] Remained=inspector.getCurrentUnits();
                foreach(GameObject unit in Remained) {
                    if(inspector.setScriptsFrom(unit)) {
                        if(inspector.getScriptType()=="Unit") {
                            inspector.removeFrom(unit);
                            Destroy(unit);
                        } else if(inspector.getScriptType()=="AgentScript") {
                            Debug.Log("Agent Remained");
                        }
                    }
                }
                onceInStep=true;
                Done();
            }
        }
    }
}
