using UnityEngine;

public class IndexListViewPopulator : MonoBehaviour
{
    [SerializeField] private IntListView listView;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            listView.DataSource = new IndexDataSource();
        }
    }
}
