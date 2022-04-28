using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iPadTouchTest : MonoBehaviour
{

    GameObject particle;

    // Start is called before the first frame update
    void Start()
    {
        if (Input.touchPressureSupported)
        {
            Debug.Log("3D touch is available！");
        }
        else
        {
            Debug.Log("3D touch is not available...");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.touchCount > 0)
        //{
        //    foreach (Touch touch in Input.touches)
        //    {
        //        if (touch.phase != TouchPhase.Ended)
        //        {
        //            // Construct a ray from the current touch coordinates
        //            Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //            if (Physics.Raycast(ray))
        //            {
        //                // Create a particle if hit
        //                Instantiate(particle, transform.position, transform.rotation);
        //            }
        //        }
        //    }
        //}
        

    }
}
