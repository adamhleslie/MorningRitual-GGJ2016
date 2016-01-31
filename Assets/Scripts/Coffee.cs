using UnityEngine;
using System.Collections;

public class Coffee : MonoBehaviour {

    // SET FILL RATE IN UNITY

    public GameObject coffeeLiquid;
    VRThrowable coffeeThrowable;
    Transform coffeeLiquidTransform;
    public float fillRate;
    float fillAmount;
    Vector3 initialScale;

    void Update() {
        if(fillAmount >= 0 && Vector3.Dot(coffeeLiquidTransform.up, Vector3.down) > 0)
        {
            fillAmount = 0;
            coffeeLiquidTransform.localScale = initialScale + (Vector3.up * (fillAmount / 100));
        }
    }

    void Start () {
        fillAmount = 0;
        coffeeThrowable = this.GetComponent<VRThrowable>();
        coffeeLiquidTransform = coffeeLiquid.GetComponent<Transform>();
        initialScale = coffeeLiquidTransform.localScale;
    }

    // void OnTriggerEnter(Collider other) {
    //     if(other.gameObject.tag == "CoffeeCollider")
    //     {
    //         Debug.Log("Coffee Mug Entered Trigger");
    //     }
    // }

    void OnTriggerStay(Collider other) {
        if(other.gameObject.tag == "CoffeeCollider")
        {
            fillAmount += fillRate;
            coffeeLiquidTransform.localScale = initialScale + (Vector3.up * (fillAmount / 100));
        }
    }

    void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "CoffeeCollider")
        {
            Debug.Log("Coffee Mug Exited Trigger: Total Fill = " + fillAmount);
        }
    }
}
