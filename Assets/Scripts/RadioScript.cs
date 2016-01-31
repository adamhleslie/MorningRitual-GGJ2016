using UnityEngine;
using System.Collections;

public class RadioScript : MonoBehaviour {
  public AudioSource[] radioAudio;
  private int audioClipIndex = 0;
  private int ritualSequenceIndex = 0;
  
    void Awake()
    {
        if (Globals.level >= 1 && Globals.level <= 3)
        {
            for (int i = 0; i < Globals.ritualSequence.Count; i++) { Globals.ritualSequence[i] = false; }
        }
    }

	// Use this for initialization
	void Start () {
    radioAudio = GetComponents<AudioSource>();
        radioAudio[audioClipIndex].Play();
        foreach (AudioSource radioAudioSource in radioAudio)
        {
            radioAudioSource.playOnAwake = false;
            switch (Globals.level)
            {
                case 0: Globals.ritualSequence.Add(false); break; //coffee
                case 1: Globals.ritualSequence.Add(false); break; //wash dishes
                case 2: Globals.ritualSequence.Add(false); break; //play piano
                case 3: Globals.ritualSequence.Add(false); break; //throw away trash
            }
        }

	}
	
	//Update is called once per frame
	void Update () {
    if (Globals.radioIsOn && !radioAudio[audioClipIndex].isPlaying) {
      /*ordering scheme, on a per scene basis: 
        intro: 0
        good-event-n: 2n+1
        bad-event-n: 2n+2
        conclusion: last thing obviously
      */
      if (ritualSequenceIndex < Globals.ritualSequence.Count){ //news stories
        if ((bool) (Globals.ritualSequence[ritualSequenceIndex]) == true)
          audioClipIndex=2*ritualSequenceIndex+1;
        else
          audioClipIndex=2*ritualSequenceIndex+2;
        ritualSequenceIndex++;
      }
      else if(ritualSequenceIndex == Globals.ritualSequence.Count){ //conclusion
        audioClipIndex=2*ritualSequenceIndex+1;
      }
      
      radioAudio[audioClipIndex].Play();
    }
    
    if (Globals.radioIsOn) {
        radioAudio[audioClipIndex].mute = false;
      }
      else{
        radioAudio[audioClipIndex].mute = true;
      }
	}
  
}
