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
    [SerializeField] private Transform cam;
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;
    [SerializeField] private Transform characterRoot;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float lockonMovementChangeSpeed = 100f;

    [Header("Stamina Costs")]
    [SerializeField] private float dodgeCost = 10f;
    [SerializeField] private float punchCost = 10f;
    [SerializeField] private float kickCost = 10f;

    [Header("Equipment")]
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private GameObject bow;
    [SerializeField] private GameObject disarmedBow;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject arrowProjectile;
    [SerializeField] private GameObject arrowSpawnPoint;
    [SerializeField] private float arrowFireForce = 1000f;

    private Vector2 movementInput;
    public Vector2 MovementInput{ set{ movementInput = value; } }

    private bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; } }

    [SerializeField]private bool isDrawn;
    public bool IsDrawn{ set{ isDrawn = value; anim.SetBool(AnimFireArrowHash, isDrawn); } }

    private bool isAiming;
    public bool IsAiming { set{ isAiming = value; AimInistalisation(); anim.SetBool(AnimAimingHash, isAiming); } }

    [SerializeField] private float mouseSenstivity = 50f;

    private float xRotation = 0f;

    private Vector2 mouseInput;
    public Vector2 MouseInput { set { mouseInput = value; if (isAiming == true) { AimLook(); } } }

    private Action characterMovement;

    private Animator anim;
    private Attributes attributes;
    private bool isArmed = true;
    private float turnSmoothVelocity;
    private int LockMovementMax = 1;
    private int LockMovementMin = -1;
    private int LockMovementNeutral = 0;

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
    int AnimAimingHash = Animator.StringToHash("isAiming");

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
        GameObject copy;

        copy = Instantiate(arrowProjectile, arrowSpawnPoint.transform.position, Quaternion.identity);
        copy.transform.LookAt(ScreenCentrePointRay().point);

        Rigidbody rb = copy.GetComponent<Rigidbody>();

        if (rb == null) { return; }

        rb.AddForce( copy.transform.forward * arrowFireForce, ForceMode.Impulse);
        Destroy(copy, 5f);
    }

    public void ToggleArrow()
    {
        if (isDrawn && isAiming)
            arrow.SetActive(true);
        else
            arrow.SetActive(false);
    }

    public void AimInistalisation()
    {
        if (isAiming)
        {
            Vector3 aimPos = ScreenCentrePointRay().point;
            characterRoot.transform.LookAt(aimPos);
            aimPos.y = transform.position.y;
            transform.LookAt(aimPos);

            characterMovement = AimMovement;
            anim.SetLayerWeight(1, 1);
            freeLookCam.Priority = 1;
            aimCam.Priority = 2;
        }
        else
        {
            ToggleArrow();
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

    RaycastHit ScreenCentrePointRay()
    {
        Vector2 screenCentrePoint = new Vector2(Screen.width / 2f, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentrePoint);
        Physics.Raycast(ray, out RaycastHit raycastHit, 999f, hitLayer);
        return raycastHit;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;   
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z), new Vector3(0.5f, 0.5f, 0.5f));
    }

}
