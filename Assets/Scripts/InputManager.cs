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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if(controls == null)
        {
            controls = new ArcherControls();

            controls.Player.Movement.performed += ctx => characterMovement.MovementInput = ctx.ReadValue<Vector2>().normalized;

            controls.Player.Sprint.started += ctx => characterMovement.IsSpritting = true;
            controls.Player.Sprint.performed += ctx => characterMovement.IsSpritting = true;
            controls.Player.Sprint.canceled += ctx => characterMovement.IsSpritting = false;

            controls.Player.Dodge.performed += ctx => characterMovement.DodgeDive();

            controls.Player.Punch.performed += ctx => characterMovement.Punch();

            controls.Player.Kick.performed += ctx => characterMovement.Kick();

            controls.Player.DrawUndrawBow.performed += ctx => characterMovement.EquipUnequipBow();

            controls.Player.FireBow.started += ctx => characterMovement.IsDrawn = true;
            controls.Player.FireBow.performed += ctx => characterMovement.IsDrawn = true;
            controls.Player.FireBow.canceled += ctx => characterMovement.IsDrawn = false;

            controls.Player.Aim.started += ctx => characterMovement.IsAiming = true;
            controls.Player.Aim.performed += ctx => characterMovement.IsAiming = true;
            controls.Player.Aim.canceled += ctx => characterMovement.IsAiming = false;

            controls.Player.CameraMovement.performed += ctx => characterMovement.MouseInput = ctx.ReadValue<Vector2>();

            controls.Player.Escape.performed += ctx => LockUnlock();

            controls.Player.PowerScale.performed += ctx => characterMovement.PowerInput = (int)ctx.ReadValue<Vector2>().y;
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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
