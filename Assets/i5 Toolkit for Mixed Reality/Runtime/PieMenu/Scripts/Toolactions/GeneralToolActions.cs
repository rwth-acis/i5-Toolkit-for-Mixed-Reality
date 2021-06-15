using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.UI;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// A collection of ToolActions that are helpfull for multiple tools
    /// </summary>
    public class GeneralToolActions : ActionHelperFunctions
    {
        #region IconOverVisualisation

        public GameObject highlightSprite;
        public GameObject objectTransformer;
        GameObject instantiatedIcon;

        /// <summary>
        /// Spawns the icon from the currently selected tool over the boundingbox of the visualisation the tool just started pointing at
        /// </summary>
        /// <param name="data"></param> The data from the corresponding focus event
        public void SpawnCurrentIconOverObject(FocusEventData data)
        {
            GameObject objectToSpawnIconOver;
            ViveWandVirtualTool virtualTool = GetVirtualTool(data.Pointer);

            if (objectTransformer != null)
            {
                IObjectTransformer transformer = objectTransformer.GetComponent<IObjectTransformer>();
                objectToSpawnIconOver = transformer.TransformObject(data.NewFocusedObject, virtualTool.currentEntry.toolSettings.toolName);
            }
            else
            {
                objectToSpawnIconOver = data.NewFocusedObject;
            }
            if (objectToSpawnIconOver != null)
            {
                if (instantiatedIcon != null)
                {
                    //This shouldn't happen, but just to be sure
                    Destroy(instantiatedIcon);
                }
                instantiatedIcon = Instantiate(highlightSprite);
                instantiatedIcon.GetComponentInChildren<Image>().sprite = GetVirtualTool(data.Pointer).currentEntry.toolSettings.iconTool;
            }
        }

        /// <summary>
        /// Updates position and rotation of the currently spawned icon over the visualisation, the tool currently points at
        /// </summary>
        /// <param name="data"></param> The data from the corresponding focus event
        public void UpdateCurrentIconOverObject(FocusEventData data)
        {
            if (instantiatedIcon != null)
            {
                GameObject target;
                ViveWandVirtualTool virtualTool = GetVirtualTool(data.Pointer);

                if (objectTransformer != null)
                {
                    IObjectTransformer transformer = objectTransformer.GetComponent<IObjectTransformer>();
                    target = transformer.TransformObject(data.NewFocusedObject, virtualTool.currentEntry.toolSettings.toolName);
                }
                else
                {
                    target = data.NewFocusedObject;
                }

                if (target != null)
                {
                    Collider[] colliders = target.GetComponentsInChildren<Collider>();
                    //Calculate the Axis Alligned BoundingBox from the colliders
                    if (colliders.Length > 0)
                    {
                        Vector3 maxpoint = colliders[0].bounds.max;
                        Vector3 minpoint = colliders[0].bounds.min;

                        foreach (var collider in colliders)
                        {

                            bool OneComponentDifferent(Vector3 vec1, Vector3 vec2, Func<float, float, bool> relation)
                            {
                                return (relation(vec1.x, vec2.x) || relation(vec1.y, vec2.y) || relation(vec1.z, vec2.z));
                            }

                            Vector3 potentialMaxPoint = collider.bounds.max;

                            if (OneComponentDifferent(maxpoint, potentialMaxPoint, (x, y) => (x > y)))
                            {
                                maxpoint = potentialMaxPoint;
                            }

                            Vector3 potentialMinPoint = collider.bounds.min;

                            if (OneComponentDifferent(maxpoint, potentialMaxPoint, (x, y) => (x < y)))
                            {
                                minpoint = potentialMinPoint;
                            }
                        }
                        //Calculate the top middle point of the AABB
                        minpoint.y = maxpoint.y;
                        Vector3 topMiddle = minpoint + 0.5f * (maxpoint - minpoint);
                        topMiddle.y += 0.3f;

                        RotateToCameraOnXZPlane(instantiatedIcon, topMiddle);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys the instantiated icon, that was spawned by SpawnCurrentIconOverVisualisation()
        /// </summary>
        public void DestroyCurrentIconOverObject()
        {
            Destroy(instantiatedIcon);
        }

        #endregion

        #region Descritption Texts

        /// <summary>
        /// Activates the button description texts on the tool, that is saved in data
        /// </summary>
        /// <param name="data"></param> The data from the corresponding input event
        public void ActivateDesciptionTexts(BaseInputEventData data)
        {
            GameObject tool = data.InputSource.Pointers[0].Controller.Visualizer.GameObjectProxy;
            GameObject buttonDescriptons = tool.transform.Find("ButtonDescriptions")?.gameObject;
            buttonDescriptons.SetActive(true);
        }

        public void DeactivateDesciptionTexts(BaseInputEventData data)
        {
            GameObject tool = data.InputSource.Pointers[0].Controller.Visualizer.GameObjectProxy;
            GameObject buttonDescriptons = tool.transform.Find("ButtonDescriptions")?.gameObject;
            buttonDescriptons.SetActive(false);
        }
        #endregion
    } 
}
