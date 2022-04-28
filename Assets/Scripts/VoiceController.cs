using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Android;
using TextSpeech;

public class VoiceController : MonoBehaviour
{
    [SerializeField]
    static string LANG_CODE = "ja-JP"; // en-US

    [SerializeField]
    static float pitch = 1.0f; //1.15

    float speed = 1.0f;


    //[SerializeField]
    //Text uiText;

    static bool isSpeaking = false;
    static bool isStartSpeaking = false;

    //public static VoiceController Instance;

    void Start()
    {
        Setup(LANG_CODE);

        //#if UNITY_ANDROID
        //        SpeechToText.instance.onResultCallback = OnPartialSpeechResult;
        //#endif
        //SpeechToText.instance.onResultCallback = OnFinalSpeechResult;

        //CheckPermission();

        TextToSpeech.instance.onStartCallBack = OnSpeakStart;
        TextToSpeech.instance.onDoneCallback = OnSpeakStop;

        Debug.Log("Voice controller is set.");
    }

    void CheckPermission() // on android
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }
    #region Text to speech

    public static void StartSpeaking(string message)
    {
        TextToSpeech.instance.StartSpeak(message);
        isStartSpeaking = true;
        Debug.Log("TTS Speaking:" + message);
    }

    public static void StopSpeaking()
    {
        TextToSpeech.instance.StopSpeak();
        Debug.Log("TTS Stop Speaking");

    }

    private IEnumerator OnSpeakStop_DelayMethod(float waitTime)
    {
        Debug.Log("Voice manager startCoroutine, Wait for: " + waitTime);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Talking stopped...");
        isSpeaking = false;
        isStartSpeaking = false;
    }

    void OnSpeakStart()
    {
        isSpeaking = true;
        Debug.Log("Talking started...");
    }

    void OnSpeakStop()
    {
        StartCoroutine("OnSpeakStop_DelayMethod", 0.00f);
    }

    public static bool IsSpeaking()
    {
        return isSpeaking;
    }

    public static bool IsStartSpeaking()
    {
        return isStartSpeaking;
    }

    #endregion

    //#region Speech to text
    //public void StartListening()
    //{
    //    SpeechToText.instance.StartRecording();
    //}

    //public void StopListening()
    //{
    //    SpeechToText.instance.StopRecording();
    //}

    //void OnFinalSpeechResult(string result)
    //{
    //    uiText.text = result;
    //}

    //void OnPartialSpeechResult(string result)
    //{
    //    uiText.text = result;
    //}

//#endregion

    void Setup(string code)
    {
        TextToSpeech.instance.Setting(code, pitch, speed);
        //SpeechToText.instance.Setting(code);

    }

    public static void SetupSpeed(float speed)
    {
        TextToSpeech.instance.Setting(LANG_CODE, pitch, speed);
    }
}
