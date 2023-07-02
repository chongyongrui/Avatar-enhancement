using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAITargetMouse : MonoBehaviour {

    [SerializeField] private Transform targetTransform;

    private bool isFollowing = false;

    private void Update() {
        if (isFollowing) {
          
        }

        if (Input.GetMouseButtonDown(0)) {
            isFollowing = !isFollowing;
        }
    }

}
