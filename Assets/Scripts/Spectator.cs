using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spectator : MonoBehaviour
{

    [SerializeField]
    private new Camera camera;

    float OFFSET_MOVE = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) == true || Input.GetKey(KeyCode.A) == true)
        {
            camera.transform.position = new Vector3(camera.transform.position.x - OFFSET_MOVE, camera.transform.position.y, camera.transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow) == true || Input.GetKey(KeyCode.D) == true)
        {
            camera.transform.position = new Vector3(camera.transform.position.x + OFFSET_MOVE, camera.transform.position.y, camera.transform.position.z);
        }
        if (Input.GetKey(KeyCode.UpArrow) == true || Input.GetKey(KeyCode.W) == true)
        {
            
        if (Input.GetKey(KeyCode.DownArrow) == true || Input.GetKey(KeyCode.S) == true)
        {
            
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        
            camera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * 6;
        }
    }

}
