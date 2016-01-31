using UnityEngine;
using System.Collections;

public class CoffeeButton : MonoBehaviour {
    
    Animation anim;
    public GameObject coffeeArea;
    public GameObject coffee;
    public Material pressableMaterial;
    public Material usedMaterial;
    public int timeValidSeconds;
    float endTime;
    bool activated;
    MeshRenderer thisRenderer;

    BoxCollider coffeeAreaCollider;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animation>();
		coffeeAreaCollider = coffeeArea.GetComponent<BoxCollider>();
		activated = false;
		endTime = 0;
		thisRenderer = this.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (activated && endTime < Time.time)
		{
			thisRenderer.material = pressableMaterial;
			activated = false;
			endTime = 0;
			coffee.SendMessage("finishFilling");
			coffeeAreaCollider.enabled = false;
			Debug.Log("Disabled Coffee Area Collider");
		}
	}

    void OnHandHoverBegin(VRHand hand)
    {
    	if (!activated)
    	{
	    	anim.Play();
	        coffeeAreaCollider.enabled = true;
	        thisRenderer.material = usedMaterial;
	        activated = true;
	        Debug.Log("Enabled Coffee Area Collider");
	        endTime = timeValidSeconds + Time.time;
	    }
    }
}
