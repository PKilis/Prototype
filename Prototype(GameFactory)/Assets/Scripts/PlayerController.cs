using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Control Settings")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float gravityModifier = .95f;
    [SerializeField] private float jumpPower = .35f;
    [SerializeField] private InputAction newMovementInput;
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensivity = 1f;
    [SerializeField] private float maxViewAngle = 60f;
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    private CharacterController characterController;

    private float currentSpeed = 8f;
    private float horizontalInput;
    private float verticalInput;

    private bool jump = false;
    private Vector3 heightMovement;

    private Transform mainCamera;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        if (Camera.main.GetComponent<CameraController>() == null)
        {
            Camera.main.gameObject.AddComponent<CameraController>();
        }
        mainCamera = GameObject.FindGameObjectWithTag("CameraPoint").transform;
    }
    private void OnEnable()
    {
        newMovementInput.Enable();
    }
    private void OnDisable()
    {
        newMovementInput.Disable();
    }
    void Update()
    {
        KeyboardInput();
    }

    private void FixedUpdate()
    {
        Move();
        AnimationChanging();
        Rotate();
    }

    private void AnimationChanging()
    {
        if (newMovementInput.ReadValue<Vector2>().magnitude > 0f)
        {
            if (currentSpeed == walkSpeed)
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Run", false);

            }
            else if(currentSpeed == runSpeed)
            {
                anim.SetBool("Run", true);
                anim.SetBool("Walk", false);
            }
        }
        else
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
    }

    private void Rotate()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + MouseInput().x, transform.eulerAngles.z);

        if (mainCamera != null)
        {           
            if (mainCamera.eulerAngles.x > maxViewAngle && mainCamera.eulerAngles.x < 180)
            {
                mainCamera.rotation = Quaternion.Euler(maxViewAngle, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z);
            }
            else if (mainCamera.eulerAngles.x > 180f && mainCamera.eulerAngles.x < 360f - maxViewAngle)
            {
                mainCamera.rotation = Quaternion.Euler(360 - maxViewAngle, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z);
            }
            else
            {
                transform.rotation = Quaternion.Euler(mainCamera.rotation.eulerAngles + new Vector3(-MouseInput().y, 0f, 0f));
            }
        }

    }

    private void Move()
    {
        if (jump)
        {
            heightMovement.y = jumpPower;
            jump = false;
        }

        heightMovement.y -= gravityModifier * Time.deltaTime;

        Vector3 localVerticalVector = transform.forward * verticalInput;
        Vector3 localHorizontcalVector = transform.right * horizontalInput;

        Vector3 movement = localVerticalVector + localHorizontcalVector;
        movement.Normalize();
        movement *= currentSpeed * Time.deltaTime;

        characterController.Move(movement + heightMovement);

        if (characterController.isGrounded)
        {
            heightMovement.y = 0f;
        }
    }

    private void KeyboardInput()
    {
        horizontalInput = newMovementInput.ReadValue<Vector2>().x;
        verticalInput = newMovementInput.ReadValue<Vector2>().y;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && characterController.isGrounded)
        {
            jump = true;
        }
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed = runSpeed;
            anim.SetBool("Run",true);
        }
        else
        {
            currentSpeed = walkSpeed;
            anim.SetBool("Run", false);

        }
    }
    private Vector2 MouseInput()
    {
        return new Vector2(invertX ? -Mouse.current.delta.x.ReadValue() : Mouse.current.delta.x.ReadValue(),
            invertY ? -Mouse.current.delta.y.ReadValue() : Mouse.current.delta.y.ReadValue()) * mouseSensivity;

        #region if input
        /*Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (invertX)
        {
            mouseInput.x = -mouseInput.x;
        }
        if (invertY)
        {
            mouseInput.y = -mouseInput.y;
        }*/
        #endregion
    }
}
