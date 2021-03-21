﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.UI;

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
        ViveWandVirtualTool virtualTool = GetVirtualToolFromPointer(data.Pointer);

        if (objectTransformer != null)
        {
            IObjectTransformer transformer = objectTransformer.GetComponent<IObjectTransformer>();
            objectToSpawnIconOver = transformer.transformObject(data.NewFocusedObject, virtualTool.currentEntry.toolName);
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
            instantiatedIcon.GetComponentInChildren<Image>().sprite = GetVirtualToolFromPointer(data.Pointer).currentEntry.iconTool;
        }
    }

    /// <summary>
    /// Updates position and rotation of the currently spawned icon over the visualisation, the tool currently points at
    /// </summary>
    /// <param name="data"></param> The data from the corresponding focus event
    public void UpdateCurrentIconOverVisualisation(FocusEventData data)
    {
        if (instantiatedIcon != null)
        {
            GameObject target;
            ViveWandVirtualTool virtualTool = GetVirtualToolFromPointer(data.Pointer);

            if (objectTransformer != null)
            {
                IObjectTransformer transformer = objectTransformer.GetComponent<IObjectTransformer>();
                target = transformer.transformObject(data.NewFocusedObject, virtualTool.currentEntry.toolName);
            }
            else
            {
                target = data.NewFocusedObject;
            }

            if (target != null)
            {
                //Try if the object has a MRTK BoundingBox
                Collider collider = target.GetComponentInChildren<BoundingBox>()?.GetComponent<Collider>();
                //If not, try if it has collider itself
                collider = target.GetComponent<Collider>();

                if (collider != null)
                {
                    Vector3 maxpoint = collider.bounds.max;
                    Vector3 minpoint = collider.bounds.min;
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
    public void DestroyCurrentIconOverVisualisation()
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