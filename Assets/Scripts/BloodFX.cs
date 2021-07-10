using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFX : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    public GameObject blood;
    public GameObject decalBlood;

    public void SpawnBlood(float rayRange)
    {
        ray.origin = transform.position;
        ray.direction = transform.forward;

        if(Physics.Raycast(ray, out hit, rayRange))
        {
            if(hit.transform.tag == "Npc")
            {
                Instantiate(blood, hit.point, Quaternion.identity);
            }
        }

        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            Vector3 vector3 = hit.point;
            vector3.y = hit.point.y + 0.5f;
            Instantiate(decalBlood, vector3, Quaternion.Euler(90, 0, 0));
        }
    }
}
