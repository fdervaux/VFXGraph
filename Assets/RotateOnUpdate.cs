using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnUpdate : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private Vector3 axis = Vector3.up;


    private void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
