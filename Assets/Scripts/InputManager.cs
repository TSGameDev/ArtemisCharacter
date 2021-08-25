using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(ActionHandler))]
public class InputManager : MonoBehaviour
{
    ArcherControls controls;
    CharacterMovement characterMovement;
    ActionHandler actionHandler;
    FiringScript fireScript;

    bool cursorLocked = true;

    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        actionHandler = GetComponent<ActionHandler>();
        fireScript = GetComponent<FiringScript>();
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

            controls.Player.Dodge.performed += ctx => actionHandler.Dodge();

            controls.Player.Punch.performed += ctx => actionHandler.Punch();

            controls.Player.Kick.performed += ctx => actionHandler.Kick();

            controls.Player.DrawUndrawBow.performed += ctx => actionHandler.EquipUnequipBow();

            controls.Player.FireBow.started += ctx => fireScript.IsDrawn = true;
            controls.Player.FireBow.performed += ctx => fireScript.IsDrawn = true;
            controls.Player.FireBow.canceled += ctx => fireScript.IsDrawn = false;

            controls.Player.Aim.started += ctx => fireScript.IsAiming = true;
            controls.Player.Aim.performed += ctx => fireScript.IsAiming = true;
            controls.Player.Aim.canceled += ctx => fireScript.IsAiming = false;

            controls.Player.CameraMovement.performed += ctx => fireScript.MouseInput = ctx.ReadValue<Vector2>().normalized;

            controls.Player.Escape.performed += ctx => LockUnlock();

            controls.Player.PowerScale.performed += ctx => fireScript.PowerInput = (int)ctx.ReadValue<Vector2>().y;
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
