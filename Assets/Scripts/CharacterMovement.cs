using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Attributes))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")] [Space(10)]
    [SerializeField] Transform cam;
    [SerializeField] CinemachineFreeLook freeLookCam;
    [SerializeField] CinemachineVirtualCamera aimCam;
    [SerializeField] Transform characterRoot;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float lockonMovementChangeSpeed = 100f;

    [Header("Stamina Costs")]
    [SerializeField] float dodgeCost = 10f;
    [SerializeField] float punchCost = 10f;
    [SerializeField] float kickCost = 10f;

    [Header("Equipment")]
    [SerializeField] GameObject bow;
    [SerializeField] GameObject disarmedBow;
    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowProjectile;
    [SerializeField] GameObject arrowSpawnPoint;
    [SerializeField] float arrowFireForce = 1000f;

    Vector2 movementInput;
    public Vector2 MovementInput{ set{ movementInput = value; } }

    bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; } }

    bool isDrawn;
    public bool IsDrawn{ set{ isDrawn = value; anim.SetBool(AnimFireArrowHash, isDrawn); } }

    bool isAiming;
    public bool IsAiming { set{ isAiming = value; AimInistalisation(); } }

    [SerializeField] float mouseSenstivity = 100f;

    float xRotation = 0f;

    Vector2 mouseInput;
    public Vector2 MouseInput { set { mouseInput = value; if (isAiming == true) { AimLook(); } } }

    Action characterMovement;

    Animator anim;
    Attributes attributes;
    bool isArmed = true;
    float turnSmoothVelocity;
    int LockMovementMax = 1;
    int LockMovementMin = -1;
    int LockMovementNeutral = 0;

    #region Anim Hashes

    int AnimXHash = Animator.StringToHash("X");
    int AnimYHash = Animator.StringToHash("Y");
    int AnimWalkHash = Animator.StringToHash("Walk");
    int AnimRunHash = Animator.StringToHash("Run");
    int AnimArmedHash = Animator.StringToHash("Armed");
    int AnimDiveHash = Animator.StringToHash("Dive");
    int AnimPunchHash = Animator.StringToHash("Punch");
    int AnimKickHash = Animator.StringToHash("Kick");
    int AnimDrawBowHash = Animator.StringToHash("DrawBow");
    int AnimUndrawBowHash = Animator.StringToHash("UndrawBow");
    int AnimFireArrowHash = Animator.StringToHash("FireArrow");
    int AnimAimingHash = Animator.StringToHash("Aiming");

    #endregion

    void Awake()
    {
        anim = GetComponent<Animator>();
        attributes = GetComponent<Attributes>();
        characterMovement = Movement;
    }

    void Update()
    {
        characterMovement();
        if(isSpritting == false){ attributes.StaminaRegen(); }
    }

    void Movement()
    {
        Vector3 Direction = new Vector3(movementInput.x, 0f, movementInput.y);

        if(Direction.magnitude >= 0.1f)
        {
            float Targetangle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Targetangle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            anim.SetBool(AnimWalkHash, true);
            RunCheck();
        }
        else
        {
            anim.SetBool(AnimWalkHash, false);
            anim.SetBool(AnimRunHash, false);

            if(isDrawn == true)
            {
                float aimAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, freeLookCam.m_XAxis.Value, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, aimAngle, 0f);
            }
        }
    }

    void AimMovement()
    {
        AimMovementX();
        AimMovementY();
    }

    void AimMovementX()
    {
        if(movementInput.x == 0)
        {
            if(anim.GetFloat(AnimXHash) > 0)
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) - (Time.deltaTime * lockonMovementChangeSpeed), LockMovementNeutral, LockMovementMax));
            else if(anim.GetFloat(AnimXHash) < 0)
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementNeutral));
        }
        else
        {
            anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (movementInput.x * Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementMax));
            RunCheck();
        }
    }

    void AimMovementY()
    {
        if(movementInput.y == 0)
        {
            if(anim.GetFloat(AnimYHash) > 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) - (Time.deltaTime * lockonMovementChangeSpeed), LockMovementNeutral, LockMovementMax));
            else if(anim.GetFloat(AnimYHash) < 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementNeutral));
        }
        else
        {
            anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (movementInput.y * Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementMax));
            RunCheck();
        }
    }
    
    void RunCheck()
    {
        if(isSpritting == true)
        {
            isSpritting = attributes.Run();
            anim.SetBool(AnimRunHash, attributes.Run());
        }
        else
        {
            anim.SetBool(AnimRunHash, false);
        }
    }

    public void DodgeDive()
    {
        anim.SetTrigger(AnimDiveHash);
        attributes.ReduceStamina(dodgeCost);
    }

    public void Punch()
    {
        anim.SetTrigger(AnimPunchHash);
        attributes.ReduceStamina(punchCost);
    }

    public void Kick()
    {
        anim.SetTrigger(AnimKickHash);
        attributes.ReduceStamina(kickCost);
    }

    public void EquipUnequipBow()
    {
        if(isArmed)
            anim.SetTrigger(AnimUndrawBowHash);
        else
            anim.SetTrigger(AnimDrawBowHash);

        isArmed = !isArmed;
        anim.SetBool(AnimArmedHash, isArmed);
    }

    public void ToggleBow()
    {
        if (isArmed)
        {
            bow.SetActive(true);
            disarmedBow.SetActive(false);
        }
        else
        {
            bow.SetActive(false);
            disarmedBow.SetActive(true);
        }
    }

    public void FireArrow()
    {
        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y);
        GameObject copy;

        if (direction.magnitude >= 0.1f)
            copy = Instantiate(arrowProjectile, arrowSpawnPoint.transform.position, arrowSpawnPoint.transform.rotation);
        else
            copy = Instantiate(arrowProjectile, arrowSpawnPoint.transform.position, cam.transform.rotation);

        Rigidbody rb = copy.GetComponent<Rigidbody>();

        if (rb == null) { return; }

        rb.AddForce( copy.transform.forward * arrowFireForce, ForceMode.Impulse);
        Destroy(copy, 5f);
    }

    public void ToggleArrow()
    {
        if (isDrawn)
            arrow.SetActive(true);
        else
            arrow.SetActive(false);
    }

    public void AimInistalisation()
    {
        anim.SetBool(AnimAimingHash, isAiming);
        if (isAiming)
        {
            characterMovement = AimMovement;
            anim.SetLayerWeight(1, 1);
            freeLookCam.Priority = 1;
            aimCam.Priority = 2;
        }
        else
        {
            characterMovement = Movement;
            anim.SetLayerWeight(1, 0);
            freeLookCam.Priority = 2;
            aimCam.Priority = 1;
        }
    }

    void AimLook()
    {
        float mouseX = mouseInput.x * mouseSenstivity * Time.deltaTime;
        float mouseY = mouseInput.y * mouseSenstivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70, 70);

        characterRoot.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;   
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z), new Vector3(0.5f, 0.5f, 0.5f));
    }

}
