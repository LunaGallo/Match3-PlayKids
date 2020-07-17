using System;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollower : MonoBehaviour {

    public Transform target;
    public float lerpSpeed = 1f;

    private void Update() {
        if (target != null) {
            transform.position = Vector3.Lerp(transform.position, target.position, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, lerpSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, target.localScale, lerpSpeed * Time.deltaTime);
        }
    }

}
