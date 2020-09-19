using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkArea : MonoBehaviour {
	
	//vectors visible in the inspector
	public Vector3 RandCenter;
	public Vector3 RandArea;
	public Vector3 EnemyCenter;
	public Vector3 EnemyArea;
	public Vector3 KnightCenter;
	public Vector3 KnightArea;
	public Vector3 getRandomPosition(Vector3 pos, Vector3 size){
		Vector3 center = pos;
		Vector3 bounds = size;
		
		//create a ray using the center and the bounds
		float yRay = center.y + bounds.y/2f;
		
		//get a random position for the ray to start from
		Vector3 rayStart = new Vector3(center.x + Random.Range(-bounds.x/2f, bounds.x/2f), yRay, center.z + Random.Range(-bounds.z/2f, bounds.z/2f));
		//store the raycast hit
		return rayStart;
		/*
		RaycastHit hit;
		
		//check if there's terrain underneath
		if(Physics.Raycast(rayStart, -Vector3.up, out hit))
			return hit.point;
		
		//if there's no terrain, return the center
		return Vector3.zero;
		*/
	}
	public bool isWithInArea(Vector3 point, Vector3 pos, Vector3 size) {
		Bounds test = new Bounds(pos,size);
		return test.Contains(point);

	}
	//draw some gizmos when the manager is selected to show the walk area
	void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(RandCenter, RandArea);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(EnemyCenter, EnemyArea);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(KnightCenter, KnightArea);
    }
}
