using System.Collections;
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
        //Monitor.SetActive(true);

        sys=FindObjectOfType<GameSystem>();
        enemysys=FindObjectOfType<EnemyArmy>();
        inspector=new UnitInspect(sys);

        sys.Academy_Initialize();
        enemysys.Academy_Initialize();
    }
    public override void AcademyReset() {
        print("Resetting Academy");
        if(firstResetPassed) {
            GameObject[] units=inspector.getCurrentUnits();
            foreach(GameObject unit in units) {
                if(inspector.setScriptsFrom(unit)) {
                    if(inspector.getScriptType()=="Unit") {
                        Destroy(unit);
                        Debug.Log("an unit remained. destroying");
                    } else if(inspector.getScriptType()=="AgentScript") {
                        Debug.LogError("an Agent unit remained after episode finishes.");
                    }
                }
            }
            enemysys.initEnemies();
        }
        firstResetPassed=true;
        sys.Academy_Awake(); //Init knights, enemys
        enemysys.Academy_Start();
        sys.Academy_Start();
        sys.startBattle(); //Asign knights, enemys
    }

    public override void AcademyStep() {
        if(!IsDone()) {
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
            if(sys.battleStarted && (sys.knightNumber<=0 || sys.enemyNumber<=0)) {
                print("Episode Ended");
                Debug.Log(((sys.knightNumber<=0)?"Knight Eliminated ":"Knight Win ")+inspector.AvgLives(inspector.getCurrentKnights()).ToString()+" "+inspector.AvgLives(inspector.getCurrentEnemys()).ToString());
                //Monitor.Log("LastAvgLivesOfKnight", inspector.AvgLives(inspector.getCurrentKnights()), this.transform);
                //Monitor.Log("LastAvgLivesOfEnemy", inspector.AvgLives(inspector.getCurrentEnemys()), this.transform);
                
                EndEpisode();
            }
        }
    }
    void EndEpisode() {
        sys.battleStarted=false;
        foreach(GameObject corpe in GameObject.FindGameObjectsWithTag("Ragdoll")) {
            print("delete copre");
            corpe.GetComponent<DeleteParticles>().DestroyMe();
        }
        Done();
    }
}
