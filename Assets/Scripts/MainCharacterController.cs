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
    public float groundClearance;
    public float clearanceSpringStrength;
    public float clearanceSpringDamper;
    public float rightingSpringStrength;
    public float rightingSpringDamper;
    public float jumpPower;
    public LayerMask groundMask;
    private bool isGrounded;
    [SerializeField]
    private bool isJumping;

    private Rigidbody rb;
    private Quaternion uprightRotation;

    private Vector3 velocity;

    public float gravity = -9.81f;
    public float turnSmoothing = 0.1f;
    private float turnSmoothVelocity;

    public Vector2 MoveInput { get; set; }
    public Vector2 TurnInput { get; set; }
    public bool JumpInput { get; set; }
    public bool ShootInput { get; set; }

    public float speed = 6f;
    public float accelleration;
    public float maxAccelleration;
    public float maxAccellerationFactor;
    Vector3 goalVelocity = Vector3.zero;
    public AnimationCurve accellerationFactorFromDot;
    public AnimationCurve maxAccellerationForceFactorFromDot;
    public Vector3 forceScale;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        uprightRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        CheckGround();


        float targetAngle = Mathf.Atan2(MoveInput.x, MoveInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothing);
        if (MoveInput.magnitude > 0.01)
        {
            uprightRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * MoveInput.magnitude;

            Vector3 goalVel = moveDir * speed;

            float accel = accelleration; //add other stuff

            goalVelocity = Vector3.MoveTowards(goalVelocity, goalVel, accel);

            Debug.DrawRay(transform.position, goalVelocity * 20, Color.cyan);

            Vector3 neededAccel = (goalVelocity - rb.velocity) / Time.fixedDeltaTime;

            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccelleration);

            rb.AddForce(Vector3.Scale(neededAccel*rb.mass, forceScale));

        /*
        Vector3 moveGoal = moveDir * speed;

        Vector3 unitVel = goalVelocity.normalized;
        float velDot = Vector3.Dot(moveDir, unitVel);

        float accel = accelleration * accellerationFactorFromDot.Evaluate(velDot);

        goalVelocity = Vector3.MoveTowards(goalVelocity, moveGoal, accel);// * Time.fixedDeltaTime);

        Vector3 neededAccel = (goalVelocity - rb.velocity) / Time.fixedDeltaTime;

        float maxAccel = maxAccelleration * maxAccellerationForceFactorFromDot.Evaluate(velDot);

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        rb.AddForce(Vector3.Scale(neededAccel*rb.mass, forceScale));
//            */

        if (JumpInput && isGrounded)
        {
            isJumping = true;
            rb.AddForce(new Vector3(0, jumpPower, 0));
        }
        else
        {
            isJumping = false;
        }

        RightingSpring();


        /*
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
        */
    }

    private void RightingSpring()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion toGoal = uprightRotation * Quaternion.Inverse(currentRotation);

        Vector3 rotAxis;
        float rotDegrees;

        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        rb.AddTorque((rotAxis * (rotRadians * rightingSpringStrength)) - (rb.angularVelocity * rightingSpringDamper));
    }

    private void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundDistance, groundMask))
        {
            isGrounded = true;
            Debug.DrawRay(transform.position, -transform.up * groundDistance, Color.green);

            velocity = rb.velocity;
            Vector3 rayDir = -transform.up;

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, velocity);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = hit.distance - groundClearance;
            float springForce = (x * clearanceSpringStrength) - (relVel * clearanceSpringDamper);

            Debug.Log(isJumping);
            if (!isJumping)
            {
                rb.AddForce(rayDir * springForce);
            }
            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * (isJumping?jumpPower*rb.mass:-springForce), hit.point);
            }
            
        }
        else
        {
            isGrounded = false;
            Debug.DrawRay(transform.position, -transform.up * groundDistance, Color.blue);
        }
    }
}
