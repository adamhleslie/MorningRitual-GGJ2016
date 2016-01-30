using UnityEngine;
using System.Collections;

public class RadioScript : MonoBehaviour {
  public AudioSource radioAudio;
  public AudioClip[] clips;
  private bool radioIsOn = false;
  private int audioClipIndex = 0;
  
  
	// Use this for initialization
	void Start () {
     radioAudio = GetComponent<AudioSource>();     
	}
	
	//Update is called once per frame
	void Update () {
    if (!radioAudio.isPlaying) {
      if (audioClipIndex<Globals.numScenes){
        audioClipIndex+=1;
        radioAudio.clip = clips[audioClipIndex];
        radioAudio.Play();
      }
    }
	}
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    if (c.gameObject.name == "Radio"){
      if (!radioIsOn) {
        radioIsOn = true;
        radioAudio.Play ();
        radioAudio.mute = false;
      }
      else{
        radioAudio.mute = true;
      }
    }
	}
  
}
