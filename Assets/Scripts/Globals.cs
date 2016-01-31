using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Globals{
    public static bool testingFloor = false;
    public static ArrayList testCollisions = new ArrayList();

    public static bool buttonPressed = false;
    public static int nextScene = 1;
    public static float ritualValue = 0;

    public static bool radioIsOn = false;

    //radio stuff
    public static ArrayList ritualSequence = new ArrayList();
    //coffee, wash dishes, play piano, throw away trash
    public static int numScenes = 3;
    
    //called when a level is loaded
    void OnLevelWasLoaded(int level) {
      switch (level){
        case 0: ritualSequence.Add(false); break;
        case 1: ritualSequence.Add(false); break;
        case 2: ritualSequence.Add(false);  break;
        case 3: ritualSequence.Add(false);  break;
      }
      
      if(level>=1 && level <=3){
        for (int i = 0; i< ritualSequence.Count; i++){ritualSequence[i]=false;}
      }
    }
}
