using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {
  public AudioSource[] radioAudio;
  public AudioClip[] clips;
  private bool radioIsOn = false;
  private int audioClipIndex = 0;
  
  
	// Use this for initialization
	void Start () {
     radioAudio = GetComponents<AudioSource>();     
	}
	
	//Update is called once per frame
	void Update () {
    if (!radioAudio[audioClipIndex].isPlaying) {
      if (audioClipIndex<Globals.numScenes){
        audioClipIndex+=1;
        radioAudio[audioClipIndex].clip = clips[audioClipIndex];
        radioAudio[audioClipIndex].Play();
      }
    }
	}
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    if (c.gameObject.name == "Radio"){
      if (!radioIsOn) {
        radioIsOn = true;
        radioAudio[audioClipIndex].mute = false;
        radioAudio[audioClipIndex].Play ();
      }
      else{
        radioAudio[audioClipIndex].mute = true;
      }
    }
	}
  
}
