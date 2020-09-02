 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxScript : MonoBehaviour
{
    HashSet<GameObject> obj=new HashSet<GameObject>();
    private void OnTriggerEnter(Collider other) {
        obj.Add(other.gameObject);
    }
    private void OnTriggerExit(Collider other) {
        obj.Remove(other.gameObject);
    }
    public void RemoveObject(GameObject o) {
        obj.Remove(o);
    }
    public HashSet<GameObject> GetCollideObjects() {
        var clonedSet = new HashSet<GameObject>(obj, obj.Comparer);
        return clonedSet;
    }
}
