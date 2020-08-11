using UnityEngine;
using System.Collections;

public class DeleteParticles : MonoBehaviour {
	
	//float visible in the inspector
	public float lifetime = 1f;
	void Start(){
		Invoke("DestroyMe", lifetime);
	}
	/*
	void Update() {
		if(gameObject.GetComponent<DeleteParticles>()) {
			CancelInvoke("DestroyMe");
			Destroy(gameObject);
		}
	}
	*/
	public void DestroyMe() {
		Destroy(gameObject);
	}
}
