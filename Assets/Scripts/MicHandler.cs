using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Itld_WakeUp_STT;
using System.Collections.Concurrent;
using System;
using UnityEngine.UI;

public class MicHandler : MonoBehaviour
{
    VoiceCopy voiceCopy;
    public static MicHandler Instance { get; private set; }

    const int FREQUENCY = 16000;
    const int IDEAL_PCM_LENGTH = 320;
    public AudioClip recordClip;
    Queue<float> recordedPCMqueue = new Queue<float>();
    public ConcurrentQueue<float> recordedPCMqueueForAppserver { get; set; } = new ConcurrentQueue<float>();

    public string VoiceStreamServerIP = "192.168.0.154";
    public string VoiceStreamServerPort = "3200";
    
    /// <summary>
    /// Read from AudioClip first.
    /// </summary>
    float[] tempPCM;
    float[] scriptPCM;

    int lastPos, currentPos;
    /// <summary>
    /// 
    /// </summary>
    Thread recordProcess;

    int micRecordDuration = 30;

    int audioSampleLength { get { return FREQUENCY * micRecordDuration; } }

    public byte[] pcm320 = new byte[320];
    System.Int16[] int16pcm320 = new System.Int16[160];
    float[] floatPCM160 = new float[160];

    /// <summary>
    /// intelloid voiceTouch Engine
    /// </summary>
    ItldVoicetouch voiceTouch;
    ItldSTTClient stt;

    int recogMode;
    const int RECOG_MODE_WAKEUP = 0;
    const int RECOG_MODE_STT = 1;

    private string strStt = "";
    private string strWakeUp = "";
    public UnityEngine.UI.Text wakeUpText;
    public UnityEngine.UI.Text sttText;

    public delegate void OnNewWakeupSentence(string sentence);
    static public event OnNewWakeupSentence OnNewWakeupSenenceEvent;

    public Text debugText;
    public Text debugTextCopied;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        voiceCopy = new VoiceCopy();
        InitIntelloid();
        tempPCM = new float[FREQUENCY * micRecordDuration];
        scriptPCM = new float[FREQUENCY * micRecordDuration];
        recordClip = Microphone.Start(null, true, micRecordDuration, FREQUENCY);
        AudioSource audio = GetComponent<AudioSource>();
        //audio.Play();

        lastPos = 0;
        currentPos = 0;
    }
    private void Start()
    {
        /*recordClip.GetData(scriptPCM, 0);
        recordClip.SetData(scriptPCM, 0);
        byte[] scriptPCMConverted = ConvertSamplesFloatArrToByteArr(scriptPCM);
        for(int i = 0; i < scriptPCMConverted.Length; i++)
        {
            debugText.text += scriptPCMConverted[i];
        }
        if (voiceCopy.isConnected)
        {
            voiceCopy.SayHello(scriptPCMConverted);
        }*/
    }
    void InitIntelloid()
    {
        int sensitivity = 10;
        voiceTouch = new ItldVoicetouch(sensitivity);
        recogMode = RECOG_MODE_WAKEUP;
        stt = new ItldSTTClient("13.209.202.237", 5116, processSTTResult);
        Debug.Log("voiceTouch.create() : " + voiceTouch.create());
        //recordProcess = new Thread(wakeupWithSTT);
        //recordProcess.Start();
        StartCoroutine(DequePCM());
    }

    //unity update
    void Update()
    {
        lock (strStt)
            if (!strStt.Equals(""))
            {
                sttText.text = strStt;
                OnNewWakeupSenenceEvent(strStt);
                strStt = "";
            }
        lock (strWakeUp)
            wakeUpText.text = strWakeUp;

        lock (recordedPCMqueue)
        {
            currentPos = Microphone.GetPosition(null);

            //Debug.Log("pushing pcm to queue...");
            //the microphones's record is looping, this is when startPoint of loop 
            if (currentPos < lastPos)
            {
                //read last region array
                int dataSize = audioSampleLength - lastPos;
                recordClip.GetData(tempPCM, lastPos);
                for (int i = 0; i < dataSize; ++i)
                {
                    recordedPCMqueue.Enqueue(tempPCM[i]);
                    recordedPCMqueueForAppserver.Enqueue(tempPCM[i]);
                }
                //read start region array
                //dataSize = currentPos;
                recordClip.GetData(tempPCM, 0);
                for (int i = 0; i < currentPos; ++i)
                {
                    recordedPCMqueue.Enqueue(tempPCM[i]);
                    recordedPCMqueueForAppserver.Enqueue(tempPCM[i]);
                }

            }
            else
            {
                int dataSize = currentPos - lastPos;
                recordClip.GetData(tempPCM, lastPos);

                for (int i = 0; i < dataSize; ++i)
                {
                    recordedPCMqueue.Enqueue(tempPCM[i]);
                    //if (i % 100 == 0) 
                    //Debug.Log(tempPCM[i]);
                }
            }
            lastPos = currentPos;
        }
    }

    //Unity PCM
    // -1 ~ 1

    System.Int16 ConvertSampleFloatToInt16(float sample)
    {
        sample *= 32768;
        if (sample > 32767) sample = 32767;
        if (sample < -32768) sample = -32768;
        return (System.Int16)sample;
    }
    
    byte[] ConvertSamplesFloatArrToByteArr(float[] samples)
    {
        for(int i = 0; i < samples.Length; ++i)
        {
            int16pcm320[i] = ConvertSampleFloatToInt16(samples[i]);
        }

        System.Buffer.BlockCopy(int16pcm320, 0, pcm320, 0, 320);
        return pcm320;
    }

    //16000 * 0.02 = 320
    IEnumerator DequePCM()
    {
        Debug.Log("Dequeue Coroutine Started");
        int result;

        strWakeUp = "say [헤이유비]";
        while (true)
        {
            while (recordedPCMqueue.Count >= 160)
            {
                //Debug.Log("ecordedPCMqueue.Count :" + recordedPCMqueue.Count);
                
                for (int i = 0; i < 160; ++i)
                {
                    floatPCM160[i] = recordedPCMqueue.Dequeue();
                }

                pcm320 = ConvertSamplesFloatArrToByteArr(floatPCM160);

                if (recogMode == RECOG_MODE_STT && stt.IsConnected() == false)
                {
                    
                    recordedPCMqueue.Clear();
                    
                    strWakeUp = "say [헤이유비]";
                }
                recogMode = stt.IsConnected() ? RECOG_MODE_STT : RECOG_MODE_WAKEUP;

                if (recogMode == RECOG_MODE_WAKEUP)
                {

                    result = voiceTouch.insertPcm(pcm320);
                    //여기에 320byte pcm 넣고.
                    switch (result)
                    {
                        case ItldVoicetouch.API_ERROR:
                            Debug.Log("VoiceTouch API_ERROR");
                            break;
                        case ItldVoicetouch.API_NO_KEYWORD_YET:
                            //Console.Write(".");
                            break;
                        case ItldVoicetouch.API_KEYWORD_DETECTED:
                            lock (strWakeUp)
                                strWakeUp = "Keyword detected";
                            //detect 하면 stt로 커넥트.
                            voiceTouch.reset();
                            Debug.Log("voiceTouch.reset()");
                            if (!stt.connect())
                            {
                                Debug.Log("stt.connect failed!");
                                recordedPCMqueue.Clear();
                                //큐지우고,
                            }
                            break;
                        case ItldVoicetouch.API_TIMEOUT:
                            Debug.Log("VoiceTouch API_TIMEOUT");
                            break;
                    }
                }
                else
                {
                    stt.send(pcm320, pcm320.Length);
                    
                    //stt 서버로 보내는 함수,
                }
            }
            yield return null;
        }
    }

    void wakeupWithSTT()
    {
        Debug.Log("Dequeue Thread Started");
        int result;

        lock(strWakeUp)
            strWakeUp = "say [헤이유비]";
        while (true)
        {
            if (recordedPCMqueue.Count >= 160)
            {
                //Debug.Log("ecordedPCMqueue.Count :" + recordedPCMqueue.Count);
                lock(recordedPCMqueue)
                    for (int i = 0; i < 160; ++i)
                    {
                        floatPCM160[i] = recordedPCMqueue.Dequeue();
                    }

                pcm320 = ConvertSamplesFloatArrToByteArr(floatPCM160);

                if (recogMode == RECOG_MODE_STT && stt.IsConnected() == false)
                {
                    lock (recordedPCMqueue)
                        recordedPCMqueue.Clear();
                    lock (strWakeUp)
                        strWakeUp = "say [헤이유비]";
                }
                recogMode = stt.IsConnected() ? RECOG_MODE_STT : RECOG_MODE_WAKEUP;

                if (recogMode == RECOG_MODE_WAKEUP)
                {
                    lock (voiceTouch)
                    {
                        result = voiceTouch.insertPcm(pcm320);
                        //여기에 320byte pcm 넣고.
                        switch (result)
                        {
                            case ItldVoicetouch.API_ERROR:
                                Debug.Log("VoiceTouch API_ERROR");
                                break;
                            case ItldVoicetouch.API_NO_KEYWORD_YET:
                                //Console.Write(".");
                                break;
                            case ItldVoicetouch.API_KEYWORD_DETECTED:
                                lock (strWakeUp)
                                    strWakeUp = "Keyword detected";
                                //detect 하면 stt로 커넥트.
                                voiceTouch.reset();
                                Debug.Log("voiceTouch.reset()");
                                if (!stt.connect())
                                {
                                    Debug.Log("stt.connect failed!");
                                    lock (recordedPCMqueue)
                                        recordedPCMqueue.Clear();
                                    //큐지우고,
                                }
                                break;
                            case ItldVoicetouch.API_TIMEOUT:
                                Debug.Log("VoiceTouch API_TIMEOUT");
                                break;
                        }
                    }
                }
                else
                {
                    stt.send(pcm320, pcm320.Length);
                    //stt 서버로 보내는 함수,
                }
            }
        }
    }

    private void OnDestroy()
    {
        stt.disconnect();
        voiceTouch.destroy();
    }

    void processSTTResult(string json)
    {
        lock (strStt)
        {
            SttResult sttResult = new SttResult();
            sttResult = JsonToObject<SttResult>(json);
            Debug.Log(sttResult.results[0].sentence);
            strStt = sttResult.results[0].sentence;
        }
        stt.disconnect();
    }

    string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    T JsonToObject<T>(string jsonData)
    {
        return JsonUtility.FromJson<T>(jsonData);
    }

    public void ResetVoiceTouchSensitivity(int sensitivity)
    {
        lock (voiceTouch)
        {
            voiceTouch.destroy();
            voiceTouch = new ItldVoicetouch(sensitivity);
            voiceTouch.create();
        }
    }

}
