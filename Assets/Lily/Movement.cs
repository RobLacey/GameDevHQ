using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] [Range(1f,10f)] float _speed;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(_speed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(_speed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0, _speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= new Vector3(0, 0, _speed * Time.deltaTime);
        }
    }
}
