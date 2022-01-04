using i5.Toolkit.Core.Utilities;
using i5.Toolkit.MixedReality.MenuPlacementSystem;
using UnityEngine;

public class DemoMainMenu : MenuBase {

    [Header("Object To Create")]
    [SerializeField] private GameObject objectToCreate;

    // Start is called before the first frame update
    void Awake() {

        if (objectToCreate == null) {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(objectToCreate));
        }

    }

    public void CreateObject() {
        GameObject createdObject = Instantiate(objectToCreate, new Vector3(gameObject.transform.position.x - 0.3f, gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.Euler(0, 0, 0));
        createdObject.SetActive(true);
    }

    public override void Initialize() {

    }

    public override void OnClose() {

    }
}
