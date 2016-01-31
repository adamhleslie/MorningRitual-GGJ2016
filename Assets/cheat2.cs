using UnityEngine;
using System.Collections;

public class cheat2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ActivateCheat()
	{
		Debug.Log("ACTIVATING CHEAT");
		for(int i = 0; i < Globals.ritualSequence.Count; i++)
		{
			Globals.ritualSequence[i] = true;
		} 
	}
}
