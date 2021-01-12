using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Byn.Awrtc;
using Byn.Awrtc.Unity;

public class XRCallUI : MonoBehaviour
{
    /// <summary>
    /// Texture of the local video
    /// </summary>
    protected Texture2D mLocalVideoTexture = null;

    /// <summary>
    /// Texture of the remote video
    /// </summary>
    protected Texture2D mRemoteVideoTexture = null;

    /// <summary>
    /// Video chat join button, will be hidden after join.
    /// </summary>
    [SerializeField] private RectTransform joinBtn;

    /// <summary>
    /// Video chat disconnect button.
    /// </summary>
    [SerializeField] private RectTransform disconnectBtn;

    [Header("Settings")]
    public int idealFPS = 30;
    public int res_divide_scale = 8;

    [Header("Video panel elements")]
    /// <summary>
    /// Image of the local camera
    /// </summary>
    [SerializeField] private RawImage uLocalVideoImage;

    /// <summary>
    /// Image of the remote camera
    /// </summary>
    [SerializeField] private RawImage uRemoteVideoImage;

    [Header("Resources")]
    [SerializeField] private Texture2D uNoCameraTexture;
    [SerializeField] private InputField roomNameField;

    [SerializeField] private XRCallApp xrCallApp;

    private bool mHasLocalVideo = false;
    private int mLocalVideoWidth = -1;
    private int mLocalVideoHeight = -1;
    private int mLocalFps = 0;
    private int mLocalFrameCounter = 0;
    private int mLocalRotation = 0;
    private FramePixelFormat mLocalVideoFormat = FramePixelFormat.Invalid;

    private bool mHasRemoteVideo = false;
    private int mRemoteVideoWidth = -1;
    private int mRemoteVideoHeight = -1;
    private int mRemoteFps = 0;
    private int mRemoteRotation = 0;
    private int mRemoteFrameCounter = 0;
    private FramePixelFormat mRemoteVideoFormat = FramePixelFormat.Invalid;

    float remoteVideoImageWidth;
    float remoteVideoImageHeight;
    
    Vector2 remoteVideoImagePos;

    public Text debugText;
    // Start is called before the first frame update
    void Start()
    {
        ChangeUIRemotePeerState(false);
        remoteVideoImageWidth = uRemoteVideoImage.rectTransform.rect.width;
        remoteVideoImageHeight = uRemoteVideoImage.rectTransform.rect.height;
        remoteVideoImagePos = uRemoteVideoImage.transform.position;
    }

    /// <summary>
    /// Updates the local video. If the frame is null it will hide the video image
    /// </summary>
    /// <param name="frame"></param>
    public virtual void UpdateLocalTexture(IFrame frame, FramePixelFormat format)
    {
        if (uLocalVideoImage != null)
        {
            if (frame != null)
            {
                UnityMediaHelper.UpdateTexture(frame, ref mLocalVideoTexture);
                uLocalVideoImage.texture = mLocalVideoTexture;
                //uLocalVideoImage.transform.localRotation = Quaternion.Euler(0, 0, frame.Rotation);
                uLocalVideoImage.transform.localScale = new Vector3(1f, -1f, 1f);
                if (uLocalVideoImage.gameObject.activeSelf == false)
                {
                    uLocalVideoImage.gameObject.SetActive(true);
                }
                //apply rotation
                //watch out uLocalVideoImage should be scaled -1 X to make the local camera appear mirrored
                //it should also be scaled -1 Y because Unity reads the image from bottom to top
                //uLocalVideoImage.transform.localRotation = Quaternion.Euler(0, 0, frame.Rotation);


                mHasLocalVideo = true;
                mLocalFrameCounter++;
                mLocalVideoWidth = frame.Width;
                mLocalVideoHeight = frame.Height;
                mLocalVideoFormat = format;
                mLocalRotation = frame.Rotation;
                //debugText.text = "" + frame.Width + " , " + frame.Height;
            }
            else
            {
                //app shutdown. reset values
                mHasLocalVideo = false;
                uLocalVideoImage.texture = null;
                uLocalVideoImage.transform.localRotation = Quaternion.Euler(0, 0, 180);
                uLocalVideoImage.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates the remote video. If the frame is null it will hide the video image.
    /// </summary>
    /// <param name="frame"></param>
    public virtual void UpdateRemoteTexture(IFrame frame, FramePixelFormat format)
    {
        if (uRemoteVideoImage != null)
        {
            if (frame != null)
            {
                UnityMediaHelper.UpdateTexture(frame, ref mRemoteVideoTexture);
                uRemoteVideoImage.texture = mRemoteVideoTexture;
                //watch out: due to conversion from WebRTC to Unity format the image is flipped (top to bottom)
                //this also inverts the rotation
                //uRemoteVideoImage.transform.localRotation = Quaternion.Euler(0, 0, frame.Rotation * -1);
                uRemoteVideoImage.transform.localScale = new Vector3(1f, -1f, 1f);
                mHasRemoteVideo = true;

                mRemoteVideoWidth = frame.Width;
                mRemoteVideoHeight = frame.Height;
                mRemoteVideoFormat = format;
                mRemoteRotation = frame.Rotation;
                mRemoteFrameCounter++;


            }
            else
            {
                mHasRemoteVideo = false;
                uRemoteVideoImage.texture = uNoCameraTexture;
                uRemoteVideoImage.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    /// <summary>
    /// Join button pressed. Tries to join a room.
    /// </summary>
    public void JoinButtonPressed()
    {
        SetupCallApp();

    }

    /// <summary>
    /// Shutdown button pressed. Shuts the network down.
    /// </summary>
    public void ShutdownButtonPressed()
    {
        xrCallApp.ResetCall();

    }

    private void SetupCallApp()
    {
        xrCallApp.SetAudio(false);
        xrCallApp.SetVideo(true);
        xrCallApp.SetAutoRejoin(false);

        xrCallApp.SetVideoDevice("VirtualCamera1");

        xrCallApp.SetIdealResolution(Screen.width / 4, Screen.height / 4);
        xrCallApp.SetIdealFps(idealFPS);

        xrCallApp.SetShowLocalVideo(true);
        xrCallApp.SetupCall();
        EnsureLength();
        Append("Trying to listen on address " + roomNameField.text);
        xrCallApp.Join(roomNameField.text);
        //calledSetUp = true;
    }
    /*bool calledSetUp;
    private void Update()
    {
        if (calledSetUp)
        {
            debugText.text = xrCallApp.IsMute().ToString();
            if (xrCallApp.IsMute())
            {
                xrCallApp.SetMute(false);
            }
        }
    }*/

    /// <summary>
    /// Adds a new message to the message view
    /// </summary>
    /// <param name="text"></param>
    public void Append(string text)
    {
        //if (uMessageOutput != null)
        //{
        //    uMessageOutput.AddTextEntry(text);
        //}
        //Debug.Log("Chat output: " + text);
    }

    private void EnsureLength()
    {
        if (roomNameField.text.Length > CallApp.MAX_CODE_LENGTH)
        {
            roomNameField.text = roomNameField.text.Substring(0, CallApp.MAX_CODE_LENGTH);
        }
    }

    public string GetRoomname()
    {
        EnsureLength();
        return roomNameField.text;
    }

    /// <summary>
    /// 리모트 UI 에 끊김 표시 또는 비표시.
    /// </summary>
    /// <param name="isConnected"></param>
    public void ChangeUIRemotePeerState(bool isConnected)
    {
        if (isConnected)
        {
            //uRemoteVideoImage.texture = uNoCameraTexture;
        }
        else
        {
            uRemoteVideoImage.texture = uNoCameraTexture;
        }
    }

    public bool mMain = true;

    /// <summary>
    /// 화면 전환
    /// </summary>
    public void CamSwitch()
    {
        if (mMain)
        {
            mMain = false;
            //debugText.text = ""+ Screen.width + " , " +  Screen.height;
            uRemoteVideoImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            //중앙에 놓고 screensize로 바꿈
            uRemoteVideoImage.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }

        else
        {
            // 다시 원래 사이즈 & 위치로
            mMain = true;
            uRemoteVideoImage.rectTransform.sizeDelta = new Vector2(remoteVideoImageWidth, remoteVideoImageHeight);
            uRemoteVideoImage.transform.position = remoteVideoImagePos;
        }
    }
}
