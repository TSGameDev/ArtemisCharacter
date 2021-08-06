using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Animator))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")] [Space(10)]
    [SerializeField] Transform cam;
    [SerializeField] CinemachineFreeLook freeLookCam;
    [SerializeField] CinemachineVirtualCamera lockCam;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float lockonMovementChangeSpeed = 100f;
    
    [Header("Lock-On Settings")]
    [SerializeField] float lockonRadius = 10f;
    [SerializeField] LayerMask enemyLayer;

    [Header("Equipment")]
    [SerializeField] GameObject bow;

    Vector2 movementInput;
    public Vector2 MovementInput { set{ movementInput = value; } }

    bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; anim.SetBool(AnimRunHash, isSpritting);} }

    bool isAiming;
    public bool IsAiming{ set{isAiming = value; anim.SetBool(AnimFireArrowHash, isAiming);} }

    Action characterMovement;

    Animator anim;
    Transform LockedEnemy;
    bool isArmed = true;
    bool isLocked = false;
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
    int AnimFallingStateHash = Animator.StringToHash("Falling_Loop");
    int AnimFallToIdleHash = Animator.StringToHash("FallToIdle");
    int AnimFallToMovementHash = Animator.StringToHash("FallToMovement");

    #endregion

    void Awake()
    {
        anim = GetComponent<Animator>();
        characterMovement = Movement;
    }

    void Update()
    {
        characterMovement();
        FallingCheck();
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
        }
        else
        {
            anim.SetBool(AnimWalkHash, false);
            anim.SetBool(AnimRunHash, false);

            if(isAiming == true)
            {
                float aimAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, freeLookCam.m_XAxis.Value, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, aimAngle, 0f);
            }
        }
    }

    void LockOnMovement()
    {
        LockOnMovementX();
        LockOnMovementY();
        LookAtLockedEnemy();
    }

    void LockOnMovementX()
    {
        if(movementInput.x == 0)
        {
            if(anim.GetFloat(AnimXHash) > 0)
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) - (Time.deltaTime * lockonMovementChangeSpeed), LockMovementNeutral, LockMovementMax));
            else if(anim.GetFloat(AnimXHash) < 0)
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementNeutral));
        }
        else
            anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (movementInput.x * Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementMax));
    }

    void LockOnMovementY()
    {
        if(movementInput.y == 0)
        {
            if(anim.GetFloat(AnimYHash) > 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) - (Time.deltaTime * lockonMovementChangeSpeed), LockMovementNeutral, LockMovementMax));
            else if(anim.GetFloat(AnimYHash) < 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementNeutral));
        }
        else
            anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (movementInput.y * Time.deltaTime * lockonMovementChangeSpeed), LockMovementMin, LockMovementMax));
    }

    void LookAtLockedEnemy()
    {
        if(LockedEnemy == null){ return; }

        Vector3 enemyPos = LockedEnemy.position;
        enemyPos.y = transform.position.y;

        transform.LookAt(enemyPos, Vector3.up);
    }

    public void DodgeDive()
    {
        anim.SetTrigger(AnimDiveHash);
    }

    public void Punch()
    {
        anim.SetTrigger(AnimPunchHash);
    }

    public void Kick()
    {
        anim.SetTrigger(AnimKickHash);
    }

    public void AnimDrawUndrawBow()
    {
        if(isArmed)
        {
            anim.SetTrigger(AnimUndrawBowHash);
            bow.SetActive(false);
        }
        else
        {
            anim.SetTrigger(AnimDrawBowHash);
            bow.SetActive(true);
        }

        isArmed = !isArmed;
        anim.SetBool(AnimArmedHash, isArmed);
    }

    public void ToggleLockon()
    {
        LockedEnemy = FindClosestEnemy();
        if(LockedEnemy != null)
        {
            isLocked = !isLocked;
            if(isLocked)
            {
                characterMovement = LockOnMovement;
                anim.SetLayerWeight(1, 1);
                freeLookCam.Priority = 1;
                lockCam.Priority = 2;
                lockCam.LookAt = LockedEnemy;

            }
            else
            {
                characterMovement = Movement;
                anim.SetLayerWeight(1,0);
                freeLookCam.Priority = 2;
                lockCam.Priority = 1;
                lockCam.LookAt = null;
            }
        }
    }

    Transform FindClosestEnemy()
    {
        Debug.Log("Finding enemy");
        float ClosestEnemyDis = Mathf.Infinity;
        Transform ClosestEnemy = null;

        Collider[] Enemies = Physics.OverlapSphere(transform.position, lockonRadius, enemyLayer);

        foreach(Collider enemy in Enemies)
        {
            float enemyDis = Vector3.Distance(transform.position, enemy.transform.position);

            if(enemyDis < ClosestEnemyDis)
            {
                ClosestEnemyDis = enemyDis;
                ClosestEnemy = enemy.transform;
            }
        }

        return ClosestEnemy;
    }

    void FallingCheck()
    {
        Collider[] GroundObjs = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z), new Vector3(0.25f, 0.25f, 0.25f));
        if(GroundObjs.Length == 0)
        {
            anim.Play(AnimFallingStateHash);
            Debug.Log("Falling!");
        }
        else
        {
            Vector3 Direction = new Vector3(movementInput.x, 0f, movementInput.y);

            if(Direction.magnitude >= 0.1)
            {
                anim.SetTrigger(AnimFallToMovementHash);
            }
            else
            {
                anim.SetTrigger(AnimFallToIdleHash);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;   
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z), new Vector3(0.5f, 0.5f, 0.5f));
        Gizmos.DrawWireSphere(transform.position, lockonRadius);
    }
}
