using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkArea : MonoBehaviour {
	
	//vectors visible in the inspector
	public Vector3 center;
	public Vector3 area;
	
	//draw some gizmos when the manager is selected to show the walk area
	void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, area);
    }
}
