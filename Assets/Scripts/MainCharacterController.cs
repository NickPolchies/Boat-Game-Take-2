using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;


    //Grounded code
    public Transform groundCheck;
    public float groundDistance;
    public float jumpPower;
    public LayerMask groundMask;
    private bool isGrounded;

    private Vector3 velocity;

    public float gravity = -9.81f;
    public float turnSmoothing = 0.1f;
    private float turnSmoothVelocity;

    public Vector2 MoveInput { get; set; }
    public Vector2 TurnInput { get; set; }
    public bool JumpInput { get; set; }
    public bool ShootInput { get; set; }

    public float speed = 6f;

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        if(JumpInput && isGrounded)
        {
            velocity.y = jumpPower;
        }

        if(MoveInput.magnitude > 0.01)
        {
            float targetAngle = Mathf.Atan2(MoveInput.x, MoveInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothing);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
