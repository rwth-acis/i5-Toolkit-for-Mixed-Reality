using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualEnvironments;

public class EnvironmentDataDisplay : DataDisplay<EnvironmentData>
{
    [Header("Environment Data Objects")]
    [SerializeField] private TextMesh environmentNameLabel;
    [SerializeField] private TextMesh environmentCreditField;
    [SerializeField] private SpriteRenderer environmentPreviewImage;

    private Interactable button;

    private void Awake()
    {
        if (environmentNameLabel == null)
        {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(environmentNameLabel));
        }
        if (environmentPreviewImage == null)
        {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(environmentPreviewImage));
        }
    }

    public override void Setup(EnvironmentData content)
    {
        button = GetComponent<Interactable>();
        if (button == null)
        {
            SpecialDebugMessages.LogComponentNotFoundError(this, nameof(Interactable), gameObject);
        }
        base.Setup(content);
    }

    public override void UpdateView()
    {
        base.UpdateView();
        if (content != null)
        {
            if(content.EnvironmentName != null)
            {
                environmentNameLabel.text = content.EnvironmentName;
            }
            if (content.EnvironmentPreviewImage != null)
            {
                environmentPreviewImage.sprite = (Sprite)content.EnvironmentPreviewImage;
            }
            if (content.EnvironmentCredit != null)
            {
                environmentCreditField.text = content.EnvironmentCredit;
            }
        }
    }
}
