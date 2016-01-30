using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {
  public AudioSource[] audios;
  private bool radioIsOn = false;
  
  
	// Use this for initialization
	void Start () {
     audios = GetComponents<AudioSource>();     
	}
	
	// Update is called once per frame
	// void Update () {
    
	// }
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    int audioIndex = 0;
    if (!radioIsOn){
      radioIsOn = true;
      //play first clip, then when each clip ends, play the next clip
      
      if (c.gameObject.name == "Radio") {
        audios[0].Play ();
      }
    }
    else{
      
    }
		
	}
}
