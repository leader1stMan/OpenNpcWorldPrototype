using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFX : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;
    public GameObject blood;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnBlood(float rayRange)
    {
        ray.origin = transform.position;
        ray.direction = transform.forward;

        if(Physics.Raycast(ray, out hit, rayRange))
        {
            if(hit.transform.tag =="Enemy")
            {
                Debug.Log("HitEnemy");
                Instantiate(blood, hit.point, Quaternion.identity);
            }
        }
    }
}
