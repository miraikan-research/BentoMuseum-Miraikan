using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager_Graph : MonoBehaviour
{

    GameObject[] routeMains;
    GameObject[] routeSubs;

    List<Transform> routeNavNodes;
    List<string> routeNavDirections;
    List<string[]> routeNavDirectionsTurn;

    bool directionMode = false;

    Graph graph;

    public static RouteManager_Graph Instance;

    private void Awake()
    {
        Instance = this;

    }

    void Start()
    {

        // Debug
        //RouteFindAround(GameObject.Find("3").transform);

        //Debug.Log("Garden -> Restaurant:");
        //Transform garden = GameObject.Find("garden").transform;
        //Transform rest = GameObject.Find("restaurant").transform;
        //RouteFindSpecific(garden, rest);

        //Debug.Log("Garden -> Conf_1:");
        //RouteFindSpecific(GameObject.Find("garden").transform, GameObject.Find("conf_1").transform);

        //Debug.Log("Conf_1 -> Garden:");

        //RouteFindSpecific(GameObject.Find("conf_1").transform, GameObject.Find("garden").transform);

        //InitializeFloorGraph("3F");
        ////Debug.Log("Paro -> Sc_1:");
        //RouteFindSpecific(GameObject.Find("Paro").transform, GameObject.Find("Sc_1").transform);
        //RouteFindSpecific(GameObject.Find("Android").transform, GameObject.Find("Nobel_1").transform);
        //RouteFindSpecific(GameObject.Find("Android").transform, GameObject.Find("OvalBridge_3F").transform);
    }

    void Update()
    {
    }

    public void ResetNav()
    {
        routeNavNodes = new List<Transform>();
        routeNavDirections = new List<string>();
        routeNavDirectionsTurn = new List<string[]>();

        Debug.Log("Nav route reset.routeNavNodes.Count = " + routeNavNodes.Count);
    }

    public List<Transform> GetNavRoute()
    {
        return routeNavNodes;
    }

    public List<string> GetNavDirect()
    {
        return routeNavDirections;
    }

    public List<string[]> GetNavDirectTurn()
    {
        return routeNavDirectionsTurn;
    }

    public GameObject[] GetMainRoute()
    {
        return routeMains;
    }

    public bool IsMainRoute(Transform t)
    {
        if (t.tag == "Route_Main")
            return true;
        return false;
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

    public bool IsDirectionMode()
    {
        return directionMode;
    }

    public void DirectionModeSwitch()
    {
        directionMode = !directionMode;
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

            msg = string.Format("Main path..  {0} rooms are directed connected to here. ", mlrChild[0]);
            msg += string.Format("{0} are on the left side. ", mlrChild[1]);
            msg += string.Format("{0} are on the right side.", mlrChild[2]);

            Debug.Log(msg);
        }
        return msg;
    }

    public int[] RouteFindAroundCount(Transform pMain)
    {
        int[] mlrChild = { 0, 0, 0 };
        if (pMain.tag == "Route_Main") //get connected, left, right information
        {

            mlrChild = new int[] { pMain.childCount, 0, 0 };

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

            string msg = string.Format("Main path.  Connected: {0}. ", mlrChild[0]);
            msg += string.Format("Left: {0}. ", mlrChild[1]);
            msg += string.Format("Right: {0}.", mlrChild[2]);
            Debug.Log(msg);
        }
        return mlrChild;
    }

    public string RouteFindSpecific(Transform start, Transform end)
    {
        ResetNav();

        string msg = "";
        if (start.name == end.name) // start and end: Same point
            //msg = "You are here already.";
            msg = "すでにここです。";
        else
        {
            Transform pStart = null, pEnd = null;
            foreach (GameObject pSub in routeSubs)
            {
                if (pSub.name == start.name)
                    pStart = pSub.transform;
                else if (pSub.name == end.name)
                    pEnd = pSub.transform;
            }

            if ((pStart != null) && (pEnd != null))
            {
                routeNavNodes.Add(pStart);

                Transform mStart = null, mEnd = null;
                mStart = pStart.parent;
                mEnd = pEnd.parent;

                if ((mStart != null) && (mEnd != null))
                {
                    if (mStart == mEnd) // same main node
                    {
                        routeNavNodes.Add(mStart);

                        routeNavDirections.Add(GetDirection(pStart, mStart));
                        routeNavDirections.Add(GetDirection(mStart, pEnd));

                        routeNavDirectionsTurn.Add(new string[] { GetDirection(pStart, mStart) });
                        routeNavDirectionsTurn.Add(getDirectionTurnByTurn(pStart, mStart, pEnd));
                    }
                    else 
                    {
                        routeNavDirections.Add(GetDirection(pStart, mStart));
                        routeNavDirectionsTurn.Add(new string[] { GetDirection(pStart, mStart) });

                        // get main route path using graph
                        List<int> mRoutes = graph.GetPath(int.Parse(mStart.name), int.Parse(mEnd.name));

                        Transform mPrev = GameObject.Find(mRoutes[0].ToString()).transform;
                        Transform mNow;
                        routeNavNodes.Add(mPrev);

                        // add fixed direction
                        for (int i = 0; i < mRoutes.Count; i++) 
                        {
                            if (i > 0)
                            {
                                Transform r = GameObject.Find(mRoutes[i].ToString()).transform;
                                routeNavNodes.Add(r);

                                mNow = r;
                                if (int.Parse(mNow.name) > int.Parse(mPrev.name))
                                    routeNavDirections.Add(getDirectionHV(mPrev, mNow));
                                else
                                    routeNavDirections.Add(getDirectionVH(mPrev, mNow));

                                mPrev = r;

                            }
                        }
                        routeNavDirections.Add(GetDirection(mEnd, pEnd));

                        // add turn by turn direction
                        List<Transform> turnRoute = new List<Transform> ();
                        turnRoute.Add(pStart);
                        for (int i = 0; i < mRoutes.Count; i++)
                        {
                            Transform r = GameObject.Find(mRoutes[i].ToString()).transform;
                            turnRoute.Add(r);
                        }
                        turnRoute.Add(pEnd);

                        for (int i = 0; i < (turnRoute.Count-2); i++) 
                        {
                            Transform pre = turnRoute[i], now = turnRoute[i+1], next = turnRoute[i + 2];
                            routeNavDirectionsTurn.Add(getDirectionTurnByTurn(pre, now, next));
                        }
                    }
                    routeNavNodes.Add(pEnd);
                }

                //// for Debugging, print the node and directiion
                //string msg_debug = "From " + routeNavNodes[0].name;
                //for (int i = 0; i < routeNavNodes.Count - 1; i++)
                //{
                //    if (i != routeNavNodes.Count)
                //        msg_debug += string.Format(" Go {0} to {1}. ", routeNavDirections[i], routeNavNodes[i + 1]);
                //}
                //Debug.Log(msg_debug);

                // for Debugging, print the node and turn by turn directiion
                string msg_debug = "From " + routeNavNodes[0].name;
                for (int i = 0; i < routeNavNodes.Count - 1; i++)
                {
                    if (i != routeNavNodes.Count)
                        msg_debug += string.Format(" Go {0} to {1}. ", string.Join("-", routeNavDirectionsTurn[i]), routeNavNodes[i + 1]);
                }
                Debug.Log(msg_debug);



                //msg += ".  Move your finger to start's entrance to navigate.";
                msg += "指を出発地の入り口に動かしてください。";
            }
        }

        
        return msg;
    }

    public string[] getDirectionTurnByTurn(Transform pre, Transform now, Transform next)
    {
        List<string> output = new List<string>();

        // create new coordinate based on location of "now"
        Vector3 preLoc = pre.position - now.position;
        Vector3 nextLoc = next.position - now.position;

        // clockwise negative, counterclockwise positive
        //Debug.Log("turn by turn angle");
        //Debug.Log(Vector2.SignedAngle(new Vector2(0,-1), new Vector2(-1, 0))); -90
        //Debug.Log(Vector2.SignedAngle(new Vector2(-1, 0), new Vector2(0, -1))); 90
        //Debug.Log(Vector2.SignedAngle(new Vector2(0, -1), new Vector2(1, 0))); 90
        //Debug.Log(Vector2.SignedAngle(new Vector2(1, 0), new Vector2(0, -1))); -90
        //Debug.Log(Vector2.SignedAngle(new Vector2(0, -1), new Vector2(0, 1))); 180
        //Debug.Log(Vector2.SignedAngle(new Vector2(0, 1), new Vector2(0, -1))); 180
        //Debug.Log(Vector2.SignedAngle(new Vector2(1, 0), new Vector2(-1, 0))); 180
        //Debug.Log(Vector2.SignedAngle(new Vector2(-1, 0), new Vector2(1, 0))); 180
        //Debug.Log(Vector2.SignedAngle(new Vector2(0.1f, 1), new Vector2(0, -1))); -174
        //Debug.Log(Vector2.SignedAngle(new Vector2(0, -1), new Vector2(0.1f, 1))); 174

        float angle = Vector2.SignedAngle(preLoc, nextLoc);
        if ((angle >= 150)|| (angle <= -150))
            output.Add("直進");
        else
        {
            if (angle < 0) output.Add("左折して "); //left turn
            else if (angle > 0) output.Add("右折して "); //left turn
            output.Add("直進");
        }
        return output.ToArray();
    }

    public string getDirectionH(Transform startP, Transform endP)
    {
        if (startP.position.x < endP.position.x)
            return "右";
        else if (startP.position.x > endP.position.x)
            return "左";
        else
            return "";
    }

    public string getDirectionV(Transform startP, Transform endP)
    {
        if (startP.position.y < endP.position.y)
            //return "上";
            return "奥";
        else if (startP.position.y > endP.position.y)
            //return "下";
            return "手前";
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
            return child.GetChild(0).name + " ||| Child not correct. Please use other direction mode.";
    }


    public string RouteString(Transform t)
    {
        if (t.tag == "Route_Main")
            return "交差点";
        else
            return t.name;
    }

    public void InitializeFloorGraph(string floor)
    {
        if (floor == "7F")
        {
            graph = new Graph(8);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);
            graph.AddEdge(5, 6);
            graph.AddEdge(6, 7);
            graph.AddEdge(1, 6);
            graph.AddEdge(2, 5);
        }

        else if (floor == "5F")
        {
            graph = new Graph(6);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(0, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);
        }

        else if (floor == "3F")
        {
            graph = new Graph(6);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 5);
        }

        else if (floor == "1F")
        {
            graph = new Graph(5);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
        }
        InitializeRoute();
        ResetNav();
        directionMode = false;
    }

    void InitializeRoute()
    {
        routeMains = GameObject.FindGameObjectsWithTag("Route_Main");
        routeSubs = GameObject.FindGameObjectsWithTag("Route_Sub");
    }
    
}