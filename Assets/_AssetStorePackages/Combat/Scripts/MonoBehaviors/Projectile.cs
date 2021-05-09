using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject Caster;
    private float horizontalSpeed;
    private Quaternion Direction;

    public float rotationSpeed = 1f;
    public float GravityFactor = 3f;
    public bool isFlying;
    private GameObject attachedObject;
    public event Action<GameObject, GameObject> ProjectileCollided;
    public void Fire(GameObject caster, Quaternion target, float speed, float range)
    {
        Caster = caster;
        horizontalSpeed = speed;

        Direction = target;
        transform.rotation = Direction;

        isFlying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFlying)
            fly();
        else
            if (attachedObject == null)
            Destroy(gameObject);
    }

    private void fly()
    {
        /*float distanceToTravel = horizontalSpeed * Time.deltaTime;

        transform.Translate(Vector3.forward * distanceToTravel);

        float step = 0f;
        step += rotationSpeed * Time.deltaTime;
        transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);

        distanceTraveled += distanceToTravel;

        if (distanceTraveled > Range)
        {
            Destroy(gameObject);
        }*/

        // Move the arrow in x and y
        Vector3 PrevPosition = transform.position;
        float dx = horizontalSpeed * Mathf.Cos(Direction.x) * Time.deltaTime;
        Vector3 XAxis = Vector3.forward;
        Vector3 YAxis = Vector3.up;

        XAxis.y = 0;
        YAxis.x = 0; YAxis.z = 0;

        transform.Translate(XAxis * dx);
        transform.Translate(YAxis * (horizontalSpeed * Mathf.Sin(Direction.x) - GravityFactor) * Time.deltaTime);

        // Rotate the arrow to face the direction of motion
        //transform.rotation = new Quaternion(Vector3.Angle(transform.forward, transform.position - PrevPosition), 0, 0, 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Caster || other.gameObject.transform.root.gameObject == Caster)
        {
            return;
        }

        if (other.gameObject.layer == 1)
        {
            return;
        }

        if (ProjectileCollided != null)
        {
            ProjectileCollided(Caster, other.gameObject);
        }
        FixedJoint fj = gameObject.AddComponent(typeof(FixedJoint)) as FixedJoint;
        fj.connectedBody = other.gameObject.GetComponent<Rigidbody>();
        isFlying = false;
        attachedObject = other.gameObject;
        Destroy(GetComponent<CapsuleCollider>());
        Debug.Log(false);
    }
}
