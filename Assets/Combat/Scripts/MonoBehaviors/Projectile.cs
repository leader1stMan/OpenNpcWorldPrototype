using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject Caster;
    private float horizontalSpeed;
    private float Range;
    private Quaternion Direction;

    private float distanceTraveled;
    private float Vx;
    private float Vy;

    public float rotationSpeed = 1f;
    public float GravityFactor = 3f;
    public bool isFlying;
    private GameObject attachedObject;
    public event Action<GameObject, GameObject> ProjectileCollided;
    public void Fire(GameObject caster, Quaternion target, float speed, float range)
    {
        Caster = caster;
        horizontalSpeed = speed;
        Range = range;

        Direction = target;
        transform.rotation = Direction;

        distanceTraveled = 0f;
        isFlying = true;
        Vx = horizontalSpeed * Mathf.Cos(target.x);
        Vy = horizontalSpeed * Mathf.Sin(target.x);
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
        transform.Translate(Vector3.forward * dx);
        transform.Translate(Vector3.up * (horizontalSpeed * Mathf.Sin(Direction.x) - GravityFactor)* Time.deltaTime);

        // Rotate the arrow to face the direction of motion
        //transform.rotation = new Quaternion(Vector3.Angle(transform.forward, transform.position - PrevPosition), 0, 0, 1);

        // Check if arrow is out of range
        distanceTraveled += dx;

        if (distanceTraveled > Range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Caster)
            return;
        if (ProjectileCollided != null)
        {
            ProjectileCollided(Caster, other.gameObject);
        }

        FixedJoint fj = gameObject.AddComponent(typeof(FixedJoint)) as FixedJoint;
        fj.connectedBody = other.gameObject.GetComponent<Rigidbody>();
        isFlying = false;
        attachedObject = other.gameObject;
        Destroy(GetComponent<CapsuleCollider>());
    }
}
