using UnityEngine;
using UnityEngine.UI;

public class HealthBar : OverHeadUIChild {
    private Slider slider;
    private Health health;
    protected override void Awake() 
    {
        base.Awake();
        slider = GetComponent<Slider>();
    }

    protected override void OnAttachmentChanged(GameObject gameObject) 
    {
        if (this.health)
        {
            this.health.OnHealthChanged_Total -= UpdateHealthBar;
        }
        
        base.OnAttachmentChanged(gameObject);
        if (gameObject.TryGetComponent(out Health health)) {
            this.health = health;
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;
            health.OnHealthChanged_Total += UpdateHealthBar;
        }
        else 
        {
            Debug.LogWarning(gameObject.name + " does not have a health component attached to it.");
        }
    }

    private void UpdateHealthBar(int amount) {
        slider.value = amount;
    }
}
