using UnityEngine;
using System.Collections;

public class swimInCircle : MonoBehaviour {

    public float centerX;
    public float centerZ;
    public float width = 1;
    public float depth = 2;

    private float totalTime;

	// Use this for initialization
	void Start () {
        centerX = transform.position.x;
        centerZ = transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {
        totalTime += Time.deltaTime;
        transform.position = new Vector3(depth * Mathf.Cos(totalTime) + centerX, transform.position.y, width * Mathf.Sin(totalTime) + centerZ);
        float rotation = Mathf.Rad2Deg * Time.deltaTime;
        transform.Rotate(0, 0, rotation);
	}
}
