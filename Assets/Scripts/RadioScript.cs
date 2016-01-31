using UnityEngine;
using System.Collections;

public class RadioScript : MonoBehaviour {
  public AudioSource[] radioAudio;
  private int audioClipIndex = 0;
  
  
	// Use this for initialization
	void Start () {
    radioAudio = GetComponents<AudioSource>();
        radioAudio[audioClipIndex].Play();
        foreach (AudioSource radioAudioSource in radioAudio){
      radioAudioSource.playOnAwake=false;
    }
	}
	
	//Update is called once per frame
	void Update () {
        if (Globals.radioIsOn && !radioAudio[audioClipIndex].isPlaying) {
      if (audioClipIndex+1 < radioAudio.Length){
        radioAudio[audioClipIndex+1].Play();
        audioClipIndex+=1;
      }
    }
    
    //debugging shit, change to onButtonPress

    if (Globals.radioIsOn) {
        radioAudio[audioClipIndex].mute = false;
      }
      else{
        radioAudio[audioClipIndex].mute = true;
      }
	}
  
}
