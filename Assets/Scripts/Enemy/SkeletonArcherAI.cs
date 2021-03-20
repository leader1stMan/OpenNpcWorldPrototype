using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonArcherAI : SkeletonAi
{
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
                stats.GetWeapon().ExecuteAttack(gameObject, gameObject.transform.position + gameObject.transform.forward * 3 + new Vector3(0, 0.4f, 0), TakeAim(), LayerMask.NameToLayer("Enemy Projectile"));
                break;
            case WeaponType.LowRange:
                stats.GetWeapon().ExecuteAttack(gameObject, target);
                break;
            default: break;
        }
    }

    Quaternion TakeAim()
    {
        /* The projectile flight is handled by the fly function.
         * According to the function, the angle of the projectile at any moment is : Theta = Theta(initial) + RotationSpeed * time of flight.
         * Also, the horizontal distance covered in any instance is : dx = Vdt . (cos(theta(initial) + rotationSpeed * time of flight), where V is the velocity of the projectile stored in the variable horizontalSpeed.
         * Solving the above equation, we get : theta(initial) = asin((x * RotationSpeed)/V) - RotationSpeed * t)
         * x is the horizontal distance from archer to target. The only unknown now is t (the time it takes the arrow to reach the target).
         * As an approximation, lets say t=x/V
         */
        /*Quaternion AimRotation;

        float x = Vector3.Distance(gameObject.transform.position, currentTarget.gameObject.transform.position);

        AimRotation = gameObject.transform.rotation;
        float AimOffset = (Mathf.Asin(x * 1 / 8) - 1 * x / 8);//* 180 / Mathf.PI;
        if (!float.IsNaN(AimOffset)){
            AimRotation.x += AimOffset;
        }

        Debug.Log(AimRotation);
        return AimRotation;*/
        return gameObject.transform.rotation;
    }
}
