using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Attributes : MonoBehaviour
{
    [SerializeField] Slider healthSlider;
    [SerializeField] float healthMax = 100f;
    [SerializeField] Slider staminaSlider;
    [SerializeField] float staminaMax = 100f;
    [SerializeField] float sprittingStaminaConsumption = 10f;
    [SerializeField] float staminaRegenRate = 10f;

    Animator anim;

    #region Animator Hashes

    int AnimHitHash = Animator.StringToHash("Hit");
    int AnimHitHeadHash = Animator.StringToHash("HitHead");

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

        int randomAnim = Random.Range(1,2);

        switch(randomAnim)
        {
            default:
            case 1:
            anim.SetTrigger(AnimHitHash);
            break;
            case 2:
            anim.SetTrigger(AnimHitHeadHash);
            break;
        }
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
