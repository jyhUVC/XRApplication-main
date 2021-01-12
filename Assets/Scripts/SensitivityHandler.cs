using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SensitivityHandler : MonoBehaviour
{
    [SerializeField] private InputField field;

    public void OnResetButton()
    {
        Debug.Log("Sensitivity Changed : " + field.text);
        MicHandler.Instance.ResetVoiceTouchSensitivity(int.Parse(field.text));
    }
}
