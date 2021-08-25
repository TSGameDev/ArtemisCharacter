using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Header("UI Elements")][Space(10)]
    [SerializeField] private Slider health;
    [SerializeField] private Slider stamina;
    [SerializeField] private TextMeshProUGUI powerTxt;

    private void Awake()
    {
        health.minValue = 0;
        stamina.minValue = 0;
    }

    public void SetHealth(int newHealth)
    {
        health.value = newHealth;
    }

    public void SetHealth(int newHealth, int maxHealth)
    {
        health.value = newHealth;
        health.maxValue = maxHealth;
    }

    public void SetStamina(float newStamina)
    {
        stamina.value = newStamina;
    }

    public void SetStamina(float newStamina, int maxStamina)
    {
        stamina.value = newStamina;
        stamina.maxValue = maxStamina;
    }

    public void SetPowerText(int newPower)
    {
        powerTxt.text = newPower.ToString();
    }
}
