using UnityEngine;
using System.Collections;

public class RadioScript : MonoBehaviour {
  public AudioSource[] radioAudio;
  private int audioClipIndex = 0;
  private int ritualSequenceIndex = 0;
  private ArrayList priorDayRitualSequence;
  
  void Awake(){
    
  }
  
  // Use this for initialization
	void Start () {
    if(Globals.ritualSequence.Count == 0)
    {
      Globals.ritualSequence.Add(false);
    }
    radioAudio = GetComponents<AudioSource>();
    radioAudio[audioClipIndex].Play();
    foreach (AudioSource radioAudioSource in radioAudio) {radioAudioSource.playOnAwake = false;}
	}
  
  void OnLevelWasLoaded(int level){
    priorDayRitualSequence = new ArrayList(Globals.ritualSequence);
    Debug.Log("curScene = " + Globals.curScene);
    if(Globals.curScene == Globals.ritualSequence.Count)
    {
      switch (Globals.curScene){
        case 0: Globals.ritualSequence.Add(false); break; //add coffee to ritual list
        case 1: Globals.ritualSequence.Add(false); break; //add wash dishes to ritual list
        case 2: Globals.ritualSequence.Add(false); break; //add play piano to ritual list
        case 3: Globals.ritualSequence.Add(false); break; //add throw away trash to ritual list
      }
    }
    Globals.radioIsOn = true;
    
    audioClipIndex = 0;
    radioAudio = GetComponents<AudioSource>();
    radioAudio[audioClipIndex].Play();
    foreach (AudioSource radioAudioSource in radioAudio) {
      radioAudioSource.playOnAwake = false;
      radioAudioSource.loop = false;
    }
    
    for (int i = 0; i < Globals.ritualSequence.Count; i++) { Globals.ritualSequence[i] = false; }
  }
	
	//Update is called once per frame
	void Update () {
    if (Globals.radioIsOn && !radioAudio[audioClipIndex].isPlaying) {
      /*ordering scheme, on a per scene basis: 
        intro: 0
        good-event-n: 2n+1
        bad-event-n: 2n+2
        conclusion: last thing
      */
      if (radioAudio.Length == 1){
        //;will fix this if i have time
      }
      else if (Globals.curScene == 0){
        if (audioClipIndex+1 < radioAudio.Length){
        radioAudio[audioClipIndex+1].Play();
        audioClipIndex+=1;
      }
      }
      else if (ritualSequenceIndex < Globals.curScene){ //news stories
        if ((bool) (priorDayRitualSequence[ritualSequenceIndex]) == true)
          audioClipIndex=2*ritualSequenceIndex+1; //play good event
        else
          audioClipIndex=2*ritualSequenceIndex+2; //play bad event
        ritualSequenceIndex++;
      }
      else if(ritualSequenceIndex == Globals.curScene){ //conclusion
        audioClipIndex=2*ritualSequenceIndex+1;
      }
      
      radioAudio[audioClipIndex].Play();
    }
    
    if (Globals.radioIsOn) {radioAudio[audioClipIndex].mute = false;}
    else{radioAudio[audioClipIndex].mute = true;}
	}
  
}
