using UnityEngine;
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
	
	private CharacterPlacement characterPlacer;
 
    void Start(){
		//get start rotation
		Vector3 rot = transform.localRotation.eulerAngles;
		rotationY = rot.y;
		rotationX = rot.x;
		
		//find the character placer
		characterPlacer = GameObject.FindObjectOfType<CharacterPlacement>();
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
		if(Input.GetKey("a")){
		transform.Translate(Vector3.right * -movespeed * timescale);
		}
		if(Input.GetKey("d")){
		transform.Translate(Vector3.right * movespeed * timescale);
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
