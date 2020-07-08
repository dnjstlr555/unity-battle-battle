using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
	
	//variables visible in the inspector
	public Animator startPanel;
	public Color activeButtonColor;
	public Color disabledButtonColor;
	public Transform levelPanelParent;
	public GameObject levelPanel;
	public GameObject levelButton;
	public Text title;
	public int maxLevelsPerPage;
	public GameObject leftButton;
	public GameObject rightButton;
	public GameObject howToPlay;
	public GameObject credits;
	public Animator transition;
	
	//not visible in the inspector
	private LevelData levelData;
	private Transform currentLevelPanel;
	private string lastScene;
	
	private List<Transform> levelPanels = new List<Transform>();
	private int page;
	
	void Start(){
		//get the level data object
		levelData = Resources.Load("Level data") as LevelData;
		
		//initialize the credits and how to play panels
		credits.SetActive(false);
		howToPlay.SetActive(false);
		
		//add the level buttons based on the level data
		addLevels();
	}
	
	void addLevels(){		
		//for each level in the level data
		for(int i = 0; i < levelData.levels.Count; i++){
			//get the level scene
			string sceneName = levelData.levels[i].scene;
			
			//add a new panel if this level uses a new scene
			if(currentLevelPanel == null || currentLevelPanel.childCount == maxLevelsPerPage || lastScene != sceneName)
				addLevelPanel();
			
			lastScene = sceneName;
			
			//create the level button and parent it to the current panel
			GameObject newButton = Instantiate(levelButton);
			newButton.transform.SetParent(currentLevelPanel, false);
			
			//set the label text for the new button
			newButton.GetComponentInChildren<Text>().text = (i + 1) + "";
			
			//check if this level is unlocked yet
			bool levelEnabled = (PlayerPrefs.GetInt("level" + i) == 1 || i == 0);
			
			//change the shadow, color and lock sprite based on the level state (whether or not it's unlocked)
			if(levelEnabled){
				newButton.GetComponent<Shadow>().effectDistance = new Vector2(3, -3);
				newButton.GetComponent<Image>().color = activeButtonColor;
				newButton.transform.Find("Lock").gameObject.SetActive(false);
			}
			else{
				newButton.GetComponent<Shadow>().effectDistance = new Vector2(1, -1);
				newButton.GetComponent<Image>().color = disabledButtonColor;
				newButton.GetComponent<Button>().enabled = false;
			}
			
			//change the button name to its index
			newButton.transform.name = "" + i;
			
			//add a onclick function to the button with the name to select the proper piece
			newButton.GetComponent<Button>().onClick.AddListener(() => { openLevel(int.Parse(newButton.transform.name)); });
			
			//only show the current panel
			if((PlayerPrefs.GetInt("level") >= levelData.levels.Count && i == 0) || PlayerPrefs.GetInt("level") == i){
				currentLevelPanel.gameObject.SetActive(true);
				page = levelPanels.IndexOf(currentLevelPanel);
				title.text = getSceneName(currentLevelPanel);
			}
		}
		
		//update the level panel buttons and don't yet show the buttons
		levelPanelParent.parent.gameObject.SetActive(false);
		checkLeftRightButtons();
	}
	
	void addLevelPanel(){
		//add a new level panel for the level buttons and parent it to the main panel
		GameObject newLevelPanel = Instantiate(levelPanel);
		newLevelPanel.transform.SetParent(levelPanelParent, false);
		
		//add it to the level panel list
		levelPanels.Add(newLevelPanel.transform);
		newLevelPanel.SetActive(false);
		
		//set the current panel to this level panel
		currentLevelPanel = newLevelPanel.transform;
	}
	
	void checkLeftRightButtons(){
		//either enable or disable the buttons by checking the current level page
		if(page == 0){
			leftButton.SetActive(false);
		}
		else{
			leftButton.SetActive(true);
		}
		
		if(page == levelPanels.Count - 1){
			rightButton.SetActive(false);
		}
		else{
			rightButton.SetActive(true);
		}
	}
	
	public void changePage(int direction){
		//change the level page when pressing the arrow button
		levelPanels[page].gameObject.SetActive(false);
		page += direction;
		
		//turn the next page on and show a short animation
		levelPanels[page].gameObject.SetActive(true);
		levelPanelParent.parent.GetComponent<Animator>().SetTrigger("effect");
		
		//update the level panel buttons
		checkLeftRightButtons();
		title.text = getSceneName(levelPanels[page]);
	}
	
	public void openLevel(int index){
		//open the level at this specific index
		StartCoroutine(level(index));
	}
	
	IEnumerator level(int index){
		//wait for the fade animation to end
		transition.SetTrigger("fade");
		
		yield return new WaitForSeconds(0.5f);
		
		//save the new level index and load it
		PlayerPrefs.SetInt("level", index);
		SceneManager.LoadScene(levelData.levels[index].scene);
	}
	
	public void options(){
		//show the options panel
		startPanel.SetBool("enabled", false);
		StartCoroutine(openOptions());
	}
	
	//hide options
	public void closeOptions(){
		startPanel.SetBool("enabled", true);
	}
	
	//open the how to play panel 
	public void howToPlayOpen(){
		startPanel.SetBool("enabled", false);
		StartCoroutine(openHowToPlay());
	}
	
	//close the how to play panel
	public void closeHowToPlay(){
		startPanel.SetBool("enabled", true);
		howToPlay.SetActive(false);
	}
	
	//open the credits panel
	public void creditsOpen(){
		startPanel.SetBool("enabled", false);
		StartCoroutine(openCredits());
	}
	
	//close the credits panel
	public void closeCredits(){
		startPanel.SetBool("enabled", true);
		credits.SetActive(false);
	}
	
	//open the campaign panel with the levels
	public void openCampaign(){
		startPanel.SetBool("enabled", false);
		levelPanelParent.parent.gameObject.SetActive(true);
	}
	
	//go back to the main menu
	public void back(){
		levelPanelParent.parent.gameObject.SetActive(false);
		startPanel.SetBool("enabled", true);
	}
	
	//quit the app
	public void quit(){
		Application.Quit();
	}
	
	//get the scene for a specific level page
	string getSceneName(Transform levelPage){
		int firstLevel = int.Parse(levelPage.GetChild(0).GetComponentInChildren<Text>().text) - 1;
		return levelData.levels[firstLevel].scene;
	}
	
	//open the options panel
	IEnumerator openOptions(){
		yield return new WaitForSeconds(0.7f);
		GetComponent<Settings>().openSettingsMenu();
	}
	
	//open the credits panel
	IEnumerator openCredits(){
		yield return new WaitForSeconds(0.7f);
		credits.SetActive(true);
	}
	
	//open the how to play panel
	IEnumerator openHowToPlay(){
		yield return new WaitForSeconds(0.7f);
		howToPlay.SetActive(true);
	}
}
