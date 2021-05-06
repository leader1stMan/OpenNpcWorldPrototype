using UnityEngine;
using System.Collections;

public class MeleeAI : CombatBase
{
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
        agent.isStopped = true;
        RotateTo(target);

        if (attackCooldown <= 0)
        {
            stats.GetWeapon().ExecuteAttack(gameObject, target);
            attackCooldown = stats.GetWeapon().Cooldown;
        }

        StopCoroutine("RotateTo");
    }

    void RotateTo(GameObject target)
    {
        Quaternion lookRotation;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / agent.angularSpeed));
    }
}
