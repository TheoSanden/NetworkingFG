using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class Health : NetworkBehaviour 
{
    public delegate void OnValueChanged(int value);

    public delegate void Event();

    public Event OnHealthZero;
    public OnValueChanged OnHealthChanged_Total;
    public OnValueChanged OnHealthChanged_Delta;

    [SerializeField] private int maxHealth = 1;
    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public int MaxHealth {
        get { return maxHealth; }
    }

    public int CurrentHealth {
        get { return currentHealth.Value; }
    }
    
    public void Start()
    {
        if (!IsHost) {
            return;}
        currentHealth.Value = maxHealth;
    }

    public void Update() 
    {
        if (IsHost && IsOwner && Input.GetKeyDown(KeyCode.E))
        {
            ChangeHealth(-10);
        }
        else if(IsOwner && Input.GetKeyDown(KeyCode.E))
        {
            TakeDamageOnServerTestServerRpc();
        }
    }
    [ServerRpc]
    private void TakeDamageOnServerTestServerRpc()
    {
        ChangeHealth(-10);
    }

    private void OnEnable() {
        currentHealth.OnValueChanged += OnCurrentHealthChange;
    }

    private void OnDisable() {
        currentHealth.OnValueChanged -= OnCurrentHealthChange;
    }

    public void ChangeHealth(int amount)
    {
        if (!IsServer && !IsHost) {
            return;
        }
        currentHealth.Value = (currentHealth.Value + amount < 0) ? 0 : (currentHealth.Value + amount > maxHealth) ? maxHealth : currentHealth.Value + amount;
    }

    public void OnCurrentHealthChange(int prev, int curr) 
    {
        OnHealthChanged_Delta?.Invoke(curr - prev);
        OnHealthChanged_Total?.Invoke(curr);

        if (curr == 0) 
        {
            OnHealthZero?.Invoke();
        }
    }
}
