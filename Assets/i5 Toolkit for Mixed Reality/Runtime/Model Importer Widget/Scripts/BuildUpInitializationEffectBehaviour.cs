using i5.Toolkit.Core.Utilities.UnityWrappers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class BuildUpInitializationEffectBehaviour : InstantiationEffectBaseBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameObject clippingPlane;
        [SerializeField] private Material effectMaterial;

        [Header("Settings")]
        [SerializeField] private float minimumHeight = -0.6f;
        [SerializeField] private float maximumHeight = 0.6f;
        [SerializeField] private float effectDuration = 1f;

        protected override void InitializeEffect()
        {
            instantiationEffectLogic = new BuildUpInstantiationEffect(clippingPlane, effectMaterial)
            {
                MinimumHeight = minimumHeight,
                MaximumHeight = maximumHeight,
                EffectDuration = effectDuration
            };
        }
    }
}