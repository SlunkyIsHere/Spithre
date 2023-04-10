using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private Vector3 startPoint = new Vector3(-2, 0, -8.5f);
    [SerializeField] private Vector3 endPoint = new Vector3(2, 0, -8.5f);
    [SerializeField] private float speed = 1.0f;
    private bool movingToEnd = true;
    
    void Update() {
        if (movingToEnd) {
            transform.position = Vector3.MoveTowards(transform.position, endPoint, speed * Time.deltaTime);
            if (transform.position == endPoint) {
                movingToEnd = false;
            }
        } else {
            transform.position = Vector3.MoveTowards(transform.position, startPoint, speed * Time.deltaTime);
            if (transform.position == startPoint) {
                movingToEnd = true;
            }
        }
    }
}
