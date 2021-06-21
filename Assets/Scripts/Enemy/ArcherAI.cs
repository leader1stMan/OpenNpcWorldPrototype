using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArcherAI : MeleeAI
{
    public float RaysPerMeter = 1;
    public float MinShootingRange;
    public LayerMask layersToIgnore;
    public LayerMask VisionMask;
    float LaunchHeight = 1f;
    public float CalculatingStep;
    bool PickingPosition = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Attack(GameObject target)
    {
        if (target == null)
            return;
        Vector3 launchPosition = transform.position + new Vector3(0, LaunchHeight);
        var hits = Physics.SphereCastAll(launchPosition, 0.1f, target.transform.position - transform.position - new Vector3(0, LaunchHeight), VisionRange, VisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, LaunchHeight), target.transform.position - transform.position - new Vector3(0, LaunchHeight), Color.green);

        if (hits.Length == 0)
            return;
        RaycastHit hit = hits[0];
        foreach (var rayHit in hits)
        {
            if (rayHit.transform == transform)
                continue;

            if (rayHit.transform.IsChildOf(transform))
                continue;

            if (hit.distance > rayHit.distance)
                hit = rayHit;
        }

        if (hits.Length > 0 && hit.transform.gameObject == target)
        {
            RotateTo(target);

            Transform temp = transform;
            temp.LookAt(target.transform);
            if (temp != transform)
                return;
            if (attackCooldown <= 0)
            {
                stats.GetWeapon().ExecuteAttack(gameObject,
                                                gameObject.transform.position + gameObject.transform.forward * 0 + new Vector3(0, LaunchHeight, 0),
                                                TakeAim(),
                                                LayerMask.NameToLayer("Enemy Projectile"));

                attackCooldown = stats.GetWeapon().Cooldown;
            }
        }
        else
        {
            if (!PickingPosition)
                PickBetterPosition(target);
        }
    }


    Quaternion TakeAim()
    {
        Quaternion AimRotation = gameObject.transform.rotation;

        // Get target position and rebase to make archer the origin
        Vector3 temp1 = currentTarget.transform.position;
        Vector3 temp2 = transform.position;
        temp1.y = 0;
        temp2.y = 0;

        float x = Vector3.Distance(temp1, temp2);
        float y = currentTarget.transform.position.y - (transform.position.y + LaunchHeight);

        // Getting weapon values
        float gravityFactor = stats.GetWeapon().ProjectileToFire.GravityFactor;
        float speed = stats.GetWeapon().ProjectileSpeed;
        float f1 = speed * speed;
        float f2 = Mathf.Sqrt((f1 * f1) - gravityFactor * (gravityFactor * (x * x) + 2 * y * f1));
        float AimOffset1 = Mathf.Atan((f1 + f2) / (gravityFactor * x));
        float AimOffset2 = Mathf.Atan((f1 - f2) / (gravityFactor * x));

        float AimOffset = Mathf.Abs(AimOffset1) > Mathf.Abs(AimOffset2) ? AimOffset2 : AimOffset1;
        AimOffset = Mathf.Asin(gravityFactor * x / (speed * speed)) / 2;

        AimRotation *= Quaternion.AngleAxis(AimOffset, transform.forward);

        return AimRotation;
    }

    void PickBetterPosition(GameObject target)
    {
        if (transform.position == target.transform.position)
            return;

        PickingPosition = true;
        Vector3 direction;
        float radius = CombatRange;
        Vector3 localPosition = transform.position - target.transform.position;

        float angleX = 0;
        float angleY = 0;
        float angleBetween = 0;
        float startAngleY = 0;
        float rotationAngle = Mathf.Acos(localPosition.normalized.y);

        Vector3 axisVector = Vector3.Cross(Vector3.up, localPosition).normalized;
        if (RotateVector(Vector3.up, axisVector, rotationAngle) != localPosition)
        {
            rotationAngle = Mathf.PI * 2.0f - rotationAngle;
        }

        List<DirectionInfo> infos = new List<DirectionInfo>();
        int count = 0;
        int limit = (int)(Math.PI * 2.0 * radius * RaysPerMeter);
        for (int i = 0; i < limit; i++)
        {
            double segmentHeight = radius * (1 - Math.Cos(angleY - startAngleY));
            double segmentRadius = Math.Sqrt(segmentHeight * (2.0 * radius - segmentHeight));

            int iterations = (int)(Math.PI * segmentRadius * 2.0 * RaysPerMeter);
            if (iterations < 1)
                iterations = 1;

            for (int index = 0; index < iterations; index++)
            {
                count++;

                Vector3 SphereDirection = new Vector3(Mathf.Cos(angleX) * Mathf.Sin(angleY), Mathf.Cos(angleY), Mathf.Sin(angleX) * Mathf.Sin(angleY));
                direction = RotateVector(SphereDirection, axisVector, rotationAngle);

                angleBetween = Vector3.AngleBetween(direction, localPosition);

                angleX += index * Mathf.Pow(-1.0f, index) * 2.0f * Mathf.PI / iterations;
                if (!Physics.SphereCast(target.transform.position, 0.1f, direction, out RaycastHit hit, radius, ~layersToIgnore))
                {
                    float length = CheckDirection(radius, direction, out Vector3 position);
                    if (length == radius)
                    {
                        agent.SetDestination(position);
                        PickingPosition = false;
                        return;
                    }
                    else if (length > radius)
                        continue;
                    else
                        infos.Add(new DirectionInfo(direction, length, position));
                }
                else
                {
                    float length = CheckDirection(hit.distance, direction, out Vector3 position);
                    if (length > radius)
                        continue;
                    infos.Add(new DirectionInfo(direction, length, position));
                }
            }
            angleY += Mathf.PI / limit;
        }

        for (int ind = 0; ind < infos.Count; ind++)
        {
            var info = infos[ind];
            float raysLength = radius * Mathf.Cos(angleBetween);
            if (info.distance > raysLength)
            {
                agent.SetDestination(info.hitPosition);
                PickingPosition = false;
                return;
            }
        }

        PickingPosition = false;
    }

    struct DirectionInfo
    {
        public Vector3 direction;
        public float distance;
        public Vector3 hitPosition;

        public DirectionInfo(Vector3 direction, float distance, Vector3 hitPosition)
        {
            this.direction = direction;
            this.distance = distance;
            this.hitPosition = hitPosition;
        }
    }

    Vector3 RotateVector(Vector3 vectorToRotate, Vector3 axis, float angle)
    {
        return vectorToRotate * Mathf.Cos(angle) + Vector3.Cross(vectorToRotate, axis) * Mathf.Sin(angle) + Vector3.Dot(vectorToRotate, axis) * axis * (1 - Mathf.Cos(angle));
    }

    float CheckDirection(float radius, Vector3 direction, out Vector3 hitPosition)
    {
        float rayLength = radius;
        Vector3 destination;
        while (rayLength > MinShootingRange)
        {
            destination = currentTarget.transform.position + direction * rayLength + new Vector3(0, -(agent.height - LaunchHeight));
            if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 0.08f, agent.areaMask))
            {
                hitPosition = navHit.position;
                return rayLength;
            }
            rayLength -= CalculatingStep;
        }
        hitPosition = new Vector3();
        return float.MaxValue;
    }
}
