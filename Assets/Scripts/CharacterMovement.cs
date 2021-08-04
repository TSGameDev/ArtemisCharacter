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
    public bool IsSpritting { set{ isSpritting = value; } }

    Animator anim;
    Transform LockedEnemy;
    bool isArmed = true;
    bool isLocked = false;
    float turnSmoothVelocity;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if(!isLocked)
        {
            Movement();
        }
        else
        {
            LockOnMovement();
            LookAtLockedEnemy();
        }
    }

    public void DodgeDive()
    {
        anim.SetTrigger("Dive");
    }

    public void Punch()
    {
        anim.SetTrigger("Punch");
    }

    public void Kick()
    {
        anim.SetTrigger("Kick");
    }

    public void AnimDrawUndrawBow()
    {
        if(isArmed)
        {
            anim.SetTrigger("UndrawBow");
            bow.SetActive(false);
        }
        else
        {
            anim.SetTrigger("DrawBow");
            bow.SetActive(true);
        }

        isArmed = !isArmed;
        anim.SetBool("Armed", isArmed);
    }

    public void FireBow()
    {
        anim.SetTrigger("FireArrow");
    }

    public void ToggleLockon()
    {
        isLocked = !isLocked;

        if(isLocked)
        {
            anim.SetLayerWeight(1, 1);
            freeLookCam.Priority = 1;
            lockCam.Priority = 2;
            LockedEnemy = FindClosestEnemy();
            lockCam.LookAt = LockedEnemy;
        }
        else
        {
            anim.SetLayerWeight(1,0);
            freeLookCam.Priority = 2;
            lockCam.Priority = 1;
            lockCam.LookAt = null;
        }
    }

    Transform FindClosestEnemy()
    {
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

    void Movement()
    {
        Vector3 Direction = new Vector3(movementInput.x, 0f, movementInput.y);

        if(Direction.magnitude >= 0.1f)
        {
            float Targetangle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Targetangle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            anim.SetBool("Walk", true);
            anim.SetBool("Run", isSpritting);
        }
        else
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
    }

    void LockOnMovement()
    {
        LockOnMovementX();
        LockOnMovementY();
        LockOnMovementFloatClamp();
        anim.SetBool("Run", isSpritting);
    }

    void LockOnMovementX()
    {
        if(movementInput.x == 0)
        {
            if(anim.GetFloat("X") > 0)
            {
                anim.SetFloat("X", anim.GetFloat("X") - (Time.deltaTime * lockonMovementChangeSpeed));
                if(anim.GetFloat("X") <= 0)
                    anim.SetFloat("X", 0);
            }
            else if(anim.GetFloat("X") < 0)
            {
                anim.SetFloat("X", anim.GetFloat("X") + (Time.deltaTime * lockonMovementChangeSpeed));
                if(anim.GetFloat("X") >= 0)
                    anim.SetFloat("X", 0);
            }
        }
        else
            anim.SetFloat("X", anim.GetFloat("X") + (movementInput.x * Time.deltaTime * lockonMovementChangeSpeed));
    }

    void LockOnMovementY()
    {
        if(movementInput.y == 0)
        {
            if(anim.GetFloat("Y") > 0)
            {
                anim.SetFloat("Y", anim.GetFloat("Y") - (Time.deltaTime * lockonMovementChangeSpeed));
                if(anim.GetFloat("Y") <= 0)
                    anim.SetFloat("Y", 0);
            }
            else if(anim.GetFloat("Y") < 0)
            {
                anim.SetFloat("Y", anim.GetFloat("Y") + (Time.deltaTime * lockonMovementChangeSpeed));
                if(anim.GetFloat("Y") >= 0)
                    anim.SetFloat("Y", 0);
            }
        }
        else
            anim.SetFloat("Y", anim.GetFloat("Y") + (movementInput.y * Time.deltaTime * lockonMovementChangeSpeed));
    }

    void LockOnMovementFloatClamp()
    {
        if(anim.GetFloat("X") > 1)
            anim.SetFloat("X", 1);
        else if(anim.GetFloat("X") < -1)
            anim.SetFloat("X", -1);

        if(anim.GetFloat("Y") > 1)
            anim.SetFloat("Y", 1);
         else if(anim.GetFloat("Y") < -1)
            anim.SetFloat("Y", -1);
    }

    void LookAtLockedEnemy()
    {
        if(LockedEnemy == null){ return; }

        Vector3 enemyPos = LockedEnemy.position;
        enemyPos.y = transform.position.y;

        transform.LookAt(enemyPos, Vector3.up);
    }
}
