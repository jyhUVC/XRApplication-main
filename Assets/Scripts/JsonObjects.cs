using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////// <summary>
///////////////////////////////////// This cs page is for json objects definition.
//////////////////// </summary>

[System.Serializable]
public class SttResult
{
    public List<SttSentence> results;
    public string _return;
    public string sampFreq;
    public float sentConfidence;
    public int sentEnd;
    public int sentIndex;
    public int sentStart;
    public string status;
};

[System.Serializable]
public class SttSentence
{
    public string sentence;
}

