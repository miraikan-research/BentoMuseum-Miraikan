using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class TouchLocManager : MonoBehaviour
{
    public Text statusText;
    public Text hitText;
    GameObject canvasGO;
    GameObject textGO;
    Font arial;

    // 5F's calibration offset is based on the Geo Cosmos
    Vector3 initPosition;
    public float calibOffX = -30f, calibOffY = -77f;
    static int screenW = 2224, screenH = 1668;
    static int camPointDist = 125;
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
        initPosition = GameObject.Find("5F").transform.position;

        // Find location to put accessible texts.
        canvasGO = GameObject.Find("AccessTextParent");
        textGO = new GameObject();
        InitializeReadText("", initPosition);

        GameObject.Find("3DAcessTextParent").gameObject.SetActive(false);

        //PrintVector(canvasGO.transform.position, "AccessTextCanvas");
        //PrintVector(initPosition, "initPosition");

        voiceOff.PlayDelayed(1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        // Hide all touch visualization points.
        for (int i = 0; i < 5; i++)
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

            // Initialize the accessble texts
            if (Input.touchCount > 2 && calib)
                foreach (Touch touch in oTouches.Skip(2).Take(Input.touchCount - 2))
                    TouchRayCast(touch, oTouches);

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
            PlayCalibSound();
        }
        prevCalib = calib;
    }

    void PlayCalibSound()
    {
        if (prevCalib != calib)
        {
            if (calib) voiceOn.Play();
            //else voiceOff.Play();
        }
        prevCalib = calib;
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

    void ChangeReadText(string t, Vector3 worldTouchPos)
    {
        // Set Text component properties.
        Text text = textGO.GetComponent<Text>();
        text.text = string.Format("{0}", t);

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
            Vector3 calibPos = Screen2WorldPos(screenPos, initPosition.z);

            calibPos.x += calibOffX;
            calibPos.y += calibOffY;
            //PrintVector(calibPos, "calib pos");

            // Transform model and accessible texts.
            GameObject.Find("5F").transform.position = calibPos;
            canvasGO.transform.position = calibPos;

            calib = true;
            //statusText.text = "Model is calibrated";
            statusText.text = "";

            GameObject calibPoint = GameObject.Find("TouchPoints").transform.Find("CalibPoint").gameObject;
            calibPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camPointDist));
            calibPoint.SetActive(true);
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

    static bool isCalibPos(Vector3 screenPos)
    {
        if (screenPos.x < screenW / 2)
            return true;
        else
            return false;
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

    static Vector3 Screen2WorldPos(Vector3 screenPos, float zPos)
    {
        Vector3 worldPos = screenPos / 10;
        worldPos.z = zPos;
        return worldPos;
    }

    void PrintVector(Vector3 vec, string name = "String name")
    {
        object[] args = { vec.x, vec.y, vec.z, name };
        Debug.Log(string.Format("{3}: x={0}, y={1}, z={2}", args));
    }
}
