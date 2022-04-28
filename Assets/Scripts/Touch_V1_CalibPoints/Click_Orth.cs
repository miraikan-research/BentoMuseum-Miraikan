using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Click_Orth : MonoBehaviour
{

    public Text statusText;
    GameObject canvasGO;
    GameObject textGO;
    Font arial;

    // 5F's calibration offset is based on the Geo Cosmos
    Vector3 initPosition;
    float calibOffX = -93f, calibOffY = 0f;
    static int screenW = 2224, screenH = 1668;
    bool calib = false;

    // Start is called before the first frame update
    void Start()
    {
        // Load the Arial font from the Unity Resources folder.
        arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        canvasGO = GameObject.Find("AccessTextParent");
        textGO = new GameObject();
        PrintVector(canvasGO.transform.position, "AccessTextCanvas");

        Calibration();
        initPosition = GameObject.Find("5F").transform.position;
        PrintVector(initPosition, "initPosition");
    }

    // Update is called once per frame
    void Update()
    {
        ////Calibration test
        //Calibration();

        //Ray test
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 vTouchPos = Input.mousePosition;
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
                //InitializeReadText(vHit.transform.name, worldTouchPos);
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
                    //if(i != readTexts.Length-1)
                    Destroy(readTexts[i]);
                }
            }

            // visualize the touch with red dot (even if it doesn't hit anything)
            GameObject touchVizObj = GameObject.Find("TouchPoints").transform.Find(string.Format("TouchPoint_{0}", 0)).gameObject;
            touchVizObj.transform.position = worldTouchPos;
            touchVizObj.SetActive(true);
        }
     
    }

    void Calibration()
    {
        if (!calib)
            statusText.text = "Please calibrate the model";

        //if (calib)
        //    statusText.text = "Model is calibrated";

        //if (Input.GetKeyDown(KeyCode.Mouse0) && (!calib))
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 screenPos = Input.mousePosition;
            PrintVector(screenPos, "ScreenPos");
            if (isCalibPos(screenPos))
            {
                Vector3 vTouchPos = Input.mousePosition;

                Vector3 calibPos = Camera.main.ScreenToWorldPoint(new Vector3(vTouchPos.x, vTouchPos.y, 180));

                calibPos.x -= calibOffX;
                calibPos.y -= calibOffY;
                PrintVector(calibPos, "CalibPos");
                GameObject.Find("5F").transform.position = calibPos;
                canvasGO.transform.position = calibPos;
                calib = true;
                statusText.text = "Model is calibrated";
            }
        }
    }

    static bool isCalibPos(Vector3 screenPos)
    {
        if (screenPos.x < screenW / 2)
            return true;
        else
            return false;
    }

    void PrintVector(Vector3 vec, string name = "String name")
    {
        object[] args = { vec.x, vec.y, vec.z, name };
        Debug.Log(string.Format("{3}: x={0}, y={1}, z={2}", args));
    }
}
