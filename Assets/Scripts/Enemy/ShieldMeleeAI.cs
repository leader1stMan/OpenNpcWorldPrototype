using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public override void Attack(GameObject target)
    {
        if (agent.enabled)
            agent.SetDestination(target.transform.position);

        if (changingState)
            return;

        Debug.Log(true);
        int random = Random.Range(0, 5);
        if (CanHit(gameObject, target.transform) && random == 0)
        {
            if (stats.isBlocking)
            {
                StartCoroutine(StopBlock());
                return;
            }
            else
            {
                base.Attack(target);
            }
        }

        if (CanHit(target, transform) && stats.shieldCooldown <= 0 && !target.GetComponent<CharacterStats>().isBlocking)
        {
            if (!stats.isBlocking)
            {
                StartCoroutine(StartBlock());
                return;
            }
        }
        else if (stats.isBlocking)
        {
            StartCoroutine(StopBlock());
            return;
        }
    }

    IEnumerator StartBlock()
    {
        Debug.Log("isBlocking");
        changingState = true;
        stats.isBlocking = true;
        agent.speed /= stats.shield.ShieldDeceleration;
        controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP, true);
        Debug.Log(controller.GetAnimationLength(AnimatorLayers.UP));
        Debug.Log(controller.animator.GetCurrentAnimatorStateInfo(1).ToString());
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    IEnumerator StopBlock()
    {
        Debug.Log("not");
        changingState = true;
        stats.isBlocking = false;
        agent.speed *= stats.shield.ShieldDeceleration;
        controller.ChangeAnimation(AnimationController.SHILD_UNEQUIP, AnimatorLayers.UP, true);
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.UP);
        changingState = false;
    }

    bool CanHit(GameObject attacker, Transform target, float rangeMultiplayer = 1)
    {
        CharacterStats characterStats = attacker.GetComponent<CharacterStats>();
        Weapon weapon = characterStats.weapon.weapon;

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
