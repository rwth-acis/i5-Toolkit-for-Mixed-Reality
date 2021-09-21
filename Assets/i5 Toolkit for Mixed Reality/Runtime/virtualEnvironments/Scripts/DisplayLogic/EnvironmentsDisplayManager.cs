using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualEnvironments;

public class EnvironmentsDisplayManager : ListViewController<EnvironmentData, EnvironmentSelectionItem>
{
    private int page = 0;
    [SerializeField] private VirtualEnvironmentsManager vem;
    private GameObject currentEnvironmentInstance;

    public void Start()
    {
        UpdateEnvironmentDisplay();
        SetPageButtonStates();
        ItemSelected += OnEnvironmentSelected;
    }
    /// <summary>
    /// Called if the user pushes the page up button
    /// Swiches to the previous page
    /// </summary>
    public void PageUp()
    {
        page = Mathf.Max(0, page - 1);
        SetPageButtonStates();
        UpdateEnvironmentDisplay();
    }

    /// <summary>
    /// Called if the user pages the page down button
    /// Switches to the next page
    /// </summary>
    public void PageDown()
    {
        page = Mathf.Min(page + 1, ((vem.environments.Count - 1) / entriesPerPage));
        SetPageButtonStates();
        UpdateEnvironmentDisplay();
    }


    /// <summary>
    /// Adapts the button states of the page up and page down buttons
    /// If the first page is shown, the up button is disabled and if the last page is shown, the down button is disabled
    /// </summary>
    private void SetPageButtonStates()
    {
        if (page == 0) // first page
        {
            pageUpButton.Enabled = false;
        }
        else
        {
            pageUpButton.Enabled = true;
        }

        if (page == ((vem.environments.Count - 1) / entriesPerPage)) // last page
        {
            pageDownButton.Enabled = false;
        }
        else
        {
            pageDownButton.Enabled = true;
        }
    }


    /// <summary>
    /// Updates the list view showing the environment lists (on the current page)
    /// </summary>
    private void UpdateEnvironmentDisplay()
    {
        if (vem.environments.Count > 0)
        {
            // get the start index and length of the sub array to display
            // make sure that it stays within the bounds of the room list
            int startIndex = Mathf.Min(page * entriesPerPage, vem.environments.Count - 1);
            int length = Mathf.Min(vem.environments.Count - startIndex, entriesPerPage);
            Items = vem.environments.GetRange(startIndex, length);
        }
        else
        {
            Items = new List<EnvironmentData>();
        }
    }

    /// <summary>
    /// Called if a element of the virtual environments list view was selected by the user
    /// </summary>
    /// <param name="sender">The selected skybox</param>
    /// <param name="e">Arguments about the list view selection event</param>
    private void OnEnvironmentSelected(object sender, ListViewItemSelectedArgs e)
    {
        vem.InstantiateEnvironment(SeletedItem);
    }
}
