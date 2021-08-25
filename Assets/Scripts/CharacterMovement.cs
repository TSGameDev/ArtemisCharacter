using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Attributes))]
[RequireComponent(typeof(UIHandler))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Cameras")] [Space(10)]
    [Tooltip("The camera game object that is used for the player character")]
    [SerializeField] private Transform cam;
    [Tooltip("The cinemachine freelock rig for the player characters orbital camera view")]
    [SerializeField] public CinemachineFreeLook freeLookCam;
    [Tooltip("The cinemachine virtaul camera used for the player characters aim camera view")]
    [SerializeField] public CinemachineVirtualCamera aimCam;

    [Header("Movement Settings")]
    [Tooltip("The time it takes to complete a turn in orbital camera mode, longer values makes the turn longer while shorter times make the turns more snappy.")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    [Tooltip("The time it takes for animation X and Y values to reach their min, max and neatural positions. Larger values mean a longer time to reach top walking and running speed when aiming")]
    [SerializeField] private float lockonMovementChangeSpeed = 100f;
    
    #region Get-Setters

    //Setter for the WASD movement input from the input manager
    private Vector2 movementInput;
    public Vector2 MovementInput{ set{ movementInput = value; } }

    //Setter for the SHIFT sprining from the input manager
    private bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; } }

    #endregion

    #region Private Variables

    //the delegate for the currently active movement style
    public Action characterMovement;
    //refernece to the players animator
    private Animator anim;
    //reference ot the attributes class
    private Attributes attributes;
    //float to be used as a refence to store the current velocity of the players turning when in orbial movement style
    private float turnSmoothVelocity;
    //The min, max and neutral values for the anim X and Y floats for aiming movement
    private int aimMovmentMax = 1;
    private int aimMovementMin = -1;
    private int aimMovementNeutral = 0;

    #endregion

    #region Anim Hashes

    //list of the animation floats, bools and triggers store in int varibales for easier changing
    int AnimXHash = Animator.StringToHash("X");
    int AnimYHash = Animator.StringToHash("Y");
    int AnimWalkHash = Animator.StringToHash("Walk");
    int AnimRunHash = Animator.StringToHash("Run");
    
    #endregion

    void Awake()
    {
        //gathering required references and setting the movement delegate to the default orbital movement style
        anim = GetComponent<Animator>();
        attributes = GetComponent<Attributes>();
        characterMovement = Movement;
    }

    void Update()
    {
        //constantly calling the current movement style and regening stamina if the player isn't sprinting
        characterMovement();
        if(isSpritting == false){ attributes.StaminaRegen(); }
    }

    //Default orbital style movement
    public void Movement()
    {
        //new vector3 taking in the WASD movement inputs
        Vector3 Direction = new Vector3(movementInput.x, 0f, movementInput.y);

        //if there is any input on the WASD movement keys
        if(Direction.magnitude >= 0.1f)
        {
            //creates the desired target angle for the player based on input
            float Targetangle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //creates the actual smooth angle transition based on the preivous angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Targetangle, ref turnSmoothVelocity, turnSmoothTime);
            //sets the players rotation to the new smooth angle
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //makes the character walk
            anim.SetBool(AnimWalkHash, true);
            //calls the function RunCheck
            RunCheck();
        }
        else
        {
            //if there isn't any WASD input walk and run anim bools become false
            anim.SetBool(AnimWalkHash, false);
            anim.SetBool(AnimRunHash, false);
        }
    }

    //main call function of the Aim style movement
    public void AimMovement()
    {
        AimMovementX();
        AimMovementY();
    }

    //Handles the Aim movement based around the X anim float value
    void AimMovementX()
    {
        //if there is NO X axis input, Left or Right.
        if(movementInput.x == 0)
        {
            //If the anim X value is greater than 0
            if(anim.GetFloat(AnimXHash) > 0)
                //Makes the X value slowly decrease to the neutral value of X
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) - (Time.deltaTime * lockonMovementChangeSpeed), aimMovementNeutral, aimMovmentMax));
            //If the anim X value is less than 0
            else if(anim.GetFloat(AnimXHash) < 0)
                //Makes the anim X value slowly increase to the neautral value of X
                anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (Time.deltaTime * lockonMovementChangeSpeed), aimMovementMin, aimMovementNeutral));
        }
        else //if there is X axis input, Left or Right.
        {
            //make the X value slowly increase or decrease to the min or max based on the value of X axis input (the direction of input meaning -1 to the left or 1 to the right)
            anim.SetFloat(AnimXHash, Mathf.Clamp(anim.GetFloat(AnimXHash) + (movementInput.x * Time.deltaTime * lockonMovementChangeSpeed), aimMovementMin, aimMovmentMax));
            //calls the function RunCheck
            RunCheck();
        }
    }

    //Handles the Aim movement based around the Y anim float value (Is the same as X but with Y inplace of X)
    void AimMovementY()
    {
        if(movementInput.y == 0)
        {
            if(anim.GetFloat(AnimYHash) > 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) - (Time.deltaTime * lockonMovementChangeSpeed), aimMovementNeutral, aimMovmentMax));
            else if(anim.GetFloat(AnimYHash) < 0)
                anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (Time.deltaTime * lockonMovementChangeSpeed), aimMovementMin, aimMovementNeutral));
        }
        else
        {
            anim.SetFloat(AnimYHash, Mathf.Clamp(anim.GetFloat(AnimYHash) + (movementInput.y * Time.deltaTime * lockonMovementChangeSpeed), aimMovementMin, aimMovmentMax));
            RunCheck();
        }
    }
    
    //Makes the character run if the player has pressed and held shift(detected in inputmanager script)
    void RunCheck()
    {
        //if the player is currently holding shift
        if(isSpritting == true)
        {
            //calls the Run function in the attributes script, comes back as false, stopping the run, if the player doesn't have enougn stamina.
            isSpritting = attributes.Run();
            //makes the character run
            anim.SetBool(AnimRunHash, attributes.Run());
        }
        else
        {
            //if the player isn't holding SHIFT then the Run anim bool is set to false
            anim.SetBool(AnimRunHash, false);
        }
    }

}
