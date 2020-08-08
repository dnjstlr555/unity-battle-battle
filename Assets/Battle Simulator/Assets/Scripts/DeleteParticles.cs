using UnityEngine;
using System.Collections;

public class DeleteParticles : MonoBehaviour {
	
	//float visible in the inspector
	public float lifetime = 1f;
	
	void Start(){
	//Destroy gameobject (or particles) after lifetime
		Invoke("DestroyMe", lifetime);
	}
	void Update() {
		if(GameObject.FindObjectOfType<MyAcademy>().IsDone()) {
			CancelInvoke("DestroyMe");
			Destroy(gameObject);
		}
	}
	public void DestroyMe() {
		Destroy(gameObject);
	}
}
