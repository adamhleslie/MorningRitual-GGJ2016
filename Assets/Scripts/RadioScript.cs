using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {
  public AudioSource audio;
  
	// Use this for initialization
	void Start () {
     AudioSource[] audios = GetComponents<AudioSource>();     
	}
	
	// Update is called once per frame
	// void Update () {
    
	// }
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    
		if (c.gameObject.name == "Enemy") {
			clickAudio.Play ();
		}
	}
}
