using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private MainCharacterController charController;

    private void Awake()
    {
        charController = GetComponent<MainCharacterController>();
    }
    void Update()
    {
        /*
        int vertical = Mathf.RoundToInt(Input.GetAxis("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxis("Horizontal"));
        bool jump = Input.GetKey("Jump");

        charController.ForwardInput = vertical;
        charController.TurnInput = horizontal;
        charController.JumpInput = jump;
        */
    }

    public void Move(InputAction.CallbackContext context)
    {
        charController.MoveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        charController.TurnInput += context.ReadValue<Vector2>();
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        charController.ShootInput = context.ReadValue<float>() > 0 ? true : false;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        charController.JumpInput = context.ReadValue<float>() > 0 ? true : false;
    }
}
