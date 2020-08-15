using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public class CamController : MonoBehaviour {
	
	//variables not visible in the inspector
	public static float movespeed;
	public static float zoomSpeed;
	public static float mouseSensitivity;
    public static float clampAngle;
	
    private float rotationY = 0;
    private float rotationX = 0;
	
	private float timescale;
	
	private GameSystem characterPlacer;
	private int Sticky = 0;
	private int StickyKey = int.MinValue;
	private Dictionary<int,GameObject> Units= new Dictionary<int, GameObject>();
	private GameObject thatKnight;
	private UnitInspect inspector;
	private DebugInfo DebugInner;
    void Start(){
		//get start rotation
		Vector3 rot = transform.localRotation.eulerAngles;
		rotationY = rot.y;
		rotationX = rot.x;
		
		//find the character placer
		characterPlacer = GameObject.FindObjectOfType<GameSystem>();
		inspector = new UnitInspect(characterPlacer);
		DebugInner = new DebugInfo(inspector);
    }
	
	void Update(){
		DebugInner.printUpdate();
		Units = new Dictionary<int, GameObject>();
		foreach(GameObject unit in inspector.getCurrentUnits()) {
			Units.Add(unit.GetInstanceID(), unit);
		}
		int[] keys = new int[Units.Keys.Count];
		Units.Keys.CopyTo(keys,0);
		//don't use time.deltatime if the timescale is 0
		if(Time.timeScale == 0){
			timescale = (1f/30f);
		}
		else{
			timescale = Time.deltaTime;
		}
		
		//if key gets pressed move left/right
		if(Input.GetKeyDown("a")){
			//transform.Translate(Vector3.right * -movespeed * timescale);
			if(keys.Length>0) {
				Sticky-=1;
				if(Sticky>=keys.Length) Sticky=0;
				if(Sticky<0) Sticky=keys.Length-1;
				StickyKey=keys[Sticky];
			}
		}
		if(Input.GetKeyDown("d")){
			//transform.Translate(Vector3.right * movespeed * timescale);
			if(keys.Length>0) {
				Sticky+=1;
				if(Sticky>=keys.Length) Sticky=0;
				if(Sticky<0) Sticky=keys.Length-1;
				StickyKey=keys[Sticky];
			}
		}
	
		//if key gets pressed move up/down
		if(Input.GetKey("w")){
		transform.Translate(Vector3.up * movespeed * timescale);
		}
		if(Input.GetKey("s")){
		transform.Translate(Vector3.up * -movespeed * timescale);
		}
		//if scrollwheel is down rotate camera
		if(Input.GetMouseButton(2)){
			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = -Input.GetAxis("Mouse Y");
			rotateCamera(mouseX, mouseY);
		}
	
		//move camera when you scroll
		transform.Translate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")) * zoomSpeed * timescale);
		if(characterPlacer.battleStarted && keys.Length>0 && (characterPlacer.knightNumber>=1 || characterPlacer.enemyNumber>=1)) {
			if(Sticky>keys.Length-1) Sticky=0;
			if(Sticky<0) Sticky=keys.Length-1;
			if(!Units.ContainsKey(StickyKey)) {
				
				StickyKey=keys[Sticky];
				Debug.Log($"no ... {StickyKey}");
			}
			if(Units.ContainsKey(StickyKey)) {
				if(inspector.setScriptsFrom(Units[StickyKey]) && !inspector.isDead()) {
					transform.position = new Vector3(Units[StickyKey].transform.position.x,transform.position.y,Units[StickyKey].transform.position.z);
					thatKnight=Units[StickyKey];
				} else {
					Debug.LogError("Undead caught while controlling cam");
				}
				//print($"{Units.Length}/{Sticky}/Front:{((Sticky+1<Units.Length)?Units[Sticky+1].tag:Units[0].tag)}/Current:{Units[Sticky].tag}/Backward:{((Sticky>0)?Units[Sticky-1].tag:Units[Units.Length-1].tag)}");
			} else {
				Debug.LogError("It wasn't on the key");
			}
			
		}
	}	
	public GameObject getStickyUnit() {
		return thatKnight;
	}
	public void printOnPanel(string str) {
		DebugInner.printOnPanel(str);
	}
	void rotateCamera(float mouseX, float mouseY){
		rotationY += mouseX * mouseSensitivity * timescale;
		rotationX += mouseY * mouseSensitivity * timescale;
	
		//clamp x rotation to limit it
		rotationX = Mathf.Clamp(rotationX, -clampAngle, clampAngle);
	
		//apply rotation
		transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
	}
}
