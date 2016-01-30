using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {
  public AudioSource[] audios;
  
	// Use this for initialization
	void Start () {
     audios = GetComponents<AudioSource>();     
	}
	
	// Update is called once per frame
	// void Update () {
    
	// }
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    
		if (c.gameObject.name == "Enemy") {
			audios[0].Play ();
		}
	}
}
