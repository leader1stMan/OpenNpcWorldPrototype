using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShieldMeleeAI : MeleeAI
{
    float blockTime = 1f;
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

        if (blockTime > 0)
            blockTime -= Time.deltaTime;
    }

    public override void Attack(GameObject target)
    {
        attack = true;
        agent.SetDestination(target.transform.position);

        int chooseMove = Random.Range(1, 10);
        if (CanHit(gameObject, target.transform) && attackCooldown <= 0 && chooseMove <= 5)
        {
            if (!stats.isBlocking)
            {
                base.Attack(target);
            }
        }
        
        if (CanHit(target, transform) && blockTime <= 0 && !stats.isBlocking && chooseMove > 5)
        {
            StartCoroutine(StartBlock());
        }
        else if (stats.isBlocking && blockTime <= 0)
        {
            StartCoroutine(StopBlock());
        }
        
        attack = false;
    }

    IEnumerator StartBlock()
    {
        blockTime = 3;
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
        controller.ChangeAnimation(AnimationController.SHILD_UNEQUIP, AnimatorLayers.UP);
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
