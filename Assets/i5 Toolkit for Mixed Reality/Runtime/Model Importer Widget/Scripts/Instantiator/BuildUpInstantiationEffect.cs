using i5.Toolkit.Core.Experimental.UnityAdapters;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class BuildUpInstantiationEffect : IInstantiationEffect
    {
        public float MinimumHeight { get; set; }
        public float MaximumHeight { get; set; }
        public float EffectDuration { get; set; }

        public GameObject ClippingPlane { get; set; }

        public Material EffectMaterial { get; set; }

        public BuildUpInstantiationEffect(GameObject clippingPlane, Material effectMaterial)
        {
            ClippingPlane = clippingPlane;
            EffectMaterial = effectMaterial;
        }

        public async Task PlayInstantiationEffectAsync(GameObject target)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            ClippingPlane clippingPlaneComponent = ClippingPlane.GetComponent<ClippingPlane>();
            List<Material[]> origMaterials = new List<Material[]>();
            ReplaceMaterialsByEffectMaterial(renderers, clippingPlaneComponent, ref origMaterials);
            await SlideClippingPlane();
            clippingPlaneComponent.ClearRenderers();
            await new WaitForEndOfFrame();
            RestoreMaterials(renderers, origMaterials);
        }

        private void ReplaceMaterialsByEffectMaterial(Renderer[] renderers, ClippingPlane clippingPlaneComponent, ref List<Material[]> origMaterials)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                origMaterials.Add(renderers[i].materials);

                Material[] replacement = new Material[renderers[i].materials.Length];
                for (int matIndex = 0; matIndex < renderers[i].materials.Length; matIndex++)
                {
                    replacement[matIndex] = EffectMaterial;
                }
                renderers[i].materials = replacement;

                clippingPlaneComponent.AddRenderer(renderers[i]);
            }
        }

        private void RestoreMaterials(Renderer[] renderers, List<Material[]> origMaterials)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].materials = origMaterials[i];
            }
        }

        private IEnumerator SlideClippingPlane()
        {
            float time = 0f;

            TransformAdapter clippingPlaneTransform = new TransformAdapter(ClippingPlane.transform);

            clippingPlaneTransform.LocalPosition = new Vector3(clippingPlaneTransform.LocalPosition.x, MinimumHeight, clippingPlaneTransform.LocalPosition.z);
            while (time < EffectDuration)
            {
                float height = Mathf.Lerp(MinimumHeight, MaximumHeight, time / EffectDuration);
                clippingPlaneTransform.LocalPosition = new Vector3(clippingPlaneTransform.LocalPosition.x, height, clippingPlaneTransform.LocalPosition.z);
                time += Time.deltaTime;
                yield return null;
            }
            clippingPlaneTransform.LocalPosition = new Vector3(clippingPlaneTransform.LocalPosition.x, MaximumHeight, clippingPlaneTransform.LocalPosition.z);
        }
    }
}