using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{

    public Transform doorPivot;
    public float openAngle = 80.0f;
    public float speed = 2f;

    private bool isOpen;
    private Quaternion closedRotation;

    void Awake()
    {
        Debug.Log("Initialization");
        Init();
    }
     private void Init()
    {
        isOpen = false;
        closedRotation = doorPivot.rotation;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isOpen)
        {
            Debug.Log("Player Entered Door");
            Vector3 Direction = other.transform.position - doorPivot.position;
            Direction.y = 0;
            float sign = Vector3.SignedAngle(Direction, doorPivot.right, Vector3.up);
            float turnAngle = openAngle;
            if (sign > 0) { turnAngle = -openAngle; }
            Quaternion targetRotation = closedRotation * Quaternion.Euler(0, turnAngle, 0);
            StartCoroutine(RotateDoor(targetRotation, other.transform));
        }
    }
   
    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("Triggered by object: " + other.name + " | Tag: " + other.tag);
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Player detected!"); // this is your if block
    //    }
    //}
   
    
    private IEnumerator RotateDoor(Quaternion targetRotation, Transform Player)
    {
        while (Quaternion.Angle(doorPivot.rotation, targetRotation) > 0.01f)
        {
            doorPivot.rotation = Quaternion.Slerp(
                doorPivot.rotation,
                targetRotation,
                speed * Time.deltaTime
            );
            yield return null; 
        }
        doorPivot.rotation = targetRotation; // snap exactly to target
        isOpen = true;
    }
}
