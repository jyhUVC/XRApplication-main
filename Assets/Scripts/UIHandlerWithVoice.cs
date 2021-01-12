using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// 음석인식을 통한 UI 컨트롤러.
/// </summary>
public class UIHandlerWithVoice : MonoBehaviour
{
    private Dictionary<string, System.Action> sentenceActions = new Dictionary<string, System.Action>();

    [Header("menu list")]
    public GameObject cpsMenu;
    public GameObject drawingMenu;

    public Dictionary<string, GameObject> menuTable;

    public void Awake()
    {
/*        // *******EDITED********
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
        // *******EDITED*********/
        MicHandler.OnNewWakeupSenenceEvent += OnSentenceChange;

        menuTable = new Dictionary<string, GameObject>
        {
            {"CPS 메뉴", cpsMenu },
            {"그리기 메뉴", drawingMenu },
        };
    }

    public void OnSentenceChange(string sentence)
    {
        foreach (KeyValuePair<string, GameObject> item in menuTable)
        {
            if(sentence.Contains(item.Key))
            {
                OnOffMenus(item.Value);
                break;
            }
        }
    }

    public void OnDestroy()
    {
        MicHandler.OnNewWakeupSenenceEvent -= OnSentenceChange;
    }

    public void OnOffMenus(GameObject menu)
    {
        if (menu.activeSelf)
            menu.SetActive(false);
        else
            menu.SetActive(true);
    }
}
