using UnityEngine;
using System.Collections;

public class DeleteParticles : MonoBehaviour {
	
	//float visible in the inspector
	public float lifetime = 1f;
	public ParticleSystem DeadParticle;
	void Start(){
		if(FindObjectOfType<GameSystem>().showeffects) {
			DeadParticle.Play();
			Invoke("DestroyMe", lifetime);
		} else {
			Destroy(this.gameObject);
		}
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
