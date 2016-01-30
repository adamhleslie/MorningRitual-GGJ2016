using UnityEngine;
using System.Collections;

public class Coffee : MonoBehaviour {

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "CoffeeCollider")
        {
            Debug.Log("Coffee Mug Entered Collider");
        }
	}

    // void OnCollisionStay(Collision collision)
    // {
    //     if (Globals.testCollisions.Count > 5)
    //     {
    //         Globals.testingFloor = true;
    //     }
    //     Debug.Log("Stay" + Globals.testingFloor);
    // }

    // void OnCollisionExit(Collision collision)
    // {
    //     if(collision.GameObject.tag == "CoffeeCollider")
    //     {
            
    //     }
    // }
}
