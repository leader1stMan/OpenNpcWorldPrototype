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
                TakeAim();
                stats.GetWeapon().ExecuteAttack(gameObject, gameObject.transform.position + gameObject.transform.forward * 3 + new Vector3(0, 0.4f, 0), gameObject.transform.rotation, LayerMask.NameToLayer("Player Projectile"));
                break;
            case WeaponType.LowRange:
                stats.GetWeapon().ExecuteAttack(gameObject, target);
                break;
            default: break;
        }
    }

    void TakeAim()
    {
        transform.rotation = Quaternion.LookRotation(currentTarget.position - transform.position);
    }
}
