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
        if (CurrentState == EnemyState.Attacking)
            RotateTo(currentTarget.gameObject);
    }

    public override void Attack(GameObject target)
    {
        attack = true;
        if (target == null)
        {
            attack = false;
            return;
        }

        if (agent.enabled)
        {
            agent.isStopped = true;
        }
        RotateTo(target);

        if (attackCooldown <= 0)
        {
            GetComponentInChildren<AnimationController>().target = target;
            controller.ChangeAnimation(AnimationController.SWORD_ATTACK, AnimatorLayers.UP);
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
