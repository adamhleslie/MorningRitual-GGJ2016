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
    public static int numScenes = 3;
    
    //called when a level is loaded
    void OnLevelWasLoaded(int level) {
      switch (level){
        case 0: ritualSequence.Add(false); break; //coffee
        case 1: ritualSequence.Add(false); break; //wash dishes
        case 2: ritualSequence.Add(false); break; //play piano
        case 3: ritualSequence.Add(false); break; //throw away trash
      }
      
      if(level>=1 && level <=3){
        for (int i = 0; i< ritualSequence.Count; i++){ritualSequence[i]=false;}
      }
    }
}
