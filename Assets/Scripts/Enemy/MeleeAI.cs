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
        if (CurrentState == EnemyState.Attacking && !agent.enabled)
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

        if (attackCooldown <= 0)
        {
            GetComponentInChildren<AnimationController>().target = target;
            controller.ChangeAnimation(AnimationController.SWORD_ATTACK, AnimatorLayers.UP);
            attackCooldown = stats.GetWeapon().Cooldown;
        }

        attack = false;
        return;
    }

    void RotateTo(GameObject target)
    {
        Vector3 targetTransform = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        transform.LookAt(targetTransform);
    }
}
