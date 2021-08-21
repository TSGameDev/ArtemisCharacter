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

    [SerializeField] LayerMask enemyLayer;

    Animator anim;

    #region Animator Hashes

    int AnimHitHash = Animator.StringToHash("Hit");
    int AnimHitHeadHash = Animator.StringToHash("HitHead");

    string AnimDeathForward = "DeathForward";
    string AnimDeathBackward = "DeathBackward";

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
        int randomAnim = Random.Range(1, 2);

        if (healthSlider.value <= 0)
        {
            switch (randomAnim)
            {
                case 1:
                    anim.Play(AnimDeathBackward);
                    break;
                case 2:
                default:
                    anim.Play(AnimDeathForward);
                    break;
                
            }
        }
        else
        {
            switch (randomAnim)
            {
                case 1:
                    anim.SetTrigger(AnimHitHash);
                    break;
                case 2:
                    anim.SetTrigger(AnimHitHeadHash);
                    break;
                default:
                    anim.SetTrigger(AnimHitHash);
                    break;
            }
        }
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
