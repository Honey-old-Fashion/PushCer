using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CerMove : MonoBehaviour {

    private float speed;

	void Update () {

        speed = CarData.carSpeed;

        transform.position -= transform.forward * speed;

	}
}
