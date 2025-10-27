using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public float speed = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Input.GetAxis("Horizontal") * Vector3.right * speed * Time.deltaTime;
        transform.position += Input.GetAxis("Forward") * Vector3.forward * speed * Time.deltaTime;
        transform.position += Input.GetAxis("Vertical") * Vector3.up * speed * Time.deltaTime;
    }
}
