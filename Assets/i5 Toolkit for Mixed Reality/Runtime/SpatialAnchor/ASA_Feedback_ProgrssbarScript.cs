using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASA_Feedback_ProgrssbarScript : MonoBehaviour, IASAFeedbackReciever
{
    public AnchorModuleScript anchorModuleScript;
    public GameObject background;

    public GameObject fillScaler;

    // Start is called before the first frame update
    void Start()
    {
        if(anchorModuleScript == null)
        {
            Debug.LogError("Progressbar has no anchor module script connected to visualize feedback for.");
            return;
        }

        Debug.Log("Progrssbar reciver registering");
        anchorModuleScript.registerFeedbackReciever(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateAnchorFinalized()
    {
        background.SetActive(false);
        fillScaler.SetActive(false);

    }

    public void CreateAnchorStarted()
    {
        background.SetActive(true);
        fillScaler.SetActive(true);
    }

    public void CreateAnchorStatusUpdate(ASAStaus status)
    {
        Debug.Log("Setting the progressbar to: " + status.scanPercentage);
        fillScaler.transform.localScale = new Vector3(status.scanPercentage, fillScaler.transform.localScale.y, fillScaler.transform.localScale.z);
    }
}
