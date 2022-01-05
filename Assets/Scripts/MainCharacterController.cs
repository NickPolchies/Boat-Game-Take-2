using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    public float moveSpeed = 2f;
    public float turnSpeed = 300f;
    public bool allowJump = true;
    public float jumpSpeed = 40f;
    public float yLookSensitivity = 0.5f;
    public float xLookSensitivity = 0.5f;

    public bool IsGrounded { get; private set; }
    public Vector2 MoveInput { get; set; }
    public Vector2 TurnInput { get; set; }
    public bool JumpInput { get; set; }

    new private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider;
    [SerializeField] private GameObject cameraAnchor;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ProcessActions();
    }

    private void CheckGrounded()
    {
        IsGrounded = false;
        float capsuleHeight = Mathf.Max(capsuleCollider.radius * 2f, capsuleCollider.height);
        Vector3 capsuleBottom = transform.TransformPoint(capsuleCollider.center - Vector3.up * capsuleHeight / 2f);
        float radius = transform.TransformVector(capsuleCollider.radius, 0f, 0f).magnitude;

        Ray ray = new Ray(capsuleBottom + transform.up * 0.01f, -transform.up);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, radius * 5f))
        {
            float normalAngle = Vector3.Angle(hit.normal, transform.up);
            if(normalAngle < slopeLimit)
            {
                float maxDist = radius / Mathf.Cos(Mathf.Deg2Rad * normalAngle) - radius + 0.02f;
                if(hit.distance < maxDist)
                {
                    IsGrounded = true;
                }
            }
        }
    }

    private void ProcessActions()
    {
        if(TurnInput != Vector2.zero)
        {
            transform.Rotate(Vector3.up, TurnInput.x * xLookSensitivity);

            float yLook = cameraAnchor.transform.eulerAngles.x;
            
            Debug.Log(yLook + ", " + (yLook + TurnInput.y * yLookSensitivity));

//            if(yLook + TurnInput.y < 89f)
            {
                cameraAnchor.transform.Rotate(Vector3.right, TurnInput.y * yLookSensitivity);
            }
            TurnInput = Vector2.zero;
        }

        MoveInput.Normalize();
        Vector3 move = transform.forward * MoveInput.y * moveSpeed * Time.fixedDeltaTime
                     + transform.right * MoveInput.x * moveSpeed * Time.fixedDeltaTime;
        rigidbody.MovePosition(transform.position + move);

        if(JumpInput && allowJump && IsGrounded)
        {
            rigidbody.AddForce(transform.up * jumpSpeed, ForceMode.VelocityChange);
        }
    }
}
