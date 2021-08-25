using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Attributes : MonoBehaviour
{
    [Header("Health & Stamina")]
    [Space(10)]
    [SerializeField] int currentHealth;
    [SerializeField] int healthMax = 100;
    [SerializeField] float currentStamina;
    [SerializeField] int staminaMax = 100;

    [Header("Stamina Rates")]
    [SerializeField] float sprittingStaminaConsumption = 10f;
    [SerializeField] float staminaRegenRate = 10f;

    Animator anim;
    UIHandler UI;

    #region Animator Hashes

    private int AnimDeadHash = Animator.StringToHash("Dead");
    private int AnimDamageHash = Animator.StringToHash("Damage");

    #endregion

    void Awake()
    {
        anim = GetComponent<Animator>();
        UI = GetComponent<UIHandler>();

        currentHealth = healthMax;
        currentStamina = staminaMax;

        UI.SetHealth(currentHealth, healthMax);
        UI.SetStamina(currentStamina, staminaMax);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UI.SetHealth(currentHealth);

        if (currentHealth <= 0)
            anim.SetTrigger(AnimDeadHash);
        else
            anim.SetTrigger(AnimDamageHash);
    }

    public void ReduceStamina(float staminaDamage)
    {
        currentStamina -= staminaDamage;
        UI.SetStamina(currentStamina);
    }

    public bool Run()
    {
        if(currentStamina <= Mathf.Epsilon)
        {
            return false;
        }
        else
        {
            currentStamina -= sprittingStaminaConsumption * Time.deltaTime;
            UI.SetStamina(currentStamina);
            return true;
        }
    }

    public void StaminaRegen()
    {
        currentStamina += staminaRegenRate * Time.deltaTime;
        UI.SetStamina(currentStamina);
    }
}
