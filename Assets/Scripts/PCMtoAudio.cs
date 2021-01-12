using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PCMtoAudio : MonoBehaviour
{
    string filePath = @"time.pcm";
    FileStream file;
    byte[] pcm = new byte[912000];
    Text debugText;
    [SerializeField] AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
        // pcm파일 열고 전송
        //file = File.Open(filePath, FileMode.Open);
        pcm = System.IO.File.ReadAllBytes(filePath);
        
        for (int i = 0; i < pcm.Length; i++)
        {
            Debug.Log(pcm[i].ToString());
        }
        //audio = GetComponent<AudioSource>();
        //BufferedStream buf = new BufferedStream(file);

        float[] f = ConvertByteToFloat(pcm);
        for (int i = 0; i < f.Length; i++)
        {
            Debug.Log(f[i].ToString());
        }
        int channels = 2;
        int sampleRate = 44100;
        AudioClip clip = AudioClip.Create("Output", f.Length, channels, sampleRate, false);
        
        clip.SetData(f, 0);
        audio.clip = clip;
        audio.pitch = 0.2f;
        audio.Play();
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
}
