using UnityEngine;
using System.Collections;

public class PlayAnimationRadio : MonoBehaviour {
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
        Globals.radioIsOn = !Globals.radioIsOn;

        Debug.Log(Globals.radioIsOn);
    }
}
