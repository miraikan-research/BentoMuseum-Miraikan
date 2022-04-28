using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickManager : MonoBehaviour
{
    public Text statusText;
    GameObject canvasGO;
    GameObject textGO;
    Font arial;

    // 5F's calibration offset is based on the Geo Cosmos
    Vector3 initPosition;
    int calibOffX = 70, calibOffY = 60;
    static int screenW = 2224, screenH = 1668;
    bool calib = false;
    int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Load the Arial font from the Unity Resources folder.
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        canvasGO = GameObject.Find("Canvas_UI");
        textGO = new GameObject();
        Debug.Log("canvasGO");
        PrintVector(canvasGO.transform.position);

        Calibration();
        initPosition = GameObject.Find("5F").transform.position;
        Debug.Log("initPosition");
        PrintVector(initPosition);

        
    }

    // Update is called once per frame
    void Update()
    {
        Calibration();

        //if moved: Calibration();

        //if (Input.touchCount > 0 || true)
        //{
        //    Debug.Log("Touch");
        //}

        if (calib)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 vTouchPos = Input.mousePosition;
                PrintVector(vTouchPos);
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

                    statusText.text = vHit.transform.name;
                    InitializeReadText(vHit.transform.name, worldTouchPos);
                    //statusText.text = "Ray hit something";
                    //if (vHit.transform.tag == "Respawn")
                    //{
                    //    Destroy(vHit.collider.gameObject);
                    //}
                }
                else
                {
                    GameObject[] readTexts;

                    readTexts = GameObject.FindGameObjectsWithTag("ReadText");

                    for (int i = 0; i < readTexts.Length; i++)
                    {
                        if(i != readTexts.Length-1)
                            Destroy(readTexts[i]);
                    }
                }

                // visualize the touch with red dot (even if it doesn't hit anything)
                GameObject touchVizObj = GameObject.Find("TouchPoints").transform.Find(string.Format("TouchPoint_{0}", 0)).gameObject;
                touchVizObj.transform.position = worldTouchPos;
                touchVizObj.SetActive(true);
            }
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

        //if (calib)
        //    statusText.text = "Model is calibrated";

        if (Input.GetKeyDown(KeyCode.Mouse0) && (!calib))
        {
            Vector3 screenPos = Input.mousePosition;

            if(isCalibPos(screenPos))
            {
                Vector3 calibPos = Screen2WorldPos(screenPos, initPosition.z);

                calibPos.x += calibOffX;
                calibPos.y += calibOffY;
                PrintVector(calibPos);
                GameObject.Find("5F").transform.position = calibPos;
                calib = true;
                statusText.text = "Model is calibrated";
            }
        }
    }

    static Vector3 Screen2WorldPos(Vector3 screenPos, float zPos)
    {
        Vector3 worldPos = screenPos / 10;
        worldPos.z = zPos;
        return worldPos;
    }

    static bool isCalibPos(Vector3 screenPos)
    {
        if (screenPos.x < screenW / 2)
            return true;
        else
            return false;
    }

    void PrintVector(Vector3 vec)
    {
        object[] args = { vec.x, vec.y, vec.z};
        Debug.Log(string.Format("x={0}, y={1}, z={2}", args));
    }


}
