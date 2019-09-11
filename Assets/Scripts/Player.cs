using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public bool isGrounded;
    public bool isSprinting;
    public float maxSpeed = 3;
    public float sprintSpeed = 9;
    public float jumpForce = 5;

    public float playerWidth = 0.15f;
    private float verticalMomentum = 0;
    private bool jumpRequest;
    public float gravity = -9.807f;
    private Transform cam;
    private World world;
    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;

    private void Start() {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }
    private void Update() {
        GetPlayerInputs();

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * maxSpeed;
        // velocity += Vector3.up * gravity * Time.deltaTime;

        // velocity.y = CheckDownSpeed(velocity.y);

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }

    private void GetPlayerInputs() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint")) {
            isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint")) {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump")) {
            jumpRequest = true;
        }
    }

    private float CheckDownSpeed(float downSpeed) {
        if (
            world.CheckForBlock(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForBlock(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForBlock(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForBlock(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) 
        ) {
            isGrounded = true;
            return 0;
        } else {
            isGrounded = false;
            return downSpeed;
        }
    }
}
