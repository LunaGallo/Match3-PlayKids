using System;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour {

    public static List<Gem> instanceList = new List<Gem>();
    private void Awake() {
        instanceList.Add(this);
    }
    private void OnDestroy() {
        instanceList.Remove(this);
    }

    public int type = 0;
    public float speed = 1f;
    [HideInInspector] public Vector3 targetPoint;

    public bool IsStopped {
        get {
            return transform.position == targetPoint;
        }
    }

    private void Update() {
        if (!IsStopped) {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        }
    }

}
