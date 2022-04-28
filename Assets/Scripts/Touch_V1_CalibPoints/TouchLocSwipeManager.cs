using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class TouchLocSwipeManager : MonoBehaviour
{
    public Text statusText;
    public Text hitText;
    GameObject canvasGO;
    Vector3 initCanvasPos;
    GameObject textGO;
    Font arial;
    Dictionary<string, string> dct = AccessTextDict.dct;

    // 5F's calibration offset is based on the Geo Cosmos
    GameObject floorGO;
    Vector3 initFloorPosition;

    public float calibOffX = -93f, calibOffY = 7f;
    static int screenW = 2224, screenH = 1668;
    static int UI_z = -80;
    static int camPointDist = 125;
    static int camFloorDist = 180;
    bool calib = false;

    //string calibrationAlert = "Please touch the left two points";
    string calibrationAlert = "";

    // Sounds when enter and exit calibration states.
    public AudioSource voiceOn;
    public AudioSource voiceOff;
    bool prevCalib = false;

    List<Touch> oTouches;

    // Start is called before the first frame update
    void Start()
    {
        // Load the Arial font from the Unity Resources folder.
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        //Calibration();
        // Find floor map location.
        floorGO = GameObject.Find("5F");
        initFloorPosition = floorGO.transform.position;

        // Find location to put accessible texts.
        canvasGO = GameObject.Find("AccessTextParent");
        textGO = new GameObject();
        InitializeReadText("", initFloorPosition);

        GameObject.Find("Canvas_UI").transform.Find("3DAcessTextParent").gameObject.SetActive(false);

        voiceOff.PlayDelayed(1.5f);

        //PrintVector(canvasGO.transform.position, "AccessTextCanvas");
        //PrintVector(initPosition, "initPosition");
    }

    // Update is called once per frame
    void Update()
    {
        // Hide all touch visualization points.
        for (int i = 0; i < 11; i++)
        {
            GameObject touchVizObj = GameObject.Find("TouchPoints").transform.Find(string.Format("TouchPoint_{0}", i)).gameObject;
            touchVizObj.SetActive(false);
        }
        GameObject.Find("TouchPoints").transform.Find("CalibPoint").gameObject.SetActive(false);


        CheckCalib();

        if (Input.touchCount > 1)
        {
            // order touches by x location from left to right.
            oTouches = (new List<Touch>(Input.touches)).OrderBy(order => order.position.x).ToList();

            // Calibrate using the first two touches.
            Calibration();

            // Emboss and 3D: Initialize the accessble Texts
            if (Input.touchCount > 2 && calib)
            {
                foreach (Touch touch in oTouches.Skip(2).Take(Input.touchCount - 2))
                    TouchRayCast(touch, oTouches);
            }

            // Visualize touches.
            foreach (Touch touch in oTouches)
            {
                Vector3 vTouchPos = touch.position;
                Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));

                // visualize the touch with red dot (even if it doesn't hit anything)
                GameObject touchVizObj = GameObject.Find("TouchPoints").transform.Find(string.Format("TouchPoint_{0}", touch.fingerId)).gameObject;
                touchVizObj.transform.position = worldTouchPos;
                touchVizObj.SetActive(true);

                //PrintVector(vTouchPos, "TouchScreenPos "+ touch.fingerId+ ": ");
                //PrintVector(worldTouchPos, "TouchWorldPos " + touch.fingerId + ": ");
            }
        }

        CheckOnOff();
    }

    void CheckOnOff()
    {
        if (prevCalib != calib)
        {
            PlayCalibSound();        }
        prevCalib = calib;
    }

    void PlayCalibSound()
    {
        if (calib) voiceOn.Play();
        //else voiceOff.Play();
    }

    void TouchRayCast(Touch touch)
    {
        Vector3 vTouchPos = touch.position;
        Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));

        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

        ////Rayを画面に表示
        //float maxDistance = 500;
        //Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);

        // Your raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Debug.Log("Ray hit something");

            hitText.text = vHit.transform.name;
            ChangeReadText(vHit.transform.name, worldTouchPos);
        }
        // If nothing is hit, clear all accessble texts
        else
        {
            hitText.text = "Nothing";
            GameObject[] readTexts;

            readTexts = GameObject.FindGameObjectsWithTag("ReadText");

            for (int i = 0; i < readTexts.Length; i++)
            {
                //if (i != readTexts.Length - 1)
                Destroy(readTexts[i]);
            }
        }
    }

    void TouchRayCast(Touch touch, List<Touch> oTouches)
    {
        Vector3 vTouchPos = touch.position;
        Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, camPointDist));
        Vector3 worldAccessTextPos = Camera.main.ScreenToWorldPoint(new Vector3(oTouches[0].position.x, oTouches[0].position.y, camPointDist));

        // The ray to the touched object in the world
        Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

        ////Rayを画面に表示
        //float maxDistance = 500;
        //Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);

        // Your raycast handling
        RaycastHit vHit;

        if (Physics.Raycast(ray.origin, ray.direction, out vHit))
        {
            Debug.Log("Ray hit something");

            hitText.text = vHit.transform.name;
            ChangeReadText(vHit.transform.name, worldAccessTextPos);
        }
        // If nothing is hit, clear all accessble texts
        else
        {
            //DestoryReadTexts();
            ClearReadText();
        }
    }

    void InitializeReadText(string t, Vector3 worldTouchPos)
    {
        //textGO = new GameObject(); //only have one textGO, just change text
        textGO.name = "textGO";
        textGO.tag = "ReadText";
        textGO.AddComponent<CanvasRenderer>();
        textGO.AddComponent<RectTransform>();
        textGO.AddComponent<Text>();
        textGO.AddComponent<AccessibleLabel>();

        //textGO.transform.position = new Vector3(0, 0, 0);
        textGO.transform.SetParent(canvasGO.transform);

        // Set Text component properties.
        Text text = textGO.GetComponent<Text>();
        text.font = arial;
        text.text = string.Format("{0}", t);
        text.fontSize = 54;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // Provide Text position and size using RectTransform.
        RectTransform rectTransform;
        rectTransform = text.GetComponent<RectTransform>();
        rectTransform.position = new Vector3(worldTouchPos.x, worldTouchPos.y, worldTouchPos.z);
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    void ChangeReadText(string goName, Vector3 worldTouchPos)
    {
        // Set Text component properties.
        Text text = textGO.GetComponent<Text>();
        if (dct.ContainsKey(goName))
            text.text = dct[goName];
        else text.text = string.Format("{0}", goName);

        // Provide Text position and size using RectTransform.
        RectTransform rectTransform;
        rectTransform = text.GetComponent<RectTransform>();
        rectTransform.position = new Vector3(worldTouchPos.x, worldTouchPos.y, worldTouchPos.z);
        CheckAddAccessLabel(textGO);
        CheckDeleteAccessLabel(statusText.gameObject);
    }

    void ClearReadText()
    {
        textGO.GetComponent<Text>().text = "";
    }

    void DestoryReadTexts()
    {
        hitText.text = "Nothing";
        GameObject[] readTexts;

        readTexts = GameObject.FindGameObjectsWithTag("ReadText");

        for (int i = 0; i < readTexts.Length; i++)
        {
            //if (i != readTexts.Length - 1)
            Destroy(readTexts[i]);
        }
    }

    void CheckCalib()
    {
        if (Input.touchCount <= 1)
        {
            calib = false;
            statusText.text = calibrationAlert;
            CheckAddAccessLabel(statusText.gameObject);
            CheckDeleteAccessLabel(textGO);
            ClearReadText();
        }
    }

    void Calibration()
    {
        if (!calib)
            statusText.text = calibrationAlert;

        calib = false;


        //Calibrate the position
        // TODO: rotation.
        if (isCalibPos(oTouches[0], oTouches[1]))
        {
            Vector3 t0 = oTouches[0].position, t1 = oTouches[1].position;
            Vector3 screenPos = new Vector3((t0.x + t1.x) / 2, (t0.y + t1.y) / 2, 0);
            Vector3 calibPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camFloorDist));

            // Debug and calculate offset.
            PrintVector(Camera.main.ScreenToWorldPoint(new Vector3(t0.x, t0.y, camFloorDist)), "t0");
            PrintVector(Camera.main.ScreenToWorldPoint(new Vector3(t1.x, t1.y, camFloorDist)), "t1");
            PrintVector(calibPos, "calib pos");


            // Transform floor model.
            calibPos.x -= calibOffX;
            calibPos.y += calibOffY;
            floorGO.transform.position = calibPos;

            // Transform accessible texts.
            canvasGO.transform.position = new Vector3(calibPos.x + initCanvasPos.x,
                calibPos.y + initCanvasPos.y, UI_z);

            // Transform the calibration point.
            GameObject calibPoint = GameObject.Find("TouchPoints").transform.Find("CalibPoint").gameObject;
            calibPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camPointDist));
            calibPoint.SetActive(true);

            calib = true;
            //statusText.text = "Model is calibrated";
            statusText.text = "";
        }
    }

    void CheckAddAccessLabel(GameObject go)
    {
        if (go.GetComponent<AccessibleLabel>() == null)
        {
            Debug.Log(go.name + ": Going to Add Access Label");
            go.AddComponent<AccessibleLabel>();
        }
    }

    void CheckDeleteAccessLabel(GameObject go)
    {
        if (go.GetComponent<AccessibleLabel>() != null)
        {
            Debug.Log(go.name + ": Going to Delete Access Label");
            Destroy(go.GetComponent<AccessibleLabel>(), 1f);
        }
    }

    static bool isCalibPos(Touch t0, Touch t1)
    {
        bool cPos = false;
        if (t1.position.x - t0.position.x < 50)
            if (t1.position.x < screenW / 2)
            {
                float d = Vector2.Distance(t0.position, t1.position);
                if ((d < 950 + 50) && (d > 950 - 50))
                    cPos = true;
            }
        return cPos;
    }

    void PrintVector(Vector3 vec, string name = "String name")
    {
        object[] args = { vec.x, vec.y, vec.z, name };
        Debug.Log(string.Format("{3}: x={0}, y={1}, z={2}", args));
    }
}
