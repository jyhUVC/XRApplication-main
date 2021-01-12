using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.WebSockets;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class VoiceCopy : MonoBehaviour
{
    [SerializeField] private MicHandler micHandler;
    public Text debugText;
    public Text debugText2;
    Uri u = new Uri("ws://192.168.0.155:13570");
    ClientWebSocket cws = null;
    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
    
    //40000 = 5초
    //80000 = 10초
    //16000 = 2초
    //160 = 0.02초
    const int MAX_PCMLENGTH = 160;

    // byte[] pcm = new byte[320];
    // byte max 값이 255로 되어있어서 255 설정했습니다.
    // 자르지 말고 전송 해 보세요
    //byte[] pcm = new byte[91200];
    byte[] pcm = new byte[90000];
    /*string filePath = @"time.pcm";
    FileStream file;*/
    byte[] data;

    public byte[] pcm320 = new byte[MAX_PCMLENGTH * 2];
    System.Int16[] int16pcm320 = new System.Int16[MAX_PCMLENGTH];
    float[] floatPCM160 = new float[MAX_PCMLENGTH];

    public bool isConnected;

    [SerializeField] AudioSource audio;
    void Start() {
        // pcm파일 열고 전송
        //file = File.Open(filePath, FileMode.Open);
        //pcm = System.IO.File.ReadAllBytes(filePath);

        /*for (int i = 0; i < pcm.Length; i++)
        {
            Debug.Log(pcm[i].ToString());
        }*/
        //audio = GetComponent<AudioSource>();
        Debug.Log(pcm.Length);
        //BufferedStream buf = new BufferedStream(file);
        Connect();
    }

    async void Connect()
    {
        cws = new ClientWebSocket();
        try
        {
            await cws.ConnectAsync(u, CancellationToken.None);
            if (cws.State == WebSocketState.Open)
            {
                Debug.Log("connected");
                debugText.text = "Connected";
                isConnected = true;
                Task.Run(SendData);
                Task.Run(ReceiveData);

            }
        }
        catch (Exception e) { Debug.Log("woe " + e.Message); }
    }

    async void SendData()
    {
        while (true)
        {
            while (micHandler.recordedPCMqueueForAppserver.Count >= MAX_PCMLENGTH)
            {

                for (int i = 0; i < MAX_PCMLENGTH; ++i)
                {
                    if(micHandler.recordedPCMqueueForAppserver.TryDequeue(out floatPCM160[i]))
                    {
                    }
                    else
                    {
                        --i;
                        continue;
                    }

                }
                byte[] pcmArr = ConvertSamplesFloatArrToByteArr(floatPCM160);
                foreach (byte i in pcmArr)
                {
                    debugText.text += Convert.ToString(i, 16) + " ";
                }
                ArraySegment<byte> b = new ArraySegment<byte>(pcmArr);
                await cws.SendAsync(b, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }

    
    /*public async void SayHello(byte[] pcmArr)
    {
        ArraySegment<byte> b = new ArraySegment<byte>(pcmArr);
        await cws.SendAsync(b, WebSocketMessageType.Binary, true, CancellationToken.None);
        Debug.Log("sending..");
        *//*byte[] tempByte = new byte[200];
        for (int i = 0; i < tempByte.Length; ++i)
            i = 10;
        var bArry = Encoding.UTF8.GetBytes(tempByte.ToString());
        ArraySegment<byte> b = new ArraySegment<byte>(bArry);
        await cws.SendAsync(b, WebSocketMessageType.Binary, true, CancellationToken.None);*//*
    }*/

    async void ReceiveData()
    {
        while(true){ 
            byte[] bufSize = new byte[4 * 1024];
            ArraySegment<byte> buf = new ArraySegment<byte>(bufSize);
            WebSocketReceiveResult r = await cws.ReceiveAsync(buf, CancellationToken.None);
            //debugText2.text = "Got: " + Encoding.UTF8.GetString(buf.Array, 0, r.Count);
            /*float[] samples = new float[buf.Array.Length * 4];
            Buffer.BlockCopy(buf.Array, 0, samples, 0, buf.Array.Length);
            int channels = 1;
            int sampleRate = 44100;
            AudioClip clip = AudioClip.Create("Output", samples.Length, channels, sampleRate, true);
            clip.SetData(samples, 0);
            audio.clip =  clip;
            audio.Play();*/
            
            //int bufSize = 0;
            //while(bufSize < bufSize.Length)
            //{
                
            //}




            float[] f = ConvertByteToFloat(buf.Array);
            ConvertClip(f);
            audio.Play();
        }
    }

    private void ConvertClip(float[] f)
    {
        int channels = 1;
        int sampleRate = 44100;
        AudioClip clip = AudioClip.Create("Output", f.Length, channels, sampleRate, false);
        clip.SetData(f, 0);
        audio.clip = clip;
    }

    private static float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 2];

        for (int i = 0; i < floatArr.Length; i++)
        {
            floatArr[i] = ((float)BitConverter.ToInt16(array, i * 2)) / 32768.0f;
        }

        return floatArr;
    }

    System.Int16 ConvertSampleFloatToInt16(float sample)
    {
        sample *= 32768;
        if (sample > 32767) sample = 32767;
        if (sample < -32768) sample = -32768;
        return (System.Int16)sample;
    }
    byte[] ConvertSamplesFloatArrToByteArr(float[] samples)
    {
        for (int i = 0; i < samples.Length; ++i)
        {
            int16pcm320[i] = ConvertSampleFloatToInt16(samples[i]);
        }

        System.Buffer.BlockCopy(int16pcm320, 0, pcm320, 0, MAX_PCMLENGTH * 2);
        return pcm320;
    }
    /*float[] tempPCM;
    float time;
    private void Update()
    {

        time += Time.deltaTime;
        if(time > 5f)
        {
            SendVoice();
            time = 0;
        }
    }

    void SendVoice()
    {
        AudioClip recordClip = Microphone.Start(null, true, 10, 16000);
        recordClip.GetData(tempPCM, 0);
    }*/
    /*public string APIDomainWS = "ws://example.com/websocket";

    public ClientWebSocket clientWebSocket;
    async void Start()
    {
        clientWebSocket = new ClientWebSocket();
        clientWebSocket.Options.AddSubProtocol("gg");

        Debug.Log("[WS]:Attempting connection.");
        try
        {
            Uri uri = new Uri(APIDomainWS);
            await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
            if (clientWebSocket.State == WebSocketState.Open)
            {
                Debug.Log("Input message ('exit' to exit): ");

                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(
                    System.Text.Encoding.UTF8.GetBytes("hello fury from unity")
                );
                await clientWebSocket.SendAsync(
                    bytesToSend,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
                ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(
                    bytesReceived,
                    CancellationToken.None
                );
                Debug.Log(Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count));
            }
            Debug.Log("[WS][connect]:" + "Connected");
        }
        catch (Exception e)
        {
            Debug.Log("[WS][exception]:" + e.Message);
            if (e.InnerException != null)
            {
                Debug.Log("[WS][inner exception]:" + e.InnerException.Message);
            }
        }

        Debug.Log("End");
    }*/
    /*const int FREQUENCY = 16000;
    const int IDEAL_PCM_LENGTH = 320;
    AudioClip recordClip;

    float[] tempPCM;

    int micRecordDuration = 10;

    Queue<float> recordedPCMqueue = new Queue<float>();
    Queue<float> copiedPCMqueue = new Queue<float>();

    int audioSampleLength { get { return FREQUENCY * micRecordDuration; } }


    private void Awake()
    {
        tempPCM = new float[FREQUENCY * micRecordDuration];
        recordClip = Microphone.Start(null, true, micRecordDuration, FREQUENCY);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        recordClip.GetData(tempPCM, 0);
        for (int i = 0; i < audioSampleLength; ++i)
        {
            recordedPCMqueue.Enqueue(tempPCM[i]);
            copiedPCMqueue.Enqueue(tempPCM[i]);
        }


    }*/
}
