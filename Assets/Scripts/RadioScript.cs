using UnityEngine;
using System.Collections;

public class RadioScript : MonoBehaviour {
  public AudioSource[] radioAudio;
  private bool radioIsOn = false;
  private int audioClipIndex = 0;
  
  
	// Use this for initialization
	void Start () {
    radioAudio = GetComponents<AudioSource>();
    foreach (AudioSource radioAudioSource in radioAudio){
      radioAudioSource.playOnAwake=false;
    }
	}
	
	//Update is called once per frame
	void Update () {
    if (radioIsOn && !radioAudio[audioClipIndex].isPlaying) {
      if (audioClipIndex+1 < radioAudio.Length){
        radioAudio[audioClipIndex+1].Play();
        audioClipIndex+=1;
      }
    }
    
    //debugging shit, change to onButtonPress
    if(Input.GetMouseButtonDown(0)){
      if (!radioIsOn) {
        radioIsOn = true;
        radioAudio[audioClipIndex].Play ();
        radioAudio[audioClipIndex].mute = false;
      }
      else{
        //radioAudio[audioClipIndex].mute = true;
      }
    }
	}
  
  //will change this to "onButtonPress" 
  void OnCollisionEnter(Collision c) {
    if (!radioIsOn) {
      radioIsOn = true;
      radioAudio[audioClipIndex].Play ();
      radioAudio[audioClipIndex].mute = false;
    }
    else{
      radioAudio[audioClipIndex].mute = true;
    }
	}
  
}
