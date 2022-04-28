using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TouchManager : MonoBehaviour
{
    public Text statusText;
    public Text hitText;
    GameObject canvasGO;
    GameObject textGO;
    Font arial;

    // 5F's calibration offset is based on the Geo Cosmos
    Vector3 initPosition;
    int calibOffX = 65, calibOffY = 55;
    static int screenW = 2224, screenH = 1668;
    bool calib = false;
    int i = 0;

    List<Touch> oTouches;

    // Start is called before the first frame update
    void Start()
    {
        // Load the Arial font from the Unity Resources folder.
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        // find canvas to generate UI.
        canvasGO = GameObject.Find("Canvas_UI");
        textGO = new GameObject();

        //Calibration();
        initPosition = GameObject.Find("5F").transform.position;
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


        if (Input.touchCount > 1)
        {
            oTouches = (new List<Touch>(Input.touches)).OrderBy(order=>order.position.x).ToList();

            // Calibrate using the first two touches.
            // The current method requires constantly holding two touch points.
            Calibration();

            if (Input.touchCount > 2 && calib)
            {
                foreach (Touch touch in oTouches.Skip(2).Take(Input.touchCount - 2))
                {
                    Vector3 vTouchPos = touch.position;
                    Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, 125));

                    // The ray to the touched object in the world
                    Ray ray = Camera.main.ScreenPointToRay(vTouchPos);

                    //Rayを画面に表示
                    float maxDistance = 500;
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green, 5, false);

                    // Your raycast handling
                    RaycastHit vHit;

                    if (Physics.Raycast(ray.origin, ray.direction, out vHit))
                    {
                        Debug.Log("Ray hit something");

                        hitText.text = vHit.transform.name;
                        InitializeReadText(vHit.transform.name, worldTouchPos);
                        //statusText.text = "Ray hit something";
                        //if (vHit.transform.tag == "Respawn")
                        //{
                        //    Destroy(vHit.collider.gameObject);
                        //}
                    }
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
            }

            // Visualize touch with red dot
            foreach (Touch touch in oTouches)
            {
                PrintVector(touch.position, "FingerTouch");
                Vector3 vTouchPos = touch.position;
                Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, 100));

                // visualize the touch with red dot (even if it doesn't hit anything)
                GameObject touchVizObj = GameObject.Find("TouchPoints").transform.Find(string.Format("TouchPoint_{0}", touch.fingerId)).gameObject;
                touchVizObj.transform.position = worldTouchPos;
                touchVizObj.SetActive(true);
            }

            // Process the rest touches: only double tap.

        }
    }

    void InitializeReadText(string t, Vector3 worldTouchPos)
    {
        textGO = new GameObject();
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

    void Calibration()
    {
        if (!calib)
            statusText.text = "Please calibrate the model";
        calib = false;

        //if (calib)
        //    statusText.text = "Model is calibrated";

        //Calibrate the position
        // TODO: rotation.
        if (isCalibPos(oTouches[0], oTouches[1]))
        {
            Vector3 t0 = oTouches[0].position, t1 = oTouches[1].position;
            Vector3 screenPos = new Vector3((t0.x+t1.x)/2, (t0.y + t1.y) / 2, 0);
            Vector3 calibPos = Screen2WorldPos(screenPos, initPosition.z);

            calibPos.x += calibOffX;
            calibPos.y += calibOffY;
            PrintVector(calibPos, "calib pos");

            GameObject.Find("5F").transform.position = calibPos;
            calib = true;
            statusText.text = "Model is calibrated";

            GameObject calibPoint = GameObject.Find("TouchPoints").transform.Find("CalibPoint").gameObject;
            calibPoint.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 100));
            calibPoint.SetActive(true);
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
        object[] args = { vec.x, vec.y, vec.z, name};
        Debug.Log(string.Format("{3}: x={0}, y={1}, z={2}", args));
    }


}
