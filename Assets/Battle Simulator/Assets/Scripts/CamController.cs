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
	private GameObject[] Knight;
	private GameObject thatKnight;
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
		//don't move the camera if we're in 2d grid mode
		if(characterPlacer.grid && characterPlacer.grid.GetBool("show"))
			return;
		
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
		if(characterPlacer.knightNumber>=1) {
			Knight=GameObject.FindGameObjectsWithTag("Knight");
			for(int i=0;i<Knight.Length;i++) {
				thatKnight=Knight[i];
				if(thatKnight.GetComponent<AgentScript>() != null && !thatKnight.GetComponent<AgentScript>().dead) {
					AvailableStick.Add(i);
				} else if(thatKnight.GetComponent<Unit>() != null && !thatKnight.GetComponent<Unit>().dead) {
					AvailableStick.Add(i);
				} else {
					continue;
				}
			}
			if(Sticky<0) Sticky=AvailableStick.Count-1;
			if(Sticky>AvailableStick.Count-1) Sticky=0;
			if(AvailableStick!=null) {
				transform.position = new Vector3(Knight[AvailableStick[Sticky]].transform.position.x,transform.position.y,Knight[AvailableStick[Sticky]].transform.position.z);
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
