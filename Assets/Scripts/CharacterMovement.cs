using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] Transform cam;
    Animator anim;
    
    Vector2 movementInput;
    public Vector2 MovementInput { set{movementInput = value;} }
    
    [SerializeField] float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

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
        }
        else
        {
            anim.SetBool("Non-Lock Walk", false);
        }
    }
}
