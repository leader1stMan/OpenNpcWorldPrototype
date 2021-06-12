using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMeleeAI : MeleeAI
{
    protected override void Start()
    {
        base.Start();
        AttackDistance = CombatRange;

        StartCoroutine(ConnectShield());
    }

    protected override void Update()
    {
        base.Update();
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

        if (changingState)
            return;

        if (CanHit(gameObject, target.transform))
        {
            if (stats.isBlocking)
            {
                StartCoroutine(StopBlock());
            }
            else
            {
                base.Attack(target);
            }
            return;
        }

        if (CanHit(target, transform, 2) && stats.shieldCooldown <= 0 && !target.GetComponent<CharacterStats>().isBlocking)
        {
            if (!stats.isBlocking)
            {
                StartCoroutine(StartBlock());
            }
            return;
        }
        else if (stats.isBlocking)
        {
            StartCoroutine(StopBlock());
        }
    }

    IEnumerator StartBlock()
    {
        changingState = true;
        stats.isBlocking = true;
        agent.speed /= stats.shield.ShieldDeceleration;
        controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP, true);
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    IEnumerator StopBlock()
    {
        changingState = true;
        stats.isBlocking = false;
        agent.speed *= stats.shield.ShieldDeceleration;
        controller.ChangeAnimation(AnimationController.SHILD_UNEQUIP, AnimatorLayers.UP, true);
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    bool CanHit(GameObject attacker, Transform target, float rangeMultiplayer = 1)
    {
        CharacterStats characterStats = attacker.GetComponent<CharacterStats>();
        Weapon weapon = characterStats.weapon;

        if (weapon == null)
        {
            AttackDefinition attack = characterStats.defaultAttack;
            if (attack == null || stats.attackCooldown > 0)
                return false;
            else
                return Vector3.Distance(transform.position, target.position) < attack.Range * rangeMultiplayer;
        }
        else
        {
            if (stats.attackCooldown > 0)
            {
                return false;
            }
            else
                return Vector3.Distance(transform.position, target.position) < weapon.Range * rangeMultiplayer;
        }
    }

    IEnumerator ConnectShield()
    {
        yield return new WaitUntil(() => controller.animator != null);
        //controller.ChangeAnimation(AnimationController.SHIELD_EQUIP, AnimatorLayers.UP, true);
    }
}
