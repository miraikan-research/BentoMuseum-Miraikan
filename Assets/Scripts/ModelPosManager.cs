using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelPosManager : MonoBehaviour
{
    GameObject floorGO;
    Vector3 initPos, initScale;
    Vector3 curtPos, curtScale;
    bool showPos = false;

    GameObject posCtrlUI, posCtrlBtns;
    Text posText;
    float transScale = 2f, sizeScale = 0.5f;

    // control speed button location no matter the floors
    GameObject speedButtons_1, speedButtons_2;
    Vector3 initSpeedPos_1, initSpeedPos_2;

    string floor;
    string other = "";
    public static ModelPosManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitializeFloor(string floorName, string otherName = "")
    {
        floor = floorName;
        other = otherName;
        floorGO = GameObject.Find(floor);

        posCtrlUI = GameObject.Find("Canvas_PosCtrl");
        speedButtons_1 = GameObject.Find("Cube_Buttons").transform.Find("Cube_SpeedControls").gameObject;
        speedButtons_2 = posCtrlUI.transform.Find("SpeedControls").gameObject;

        LoadGame(floor + other);

        initPos = floorGO.transform.position;
        initScale = floorGO.transform.localScale;

        initSpeedPos_1 = speedButtons_1.transform.position;
        initSpeedPos_2 = speedButtons_2.transform.position;


        //PrintVector(initPos, "initPos");
        //PrintVector(initScale, "initScale");

        // find btns
        posCtrlBtns = posCtrlUI.transform.Find("PosCtrlBtns").gameObject;
        // find pos text
        posText = posCtrlBtns.transform.Find("Button_Pos").gameObject.transform.Find("Text").GetComponent<Text>();
        //Debug.Log(posText.text);



        // hide buttons
        posCtrlBtns.SetActive(false);
        //Debug.Log(posCtrlBtns.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        curtPos = floorGO.transform.position;
        curtScale = floorGO.transform.localScale;

        if (showPos)
        {
            // show pos values
            posText.text = string.Format("x,y,z\n{0:0.0},{1:0.0},{2:0}\nscale{3:0.000}",
                curtPos.x, curtPos.y, curtPos.z,curtScale.x);

            // transform and scale
            floorGO.transform.position = initPos;
            floorGO.transform.localScale = initScale;

            speedButtons_1.transform.position = initSpeedPos_1;
            speedButtons_2.transform.position = initSpeedPos_2;

        }
    }

    public void OnSaveClick()
    {
        PlayerPrefs.SetFloat(floor + other + "SavedX", initPos.x);
        PlayerPrefs.SetFloat(floor + other + "SavedY", initPos.y);
        PlayerPrefs.SetFloat(floor + other + "SavedScale", initScale.x);
        PlayerPrefs.SetFloat(floor + other + "SpeedButton1_SavedX", initSpeedPos_1.x);
        PlayerPrefs.SetFloat(floor + other + "SpeedButton1_SavedY", initSpeedPos_1.y);
        PlayerPrefs.SetFloat(floor + other + "SpeedButton2_SavedX", initSpeedPos_2.x);
        PlayerPrefs.SetFloat(floor + other + "SpeedButton2_SavedY", initSpeedPos_2.y);
        PlayerPrefs.Save();
        Debug.Log("Pos data saved!");
    }


    public void OnResetClick()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Data reset complete");
    }

    void LoadGame(string FloorName)
    {
        if (PlayerPrefs.HasKey(FloorName+"SavedX"))
        {
            float x = PlayerPrefs.GetFloat(FloorName + "SavedX");
            float y = PlayerPrefs.GetFloat(FloorName + "SavedY");
            float scale = PlayerPrefs.GetFloat(FloorName + "SavedScale");

            floorGO.transform.position = new Vector3(x,y,0f);
            floorGO.transform.localScale = new Vector3(scale, scale, scale);

            Debug.Log(FloorName + " Pos data loaded!");
        }
        else
            Debug.Log(FloorName + " There is no saved pos data!");

        if (PlayerPrefs.HasKey(FloorName + "SpeedButton1_SavedX"))
        {
            float x = PlayerPrefs.GetFloat(FloorName + "SpeedButton1_SavedX");
            float y = PlayerPrefs.GetFloat(FloorName + "SpeedButton1_SavedY");
            speedButtons_1.transform.position = new Vector3(x, y, 0f);

            x = PlayerPrefs.GetFloat(FloorName + "SpeedButton2_SavedX");
            y = PlayerPrefs.GetFloat(FloorName + "SpeedButton2_SavedY");
            speedButtons_2.transform.position = new Vector3(x, y, 0f);

            Debug.Log("SpeedButton Pos data loaded!");
        }
        else
            Debug.Log(FloorName + " SpeedButton There is no saved pos data!");
    }

    public void OnShowClick()
    {
        if (!showPos)
        {
            posCtrlBtns.SetActive(true);
            showPos = true;
        }
        else
        {
            posCtrlBtns.SetActive(false);
            showPos = false;
        }
    }

    public void OnUpClick()
    {
        initPos.y += 0.1f * transScale;
    }

    public void OnDownClick()
    {
        initPos.y -= 0.1f * transScale;
    }

    public void OnLeftClick()
    {
        initPos.x -= 0.1f * transScale;
    }

    public void OnRightClick()
    {
        initPos.x += 0.1f * transScale;
    }

    public void OnLargeClick()
    {
        initScale += new Vector3(0.01f, 0.01f, 0.01f) * sizeScale;
    }

    public void OnSmallClick()
    {
        initScale -= new Vector3(0.01f, 0.01f, 0.01f) * sizeScale;
    }

    public void OnClick_SpeedButtons_Left()
    {
        initSpeedPos_1.x -= 0.1f * transScale;
        initSpeedPos_2.x -= 0.1f * transScale;
    }

    public void OnClick_SpeedButtons_Right()
    {
        initSpeedPos_1.x += 0.1f * transScale;
        initSpeedPos_2.x += 0.1f * transScale;
    }


    void PrintVector(Vector3 vec, string name = "String name")
    {
        object[] args = { vec.x, vec.y, vec.z, name };
        Debug.Log(string.Format("{3}: x={0}, y={1}, z={2}", args));
    }
}
