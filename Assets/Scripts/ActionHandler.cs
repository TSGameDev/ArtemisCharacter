using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHandler : MonoBehaviour
{
    [Header("Stamina Costs")][Space(10)]
    [Tooltip("The stamina cost of a dodge roll or dive.")]
    [SerializeField] private float dodgeCost = 10f;
    [Tooltip("The stamina cost of a punch action")]
    [SerializeField] private float punchCost = 10f;
    [Tooltip("The stamina cost of a kick action")]
    [SerializeField] private float kickCost = 10f;

    [Header("Animations")]
    [SerializeField] private int dodgeActionID;
    [SerializeField] private int punchActionID;
    [SerializeField] private int kickActionID;
    [SerializeField] private int equipBowActionID;
    [SerializeField] private int unequipBowActionID;

    [Header("Equipment")]
    [Tooltip("The skinned bow gameobject on the erica prefab")]
    [SerializeField] private GameObject bow;
    [Tooltip("The bow gameobject on the back of the erica prefav")]
    [SerializeField] private GameObject disarmedBow;

    #region Anim Hashes

    int AnimActionHash = Animator.StringToHash("Action");
    int AnimActionIDHash = Animator.StringToHash("ActionID");
    int AnimArmedHash = Animator.StringToHash("Armed");

    #endregion

    #region Private Variables

    private Animator anim;
    private Attributes attributes;
    //bool for keeping track of if the player if currently got a bow equiped or not
    private bool isArmed = true;

    #endregion

    private void Awake()
    {
        anim = GetComponent<Animator>();
        attributes = GetComponent<Attributes>();
    }

    public void Dodge()
    {
        anim.SetTrigger(AnimActionHash);
        anim.SetInteger(AnimActionIDHash, dodgeActionID);
        attributes.ReduceStamina(dodgeCost);
    }

    public void Punch()
    {
        anim.SetTrigger(AnimActionHash);
        anim.SetInteger(AnimActionIDHash, punchActionID);
        attributes.ReduceStamina(punchCost);
    }

    public void Kick()
    {
        anim.SetTrigger(AnimActionHash);
        anim.SetInteger(AnimActionIDHash, kickActionID);
        attributes.ReduceStamina(kickCost);
    }

    public void EquipUnequipBow()
    {
        if (isArmed)
        {
            anim.SetTrigger(AnimActionHash);
            anim.SetInteger(AnimActionIDHash, unequipBowActionID);
        }
        else
        {
            anim.SetTrigger(AnimActionHash);
            anim.SetInteger(AnimActionIDHash, equipBowActionID);
        }

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

}
