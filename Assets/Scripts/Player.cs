using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    public float walkSpeed = 3;
    public float sprintSpeed = 9;
    public float jumpForce = 5;

    public float playerWidth = 0.15f;
    public float playerHeight = 1.8f;
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

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);

    }
    private void Update()
    {
        GetPlayerInputs();
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Apply gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        Vector3 baseVel = velocity = ((transform.forward * vertical) + transform.right * horizontal) * Time.fixedDeltaTime;
        velocity = baseVel * (isSprinting ? sprintSpeed : walkSpeed);

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);

    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }

    private float CheckDownSpeed(float downSpeed)
    {
        Vector3 lb = new Vector3(-playerWidth, downSpeed, -playerWidth);
        Vector3 lf = new Vector3(-playerWidth, downSpeed, playerWidth);
        Vector3 rb = new Vector3(playerWidth, downSpeed, -playerWidth);
        Vector3 rf = new Vector3(playerWidth, downSpeed, playerWidth);
        if (
            world.CheckForBlock(transform.position + lb) ||
            world.CheckForBlock(transform.position + lf) ||
            world.CheckForBlock(transform.position + rb) ||
            world.CheckForBlock(transform.position + rf)
        )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        Vector3 lb = new Vector3(-playerWidth, 2f + upSpeed, -playerWidth);
        Vector3 lf = new Vector3(-playerWidth, 2f + upSpeed, playerWidth);
        Vector3 rb = new Vector3(playerWidth, 2f + upSpeed, -playerWidth);
        Vector3 rf = new Vector3(playerWidth, 2f + upSpeed, playerWidth);
        if (
            world.CheckForBlock(transform.position + lb) ||
            world.CheckForBlock(transform.position + lf) ||
            world.CheckForBlock(transform.position + rb) ||
            world.CheckForBlock(transform.position + rf)
        )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            return world.CheckForBlock(transform.position + new Vector3(0f, 0f, playerWidth))
            || world.CheckForBlock(transform.position + new Vector3(0f, 1f, playerWidth));
        }
    }

    public bool back
    {
        get
        {
            return world.CheckForBlock(transform.position + new Vector3(0f, 0f, -playerWidth))
            || world.CheckForBlock(transform.position + new Vector3(0f, 1f, -playerWidth));
        }
    }

    public bool left
    {
        get
        {
            return world.CheckForBlock(transform.position + new Vector3(-playerWidth, 0f, 0f))
            || world.CheckForBlock(transform.position + new Vector3(-playerWidth, 1f, 0f));
        }
    }

    public bool right
    {
        get
        {
            return world.CheckForBlock(transform.position + new Vector3(playerWidth, 0f, 0f))
            || world.CheckForBlock(transform.position + new Vector3(playerWidth, 1f, 0f));
        }
    }
}
