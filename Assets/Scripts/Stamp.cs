using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamp : MonoBehaviour
{
    [SerializeField] private float drawDistance = 20f;
    [SerializeField] private GameObject stamp; 
    [SerializeField] private Toggle stampSwitch;
    [SerializeField] private GameObject cam;
    private GameObject newStamp;

    //public TrackedPose tpd;
    public Text debugText;

    Anchor drawAnchor;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && stampSwitch.isOn == true)
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, drawDistance));
            drawAnchor = Session.CreateAnchor(new Pose(point, Quaternion.identity));
            newStamp = GameObject.Instantiate(stamp, drawAnchor.transform.position, drawAnchor.transform.rotation, drawAnchor.transform);
            //newCube.transform.position = point;
        }

        if (newStamp != null)
        {
            newStamp.transform.position = drawAnchor.transform.position;
            //newStamp.gameObject.transform.LookAt(cam.gameObject.transform);
        }


    }
}
