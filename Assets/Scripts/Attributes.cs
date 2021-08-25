using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Attributes : MonoBehaviour
{
    [Header("Health & Stamina")][Space(10)]
    [SerializeField] Slider healthSlider;
    [SerializeField] float healthMax = 100f;
    [SerializeField] Slider staminaSlider;
    [SerializeField] float staminaMax = 100f;

    [Header("Stamina Rates")]
    [SerializeField] float sprittingStaminaConsumption = 10f;
    [SerializeField] float staminaRegenRate = 10f;

    Animator anim;

    #region Animator Hashes

    private int AnimDeadHash = Animator.StringToHash("Dead");
    private int AnimDamageHash = Animator.StringToHash("Damage");

    #endregion

    void Awake()
    {
        healthSlider.maxValue = healthMax;
        staminaSlider.maxValue = staminaMax;
        healthSlider.minValue = 0;
        staminaSlider.minValue = 0;
        healthSlider.value = healthSlider.maxValue;
        staminaSlider.value = staminaSlider.maxValue;

        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        healthSlider.value -= damage;

        if (healthSlider.value <= 0)
            anim.SetTrigger(AnimDeadHash);
        else
            anim.SetTrigger(AnimDamageHash);
    }

    public void ReduceStamina(float staminaDamage)
    {
        staminaSlider.value -= staminaDamage;
    }

    public bool Run()
    {
        if(staminaSlider.value <= Mathf.Epsilon)
        {
            return false;
        }
        else
        {
            staminaSlider.value -= sprittingStaminaConsumption * Time.deltaTime;
            return true;
        }
    }

    public void StaminaRegen()
    {
        staminaSlider.value += staminaRegenRate * Time.deltaTime;
    }
}
