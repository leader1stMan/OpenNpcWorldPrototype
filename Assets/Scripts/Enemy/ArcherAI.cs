using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherAI : MeleeAI
{
    LayerMask layersToIgnore;
    float LaunchHeight = 1f;

    public override void Attack(GameObject target)
    {
        if (attackCooldown <= 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit))
            {
                if (hit.transform.gameObject == target)
                {
                    attackCooldown = stats.GetWeapon().Cooldown;

                    stats.GetWeapon().ExecuteAttack(gameObject,
                                                    gameObject.transform.position + gameObject.transform.forward * 0 + new Vector3(0, LaunchHeight, 0),
                                                    TakeAim(),
                                                    LayerMask.NameToLayer("Enemy Projectile"));
                }
            }
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

    void PickBetterPosition()
    {
        //implemet later
        Debug.Log("Pick");
    }
}
