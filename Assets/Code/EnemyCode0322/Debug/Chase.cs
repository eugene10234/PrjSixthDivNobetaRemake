using Nobeta.Sam.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using VContainer;

public class Chase : MonoBehaviour
{
    //public Transform target;
    //public float velocity;
    //List<Vector3> route;
    //int currentIndex;
    //bool isLockedOn;
    //bool isReScanning;
    //public IPathFindingStrat _pfs;
    //[Inject]
    //public void Construct(IPathFindingStrat pfs)
    //{ 
    //    _pfs = pfs;
    //}
    //void Start()
    //{
    //    route = _pfs.CalculatePath(transform.position, target.position);
    //    currentIndex = 0;
    //    isLockedOn = false;
    //}

    //private void Update()
    //{
    //    Pursue();
    //}

    //void CalibrateRoute()
    //{
    //    isReScanning = true;
    //    route = _pfs.CalculatePath(transform.position, target.position);
    //    currentIndex = 0;
    //    isLockedOn = false;
    //}

    //IEnumerator ReScanning()
    //{
    //    if (!isReScanning)
    //    { 
    //        CalibrateRoute();
    //        isReScanning = false;
    //        yield return new WaitForSeconds(3.0f);
    //    }
    //}
    //void Pursue()
    //{
        
    //    LayerMask mask = LayerMask.GetMask("Player", "Obstacle");

    //    Physics.SphereCast(transform.position, 5f, transform.forward, out RaycastHit hit, 15f, mask);

    //    if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
    //    {
    //        isLockedOn = true;
    //    }
    //    if (isLockedOn == true)
    //    { 
    //        Vector3 direction = (target.position - transform.position).normalized;
    //        transform.position += direction*velocity * Time.deltaTime;
    //        if (Vector3.Distance(transform.position, target.position) < 3f)
    //        {
    //            velocity = 0f;
    //            //Debug.Log("Target Reached");
    //        }
    //    }
    //    else if (isLockedOn == false)
    //    {
    //        if (route == null || currentIndex >= route.Count) return;

    //        Vector3 point = route[currentIndex];

    //        Vector3 direction = (point - transform.position).normalized;

    //        transform.rotation = Quaternion.LookRotation(direction);

    //        transform.position = Vector3.MoveTowards(
    //            transform.position,
    //            point,
    //            velocity * Time.deltaTime
    //        );

    //        if (Vector3.Distance(transform.position, point) < 0.1f)
    //        {
    //            currentIndex++;
    //        }
    //    }
    //}
}
