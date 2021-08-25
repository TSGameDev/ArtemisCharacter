using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringScript : MonoBehaviour
{
    [Header("Aim Settings")]
    [Tooltip("The sensitivity of the mouse when aiming")]
    [SerializeField] private float aimSensitivity = 50f;
    [Tooltip("The root of the player character game object for oribtal camera lookat and aim camera follow")]
    [SerializeField] private Transform characterRoot;

    [Header("Equipment")]
    [Tooltip("The layer for the arrow raycast to hit. should include default layer and any additional layers that arrows may need to interact with.")]
    [SerializeField] private LayerMask hitLayer;
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

    #region Private Variables

    private CharacterMovement charMovement;
    private UIHandler UI;
    private Animator anim;

    //float for the aiming camera, used to inverse and clamp the Y mouse input
    private float xRotation = 0f;
    //the current force behind the arrow
    private int arrowFireForce = 100;

    #endregion

    #region Get-Setters

    //Setter for the LEFT MOUSE firing from the input manager, also sets the FireArrow anim bool to the value of isDrawn when ever the input manager gets an input
    private bool isDrawn;
    public bool IsDrawn { set { isDrawn = value; anim.SetBool(AnimFireArrowHash, isDrawn); } }

    //Setter for the RIGHT MOUSE aiming from the input manager, also calls the AimInistalisation function and makes the anim bool of Aiming equal to the isAiming bool when the input manager notices an input
    private bool isAiming;
    public bool IsAiming { set { isAiming = value; AimInistalisation(); anim.SetBool(AnimAimingHash, isAiming); } }

    //Setter for the MOUSE input from the input manager, also calls the AimLook function if the player is aiming when the input manager notices an input
    private Vector2 mouseInput;
    public Vector2 MouseInput { set { mouseInput = value; if (isAiming == true) { AimLook(); } } }

    private int powerInput;
    public int PowerInput { set { powerInput = value; PowerScale(); } }

    #endregion

    #region Anim Hashes

    int AnimFireArrowHash = Animator.StringToHash("FireArrow");
    int AnimAimingHash = Animator.StringToHash("isAiming");

    #endregion

    private void Awake()
    {
        charMovement = GetComponent<CharacterMovement>();
        UI = GetComponent<UIHandler>();
        anim = GetComponent<Animator>();
    }

    //Spawns in the arrow projectile with it facing towards the crosshair point in world space and applys a forward force to fire the arrow.
    public void FireArrow()
    {
        GameObject copy;

        copy = Instantiate(arrowProjectile, arrowSpawnPoint.transform.position, Quaternion.identity);
        copy.transform.LookAt(ScreenCentrePointRay().point);

        Rigidbody rb = copy.GetComponent<Rigidbody>();

        if (rb == null) { return; }

        rb.AddForce(copy.transform.forward * arrowFireForce, ForceMode.Impulse);
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

            charMovement.characterMovement = charMovement.AimMovement;
            charMovement.freeLookCam.Priority = 1;
            charMovement.aimCam.Priority = 2;
        }
        else
        {
            ToggleArrow();
            charMovement.characterMovement = charMovement.Movement;
            charMovement.freeLookCam.Priority = 2;
            charMovement.aimCam.Priority = 1;
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
        if (powerInput >= 0)
        {
            arrowFireForce = Mathf.Clamp(arrowFireForce + 5, arrowForceMin, arrowForceMax);
        }
        else if (powerInput <= 0)
        {
            arrowFireForce = Mathf.Clamp(arrowFireForce - 5, arrowForceMin, arrowForceMax);
        }
        UI.SetPowerText(arrowFireForce);
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
