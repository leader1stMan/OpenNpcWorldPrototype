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
    float LaunchHeight = 1f;
    bool PickingPosition = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        Debug.DrawLine(transform.position, agent.destination, Color.red);
    }

    public override void Attack(GameObject target)
    {
        if (target == null || changingState)
            return;
        Vector3 launchPosition = transform.position + new Vector3(0, LaunchHeight);
        var hits = Physics.SphereCastAll(launchPosition, 0.1f, target.transform.position - transform.position - new Vector3(0, LaunchHeight), VisionRange, VisionMask);
        Debug.DrawRay(transform.position + new Vector3(0, LaunchHeight), target.transform.position - transform.position - new Vector3(0, LaunchHeight), Color.green, 1f);

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
        float f2 = Mathf.Sqrt((f1 * f1) - gravityFactor * (gravityFactor*(x * x) + 2 * y * f1));
        float AimOffset1 = Mathf.Atan((f1 + f2) / (gravityFactor * x));
        float AimOffset2 = Mathf.Atan((f1 - f2) / (gravityFactor * x));

        float AimOffset = Mathf.Abs(AimOffset1) > Mathf.Abs(AimOffset2) ? AimOffset2 : AimOffset1;
        AimOffset = Mathf.Asin(gravityFactor * x / (speed * speed)) / 2;

        AimRotation *= Quaternion.AngleAxis(AimOffset, transform.forward);

        return AimRotation;
    }

    void PickBetterPosition(GameObject target)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        if (transform.position == target.transform.position)
            return;

        PickingPosition = true;

        List<Vector3> possiblePositions = new List<Vector3>();
        Vector3 direction;
        float radius = CombatRange;
        Vector3 localPosition = target.transform.position - gameObject.transform.position;

        float angleX = 0;
        float angleY = 0;
        float startAngleY = 0;
        float rotationAngle = Mathf.Acos(localPosition.normalized.y);

        Vector3 axisVector = Vector3.Cross(Vector3.up, localPosition).normalized;
        if (RotateVector(Vector3.up, axisVector, rotationAngle) != localPosition)
        {
            rotationAngle = Mathf.PI * 2.0f - rotationAngle;
        }

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
                angleX += index * Mathf.Pow(-1.0f, index) * 2.0f * Mathf.PI / iterations;

                Vector3 SphereDirection = new Vector3(Mathf.Cos(angleX) * Mathf.Sin(angleY), Mathf.Cos(angleY), Mathf.Sin(angleX) * Mathf.Sin(angleY));
                direction = RotateVector(SphereDirection, axisVector, rotationAngle);
                if (!Physics.SphereCast(target.transform.position, 0.1f, direction, out RaycastHit hit, radius, layersToIgnore))
                {
                    Vector3 destination = target.transform.position + direction * radius + new Vector3(0, -(agent.height - LaunchHeight));
                    if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 0.1f, agent.areaMask))
                    {
                        Debug.DrawLine(destination, navHit.position, Color.blue, 10f);
                        agent.SetDestination(navHit.position);
                        watch.Stop();
                        PickingPosition = true;
                        return;
                    }
                }
                else
                {
                    possiblePositions.Add(direction);
                }
                count++;
            }
            angleY += Mathf.PI / limit;
        }
        PickingPosition = false;

        watch.Stop();
        //print(watch.Elapsed + "     " + count);
    }

    Vector3 RotateVector(Vector3 vectorToRotate, Vector3 axis, float angle)
    {
        return vectorToRotate * Mathf.Cos(angle) + Vector3.Cross(vectorToRotate, axis) * Mathf.Sin(angle) + Vector3.Dot(vectorToRotate, axis) * axis * (1 - Mathf.Cos(angle));
    }
}
