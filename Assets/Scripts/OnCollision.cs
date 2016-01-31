using UnityEngine;
using System.Collections;

public class OnCollision : MonoBehaviour {
    int numberStay = 0;
    public bool dishes;
    // ArrayList curCollisions = new ArrayList();

    // void Start()
    // {
    //     curCollisions = new ArrayList();
    // }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        if(!dishes || (dishes && collision.gameObject.tag == "Dishes"))
        {
            numberStay++;
        }

        Debug.Log("Entered");
        if(dishes && numberStay == 2)
        {
            Debug.Log("Dishes DONE");
            Globals.ritualSequence[1] = true;
        }
        else if (!dishes && numberStay == 5)
        {
            Debug.Log("TRASHCAN DONE");
            Globals.ritualSequence[3] = true;
        }
	}

    // void OnCollisionStay(Collision collision)
    // {
    //     if (dishes && !((bool)Globals.ritualSequence[1]) && curCollisions.Count >= 2)
    //     {
    //         int numDishes = 0;
    //         for(int i = 0; i < curCollisions.Count && numDishes < 2; i++)
    //         {
    //             if(((Collision)curCollisions[i]).gameObject.tag == "Dishes")
    //             {
    //                 numDishes++;
    //             }
    //         }
    //         if(numDishes == 2){

    //         }
    //     }
    //     else if (!dishes && !((bool)Globals.ritualSequence[3]) && curCollisions.Count > 5)
    //     {
    //         Debug.Log("TRASHCAN DONE");
    //         Globals.ritualSequence[3] = true;
    //     }
    //     //Debug.Log("Stay" + Globals.testingFloor + Globals.testCollisions.Count);
    // }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit");
    }
}
