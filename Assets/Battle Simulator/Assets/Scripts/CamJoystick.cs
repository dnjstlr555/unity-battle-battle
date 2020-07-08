using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CamJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {
	
	//variables visible in the inspector
	public Image background;
	public Image knob;
	public float sensitivity;
	public float zoomSpeed;
	public float rotateSpeed;
	public Slider zoomIndicator;
	
	//not visible in the inspector
	private Transform cameraController;
	private Vector3 direction;
	private CharacterPlacement characterPlacer;
	
	private float start;
	private float startRotation;
	
	private bool joystick;
	private bool noTouch;
	
	void Start(){
		//get the camera controller and character placer
		cameraController = Camera.main.gameObject.transform.parent;
		direction = Vector3.zero;
		characterPlacer = GameObject.FindObjectOfType<CharacterPlacement>();
	}
	
	void Update(){
		//move the camera controller (which also moves the camera) in the joystick direction
		cameraController.Translate(Time.deltaTime * direction * -sensitivity);
		
		//zoom if the grid is not active
		if(characterPlacer.leftPanelAnimator.GetBool("hide panel") || !characterPlacer.leftPanelAnimator.gameObject.activeSelf)
			Zoom();
	}

	public virtual void OnDrag(PointerEventData data){
		//store the drag position
		Vector2 pos = Vector2.zero;
		
		//if the drag is valid..
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(background.rectTransform, data.position, data.pressEventCamera, out pos)){
			//we're now moving the joystick
			joystick = true;
			
			//update the drag position to fit the background size
			pos.x /= background.rectTransform.sizeDelta.x;	
			pos.y /= background.rectTransform.sizeDelta.y;	
			
			//get the x and y values for the move direction
			float x = (background.rectTransform.pivot.x == 1) ? pos.x * 2 + 1 : pos.x * 2 - 1;
			float y = (background.rectTransform.pivot.y == 1) ? pos.y * 2 + 1 : pos.y * 2 - 1;
			
			//apply the move direction
			direction = new Vector3(x, 0, y);
			
			//keep the direction value from getting too big
			if(direction.magnitude > 1)
				direction.Normalize();
			
			//update the sprite position
			knob.rectTransform.anchoredPosition = new Vector3(direction.x * (background.rectTransform.sizeDelta.x/2.5f), direction.z * (background.rectTransform.sizeDelta.y/2.5f));
		}
	}
	
	//start updating the knob sprite immediately when the player touches the screen
	public virtual void OnPointerDown(PointerEventData data){
		OnDrag(data);
	}
	
	public virtual void OnPointerUp(PointerEventData data){
		//reset the direction, sprite position and joystick value
		direction = Vector3.zero;
		knob.rectTransform.anchoredPosition = Vector3.zero;
		joystick = false;
	}
	
	void Zoom(){
		//if there's no fingers on the screen, we're not touching it
		if(Input.touchCount == 0)
			noTouch = true;
			
		if(Input.touchCount == 2){
            //store two touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            //find the position in the previous frame of each touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            //find the magnitude of the vector (the distance) between the touches in each frame
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            //find the difference in the distances between each frame
            float z = (prevTouchDeltaMag - touchDeltaMag) * 0.001f * zoomSpeed;
			
			//zoom camera by moving it forward/backward 
			if((z > 0 && Camera.main.fieldOfView < 100) || (z < 0 && Camera.main.fieldOfView > 15)){
				Camera.main.fieldOfView += z;
			}
			
			//we are now touching the screen, so it should show the zoom indicator and update its value
			noTouch = false;
			zoomIndicator.gameObject.SetActive(true);
			zoomIndicator.value = Camera.main.fieldOfView;
		}
		else{ 
			//rotate the camera if we're not zooming or moving
			if(!joystick && noTouch)
				Rotate();
			
			zoomIndicator.gameObject.SetActive(false);
		}
	}
	
	void Rotate(){
		//get the start drag position
		if(Input.GetMouseButtonDown(0)){
			start = Input.mousePosition.x;
			startRotation = cameraController.transform.eulerAngles.y;
		}
		
		if(Input.GetMouseButton(0)){
			//update the camera rotation by comparing the current drag position to the start drag position
			cameraController.transform.eulerAngles = new Vector3(0, startRotation + ((Input.mousePosition.x - start) * rotateSpeed), 0);
		}
	}
}
