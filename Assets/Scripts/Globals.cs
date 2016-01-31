using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Globals{
    public static bool testingFloor = false;
    public static ArrayList testCollisions = new ArrayList();

    public static bool buttonPressed = false;
    public static int curScene = 0;

    public static bool radioIsOn = true;

    //radio stuff
    public static ArrayList ritualSequence = new ArrayList(); //coffee, dishes, piano, trash
    public static int numScenes = 3;
    public static int currentLevel = 0;
}
