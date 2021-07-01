using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShieldMeleeAI : MeleeAI
{
    public float blockTime;
    public bool changingState;

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

    protected override void MoveAnimaton()
    {
        if (blockTime <= 0)
            base.MoveAnimaton();
    }

    public override void Attack(GameObject target)
    {
        attack = true;
        if (agent.enabled)
            agent.SetDestination(target.transform.position);

        int chooseMove = Random.Range(1, 10);
        if (CanHit(gameObject, target.transform) && attackCooldown <= 0 && chooseMove <= 5)
        {
            if (!stats.isBlocking)
            {
                base.Attack(target);
                return;
            }
        }
        
        if (attackCooldown <= 0)
        {
            if (CanHit(target, transform) && blockTime <= 0 && !stats.isBlocking && chooseMove > 5)
            {
                StartBlock();
            }
            else if (stats.isBlocking && blockTime <= 0)
            {
                StartCoroutine(StopBlock());
            }
        }

        attack = false;
    }

    void StartBlock()
    {
        blockTime = 3;
        print("block");
        controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP, true);
    }

    IEnumerator StopBlock()
    {
        print("stop block");
        controller.ChangeAnimation(AnimationController.SHILD_UNEQUIP, AnimatorLayers.UP);
        changingState = true;
        stats.isBlocking = false;
        yield return new WaitForSeconds(controller.GetAnimationLength(AnimatorLayers.UP));
        changingState = false;
    }

    bool CanHit(GameObject attacker, Transform target)
    {
        CharacterStats characterStats = attacker.GetComponent<CharacterStats>();
        Weapon weapon = characterStats.weapon.weapon;

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
