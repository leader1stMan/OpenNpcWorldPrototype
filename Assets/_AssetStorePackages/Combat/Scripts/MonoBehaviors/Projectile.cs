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
            Fly();
        else
            if (attachedObject == null)
            Destroy(gameObject);
    }

    private void Fly()
    {
        // Move the arrow in x and y
        float dx = horizontalSpeed * Mathf.Cos(Direction.x) * Time.deltaTime;
        Vector3 XAxis = Vector3.forward;
        Vector3 YAxis = Vector3.up;

        XAxis.y = 0;
        YAxis.x = 0; YAxis.z = 0;

        transform.Translate(XAxis * dx);
        transform.Translate(YAxis * (horizontalSpeed * Mathf.Sin(Direction.x) - GravityFactor) * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Caster)
            return;

        ProjectileCollided?.Invoke(Caster, other.gameObject);

        FixedJoint fj = gameObject.AddComponent(typeof(FixedJoint)) as FixedJoint;
        fj.connectedBody = other.gameObject.GetComponent<Rigidbody>();
        isFlying = false;
        attachedObject = other.gameObject;
        Destroy(GetComponent<CapsuleCollider>());
    }
}
