using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonArcherAI : SkeletonAi
{

    float LaunchHeight = 1f;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Attack(GameObject target)
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);

        switch (stats.GetWeapon().type)
        {
            case WeaponType.LongRange:
                stats.GetWeapon().ExecuteAttack(gameObject, gameObject.transform.position + gameObject.transform.forward * 0 + new Vector3(0, LaunchHeight, 0), TakeAim(), LayerMask.NameToLayer("Enemy Projectile"));
                break;
            case WeaponType.LowRange:
                stats.GetWeapon().ExecuteAttack(gameObject, target);
                break;
            default: break;
        }
    }

    Quaternion TakeAim()
    {
        Quaternion AimRotation;

        AimRotation = gameObject.transform.rotation;

        // Get target position and rebase to make archer the origin
        float x;
        Vector3 temp1 = currentTarget.transform.position;
        Vector3 temp2 = transform.position;
        temp1.y = 0;
        temp2.y = 0;
        x = Vector3.Distance(temp1, temp2);
        float y = currentTarget.transform.position.y - (transform.position.y + LaunchHeight);

        // Getting weapon values
        //float g = stats.GetWeapon().ProjectileToFire.GravityFactor; ERROR
        float v = stats.GetWeapon().ProjectileSpeed;
        float f1 = v * v;
        //   float f2 = Mathf.Sqrt((f1 * f1) - g * (g*(x * x) + 2 * y * f1));
        // float AimOffset1 = Mathf.Atan((f1 + f2) / (g * x));
        // float AimOffset2 = Mathf.Atan((f1 - f2) / (g * x));

        // float AimOffset = Mathf.Abs(AimOffset1) > Mathf.Abs(AimOffset2) ? AimOffset2 : AimOffset1;
        //AimOffset = Mathf.Asin(g * x / (v * v)) / 2;

        // AimRotation *= Quaternion.AngleAxis(AimOffset, transform.forward);

        //  return AimRotation;
        return Quaternion.identity;
    }
}
