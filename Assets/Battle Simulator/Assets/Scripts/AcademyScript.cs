using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AcademyScript : MonoBehaviour
{
    // Start is called before the first frame update
    public StatsRecorder Logger;
    public void Awake()
    {
        //Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        Academy.Instance.AutomaticSteppingEnabled = true;
        Logger = Academy.Instance.StatsRecorder;
    }
}
