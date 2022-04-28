using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;


public class TouchManager_V2 : MonoBehaviour
{
    GameObject tpGO;
    List<GameObject> tps;
    List<Touch> oTouches;
    static int camPointDist = 125;

    Text statusText;
    GameObject accessTextGO;
    List<GameObject> accessTexts;

    Dictionary<string, string> dct = AccessTextDict.dct;

    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    AudioSource audioSource;

    enum DirectionState {Start, End}
    DirectionState directionState = DirectionState.End;
    int navigationOrder = 0;

    Transform pStart = null, pEnd = null;

    Transform prevNav;
    string prevMsg;

    private void Awake()
    {
        //GameObject.Find("Canvas_UI").transform.Find("AccessText_PlaceHolder").gameObject.GetComponent<Text>().text = "   ";
    }

    // Start is called before the first frame update
    void Start()
    {
        tpGO = GameObject.Find("MainModel").transform.Find("TP").gameObject;
        tpGO.SetActive(false);
        tps = new List<GameObject>();

        statusText = GameObject.Find("Canvas_UI").transform.Find("TouchStatusText").gameObject.GetComponent<Text>();

        accessTextGO = GameObject.Find("Canvas_UI").transform.Find("AccessText").gameObject;
        accessTextGO.SetActive(false);

        LoadAudioClipsDct();
    }

    // Update is called once per frame
    void Update()
    {
        // On multi-touch device
        for (int i = 0; i < tps.Count; i++)
        {
            tps[i].SetActive(false);
        }

        if (Input.touchCount > 0)
        {
            // order touches by x location from left to right.
            oTouches = (new List<Touch>(Input.touches)).OrderBy(order => order.position.x).ToList();

            for (int i = 0; i < oTouches.Count; i++)
            {
                Touch touch = oTouches[i];

                Vector3 vTouchPos = touch.position;
                Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));

                // visualize touch point
                if (i >= tps.Count)
                {
                    //initialize new touch point
                    GameObject tp = Instantiate(tpGO, worldTouchPos, Quaternion.identity);
                    tp.transform.Find("FingerID").gameObject.GetComponent<TextMesh>().text = String.Format("{0}", touch.fingerId);
                    tps.Add(tp);
                }
                else
                {
                    tps[i].transform.position = worldTouchPos;
                    tps[i].transform.Find("FingerID").gameObject.GetComponent<TextMesh>().text = String.Format("{0}", touch.fingerId);

                    tps[i].SetActive(true);
                }

                //accessTextGO.transform.position = worldTouchPos;

                // detect object for double tap
                if ((touch.tapCount == 2) && (touch.phase == TouchPhase.Ended))
                {
                    tps[i].GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                    if (!RouteManager.Instance.IsDirectionMode())
                        TouchRayHit(vTouchPos, worldTouchPos);
                }
                else
                {
                    tps[i].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                }

                //if (RouteManager.Instance.IsDirectionMode())
                if (!RouteManager.Instance.IsDirectionMode())
                {
                    if ((touch.tapCount == 3) && (touch.phase == TouchPhase.Ended))
                    {
                        Debug.Log("TripleTouch");
                        TouchRayHitDirection(vTouchPos, worldTouchPos);
                    }
                }

                if ((touch.tapCount == 1) && ((touch.phase == TouchPhase.Moved)||(touch.phase == TouchPhase.Began)))
                {
                    tps[i].GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
                    // navigation mode
                    //if ((RouteManager.Instance.IsDirectionMode()) && (directionState == DirectionState.End))
                    if ((!RouteManager.Instance.IsDirectionMode()) && (directionState == DirectionState.End))
                    {
                        TouchRayHitNavigation(vTouchPos, worldTouchPos);

                        if (!VoiceController.IsSpeaking())
                        {
                            Debug.Log("TTS isn't speaking. Looking for nagivation touch.");
                        }
                        else
                        {
                            Debug.Log("TTS started speaking. Isn't looking for nagivation touch.");
                        }
                    }
                    else
                    { Debug.Log("directionState != DirectionState.End"); }

                }
                else
                {
                    tps[i].GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                }

            }
        }

        //// Debug on computer
        //tpGO.SetActive(false);
        //if (Input.GetKey(KeyCode.Mouse0))
        //{
        //    // viz touch point
        //    Vector3 vTouchPos = Input.mousePosition;
        //    Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));
        //    tpGO.transform.position = worldTouchPos;
        //    tpGO.SetActive(true);

        //    TouchRayHit(vTouchPos, worldTouchPos);
        //}

    }

    void TouchRayHitNavigation(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);
        // Raycast handling
        RaycastHit vHit;

        List<Transform> routeList = RouteManager.Instance.GetNavRoute();
        List<string> routeDirectList = RouteManager.Instance.GetNavDirect();

        SetNodesColor(routeList, Color.yellow);

        if ((routeDirectList == null)||(routeDirectList.Count == 0))
        {
            return;
        }

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Transform touched = vHit.transform;

            Debug.Log("Navigation Ray hit: " + touched.name);
            Debug.Log("navigationOrder: " + navigationOrder);


            if (touched == routeList[navigationOrder])
            {
                Debug.Log("routeList[navigationOrder]: " + routeList[navigationOrder].name);
                //Debug.Log("routeDirectList[navigationOrder]: " + routeDirectList[navigationOrder]);


                if (navigationOrder < routeDirectList.Count)
                {
                    Transform tNow = routeList[navigationOrder], tNext = routeList[navigationOrder + 1];
                    string msg = "";
                    if (navigationOrder == 0)
                    {
                        msg = String.Format("You reached {0}'s entrance. Next, move {1} to {2}.",
                            RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext));
                        //VoiceController.StartSpeaking(msg);
                        //navigationOrder++;
                        //Debug.Log(msg);

                    }
                    else
                    {
                        if(navigationOrder < routeDirectList.Count - 1)
                            msg = String.Format("You reached {0}. Next, move {1} to {2} {3}.",
                            RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext), navigationOrder+1);
                        else
                            msg = String.Format("You reached {0}. Next, move {1} to {2}.",
                            RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext));

                        //VoiceController.StartSpeaking(msg);
                        //navigationOrder++;
                        //Debug.Log(msg);
                    }
                    StartCoroutine(StopStartSpeaking_DelayMethod(msg));
                    navigationOrder++;
                    Debug.Log("Speaking msg: " + msg) ;

                    tNow.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    tNext.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                }
                else if(navigationOrder == routeDirectList.Count)
                {
                    Transform tNow = routeList[navigationOrder];
                    string msg = String.Format("You reached the destination: {0}. ", RouteManager.Instance.RouteString(tNow));
                    //VoiceController.StartSpeaking(msg);

                    StartCoroutine(StopStartSpeaking_DelayMethod(msg));

                    SetNodesColor(routeList, Color.green);

                    RouteManager.Instance.ResetNav();
                    navigationOrder = 0;
                    Debug.Log(msg);
                }
            }
        }
    }

    void SetNodesColor(List<Transform> routeList, Color color)
    {

        foreach (Transform route in routeList)
        {
            route.GetComponent<Renderer>().material.SetColor("_Color", color);
        }
    }

    static string GetPathBasedOnOS()
    {
        if (Application.isEditor)
        {
            Debug.Log("is Editor");
            return Application.dataPath;
        }
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
        {
            Debug.Log("is Mobile");
            return Application.persistentDataPath;
        }
        else // For standalone player.
        {
            Debug.Log("is Standalone");
            return Application.persistentDataPath;
        }
    }

    void LoadAudioClipsDct()
    {
        audioSource = GameObject.Find("AudioController").GetComponent<AudioSource>();

        foreach (string file in dct.Keys.ToArray())
        {
            //Debug.Log("File Path:" + file);
            string audioName = file;
            audioClips.Add(audioName, Resources.Load<AudioClip>("Audios/" + audioName));
        }
        Debug.Log(audioClips.Count() + " audio clips loaded");
    }

    void LoadAudioClips()
    {
        audioSource = GameObject.Find("AudioController").GetComponent<AudioSource>();

        string info = GetPathBasedOnOS() + "/Resources/Audios/";

        //Debug.Log("App Path:" + Application.dataPath);
        //Debug.Log("Audios Path:" + info);

        string[] fileInfo = Directory.GetFiles(info, "*.mp3");
        foreach (string file in fileInfo)
        {
            Debug.Log("File Path:" + file);
            string audioName = file.Split('.')[0].Split('/').Last();
            audioClips.Add(audioName, Resources.Load<AudioClip>("Audios/" + audioName));
        }
        Debug.Log(audioClips.Count() + " audio clips loaded");
    }

    void PlayAudioClip(string audioName)
    {
        audioSource.Stop();
        if (VoiceController.IsSpeaking())
            VoiceController.StopSpeaking();

        if (audioClips.ContainsKey(audioName))
        {
            audioSource.clip = audioClips[audioName];
            audioSource.Play();
            Debug.Log("Play audio file: " + audioName);
        }
    }

    private IEnumerator StopStartSpeaking_DelayMethod( string msg)
    {
        float waitTime = 0.25f;
        if (VoiceController.IsSpeaking())
        {
            VoiceController.StopSpeaking();
            //yield return new WaitForSeconds(waitTime);
        }
        else
        {
            Debug.Log("Stop->Start: Not speaking, cannot stop");
        }

        float waitTimeStop = 0.5f;
        yield return new WaitForSeconds(waitTimeStop);
        Debug.Log("Stop->Start:  Stop Coroutine, Wait for: " + waitTimeStop);


        if (!VoiceController.IsSpeaking())
        {
            Debug.Log("Stop->Start: Start after stopped");
            VoiceController.StartSpeaking(msg);
            yield return new WaitForSeconds(waitTime);
            Debug.Log("Stop->Start: Start Coroutine, Wait for: " + waitTime);

        }
        else
        {
            Debug.Log("Stop->Start: Still speaking, cannot start");
        }
    }


    void TripleTouchStateChange(Transform touched)
    {
        switch (directionState)
        {
            case DirectionState.End:
                //audioSource.Stop();
                pEnd = touched;
                string msg = pEnd.name + ". Destination marked. Please mark start point";
                directionState = DirectionState.Start;
                StartCoroutine(StopStartSpeaking_DelayMethod(msg));
                break;

            case DirectionState.Start:
                //audioSource.Stop();
                pStart = touched;
                string msg2 = pStart.name + ". Start point marked. " + RouteManager.Instance.RouteFindSpecific(pStart, pEnd);
                directionState = DirectionState.End;
                navigationOrder = 0;
                StartCoroutine(StopStartSpeaking_DelayMethod(msg2));
                break;
        }
    }

    void TouchRayHitDirection(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

        // Rayを画面に表示
        float maxDistance = 500;
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);
        // Raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Debug.Log("Ray hit something");
            Transform touched = vHit.transform;
            if(RouteManager.Instance.IsSubRoute(touched))
                TripleTouchStateChange(touched);
        }
    }


    void TouchRayHit(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

        // Rayを画面に表示
        float maxDistance = 500;
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);
        // Raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Debug.Log("Ray hit something");

            statusText.text = vHit.transform.name;

            // generate an access label
            InitialAcessLabel(worldTouchPos, vHit.transform.name);

            if (!RouteManager.Instance.IsDirectionMode())
            {
                Transform pMain = vHit.transform;
                if (RouteManager.Instance.IsMainRoute(pMain))
                {
                    string msg = RouteManager.Instance.RouteFindAround(pMain);
                    StartCoroutine(StopStartSpeaking_DelayMethod(msg));
                }
                else
                    PlayAudioClip(vHit.transform.name);
            }
            else
            {
                Debug.Log("direction mode");

                // exploration mode: read main path
                Transform pMain = vHit.transform;
                string msg = RouteManager.Instance.RouteFindAround(pMain);

                StartCoroutine(StopStartSpeaking_DelayMethod(msg));
            }
        }
        else
        {
            statusText.text = "Touch status";
        }
    }

    void InitialAcessLabel(Vector3 worldTouchPos, string labelName)
    {
        GameObject access = Instantiate(accessTextGO,
            new Vector3(worldTouchPos.x, worldTouchPos.y, accessTextGO.transform.position.z),
            Quaternion.identity);
        string text = "";

        if (dct.ContainsKey(labelName))
            text = dct[labelName];
        else text = labelName;

        access.GetComponent<Text>().text = text;
        access.transform.SetParent(GameObject.Find("Canvas_UI").transform);
        access.transform.localScale = new Vector3(1f, 1f, 1f) *1;
        access.SetActive(true);

        Debug.Log("word Length = " + text.Length);
        Destroy(access, WordReadSec(text.Length));
    }

    float WordReadSec(float len)
    {
        if (len <= 3)
            return 0.7f * len;
        else if (len <= 5)
            return 0.5f * len;
        else if (len <= 10)
            return 0.3f * len;
        else
            return (float)(-0.0014 * (float)Math.Pow((float)len, 2) + 0.2833 * len - 0.1726 + 0.5);
    }

    void PlayAudioFile()
    {

    }
}
