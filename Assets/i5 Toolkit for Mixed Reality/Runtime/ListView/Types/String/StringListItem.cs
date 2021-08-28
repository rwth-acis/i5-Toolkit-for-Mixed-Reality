using TMPro;
using UnityEngine;

public class StringListItem : ListItem<string>
{
    [SerializeField] private TextMeshPro textLabel;

    public override void UpdateView()
    {
        base.UpdateView();

        if (Visible)
        {
            textLabel.text = ItemContent;
        }
    }
}
