using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private float health;
    private float lerpTimer;
    [Header("Health Bar")]
    public float maxHealth = 100f;
    public float chipSpeed = 2f;

    public Image frontHealthBar;
    public Image removeHealthBar;
    public Image addHealthBar;
    public TextMeshProUGUI healthText;

    [Header("Damage Overlay")] 
    public Image overlay;
    public float duration;
    public float fadeSpeed;

    private float durationTimer;

    void Start()
    {
        health = maxHealth;
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
    }

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
        if (healthText != null)
        {
            healthText.SetText(health.ToString() + "/100");
        }

        if (overlay.color.a > 0)
        {
            if (health < 30)
                return;
            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                float tempAlpha = overlay.color.a;
                tempAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, tempAlpha);
            }
        }
    }

    public void UpdateHealthUI()
    {
        Debug.Log(health);
        float fillF = frontHealthBar.fillAmount;
        float fillB = removeHealthBar.fillAmount;
        float hFraction = health / maxHealth;
        if (fillB > hFraction)
        {
            addHealthBar.fillAmount = 0;
            frontHealthBar.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete *= percentComplete;
            removeHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
            addHealthBar.fillAmount = frontHealthBar.fillAmount;
        }
        if (fillF < hFraction)
        {
            removeHealthBar.fillAmount = 0;
            addHealthBar.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete *= percentComplete;
            frontHealthBar.fillAmount = Mathf.Lerp(fillF, addHealthBar.fillAmount, percentComplete);
            removeHealthBar.fillAmount = frontHealthBar.fillAmount;
        }
        
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        lerpTimer = 0f;
        durationTimer = 0;
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
    }

    public void RestoreHealth(float healAmount)
    {
        health += healAmount;
        lerpTimer = 0f;
    }
}
