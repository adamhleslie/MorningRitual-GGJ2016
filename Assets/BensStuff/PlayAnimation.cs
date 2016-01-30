using UnityEngine;
using System.Collections;

public class PlayAnimation : MonoBehaviour {
    Animation anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnHandHoverBegin(VRHand hand)
    {
        anim.Play();
        Globals.buttonPressed = !Globals.buttonPressed;
        Debug.Log(Globals.buttonPressed);
    }
}
