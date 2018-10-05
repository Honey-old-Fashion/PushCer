using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCarMove : MonoBehaviour {

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float curveSpeed;

	// Use this for initialization
	void Start () {

        moveSpeed = 12f;
        curveSpeed = 1f;
	}
	
	// Update is called once per frame
	void Update () {

        // 移動
        if (Input.GetKey("right"))
        {
            transform.Rotate(0, curveSpeed, 0);
        }
        if (Input.GetKey("left"))
        {
            transform.Rotate(0, -curveSpeed, 0);
        }
        transform.position -= transform.forward * moveSpeed * Time.deltaTime;

        if(Input.GetKey("s"))
        {
            moveSpeed += 0.5f;
        }
        if (Input.GetKey("a"))
        {
            moveSpeed -= 0.5f;
        }

        if (moveSpeed < 0)
        {
            moveSpeed = 0;
        }
    }
}
