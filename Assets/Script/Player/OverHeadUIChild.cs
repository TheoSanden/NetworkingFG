using UnityEngine;

public class OverHeadUIChild : MonoBehaviour 
{
    private OverHeadUI overHeadUI;
    private GameObject Owner;
    protected virtual void Awake() 
    {
        overHeadUI = GetComponentInParent<OverHeadUI>();
        if (!overHeadUI) 
        {
            Destroy(this);
        }
    }

    protected virtual void OnEnable() 
    {
        overHeadUI.onAttachmentChange +=  OnAttachmentChanged;
    }

    protected virtual void OnDisable() 
    {
        overHeadUI.onAttachmentChange -= OnAttachmentChanged;
    }

    protected virtual void OnAttachmentChanged(GameObject gameObject) 
    {
        Owner = gameObject;
    }
}
