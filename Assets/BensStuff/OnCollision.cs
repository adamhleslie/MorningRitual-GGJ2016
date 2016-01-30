using UnityEngine;
using System.Collections;

public class OnCollision : MonoBehaviour {
    int numberStay = 0;

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        numberStay++;
        Debug.Log("Enter");
        Globals.testCollisions.Add(collision);
	}

    void OnCollisionStay(Collision collision)
    {
        if (numberStay > 5)
        {
            Globals.testingFloor = true;

        }
        Debug.Log("Stay" + Globals.testingFloor);
    }

    void OnCollisionExit(Collision collision)
    {
        numberStay--;
        if (numberStay <= 5)
        {
            Globals.testingFloor = false;
        }
        Debug.Log("Exit");
    }
}
