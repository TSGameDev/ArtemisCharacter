using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Attributes))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Cameras")] [Space(10)]
    [Tooltip("The camera game object that is used for the player character")]
    [SerializeField] private Transform cam;
    [Tooltip("The cinemachine freelock rig for the player characters orbital camera view")]
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [Tooltip("The cinemachine virtaul camera used for the player characters aim camera view")]
    [SerializeField] private CinemachineVirtualCamera aimCam;

    [Header("Movement Settings")]
    [Tooltip("The root of the player character game object for oribtal camera lookat and aim camera follow")]
    [SerializeField] private Transform characterRoot;
    [Tooltip("The time it takes to complete a turn in orbital camera mode, longer values makes the turn longer while shorter times make the turns more snappy.")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    [Tooltip("The time it takes for animation X and Y values to reach their min, max and neatural positions. Larger values mean a longer time to reach top walking and running speed when aiming")]
    [SerializeField] private float lockonMovementChangeSpeed = 100f;
    [Tooltip("The sensitivity of the mouse when aiming")]
    [SerializeField] private float aimSensitivity = 50f;

    [Header("Stamina Costs")]
    [Tooltip("The stamina cost of a dodge roll or dive.")]
    [SerializeField] private float dodgeCost = 10f;
    [Tooltip("The stamina cost of a punch action")]
    [SerializeField] private float punchCost = 10f;
    [Tooltip("The stamina cost of a kick action")]
    [SerializeField] private float kickCost = 10f;

    [Header("Equipment")]
    [Tooltip("The layer for the arrow raycast to hit. should include default layer and any additional layers that arrows may need to interact with.")]
    [SerializeField] private LayerMask hitLayer;
    [Tooltip("The skinned bow gameobject on the erica prefab")]
    [SerializeField] private GameObject bow;
    [Tooltip("The bow gameobject on the back of the erica prefav")]
    [SerializeField] private GameObject disarmedBow;
    [Tooltip("The attached arrow gameobject to the erica prefab")]
    [SerializeField] private GameObject arrow;
    [Tooltip("The arrow prefab made for spawning and firing")]
    [SerializeField] private GameObject arrowProjectile;
    [Tooltip("The arrow spawnpoint on the erica prefab")]
    [SerializeField] private GameObject arrowSpawnPoint;
    [Tooltip("The max power of the players shot")]
    [SerializeField] private int arrowForceMax;
    [Tooltip("The min power of the players shot")]
    [SerializeField] private int arrowForceMin;

    [Header("UI Settings")]
    [Tooltip("The text element for displaying current power")]
    [SerializeField] private TextMeshProUGUI powerTxt;

    [Header("SFX")]
    [SerializeField] private AudioSource footStepSource;
    [SerializeField] private AudioSource bowSource;

    [Tooltip("The array/collection of footstep SFX to be played at random when play is walking on stone")]
    [SerializeField] private AudioClip[] stoneFootstepSfx;
    [Tooltip("The SFX of the string being pulledback")]
    [SerializeField] private AudioClip bowdrawingSfx;
    [Tooltip("The SFX of the string being released")]
    [SerializeField] private AudioClip bowreleasingSfx;
    
    #region Get-Setters

    //Setter for the WASD movement input from the input manager
    private Vector2 movementInput;
    public Vector2 MovementInput{ set{ movementInput = value; } }

    //Setter for the SHIFT sprining from the input manager
    private bool isSpritting;
    public bool IsSpritting { set{ isSpritting = value; } }

    //Setter for the LEFT MOUSE firing from the input manager, also sets the FireArrow anim bool to the value of isDrawn when ever the input manager gets an input
    private bool isDrawn;
    public bool IsDrawn{ set{ isDrawn = value; anim.SetBool(AnimFireArrowHash, isDrawn); } }

    //Setter for the RIGHT MOUSE aiming from the input manager, also calls the AimInistalisation function and makes the anim bool of Aiming equal to the isAiming bool when the input manager notices an input
    private bool isAiming;
    public bool IsAiming { set{ isAiming = value; AimInistalisation(); anim.SetBool(AnimAimingHash, isAiming); } }

    //Setter for the MOUSE input from the input manager, also calls the AimLook function if the player is aiming when the input manager notices an input
    private Vector2 mouseInput;
    public Vector2 MouseInput { set{ mouseInput = value; if (isAiming == true) { AimLook(); } } }

    private int powerInput;
    public int PowerInput { set{ powerInput = value; PowerScale(); }  }

    #endregion

    #region Private Variables

    //the delegate for the currently active movement style
    private Action characterMovement;
    //refernece to the players animator
    private Animator anim;
    //reference ot the attributes script
    private Attributes attributes;
    //bool for keeping track of if the player if currently got a bow equiped or not
    private bool isArmed = true;
    //float to be used as a refence to store the current velocity of the players turning when in orbial movement style
    private float turnSmoothVelocity;
    //float for the aiming camera, used to inverse and clamp the Y mouse input
    private float xRotation = 0f;
    //The min, max and neutral values for the anim X and Y floats for aiming movement
    private int aimMovmentMax = 1;
    private int aimMovementMin = -1;
    private int aimMovementNeutral = 0;
    //the current force behind the arrow
    private float arrowFireForce = 1000f;

    #endregion

    #region Anim Hashes

    //list of the animation floats, bools and triggers store in int varibales for easier changing
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
        //gathering required references and setting the movement delegate to the default orbital movement style
        anim = GetComponent<Animator>();
        attributes = GetComponent<Attributes>();
        characterMovement = Movement;
        PowerScale();
    }

    void Update()
    {
        //constantly calling the current movement style and regening stamina if the player isn't sprinting
        characterMovement();
        if(isSpritting == false){ attributes.StaminaRegen(); }
    }

    //Default orbital style movement
    void Movement()
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
    void AimMovement()
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

    //Makes the character dodge or dive and reduces stamina by the dodge cost
    public void DodgeDive()
    {
        anim.SetTrigger(AnimDiveHash);
        attributes.ReduceStamina(dodgeCost);
    }

    //Makes the character punch and reduces stamina by punch cost
    public void Punch()
    {
        anim.SetTrigger(AnimPunchHash);
        attributes.ReduceStamina(punchCost);
    }

    //Makes the character kick and reduces stamina by kick cost
    public void Kick()
    {
        anim.SetTrigger(AnimKickHash);
        attributes.ReduceStamina(kickCost);
    }

    //Makes the player equip or unequip their bow, also makes the isArmed bool equal to its opposite to keep track.
    public void EquipUnequipBow()
    {
        if(isArmed)
            anim.SetTrigger(AnimUndrawBowHash);
        else
            anim.SetTrigger(AnimDrawBowHash);

        isArmed = !isArmed;
        anim.SetBool(AnimArmedHash, isArmed);
    }

    //Toggles the graphical elements for equiping and unequiping the bow
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

    //Spawns in the arrow projectile with it facing towards the crosshair point in world space and applys a forward force to fire the arrow.
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

    //toggle the graphical element of drawing and knocking the arrow
    public void ToggleArrow()
    {
        if (isDrawn && isAiming)
            arrow.SetActive(true);
        else
            arrow.SetActive(false);
    }

    //Changes a bunch of settings to swap between orbital movement and aiming moveming
    public void AimInistalisation()
    {
        if (isAiming)
        {
            Vector3 aimPos = ScreenCentrePointRay().point;
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

    //handles the aiming logic
    void AimLook()
    {
        float mouseX = mouseInput.x * Time.deltaTime * aimSensitivity;
        float mouseY = mouseInput.y * Time.deltaTime * aimSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70, 70);

        characterRoot.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void PowerScale()
    {
        if(powerInput >= 0)
        {
            arrowFireForce = Mathf.Clamp(arrowFireForce + 5, arrowForceMin, arrowForceMax);
        }
        else if(powerInput <= 0)
        {
            arrowFireForce = Mathf.Clamp(arrowFireForce - 5, arrowForceMin, arrowForceMax);
        }
        powerTxt.text = arrowFireForce.ToString();
    }

    //handles the setting and playing of a footstep soundeffect
    public void FootStepSFX()
    {
        if (footStepSource.isPlaying == true) { footStepSource.Stop(); }

        int ranNum = UnityEngine.Random.Range(1, stoneFootstepSfx.Length);
        footStepSource.clip = stoneFootstepSfx[ranNum];
        footStepSource.Play();
    }

    public void ArrowFireSFX(int IsBeingDrawn)
    {
        if (bowSource.isPlaying == true) { bowSource.Stop(); }

        switch(IsBeingDrawn)
        {
            case 1:
                bowSource.clip = bowdrawingSfx;
                break;
            case 2:
                bowSource.clip = bowreleasingSfx;
                break;
        }

        bowSource.Play();
    }

    //casts a ray from the centre of the screen and returns the hit data
    RaycastHit ScreenCentrePointRay()
    {
        Vector2 screenCentrePoint = new Vector2(Screen.width / 2f, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentrePoint);
        Physics.Raycast(ray, out RaycastHit raycastHit, 999f, hitLayer);
        return raycastHit;
    }


}
