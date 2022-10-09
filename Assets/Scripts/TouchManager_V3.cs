using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;


public class TouchManager_V3 : MonoBehaviour
{
    // viz: touch
    GameObject tpGO;
    List<GameObject> tps;
    List<Touch> oTouches;
    static int camPointDist = 125;

    // viz: touched GO
    Text statusText;
    GameObject accessTextGO;
    List<GameObject> accessTexts;

    // script dic Done use CSV 
    Dictionary<string, string> dct = AccessTextDict.dct;
    // load audio clips one Done use TTS
    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    AudioSource audioSource;

    // use CSV
    TextAsset csvFile; // CSVファイル
    Dictionary<string, ExhibitionScript> csvData; // CSVの中身を入れるdict
    int height = 0; // CSVの行数

    // exploration
    //[SerializeField]
    string floor = "5F";
    ExhibitionScript prevEs = null;

    // navigation
    enum DirectionState { Pre, Start, End, Guide }
    DirectionState directionState = DirectionState.Pre;
    int navigationOrder = 0;
    Transform pStart = null, pEnd = null;
    bool turnMode = false;

    // button texts
    [SerializeField]
    Text buttonNavText;
    [SerializeField]
    Text buttonTurnText;

    // save log CSV
    string systemTime;
    CsvExporter csvExporter;
    int saveTime = 5 * 60 * 40; //5 min
    string hitObjectName;
    string navMode;

    //automated guide
    string inst_Guide1, inst_Guide2;
    Dictionary<string, ExhibitionScript> csvGuideData; // CSVの中身を入れるdict
    List<string> inst1;
    List<float> inst1_times;
    bool touchEnabled = true;
    int speakOrder = 0;

    void Start()
    {
        // viz: touch
        tpGO = GameObject.Find("MainModel").transform.Find("TP").gameObject;
        tpGO.SetActive(false);
        tps = new List<GameObject>();

        // viz: touched GO
        statusText = GameObject.Find("Canvas_UI").transform.Find("TouchStatusText").gameObject.GetComponent<Text>();
        accessTextGO = GameObject.Find("Canvas_UI").transform.Find("AccessText").gameObject;
        accessTextGO.SetActive(false);

        // load audio clips
        LoadAudioClipsDct();

        InitializeFloor(floor);

        InitializeCSVWriter();

        //load automated text
        (inst1, inst1_times) = LoadTextStrings("Instruct1_Overall");
        inst_Guide1 = LoadText("Instruct2_GuideStart");
        inst_Guide2 = LoadText("Instruct2_GuideStart_2");
        csvGuideData = LoadCSV("5F_GuidedTour");

        if (floor == "5F")
        {
            RouteManager_Graph.Instance.Guide_FindNodeDirectTurn();
        }
    }

    private void InitializeFloor(string floor)
    {
        // set other floors inactive and selected floor active
        string[] floors = new string[] { "0F", "1F", "3F", "5F", "7F" };
        foreach (string f in floors)
            GameObject.Find("MainModel").transform.Find(f).gameObject.SetActive(false);

        GameObject floorGO = GameObject.Find("MainModel").transform.Find(floor).gameObject;
        floorGO.SetActive(true);

        RouteManager_Graph.Instance.InitializeFloorGraph(floor);
        buttonNavText.text = "Nav:N";

        ModelPosManager.Instance.InitializeFloor(floor);

        // load csv file
        csvData = LoadCSV(floor);
    }

    #region Single Tap Buttons

    void SaveOneClickButton(string buttonName)
    {
        //save Log
        // headers: {"time", "mode", "modeDetail", "obj", "floor", "tapCount", "tapPhase", "loc_x", "loc_y"};

        string[] saveLine = { GetSystemTime_milliSec(), GetMode(), navMode, buttonName, floor, null, null, null, null };
        csvExporter.AppendCSV(saveLine);
    }

    public void OnClick_0F()
    {
        Debug.Log("Initialize 0F_key");
        floor = "0F";
        InitializeFloor(floor);

        SaveOneClickButton("OnClick_0F");
    }

    public void OnClick_1F()
    {
        Debug.Log("Initialize 1F");
        floor = "1F";
        InitializeFloor(floor);

        SaveOneClickButton("OnClick_1F");
    }

    public void OnClick_3F()
    {
        Debug.Log("Initialize 3F");
        floor = "3F";
        InitializeFloor(floor);

        SaveOneClickButton("OnClick_3F");
    }

    public void OnClick_5F()
    {
        Debug.Log("Initialize 5F");
        floor = "5F";
        InitializeFloor(floor);

        SaveOneClickButton("OnClick_5F");
    }

    public void OnClick_7F()
    {
        Debug.Log("Initialize 7F Exp");
        floor = "7F";
        InitializeFloor(floor);

        SaveOneClickButton("OnClick_7F");
    }

    public void OnClick_7F_Conf()
    {
        Debug.Log("Initialize 7F Conf");
        floor = "7F";
        InitializeFloor(floor);
        ModelPosManager.Instance.InitializeFloor(floor, "Conf");

        SaveOneClickButton("OnClick_7F_Conf");
    }

    public void OnClick_InstStart()
    {
        //touchEnabled = false;
        StartCoroutine(StopStartSpeaking_DelayMethod_Auto(inst1, inst1_times));

        SaveOneClickButton("OnClick_InstStart");
    }

    public void OnClick_InstGuide()
    {
        StopSpeaking();
        StartCoroutine(StopStartSpeaking_DelayMethod_Button(inst_Guide1));
        //StartCoroutine(StopStartSpeaking_DelayMethod_Button("テスト　テスト　テスト　もけー"));

        RouteManager_Graph.Instance.DirectionModeTrue();
        directionState = DirectionState.Guide;
        Debug.Log("Direction Mode = " + RouteManager_Graph.Instance.IsDirectionMode());
        navigationOrder = 0;

        SaveOneClickButton("OnClick_InstGuide");
    }

    public void OnClick_Turn()
    {
        turnMode = !turnMode;
        if (turnMode)
        {
            buttonTurnText.text = "Turn by turn: Y";
        }
        else
        {
            buttonTurnText.text = "Turn by turn: N";
        }

        SaveOneClickButton("turnMode = "+ turnMode);
    }
    #endregion

    #region Doublet tap buttons

    public bool ButtonsClicked(Transform hit)
    {
        bool clicked = true;
        if (hit.name == "Cube_Button_Stop")
            OnClick_Stop();
        else if (hit.name == "Cube_Button_Nav")
            OnClick_Nav();
        else if (hit.name == "Cube_Button_Speed_Down")
            OnClick_SpeedDown();
        else if (hit.name == "Cube_Button_Speed_Up")
            OnClick_SpeedUp();
        else
            clicked = false;

        if(clicked)
            hitObjectName = hit.name;
        return clicked;
    }

    public void OnClick_Stop()
    {
        StopSpeaking();
        if ((RouteManager_Graph.Instance.IsDirectionMode()) && (directionState == DirectionState.Start))
        {
            directionState = DirectionState.Pre;
            StartCoroutine(StopStartSpeaking_DelayMethod_Button("目的地を２回タップしてください。"));
        }
    }

    float speed = 1.0f;
    public void OnClick_SpeedDown()
    {
        speed -= 0.1f;
        VoiceController.SetupSpeed(speed);
        StartCoroutine(StopStartSpeaking_DelayMethod_Button("読み上げ速度　" + String.Format("{0:0.0}", speed)));
    }

    public void OnClick_SpeedUp()
    {
        speed += 0.1f;
        VoiceController.SetupSpeed(speed);
        StartCoroutine(StopStartSpeaking_DelayMethod_Button("読み上げ速度　" + String.Format("{0:0.0}", speed)));
    }

    public void OnClick_Nav()
    {
        RouteManager_Graph.Instance.DirectionModeSwitch();
        if (RouteManager_Graph.Instance.IsDirectionMode())
        {
            StartCoroutine(StopStartSpeaking_DelayMethod_Button("ナビモード。 目的地を２回タップしてください。"));
            buttonNavText.text = "Nav:Y";
        }
        else
        {
            StartCoroutine(StopStartSpeaking_DelayMethod_Button("探索モード。音声ガイドを聞きたい場所をダブルタップして下さい。"));
            buttonNavText.text = "Nav:N";
        }
    }
    #endregion

    #region CSV writer

    void InitializeCSVWriter()
    {
        //save CSV with current system time as title YYYYmmdd_hhmmss
        systemTime = GetSystemTime();
        Debug.Log("System time:" + systemTime + ": New CSV file");
        csvExporter = new CsvExporter(systemTime + "_Obento_Log");
    }

    string GetSystemTime()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }

    string GetSystemTime_milliSec()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
    }

    string GetMode()
    {
        if (RouteManager_Graph.Instance.IsDirectionMode())
            return "Navigation";
        return "Exploration";
    }

    #endregion

    void Update()
    {
        if (Time.frameCount % saveTime == 0)
        {
            Debug.Log("Time.frameCount: " + Time.frameCount);
            InitializeCSVWriter();
        }

        hitObjectName = null;
        navMode = null;
        if(RouteManager_Graph.Instance.IsDirectionMode())
        {
            if (directionState == DirectionState.Pre)
                navMode = "Idle";
            else if (directionState == DirectionState.Start)
                navMode = "Selection";
            else if (directionState == DirectionState.End)
                navMode = "Follow";
            else if (directionState == DirectionState.Guide)
                navMode = "Guidede Exploration";
        }
        
        // On multi-touch device
        for (int i = 0; i < tps.Count; i++)
        {
            tps[i].SetActive(false);
        }

        if (Input.touchCount > 0 && touchEnabled)
        {
            //// order touches by x location from left to right. (No need now)
            //oTouches = (new List<Touch>(Input.touches)).OrderBy(order => order.position.x).ToList();
            oTouches = Input.touches.ToList();

            for (int i = 0; i < oTouches.Count; i++)
            {
                Touch touch = oTouches[i];

                Vector3 vTouchPos = touch.position;
                Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));

                GameObject tp;
                // visualize touch point
                if (i >= tps.Count)
                {
                    //initialize new touch point GO
                    tp = Instantiate(tpGO, new Vector3 (worldTouchPos.x, worldTouchPos.y, tpGO.transform.position.z), Quaternion.identity);
                    tps.Add(tp);
                }
                else
                {
                    // use existing touch point GO
                    tp = tps[i];
                    tp.transform.position = worldTouchPos;
                }

                //also viz finger ID
                tp.transform.Find("FingerID").gameObject.GetComponent<TextMesh>().text = String.Format("i{0}", touch.fingerId);
                tp.transform.Find("Count").gameObject.GetComponent<TextMesh>().text = String.Format("{0}", touch.tapCount);
                tp.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                tp.SetActive(true);

                // double tab: detect object
                if ((touch.tapCount == 2) && (touch.phase == TouchPhase.Ended))
                {
                    tp.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
                    // exploration: detect object
                    TouchRayHit(vTouchPos, worldTouchPos);

                    if (RouteManager_Graph.Instance.IsDirectionMode())
                    {
                        TouchRayHitNavigation(vTouchPos, worldTouchPos);
                    }

                    Debug.Log("touch.deltaTime = " + touch.deltaTime);
                    if (touch.deltaTime > 2f)
                        ResetData_AllDetailLevel();
                }
                // triple tap: set up navigation
                else if((touch.tapCount == 3) && (touch.phase == TouchPhase.Ended))
                {
                    tp.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);

                    //if (RouteManager_Graph.Instance.IsDirectionMode())
                    //{
                    //    TouchRayHitNavigation(vTouchPos, worldTouchPos);
                    //}
                }
                    

                // single tap: detect correct node during navigation
                if (touch.tapCount == 1)
                {
                    if ((touch.phase == TouchPhase.Moved) || (touch.phase == TouchPhase.Began))
                    {
                        tp.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
                        // navigation mode
                        if (RouteManager_Graph.Instance.IsDirectionMode())
                        {
                            if (directionState == DirectionState.End)
                            {
                                TouchRayHitFollow(vTouchPos, worldTouchPos);

                                if (!VoiceController.IsSpeaking())
                                    Debug.Log("TTS isn't speaking. Looking for nagivation touch.");
                                else
                                    Debug.Log("TTS started speaking. Isn't looking for nagivation touch.");
                            }
                            else if (directionState == DirectionState.Guide)
                                TouchRayHitFollow_GuidedExploration(vTouchPos, worldTouchPos);

                            else
                                Debug.Log("directionState != DirectionState.End or Guide");
                        }
                    }
                    else if (touch.phase == TouchPhase.Stationary) // in Guide, the touch will be recognized even if it is moved ahead of time
                    {
                        tp.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                        if (RouteManager_Graph.Instance.IsDirectionMode() && (directionState == DirectionState.Guide))
                            TouchRayHitFollow_GuidedExploration(vTouchPos, worldTouchPos);
                    }  
                }

                //save Log
                // headers: {"time", "mode", "modeDetail", "obj", "floor", "tapCount", "tapPhase", "loc_x", "loc_y"};
                string[] saveLine = { GetSystemTime_milliSec(), GetMode(), navMode ,hitObjectName, floor,touch.tapCount.ToString(), touch.phase.ToString(), touch.position.x.ToString(), touch.position.y.ToString()};
                csvExporter.AppendCSV(saveLine);
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

    #region Load data

    string LoadText(string textName)
    {
        TextAsset txt = Resources.Load("Text/" + textName) as TextAsset;
        StringReader reader = new StringReader(txt.text);

        string output = "";
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            output += line;
        }
        return output;
    }

    (List<string>, List<float>) LoadTextStrings(string textName)
    {
        TextAsset txt = Resources.Load("Text/" + textName) as TextAsset;
        StringReader reader = new StringReader(txt.text);


        List<string> outStrings = new List<string>();
        List<float> outTimes = new List<float>();
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            Debug.Log("Line = " + line);
            Debug.Log("Word count = " + line.Count());
            string[] lineSplit = line.Split('|');
            outStrings.Add(lineSplit[1]);
            outTimes.Add(float.Parse(lineSplit[0]));
        }
        return (outStrings, outTimes);
    }

    Dictionary<string, ExhibitionScript> LoadCSV(string floor)
    {
        Dictionary<string, ExhibitionScript> csvData = new Dictionary<string, ExhibitionScript>();
        csvFile = Resources.Load("CSV/" + floor) as TextAsset; /* Resouces/CSV下のCSV読み込み */
        StringReader reader = new StringReader(csvFile.text);
        height = 0;

        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            string[] lineElements = line.Split(',');
            List<string> headers = new List<string>();
            if (height == 0)
            {
                headers = (new List<string>(lineElements)).ToList();
            }
            else
            {
                ExhibitionScript es = new ExhibitionScript()
                {
                    Tag = lineElements[0],
                    Name = lineElements[1],
                    Keyword = lineElements[2],
                    Access = lineElements[3],
                    Summary = lineElements[4]
                };
                csvData.Add(lineElements[0], es);
            }
            height++; // 行数加算
        }

        Debug.Log(floor + "CSV loaded. " + height +" lines.");

        //// debug csv
        //foreach (KeyValuePair<string, ExhibitionScript> kvp in csvData)
        //{
        //    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
        //    Debug.Log(string.Format("Key = {0}, Name = {1}", kvp.Key, kvp.Value.Name));
        //}

        return csvData;
    }

    public void ResetData_AllDetailLevel()
    {
        Debug.Log("reset all detail level");
        foreach (KeyValuePair<string, ExhibitionScript> kvp in csvData)
        {
            kvp.Value.ResetDetailLevel();
        }
    }

    public List<string> GetData_AllKeyword()
    {
        List<string> msg = new List<string>();
        foreach (KeyValuePair<string, ExhibitionScript> kvp in csvData)
        {
            string kw = kvp.Value.Keyword;
            if (!msg.Contains(kw))
                if(kw.Length>0)
                    msg.Add(kw);
        }
        return msg;
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
    #endregion

    #region double tap: exploration
    void TouchRayHit(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

        //// Rayを画面に表示
        //float maxDistance = 500;
        //Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);

        // Raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Debug.Log("Double tap: Ray hit something");

            Transform pMain = vHit.transform;

            if((ButtonsClicked(pMain))||(RouteManager_Graph.Instance.IsDirectionMode()))
                    return;

            string labelname = pMain.name;
            hitObjectName = labelname;

            // viz name with access label
            statusText.text = labelname;
            InitialAcessLabel(worldTouchPos, labelname);

            if (RouteManager_Graph.Instance.IsMainRoute(pMain))
            {
                string msg = "";
                if ((pMain.name =="0") && (floor == "5F"))
                {
                    ExhibitionScript es = csvData["Rocket"];
                    msg += string.Format("{0}。 {1}", es.Name, es.Access);
                }
                    
                msg += GetIntersectionInfo(pMain, floor);
                StartCoroutine(StopStartSpeaking_DelayMethod(msg));
            }
            else
            {
                string msg = GetExhibitInfo(pMain, floor);
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

        string text = DataName(labelName);

        access.GetComponent<Text>().text = text;
        access.transform.SetParent(GameObject.Find("Canvas_UI").transform);
        access.transform.localScale = new Vector3(1f, 1f, 1f) * 1;
        access.SetActive(true);

        Debug.Log("word Length = " + text.Length);
        Destroy(access, WordReadSec(text.Length));
    }

    string DataName(string labelName)
    {
        if (csvData.ContainsKey(labelName))
            return csvData[labelName].Name;
        return labelName;
    }

    string GetGuideExhibitInfo(Transform pMain)
    {
        string labelName = pMain.name;
        ExhibitionScript es = null;
        if (csvGuideData.ContainsKey(labelName))
            es = csvGuideData[labelName];

        string msg = "";
        if (es == null)
            Debug.Log("No Guide ExhibitionScript");
        else
        {
            navMode = "Guide";
            msg = string.Format("{0}。　{1}。　{2}。", es.Name, es.Summary, es.Access);
        }
        return msg;
    }

    string GetExhibitInfo(Transform pMain, string floorName, bool typeOn = true, bool detailInstOn = true)
    {
        string labelName = pMain.name;
        ExhibitionScript es = null;
        if (csvData.ContainsKey(labelName))
            es = csvData[labelName];

        string msg = "";
        if (es == null)
            Debug.Log("No ExhibitionScript");

        if (es != null)
        {
            if ((prevEs != null) && (es.Tag != prevEs.Tag))
                prevEs.ResetDetailLevel();

            if ((es.Tag.Contains("SymbolZone")) || (es.Tag.Contains("VisionariesLab"))) //閉鎖中
            {
                msg = string.Format("{0}。 {1}", es.Name, es.Summary);
            }
            else
            {
                // regular exhibition
                if (es.Summary.Length > 0)
                {
                    if (es.GetDetailLevel() == 0)
                    {
                        navMode = "Level1";

                        if ((Is3F5F(floorName)) && (es.Tag.Contains("Entrance"))) //入口
                        {
                            msg = string.Format("{0} ", es.Name);
                            msg += String.Join("、 ", GetData_AllKeyword());
                            msg += "タイプの展示があります。";
                        }

                        else if (es.Keyword.Length > 0 && typeOn)
                        {
                            msg = string.Format("{0}。 {1}タイプの展示です。{2}", es.Name, es.Keyword, es.Access);
                        }
                        else
                            msg = string.Format("{0}。　{1}", es.Name, es.Access);

                        if(detailInstOn)
                            msg += " 詳細を聞くにはダブルタップしてください。";
                    }
                    else
                    {
                        navMode = "Level2";
                        msg = string.Format("{0} ", es.Summary);
                    }
                    es.IncrementDetailLevel();
                }
                else //No summary
                {
                    if (es.Keyword.Length > 0 && typeOn)
                        msg = string.Format("{0}。　{1}タイプの展示です。{2}", es.Name, es.Keyword, es.Access);
                    else
                        msg = string.Format("{0}。　{1}", es.Name, es.Access);
                }
            }

            prevEs = es;
        }
        else if (labelName == "Hollow") //In code modificatins
            msg = "吹き抜け";
        else
            msg = labelName;
        return msg;

        // e.g. 1F&7F:  Double-tap: {name}. {summary}.
        // e.g. 3F&5F:  Double-tap: {name}. {keyword}タイプの展示です。{accessibility}.詳細を聞くにはダブルタップしてください。
        //              Double-tap again: {summary}.
        // e.g. 3F&5F entrance:  {name}. {summary}. {keyword1}{keyword2}{keyword3}タイプの展示があります。
    }

    string GetIntersectionInfo(Transform pMain, string floorName)
    {
        string msg = "この交差点では、";
        Transform[] pChildren = pMain.GetComponentsInChildren<Transform>();
        Transform[] opChildren = (new List<Transform>(pChildren)).OrderBy(o => o.position.y).ThenBy(o => o.position.x).ToArray();


        for (int i = 0; i < opChildren.Length; i++)
        {
            ExhibitionScript es = null;
            Transform c = opChildren[i];
            if (csvData.ContainsKey(c.name))
            {
                if (i > 1)
                    msg += " と ";

                msg += RouteManager_Graph.Instance.GetDirection(pMain, c) + "に";

                es = csvData[c.name];
                if (Is1F7F(floorName))
                    msg += es.Name;
                else if (Is3F5F(floorName))
                {
                    if (es.Keyword.Length > 0)
                        msg += es.Keyword;
                    else if (es.Tag.Contains("Entrance"))
                        msg += "入り口";
                    else if (es.Tag.Contains("OvalBridge"))
                        msg += "オーバル　ブリッジ";
                    else
                        msg += es.Name;
                }
                    
            }
        }

        if (Is1F7F(floorName))
            msg += "に通じます。";

        if (Is3F5F(floorName))
        {
            msg += "タイプの展示があります。";
            int[] aroundCount = RouteManager_Graph.Instance.RouteFindAroundCount(pMain);
            msg += string.Format("左側には{0}つの展示があります。右側には{1}つの展示があります。", aroundCount[1], aroundCount[2]);
        } 
        return msg;

        // e.g. 1F&7F: この交差点では、レストラン Miraikan Kitchen（下）と中庭（上）を通じます。
        // e.g. 3F&5F: この交差点では、宇宙（下）と地球（上）タイプの展示を通じます。左側はNつの展示があります。右側はNつの展示があります。
        // e.g. 3F&5F: この交差点では、下に宇宙　と　上に地球　タイプの展示を通じます。左側はNつの展示があります。右側はNつの展示があります。
    }

    bool Is1F7F(string floorName)
    {
        return ((floorName == "7F") || (floorName == "1F"));
    }

    bool Is3F5F(string floorName)
    {
        return ((floorName == "3F") || (floorName == "5F") || (floorName == "0F"));
    }

    #endregion

    #region triple tap: define navigation 
    void TripleTouchStateChange(Transform touched)
    {
        switch (directionState)
        {
            case DirectionState.Pre:
            case DirectionState.End: 
                //audioSource.Stop();
                pEnd = touched;
                //string msg = pEnd.name + ". Destination marked. Please mark start point";
                string msg = csvData[pEnd.name].Name  + "。目的地が選ばれました。　出発地を２回タップしてください。";
                directionState = DirectionState.Start;
                StartCoroutine(StopStartSpeaking_DelayMethod(msg));
                break;

            case DirectionState.Start:
                //audioSource.Stop();
                pStart = touched;
                //string msg2 = pStart.name + ". Start point marked. " + RouteManager.Instance.RouteFindSpecific(pStart, pEnd); Move your finger to start's entrance to navigate.
                string msg2 = csvData[pStart.name].Name + "。　出発地が選ばれました。" + RouteManager_Graph.Instance.RouteFindSpecific(pStart, pEnd);
                if(pStart.name == pEnd.name)
                    directionState = DirectionState.Pre;
                else
                    directionState = DirectionState.End;
                navigationOrder = 0;
                StartCoroutine(StopStartSpeaking_DelayMethod(msg2));
                break;
        }
    }

    void TouchRayHitNavigation(Vector3 vTouchPos, Vector3 worldTouchPos)
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
            Debug.Log("Navigation Ray hit something");
            Transform touched = vHit.transform;
            hitObjectName = touched.name;

            if (RouteManager_Graph.Instance.IsSubRoute(touched))
                TripleTouchStateChange(touched);
        }
    }
    #endregion

    #region single tap: follow a guided exploration
    void TouchRayHitFollow_GuidedExploration(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        List<Transform> routeList = RouteManager_Graph.Instance.GetGuide_NavRoute();
        List<string> routeDirectList = RouteManager_Graph.Instance.GetGuide_NavDirect();
        List<string[]> routeDirectTurnList = RouteManager_Graph.Instance.GetGuide_NavDirectTurn();
        List<string> routeDistance = RouteManager_Graph.Instance.GetGuide_NavDistance();

        if ((routeDirectList == null) || (routeDirectList.Count == 0))
            return;
        if (VoiceController.IsSpeaking())
            return;

        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Transform touched = vHit.transform;
            Debug.Log("Follow Ray hit: " + touched.name + ". NavigationOrder: " + navigationOrder);

            if (touched.name != routeList[navigationOrder].name)
            {
                // ErrorSound
            }
            else
            {
                hitObjectName = touched.name;
                Debug.Log("Touched correct point. Name: " + routeList[navigationOrder].name);

                if (navigationOrder < routeDirectList.Count)
                {
                    Transform tNow = routeList[navigationOrder], tNext = routeList[navigationOrder + 1];
                    string msg = "";
                    if (navigationOrder >= 0)
                    {
                        //msg = String.Format("You reached {0}'s entrance. Next, move {1} to {2}.",
                        //    RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext));

                        msg = GetGuideExhibitInfo(tNow);
                        if (navigationOrder > 2 && tNow.name == "Entrance_5F") // entrance again
                            msg = String.Format("{0}に到着しました。最後は、", csvGuideData[tNow.name].Name);

                        if (navigationOrder > 0)// skip the first one. Encoded in script.
                        {
                            if (turnMode) // incorrect 
                            {
                                string next = "次の";

                                string[] turnbyturnText = routeDirectTurnList[navigationOrder];
                                string turnText, straightText;
                                if (turnbyturnText.Length > 1)
                                {
                                    turnText = turnbyturnText[0];
                                    straightText = turnbyturnText[1];
                                }
                                else
                                {
                                    turnText = "";
                                    straightText = turnbyturnText[0];
                                }

                                string exhName = RouteManager_Graph.Instance.RouteString_Guide(tNext);
                                if (exhName.Contains("ntrance"))
                                    exhName = csvData[tNext.name].Name;
                                if (exhName.Contains("0"))
                                    exhName = csvData["Rocket"].Name.Split('。')[0];

                                msg += String.Format("{0}、{3}{1}に{2}してください",
                                turnText,
                                exhName,
                                straightText,
                                next);
                            }
                            else //north up mode
                            {
                                string direction = routeDirectList[navigationOrder];
                                string distance = routeDistance[navigationOrder];
                                string direct_dist = "";
                                if (distance.Contains("|"))
                                {
                                    string[] distances = distance.Split('|');
                                    string[] directions = direction.Split(' ');
                                    for (int i = 0; i < distances.Length; i++)
                                    {
                                        direct_dist += String.Format("「{0}」　 {1}", directions[i], distances[i]);
                                        if (i != distances.Length - 1)
                                            direct_dist += " センチ　";
                                    }
                                }
                                else
                                    direct_dist = String.Format("{0} {1}", direction, distance);
                                msg += String.Format("「{0}先」の{1}に動かしてください",
                                    direct_dist,
                                    RouteManager_Graph.Instance.RouteString_Guide(tNext));
                            }
                                
                        }                      
                    }
                    StartCoroutine(StopStartSpeaking_DelayMethod_NoDelay4DisableTouch(msg));
                    navigationOrder++;
                    Debug.Log("Speaking msg: " + msg);

                    tNow.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    tNext.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                }
                else if (navigationOrder == routeDirectList.Count)
                {
                    Transform tNow = routeList[navigationOrder];
                    string msg = "";
                    msg = GetGuideExhibitInfo(tNow);
                    if (tNow.name == "Entrance_5F") // entrance again
                        msg = String.Format("{0}に到着しました。", csvGuideData[tNow.name].Name);

                    //string msg = String.Format("You reached the destination: {0}. ", RouteManager.Instance.RouteString(tNow));

                    tNow.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

                    msg += inst_Guide2;
                    StartCoroutine(StopStartSpeaking_DelayMethod_NoDelay4DisableTouch(msg));
                    navigationOrder = 0;
                    Debug.Log(msg);

                    // Change to mode exploration mode
                    OnClick_Nav();
                }
            }
        }
    }
    #endregion

    #region single tap: follow navigation
    void TouchRayHitFollow(Vector3 vTouchPos, Vector3 worldTouchPos)
    {
        List<Transform> routeList = RouteManager_Graph.Instance.GetNavRoute();
        List<string> routeDirectList = RouteManager_Graph.Instance.GetNavDirect();
        List<string[]> routeDirectTurnList = RouteManager_Graph.Instance.GetNavDirectTurn();

        if ((routeDirectList == null)||(routeDirectList.Count == 0))
        {
            return;
        }

        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);
        // Raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Transform touched = vHit.transform;

            Debug.Log("Follow Ray hit: " + touched.name);
            Debug.Log("navigationOrder: " + navigationOrder);

            if (touched == routeList[navigationOrder])
            {
                hitObjectName = touched.name;

                Debug.Log("Touched correct point. Name: " + routeList[navigationOrder].name);

                if (navigationOrder < routeDirectList.Count)
                {
                    Transform tNow = routeList[navigationOrder], tNext = routeList[navigationOrder + 1];
                    string msg = "";
                    if (navigationOrder == 0)
                    {
                        //msg = String.Format("You reached {0}'s entrance. Next, move {1} to {2}.",
                        //    RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext));

                        msg = String.Format("{0}の入口に到着しました。　{1}の{2}に動かしてください",
                            csvData[RouteManager_Graph.Instance.RouteString(tNow)].Name,
                            routeDirectList[navigationOrder],
                            RouteManager_Graph.Instance.RouteString(tNext));

                        // viz selected routes in yellow, not selected in green
                        SetNodesColor(routeList, Color.yellow, Color.green);
                    }
                    else
                    {
                        if(navigationOrder < routeDirectList.Count - 1) // 交差点
                        {
                            //msg = String.Format("You reached {0}. Next, move {1} to {2} {3}.",
                            //RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext), navigationOrder + 1);

                            //msg = String.Format("{0}に到着しました。　{1}の{2}{3}に動かしてください",
                            //RouteManager_Graph.Instance.RouteString(tNow),
                            //routeDirectList[navigationOrder],
                            //RouteManager_Graph.Instance.RouteString(tNext),
                            //navigationOrder + 1);
                            string next = "";
                            if (navigationOrder > 1)
                                next = "次の";

                            if (turnMode)
                            {
                                string[] turnbyturnText = routeDirectTurnList[navigationOrder];
                                string turnText, straightText;
                                if (turnbyturnText.Length > 1)
                                {
                                    turnText = turnbyturnText[0];
                                    straightText = turnbyturnText[1];
                                }
                                else
                                {
                                    turnText = "";
                                    straightText = turnbyturnText[0];
                                }

                                msg = String.Format("{0}に到着しました。　{1}、{4}{2}に{3}してください",
                                RouteManager_Graph.Instance.RouteString(tNow),
                                turnText,
                                RouteManager_Graph.Instance.RouteString(tNext),
                                straightText,
                                next);
                            }
                            else
                                msg = String.Format("{0}に到着しました。　{1}の{2}に動かしてください",
                                RouteManager_Graph.Instance.RouteString(tNow),
                                routeDirectList[navigationOrder],
                                RouteManager_Graph.Instance.RouteString(tNext));
                        }
                        else
                        {
                            //msg = String.Format("You reached {0}. Next, move {1} to {2}.",
                            //RouteManager.Instance.RouteString(tNow), routeDirectList[navigationOrder], RouteManager.Instance.RouteString(tNext));

                            if (turnMode)
                            {
                                string[] turnbyturnText = routeDirectTurnList[navigationOrder];
                                string turnText, straightText;
                                if (turnbyturnText.Length > 1)
                                {
                                    turnText = turnbyturnText[0];
                                    straightText = turnbyturnText[1];
                                }
                                else
                                {
                                    turnText = "";
                                    straightText = turnbyturnText[0];
                                }

                                msg = String.Format("{0}に到着しました。　{1}、{2}に{3}してください",
                                RouteManager_Graph.Instance.RouteString(tNow),
                                turnText,
                                csvData[RouteManager_Graph.Instance.RouteString(tNext)].Name,
                                straightText);
                            }
                            else
                                msg = String.Format("{0}に到着しました。　{1}の{2}に動かしてください",
                                RouteManager_Graph.Instance.RouteString(tNow),
                                routeDirectList[navigationOrder],
                                csvData[RouteManager_Graph.Instance.RouteString(tNext)].Name);
                        }
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
                    //string msg = String.Format("You reached the destination: {0}. ", RouteManager.Instance.RouteString(tNow));
                    string msg = String.Format("目的地　{0}に到着しました。",
                        csvData[RouteManager_Graph.Instance.RouteString(tNow)].Name);

                    directionState = DirectionState.Pre;

                    tNow.GetComponent<Renderer>().material.SetColor("_Color", Color.green);


                    //VoiceController.StartSpeaking(msg);

                    StartCoroutine(StopStartSpeaking_DelayMethod(msg));

                    //SetNodesColor(routeList, Color.green);

                    RouteManager_Graph.Instance.ResetNav();
                    navigationOrder = 0;
                    Debug.Log(msg);
                }
            }
        }
    }

    void SetNodesColor(List<Transform> routeList, Color color1, Color color2)
    {

        foreach (GameObject go in RouteManager_Graph.Instance.GetMainRoute())
        {
            if (routeList.Contains(go.transform))
                go.GetComponent<Renderer>().material.SetColor("_Color", color1);
            else
                go.GetComponent<Renderer>().material.SetColor("_Color", color2);

        }
    }
    #endregion

    private IEnumerator StopStartSpeaking_DelayMethod(string msg)
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

    private IEnumerator StopStartSpeaking_DelayMethod_NoDelay4DisableTouch(string msg)
    {
        //float waitTime = 0.25f;
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
            //yield return new WaitForSeconds(waitTime);
            //Debug.Log("Stop->Start: Start Coroutine, Wait for: " + waitTime);

        }
        else
        {
            Debug.Log("Stop->Start: Still speaking, cannot start");
        }
    }

    private IEnumerator StopStartSpeaking_DelayMethod_Auto(List<string> msgs, List<float> delayTimes)
    {
        float speedOneWord = 0.18f;
        if (VoiceController.IsSpeaking())
            VoiceController.StopSpeaking();
        else
            Debug.Log("Stop->Start: Not speaking, cannot stop");

        //int speakCount = 0;
        //while (speakCount < msgs.Count())
        for (int speakCount = 0; speakCount < msgs.Count; speakCount++)
        {
            if (!VoiceController.IsSpeaking())
            {
                Debug.Log("Start when not speaking");
                yield return StartCoroutine(Speak_Pause(msgs[speakCount], delayTimes[speakCount] + msgs[speakCount].Count()* speedOneWord));
                Debug.Log("Start Coroutine, Wait for: " + (delayTimes[speakCount]));
                //speakCount += 1;

            }
            else
            {
                Debug.Log("Stop->Start: Still speaking, cannot start");
                Debug.Log("speakCount = " + speakCount);
            }
        }
        touchEnabled = true;
    }

    public void OnClick_SpeakNext()
    {
        StopStartSpeaking_DelayMethod_Button_Oneline(inst1,inst1_times);
        if (speakOrder < inst1.Count() - 1)
            speakOrder += 1;
        else
            speakOrder = 0;

        SaveOneClickButton("OnClick_SpeakNext");
    }

    public void OnClick_SpeakLast()
    {
        if (speakOrder >= 1)
            speakOrder -= 1;
        else
            speakOrder = 0;
        StopStartSpeaking_DelayMethod_Button_Oneline(inst1, inst1_times);

        SaveOneClickButton("OnClick_SpeakLast");
    }

    private void StopStartSpeaking_DelayMethod_Button_Oneline(List<string> msgs, List<float> delayTimes)
    {
        if (VoiceController.IsSpeaking())
            VoiceController.StopSpeaking();

        Debug.Log("Start when not speaking");
        StartCoroutine(Speak_Pause(msgs[speakOrder], 0));
        Debug.Log("Start Coroutine, Wait for: " + (delayTimes[speakOrder]));

    }

    private IEnumerator Speak_Pause(string msg, float time)
    {
        yield return new WaitForSeconds(0.5f);
        VoiceController.StartSpeaking(msg);
        yield return new WaitForSeconds(time);
    }

    private IEnumerator StopStartSpeaking_DelayMethod_Button(string msg)
    {
        if (VoiceController.IsSpeaking())
            VoiceController.StopSpeaking();
        else
            Debug.Log("Stop->Start: Not speaking, cannot stop");

        float waitTimeStop = 0.5f;
        yield return new WaitForSeconds(waitTimeStop);
        Debug.Log("Stop->Start:  Stop Coroutine, Wait for: " + waitTimeStop);

        float waitTime = 0.25f;
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

        waitTime = 1f;
        yield return new WaitForSeconds(waitTime);
    }

    private void StopSpeaking()
    {
        if (VoiceController.IsSpeaking())
        {
            VoiceController.StopSpeaking();
        }
        else
        {
            Debug.Log("Stop->Start: Not speaking, cannot stop");
        }
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
}

public class ExhibitionScript
{
    public string Tag { get; set; }
    public string Name { get; set; }
    public string Keyword { get; set; }
    public string Access { get; set; }
    public string Summary { get; set; }
    public int maxDetailLevel = 2;
    public int currentDetailLevel = 0;
    public int GetDetailLevel()
    {
        return currentDetailLevel;
    }
    public void IncrementDetailLevel()
    {
        currentDetailLevel++;
        if (currentDetailLevel >= maxDetailLevel)
            currentDetailLevel = 0;
    }

    public void ResetDetailLevel()
    {
        Debug.Log(Tag + " : reset detal level");
        currentDetailLevel = 0;
    }
}