using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStateManager : MonoBehaviour
{

    [SerializeField] private GameObject CollapsableMenu;
    private bool windowIsOpen;

    // Start is called before the first frame update
    void Start()
    {
        windowIsOpen = false;
    }

    /// <summary>
    /// Called if the window is open and the open/close button is pressed
    /// Deactivates the menu window
    /// </summary>
    public void CloseWindow()
    {
        CollapsableMenu.SetActive(false);
    }

    /// <summary>
    /// Called if the window is closed and the open/close button is pressed
    /// Activates the menu window
    /// </summary>
    public void OpenWindow()
    {
        CollapsableMenu.SetActive(true);
    }

    /// <summary>
    /// Called if the open/close button is pressed
    /// Sets the menu state
    /// </summary>
    public void manageWindow()
    {
        Debug.Log("Managing Window");
        if (windowIsOpen)
        {
            CloseWindow();
            windowIsOpen = false;
        }
        else
        {
            OpenWindow();
            windowIsOpen = true;
        }
    }
}
