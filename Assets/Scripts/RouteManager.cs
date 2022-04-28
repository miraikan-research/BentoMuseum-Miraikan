using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class RouteManager : MonoBehaviour
{
    GameObject[] routeMains;
    GameObject[] routeSubs;

    List<Transform> routeNavNodes;
    List<string> routeNavDirections;

    GameObject tpGO;
    static int camPointDist = 125;

    Dropdown routeMode;
    bool directionMode = false;

    public static RouteManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        routeMains = GameObject.FindGameObjectsWithTag("Route_Main");
        routeSubs = GameObject.FindGameObjectsWithTag("Route_Sub");

        routeMode = GameObject.Find("RouteModeDropdown").GetComponent<Dropdown>();
        Debug.Log(routeMode.value);
        ResetNav();

        //Debug.Log(routeMains.Length + " main route points");
        //Debug.Log(routeSubs.Length + " sub route points");
        //foreach (GameObject route in routeMains)
        //{
        //    Debug.Log(route.transform.name);
        //    Debug.Log(route.transform.position);
        //}

        //tpGO = GameObject.Find("MainModel").transform.Find("TP").gameObject;
        //tpGO.SetActive(false);

        //Debug.Log("CoStudio -> Cell:");
        //string msg1 = RouteFindSpecific(GameObject.Find("CoStudio").transform, GameObject.Find("Cell").transform);
        //PrintTransformList(routeNavNodes);
        //PrintDirectionList(routeNavDirections);

        //Debug.Log("UniverseA -> Books:");
        //RouteFindSpecific(GameObject.Find("UniverseA").transform, GameObject.Find("Books").transform);
        //PrintTransformList(routeNavNodes);
        //PrintDirectionList(routeNavDirections);

        //Debug.Log("Books -> Medicine:");
        //RouteFindSpecific(GameObject.Find("Books").transform, GameObject.Find("Medicine").transform);
        //Debug.Log("CoStudio -> Books:");
        //RouteFindSpecific(GameObject.Find("CoStudio").transform, GameObject.Find("Books").transform);
        //Debug.Log("Survival -> GeoScope:");
        //RouteFindSpecific(GameObject.Find("Survival").transform, GameObject.Find("GeoScope").transform);

        //Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

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

    public void OnRouteModeChange()
    {
        if (routeMode.value == 0)
        {
            directionMode = false;
            VoiceController.StartSpeaking("Exploration Mode");
        }
        else
        {
            directionMode = true;
            VoiceController.StartSpeaking("Navigation Mode. Triple tap to mark the destination.");
        }
    }

    public bool IsDirectionMode()
    {
        return directionMode;
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
            //Debug.Log(vHit.transform.name);
            Transform pMain = vHit.transform;
            string msg = RouteFindAround(pMain);
        }
        else
        {
            Debug.Log("Ray hit nothing");
        }
    }

    public string RouteFindAround(Transform pMain)
    {
        string msg = "";

        if (pMain.tag == "Route_Main") //get connected, left, right information
        {
            
            int[] mlrChild = { pMain.childCount, 0, 0 };

            foreach (GameObject route in routeMains)
            {
                if (route.transform.name != pMain.name)
                {
                    if (route.transform.position.x < pMain.position.x)
                        mlrChild[1] += route.transform.childCount;
                    else
                        mlrChild[2] += route.transform.childCount;
                }
            }

            if (pMain.name != "0")
            {
                msg = string.Format("Main path..  {0} exhibitions are directed connected to here.", mlrChild[0]);
                msg += string.Format("{0} are on the left side.", mlrChild[1]);
                msg += string.Format("{0} are on the right side.", mlrChild[2]);
            }

            else
            {
                msg = string.Format("An rocket engine is here on the main path. You need to walk around it. The entrance and 1 exhibition are close to here.");
                msg += string.Format("{0} are on the left side.", mlrChild[1]);
                msg += string.Format("{0} are on the right side.", mlrChild[2]);
                //msg = "Hello";
            }

            Debug.Log(msg);
        }
        return msg;
    }

    public bool IsSubRoute(Transform t)
    {
        foreach (GameObject pSub in routeSubs)
        {
            if (pSub.name == t.name)
                return true;
        }
        return false;
    }

    public bool IsMainRoute(Transform t)
    {
        if (t.tag == "Route_Main")
            return true;
        return false;
    }

    public string RouteString(Transform t)
    {
        if (t.tag == "Route_Main")
            return "main path";
        else
            return t.name;
    }

    public List<Transform> GetNavRoute()
    {
        return routeNavNodes;
    }

    public List<string> GetNavDirect()
    {
        return routeNavDirections;
    }

    public void ResetNav()
    {
        routeNavNodes = new List<Transform>();
        routeNavDirections = new List<string>();
        Debug.Log("Nav route reset. RouteNavNodes.Count = " + routeNavNodes.Count);
    }

    public string RouteFindSpecific(Transform start, Transform end)
    {
        ResetNav();

        string msg = "";
        if (start.name == end.name)
            msg = "You are here already.";
        else
        {
            Transform pStart = null, pEnd  = null;
            foreach (GameObject pSub in routeSubs)
            {
                if (pSub.name == start.name)
                    pStart = pSub.transform;
                else if (pSub.name == end.name)
                    pEnd = pSub.transform;
            }

            if(( pStart != null) && (pEnd != null))
            {
                routeNavNodes.Add(pStart);

                Transform mStart = null, mEnd = null;
                mStart = pStart.parent;
                mEnd = pEnd.parent;
                if ((mStart != null) && (mEnd != null))
                {
                    if (mStart == mEnd) // same main node
                    {

                        //object[] args = { pStart.name, getDirectionV(pStart, mStart), mStart.name, getDirectionV(mStart, pEnd), pEnd.name };
                        //object[] args = { pStart.name, getDirectionV(pStart, mStart), "main path", getDirectionV(mStart, pEnd), pEnd.name };
                        //msg = string.Format("From {0}, go {1} to {2}, then go {3} to {4}", args);

                        routeNavNodes.Add(mStart);
                        if (mStart.childCount >2)
                        {
                            routeNavDirections.Add(getDirectionHV(pStart, mStart));
                            routeNavDirections.Add(getDirectionHV(mStart, pEnd));
                        }
                        else
                        {
                            routeNavDirections.Add(getDirectionV(pStart, mStart));
                            routeNavDirections.Add(getDirectionV(mStart, pEnd));
                        }

                    }
                    else
                    {
                        routeNavNodes.Add(mStart);

                        // get main routes in between mStart and mEnd
                        float[] range = { mStart.position.x, mEnd.position.x };
                        List<Transform> mRoutes = new List<Transform>() { mEnd };
                        foreach (GameObject route in routeMains)
                        {
                            if ((route.transform.name != mStart.name) && (route.transform.name != mEnd.name))
                            {
                                if ((route.transform.position.x > range.Min()) && (route.transform.position.x < range.Max()))
                                    mRoutes.Add(route.transform);
                            }
                        }
                        List<Transform> omRoutes = new List<Transform>();
                        if (range[1] > range[0])
                            omRoutes = mRoutes.OrderBy(order => order.position.x).ToList();
                        else
                            omRoutes = mRoutes.OrderByDescending(order => order.position.x).ToList();
                        string rs = string.Format("From {0},", pStart.name);
                        //rs += string.Format(" go {0} to {1}", getDirectionV(pStart, mStart), mStart.name);
                        //rs += string.Format(" go {0} to {1}", getDirectionV(pStart, mStart), "main path");

                        if (mStart.childCount > 2)
                            routeNavDirections.Add(getDirectionHV(pStart, mStart));
                        else
                            routeNavDirections.Add(getDirectionV(pStart, mStart));

                        Transform mPrev, mNow;
                        mPrev = mStart;
                        foreach (Transform r in omRoutes)
                        {
                            mNow = r;
                            //rs += string.Format(", go {0} to {1}", getDirectionH(mPrev, mNow), "main path");

                            routeNavNodes.Add(r);
                            routeNavDirections.Add(getDirectionH(mPrev, mNow));
                            mPrev = r;
                        }

                        //rs += string.Format(", then go {0} to {1}", getDirectionV(mEnd, pEnd), pEnd.name);

                        if (mEnd.childCount > 2)
                            routeNavDirections.Add(getDirectionHV(mEnd, pEnd));
                        else
                            routeNavDirections.Add(getDirectionV(mEnd, pEnd));

                        //msg = rs;
                    }

                    routeNavNodes.Add(pEnd);

                }
                msg += ".  Move your finger to start's entrance to navigate.";
            }
        }

        Debug.Log(msg);
        return msg;
    }

    void PrintTransformList(List<Transform> tl)
    {
        string msg = "Transform list: ";
        foreach (Transform t in tl)
            msg += t.name + ", ";
        Debug.Log(msg);
    }

    void PrintDirectionList(List<string> tl)
    {
        string msg = "Direction list: ";
        foreach (string t in tl)
            msg += t + ", ";
        Debug.Log(msg);
    }

    public string getDirectionH(Transform startP, Transform endP)
    {
        if (startP.position.x < endP.position.x)
            return "right";
        else if (startP.position.x > endP.position.x)
            return "left";
        else
            return "";
    }

    public string getDirectionV(Transform startP, Transform endP)
    {
        if (startP.position.y < endP.position.y)
            return "up";
        else if (startP.position.y > endP.position.y)
            return "down";
        else
            return "";
    }

    public string getDirectionHV(Transform startP, Transform endP)
    {
        string msg = "";
        msg += getDirectionH(startP, endP);
        msg += " ";
        msg += getDirectionV(startP, endP);
        return msg;
    }

    public string getDirectionVH(Transform startP, Transform endP)
    {
        string msg = "";
        msg += getDirectionV(startP, endP);
        msg += " ";
        msg += getDirectionH(startP, endP);
        return msg;
    }

    public string GetDirection(Transform p1, Transform p2)
    {
        Transform child;
        if ((p1.childCount == 1) && (p1.tag == "Route_Sub"))
        {
            child = p1;
        }
        else if ((p2.childCount == 1) && (p2.tag == "Route_Sub"))
        {
            child = p2;
        }
        else
            return "No Child. Please use other direction mode.";

        if (child.GetChild(0).name == "H")
            return getDirectionH(p1, p2);
        else if (child.GetChild(0).name == "V")
            return getDirectionV(p1, p2);
        else if (child.GetChild(0).name == "HV")
        {
            if (child == p2)
                return getDirectionHV(p1, p2);
            else
                return getDirectionVH(p1, p2);
        }
        else if (child.GetChild(0).name == "VH")
        {
            if (child == p2)
                return getDirectionVH(p1, p2);
            else
                return getDirectionHV(p1, p2);
        }
        else
            return child.GetChild(0).name +" ||| Child not correct. Please use other direction mode.";
    }
}
