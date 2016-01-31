using UnityEngine;
using System.Collections;

public class Coffee : MonoBehaviour {

    public GameObject coffeeLiquid;
    Transform coffeeLiquidTransform;
    float timeStart;
    float fillAmount;

    void Start () {
        timeStart = 0;
        fillAmount = 0;
        coffeeLiquidTransform = coffeeLiquid.GetComponent<Transform>();
    }

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "CoffeeCollider")
        {
            Debug.Log("Coffee Mug Entered Trigger");
            timeStart = Time.time;
        }
    }

    void OnTriggerStay(Collider other) {
        // if(other.gameObject.tag == "CoffeeCollider")
        // {
        //     Debug.Log("Coffee Mug Within Trigger");
        // }
    }

    void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "CoffeeCollider")
        {
            finishFilling();
        }
    }

    void finishFilling() {
        if(timeStart != 0)
        {
            fillAmount += Time.time - timeStart;
            timeStart = 0;
            Debug.Log("Filled to " + fillAmount);
            coffeeLiquidTransform.localScale = coffeeLiquidTransform.localScale + (Vector3.up * (fillAmount / 100));
        }
    }
}
