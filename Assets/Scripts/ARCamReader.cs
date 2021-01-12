//#if UNITY_EDITOR || (!UNITY_WEBGL && !UNITY_WSA)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Byn.Awrtc.Native;
using Byn.Awrtc.Unity;
using UnityEngine.UI;

public class ARCamReader : MonoBehaviour
{
    private string _DeviceName = "VirtualCamera1";
    [SerializeField] private Material arBackground;
    private string mUsedDeviceName;

    [SerializeField] private int res_divide_scale = 8;

    /// <summary>
    /// FPS the virtual device is suppose to have.
    /// (This isn't really used yet except to filter
    /// out this device if MediaConfig requests specific FPS)
    /// </summary>
    [SerializeField] private int _Fps = 30;


    /// <summary>
    /// Interface for video device input.
    /// </summary>
    private NativeVideoInput mVideoInput;

    /// <summary>
    /// Can be used to output the image sent for testing
    /// </summary>
    [SerializeField] private RawImage _DebugTarget = null;

    private Texture2D tex2D;
    RenderTexture resizedRenderTex;

    private void Awake()
    {
        mUsedDeviceName = _DeviceName;
    }
    // Start is called before the first frame update
    void Start()
    {
        mVideoInput = UnityCallFactory.Instance.VideoInput;
        mVideoInput.AddDevice(mUsedDeviceName, Screen.width / 4, Screen.height / 4 , _Fps);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        //GetArTexFromActiveRenderTex();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(resizedRenderTex == null)
            resizedRenderTex = new RenderTexture(source.width / res_divide_scale, source.height / res_divide_scale, 8);
        Graphics.Blit(source, resizedRenderTex);

        RenderTexture sourceRt = resizedRenderTex;

        if (!sourceRt)
            return;

        if (_DebugTarget != null)
        {
            _DebugTarget.texture = sourceRt;
            //_DebugTarget.GetComponent<RectTransform>().sizeDelta = new Vector2(sourceRt.width / 4, sourceRt.height / 4);
        }
        else
            Debug.Log("Debug taget UIis null");
        
        //update the internal WebRTC device
        Texture2D tex2D = RenderTexToTexture2D(sourceRt, resizedRenderTex.width, resizedRenderTex.height);
        //tex2D.Resize(RenderTexture.active.width / 4, RenderTexture.active.height / 4);
        mVideoInput.UpdateFrame(mUsedDeviceName, tex2D.GetRawTextureData(), sourceRt.width, sourceRt.height, WebRtcCSharp.VideoType.kABGR, 0, true);

        Graphics.Blit(source, destination);
    }

    private void GetArTexFromActiveRenderTex()
    {

        if (!RenderTexture.active)
            return;

        if (_DebugTarget != null)
        {
            _DebugTarget.texture = RenderTexture.active;
            //_DebugTarget.GetComponent<RectTransform>().sizeDelta = new Vector2(RenderTexture.active.width / 4, RenderTexture.active.height / 4);
        }
        else
            Debug.Log("Debug taget UIis null");

        //update the internal WebRTC device
        Texture2D tex2D = RenderTexToTexture2D(RenderTexture.active, RenderTexture.active.width, RenderTexture.active.height);

        //tex2D.Resize(RenderTexture.active.width / 4, RenderTexture.active.height / 4);
        mVideoInput.UpdateFrame(mUsedDeviceName, tex2D.GetRawTextureData(), RenderTexture.active.width / 4, RenderTexture.active.height / 4, WebRtcCSharp.VideoType.kABGR, 0, true);
    }

    private void GetARTexFromMaterial()
    {
        if (!arBackground.mainTexture)
            return;

        Texture2D tex2D = arBackground.mainTexture as Texture2D;
        if (tex2D == null)
        {
            tex2D = TextureToTexture2D(arBackground.mainTexture, 10, 10);
        }

        //update the internal WebRTC device
        mVideoInput.UpdateFrame(mUsedDeviceName, tex2D.GetRawTextureData(), 10, 10, WebRtcCSharp.VideoType.kBGRA, 0, true);

        //update debug output if available

        if (_DebugTarget != null)
        {
            _DebugTarget.texture = arBackground.mainTexture;
            _DebugTarget.GetComponent<RectTransform>().sizeDelta = new Vector2(tex2D.width / 4, tex2D.height / 4);
        }
        else
            Debug.Log("Debug taget UIis null");
    }

    private Texture2D RenderTexToTexture2D(RenderTexture renderTex, int copyWidth, int copyHeight)
    {
        if(tex2D == null)
        {
            tex2D = new Texture2D(copyWidth, copyHeight, TextureFormat.RGBA32, false);
            
        }
        tex2D.ReadPixels(new Rect(0, 0, copyWidth, copyHeight), 0, 0);
        tex2D.Apply();
        return tex2D;
    }

    private Texture2D TextureToTexture2D(Texture texture, int texWidth, int texHeight)
    {
        Texture2D texture2D = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texWidth, texHeight, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }
}
//#endif
