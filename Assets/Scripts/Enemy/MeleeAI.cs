using UnityEngine;
using System.Collections;

public class MeleeAI : CombatBase
{
    public float CombatRange;

    protected bool changingState;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(ConnectWeapon());
    }

    protected override void Update()
    {
        base.Update();
        
        if (CurrentState == EnemyState.Attacking)
            RotateTo(currentTarget.gameObject);
    }

    public override void Attack(GameObject target)
    {
        if (target == null || changingState)
            return;

        //RotateTo(target);

        if (attackCooldown <= 0)
        {
            StartCoroutine(AttackState(target));
        }
    }

    IEnumerator AttackState(GameObject target)
    {
        changingState = true;

        controller.ChangeAnimation(AnimationController.SWORD_ATTACK, AnimatorLayers.UP, true);
        stats.GetWeapon().ExecuteAttack(gameObject, target);
        attackCooldown = controller.GetAnimationLength(AnimatorLayers.UP);

        yield return new WaitForSeconds(attackCooldown);

        changingState = false;
    }

    void RotateTo(GameObject target)
    {
        Quaternion lookRotation;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / agent.angularSpeed));
    }

    IEnumerator ConnectWeapon()
    {
        yield return new WaitUntil(() => controller.animator != null);
        controller.ChangeAnimation(AnimationController.SWORD_EQUIP, AnimatorLayers.UP, true);
    }
}
