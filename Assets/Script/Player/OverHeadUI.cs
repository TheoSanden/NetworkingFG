using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(TMP_Text))]
public class OverHeadUI : MonoBehaviour 
{
    public delegate void OnAttachedChanged(GameObject gameObject);
    public OnAttachedChanged onAttachmentChange;
    
    [SerializeField] private Vector2 offset;
    private GameObject attachedTo;
    private TMP_Text text;
    private Camera camera;
    public void Initialize()
    {
        text = this.GetComponent<TMP_Text>();
        camera = Camera.main;
    }
    public void AttachTo(GameObject obj) 
    {
        attachedTo = obj;
        onAttachmentChange?.Invoke(obj);
    }
    public void SetName(string name) 
    {
        text.text = name;
    }
    public void Update() 
    {
        if (!attachedTo) 
        {
            return;
        }

        this.transform.position = GetScreenSpacePosition();
    }
    private Vector2 GetScreenSpacePosition() 
    {
        return camera.WorldToScreenPoint(attachedTo.transform.position + (Vector3)offset);
    }
}
