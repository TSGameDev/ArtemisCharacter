using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] GameObject bow;
    Animator anim;
    [SerializeField] float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    Vector2 movementInput;
    public Vector2 MovementInput { set{ movementInput = value; } }

    bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; } }

    bool isArmed = true;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 Direction = new Vector3(movementInput.x, 0f, movementInput.y);
        if(Direction.magnitude >= 0.1f)
        {
            float Targetangle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Targetangle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            anim.SetBool("Non-Lock Walk", true);
            anim.SetBool("Non-Lock Run", isSpritting);
        }
        else
        {
            anim.SetBool("Non-Lock Walk", false);
            anim.SetBool("Non-Lock Run", false);
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
}
