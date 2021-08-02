using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement))]
public class InputManager : MonoBehaviour
{
    ArcherControls controls;
    CharacterMovement characterMovement;

    bool cursorLocked = true;

    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        LockUnlock();
    }

    private void OnEnable()
    {
        if(controls == null)
        {
            controls = new ArcherControls();

            controls.Player.Movement.performed += ctx => characterMovement.MovementInput = ctx.ReadValue<Vector2>().normalized;

            controls.Player.Sprint.started += _ => characterMovement.IsSpritting = true;
            controls.Player.Sprint.performed += _ => characterMovement.IsSpritting = true;
            controls.Player.Sprint.canceled += _ => characterMovement.IsSpritting = false;

            controls.Player.Dodge.performed += _ => characterMovement.DodgeDive();

            controls.Player.Punch.performed += _ => characterMovement.Punch();

            controls.Player.Kick.performed += _ => characterMovement.Kick();
        }
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void LockUnlock()
    {
        cursorLocked = !cursorLocked;
        if(cursorLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
