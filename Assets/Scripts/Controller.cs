using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveSpeed=6;
    Vector3 velocity;
    new Rigidbody rigidbody;
    Camera viewCamera;
    private void Start() {
        rigidbody=GetComponent<Rigidbody>();
        viewCamera=Camera.main;
    }
    private void Update() {
        
        Vector3 mousePosition=viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,viewCamera.transform.position.y));
        transform.LookAt(mousePosition+Vector3.up*transform.position.y);
        velocity=new Vector3 (Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical")).normalized*moveSpeed;
    }
    private void FixedUpdate() {
        rigidbody.MovePosition(rigidbody.position+velocity*Time.fixedDeltaTime);
    }
}
