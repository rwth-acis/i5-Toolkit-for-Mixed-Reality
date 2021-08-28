using System.Collections.Generic;
using UnityEngine;

public class IndexListViewPopulator : MonoBehaviour
{
    [SerializeField] private IntListView indexListView;
    [SerializeField] private StringListView stringListView;

    [SerializeField] private List<string> stringItems;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            indexListView.DataSource = new IndexDataSource();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            stringListView.DataSource = new ListDataSource<string>()
            {
                List = stringItems
            };
        }
    }
}
