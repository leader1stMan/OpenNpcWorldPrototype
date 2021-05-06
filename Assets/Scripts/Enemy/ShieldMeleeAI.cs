using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMeleeAI : MeleeAI
{
    float blockCooldown;
    bool changingState;
    public float CombatRange;

    protected override void Start()
    {
        base.Start();
        AttackDistance = CombatRange;
    }

    protected override void Update()
    {
        base.Update();

        if (blockCooldown > 0)
            blockCooldown -= Time.deltaTime;
    }

    protected override void ManageStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState == EnemyState.Attacking && stats.isBlocking)
            StartCoroutine(StopBlock());
        base.ManageStateChange(oldState, newState);
    }
    public override void Attack(GameObject target)
    {
        agent.SetDestination(target.transform.position);
        if (CanHit(gameObject, target.transform))
        {
            if (stats.isBlocking)
                StartCoroutine(StopBlock());
            base.Attack(target);
            return;
        }

        if (CanHit(target, transform) && blockCooldown <= 0)
        {
            if (!stats.isBlocking)
                StartCoroutine(StartBlock());
            return;
        }
        else if (stats.isBlocking)
            StartCoroutine(StopBlock());
    }

    IEnumerator StartBlock()
    {
        print("block");
        controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP, true);
        changingState = true;
        stats.isBlocking = true;
        agent.speed /= stats.shield.ShieldDeceleration;
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    IEnumerator StopBlock()
    {
        print("stop block");
        controller.ChangeAnimation(AnimationController.SHILD_UNEQUIP, AnimatorLayers.UP, true);
        changingState = true;
        stats.isBlocking = false;
        agent.speed *= stats.shield.ShieldDeceleration;
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    bool CanHit(GameObject attacker, Transform target)
    {
        CharacterStats characterStats = attacker.GetComponent<CharacterStats>();
        Weapon weapon = characterStats.weapon;

        if (weapon == null)
        {
            AttackDefinition attack = characterStats.defaultAttack;
            if (attack == null || stats.attackCooldown > 0)
                return false;
            else
                return Vector3.Distance(transform.position, target.position) < attack.Range;
        }
        else
        {
            if (stats.attackCooldown > 0)
            {
                return false;
            }
            else
                return Vector3.Distance(transform.position, target.position) < weapon.Range;
        }
    }
}
