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
	private GameObject[] Units;
	private GameObject thatKnight;
	private UnitInspect inspector;
	List<int> AvailableStick = new List<int>();
    void Start(){
		//get start rotation
		Vector3 rot = transform.localRotation.eulerAngles;
		rotationY = rot.y;
		rotationX = rot.x;
		
		//find the character placer
		characterPlacer = GameObject.FindObjectOfType<GameSystem>();
		
    }
	
	void Update(){
		inspector = new UnitInspect(characterPlacer);
		
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
		Sticky-=1;
		}
		if(Input.GetKeyDown("d")){
		//transform.Translate(Vector3.right * movespeed * timescale);
		Sticky+=1;
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
		AvailableStick.Clear();
		if(characterPlacer.knightNumber>=1 || characterPlacer.enemyNumber>=1) {
			Units=inspector.getCurrentUnits();
			for(int i=0;i<Units.Length;i++) {
				if(inspector.setScriptsFrom(Units[i]) && !inspector.isDead()) {
					AvailableStick.Add(i);
				} else {
					continue;
				}
			}
			if(Sticky<0) Sticky=AvailableStick.Count-1;
			if(Sticky>AvailableStick.Count-1) Sticky=0;
			if(AvailableStick.Contains(Sticky)) {
				transform.position = new Vector3(Units[AvailableStick[Sticky]].transform.position.x,transform.position.y,Units[AvailableStick[Sticky]].transform.position.z);
			}
		}
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
