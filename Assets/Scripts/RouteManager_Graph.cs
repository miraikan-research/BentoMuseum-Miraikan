using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RouteManager_Graph : MonoBehaviour
{

    GameObject[] routeMains;
    GameObject[] routeSubs;

    List<Transform> routeNavNodes;
    List<string> routeNavDirections;
    List<string[]> routeNavDirectionsTurn;

    List<Transform> guide_NavNodes;
    List<string> guide_Directs;
    List<string> guide_NavDirections;
    List<string[]> guide_NavDirectionsTurn;
    List<string> guide_NavDistance;

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

    public void DirectionModeTrue()
    {
        directionMode = true;
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

    private List<Transform> Guide_FindNavNodes()
    {
        List<Transform> nodes = new List<Transform>();
        guide_Directs = new List<string>();

        int[] route_1 = new int[] { 0, 3, 4, 5 };
        int[] route_2 = new int[] { 5, 4, 3, 0, 1, 2 };
        int[] route_3 = new int[] { 2, 1, 0 };

        // add lobby
        Transform obj = GameObject.Find("5F").transform.Find("Lobby");
        nodes.Add(obj);
        guide_Directs.Add("V");

        for (int i = 0; i < route_1.Length; i++)
        {
            Transform mainPath = GameObject.Find("5F").transform.Find(route_1[i].ToString());
            Transform[] allChildren = mainPath.GetComponentsInChildren<Transform>();
            Transform[] oChildren = allChildren.OrderBy(order => order.position.y).ToArray();
            nodes.Add(oChildren[0]);

            if (i == route_1.Length-1)
            {
                oChildren = allChildren.OrderBy(order => order.position.x).ToArray();
                nodes.Add(oChildren[0]);

                guide_Directs.Add("HV");
                guide_Directs.Add("VH");
            }
            else
                guide_Directs.Add("H");
        }

        for (int i = 0; i < route_2.Length; i++)
        {
            Transform mainPath = GameObject.Find("5F").transform.Find(route_2[i].ToString());
            Transform[] allChildren = mainPath.GetComponentsInChildren<Transform>();
            Transform[] oChildren = allChildren.OrderByDescending(order => order.position.y).ToArray();
            nodes.Add(oChildren[0]);

            if (i == route_2.Length - 1)
                guide_Directs.Add("V");
            else
                guide_Directs.Add("H");

        }

        for (int i = 0; i < route_3.Length; i++)
        {
            Transform mainPath = GameObject.Find("5F").transform.Find(route_3[i].ToString());
            Transform[] allChildren = mainPath.GetComponentsInChildren<Transform>();
            Transform[] oChildren = allChildren.OrderBy(order => order.position.y).ToArray();
            nodes.Add(oChildren[0]);

            if (i != route_3.Length - 1)
                guide_Directs.Add("H");
        }

        // add rocket engine
        obj = GameObject.Find("5F").transform.Find("0");
        nodes.Add(obj);
        guide_Directs.Add("V");

        //string dub_string = "";
        //foreach (Transform item in nodes)
        //{
        //    dub_string += item.name;
        //    dub_string += " ; ";
        //}
        //Debug.Log(dub_string);

        //dub_string = "";
        //foreach (string item in guide_Directs)
        //{
        //    dub_string += item;
        //    dub_string += " ; ";
        //}
        //Debug.Log(dub_string);

        //Debug.Log("Node count = " + nodes.Count());
        //Debug.Log("guide_Directs count = " + guide_Directs.Count());

        return nodes;
    }

    public void Guide_FindNodeDirectTurn()
    {
        guide_NavNodes = Guide_FindNavNodes();
        guide_NavDirections = new List<string>();
        guide_NavDirectionsTurn = new List<string[]>();
        guide_NavDistance = new List<string>();

        GameObject dummyStart = GameObject.Instantiate(guide_NavNodes[0].gameObject);
        Vector3 dummyLoc = dummyStart.transform.position;
        dummyStart.transform.position = new Vector3(dummyLoc.x-100, dummyLoc.y, dummyLoc.z);

        List<Transform > dummy_guide_NavNodes = guide_NavNodes.ToList();
        dummy_guide_NavNodes.Insert(0, dummyStart.transform);


        for (int i = 0; i < guide_NavNodes.Count-1; i++)
        {
            guide_NavDirections.Add(GetDirection(guide_NavNodes[i], guide_NavNodes[i + 1], guide_Directs[i]));
            //guide_NavDirectionsTurn.Add(new string[] { GetDirection(guide_NavNodes[i], guide_NavNodes[i + 1], guide_Directs[i]) });
            guide_NavDirectionsTurn.Add(getDirectionTurnByTurn(dummy_guide_NavNodes[i], dummy_guide_NavNodes[i + 1], dummy_guide_NavNodes[i + 2]));

            // add distance
            string dist_0 = "1";
            string dist_1 = " センチ　";
            if (i == 0)
                dist_0 = "2.5";
            else if (i == 3)
                dist_0 = "1.5";
            else if (i == 4) // Oval bright
                dist_0 = "1.5";
            else if (i == 5) // Oval bright
                dist_0 = "5|1";
            else if (i == 9)
                dist_0 = "0.5";
            else if (i == 10)
                dist_0 = "0.5";
            else if (i == 11)
                dist_0 = "3";
            else if (i == 13)
                dist_0 = "0.5";
            else if (i == 14)
                dist_0 = "3";

            guide_NavDistance.Add(dist_0 + dist_1);

        }

        dummyStart.SetActive(false);

        //string dub_string = "";
        //foreach (string item in guide_NavDirections)
        //{
        //    dub_string += item;
        //    dub_string += " ; ";
        //}
        //Debug.Log("guide_NavDirections count = " + guide_NavDirections.Count());
        //Debug.Log(dub_string);

        //string dub_string = "";
        //foreach (string[] items in guide_NavDirectionsTurn)
        //{
        //    foreach (string item in items)
        //    {
        //        dub_string += item;
        //        dub_string += ";";
        //    }
        //    dub_string += " . ";
        //}
        //Debug.Log("guide_NavDirectionsTurn count = " + guide_NavDirectionsTurn.Count());
        //Debug.Log(dub_string);
    }

    public List<Transform> GetGuide_NavRoute()
    {
        return guide_NavNodes;
    }

    public List<string> GetGuide_NavDirect()
    {
        return guide_NavDirections;
    }

    public List<string[]> GetGuide_NavDirectTurn()
    {
        return guide_NavDirectionsTurn;
    }

    public List<string> GetGuide_NavDistance()
    {
        return guide_NavDistance;
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

    public string GetDirection(Transform p1, Transform p2, string direct)
    {
        if (direct == "H")
            return getDirectionH(p1, p2);
        else if (direct == "V")
            return getDirectionV(p1, p2);
        else if (direct == "HV")
            return getDirectionHV(p1, p2);
        else if (direct == "VH")
            return getDirectionVH(p1, p2);
        else
            return "Direction string INCORRECT.";
    }

    public string RouteString(Transform t)
    {
        if (t.tag == "Route_Main")
            return "交差点";
        else
            return t.name;
    }

    public string RouteString_Guide(Transform t)
    {
        if (t.name == "Entrance_5F")
            return "入り口";
        else if (t.tag == "Route_Sub"　|| t.name == "0")
            return "展示";
        else if (t.tag == "Route_Main")
            return "交差点";

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