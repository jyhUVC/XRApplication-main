using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Text debugText;

    TrackedPoseDriver td;
    // Start is called before the first frame update
    void Start()
    {
        //slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        //debugText.text = slider.value.ToString();
        mainCam.fieldOfView = slider.value;
    }
}
