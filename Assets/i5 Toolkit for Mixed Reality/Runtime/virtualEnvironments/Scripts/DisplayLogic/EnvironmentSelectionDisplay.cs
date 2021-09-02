using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSelectionDisplay : MonoBehaviour
{
    [SerializeField] private GameObject CreditTable;

    /// <summary>
    /// This function is called, when the environment selection display button is pressed, 
    /// </summary>
    public void toggleCredits()
    {
        CreditTable.SetActive(!CreditTable.activeSelf);
    }
}
