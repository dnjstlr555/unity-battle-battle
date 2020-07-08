using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    private LevelData levelData;
    // Start is called before the first frame update
    void Start()
    {
        levelData = Resources.Load("Level data") as LevelData;
        PlayerPrefs.SetInt("level", 0);
        //PlayerPrefs.SetInt("level ")
        //print(PlayerPrefs.GetInt("level"));
		SceneManager.LoadScene(levelData.levels[0].scene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
