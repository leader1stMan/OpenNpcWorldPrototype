﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon.asset", menuName = "Attack/Weapon")]
public class Weapon : AttackDefinition
{
    public Rigidbody weaponPref;
    public WeaponType type;

    public Projectile ProjectileToFire;
    public float ProjectileSpeed;

    public void ExecuteAttack(GameObject attacker, Vector3 startSpot, Quaternion target, int layer)
    {
        Projectile projectile = Instantiate(ProjectileToFire, startSpot, target);
        projectile.Fire(attacker, target, ProjectileSpeed, Range);

        projectile.gameObject.layer = layer;
        
        projectile.ProjectileCollided += OnProjectileCollided;
    }

    private void OnProjectileCollided(GameObject attacker, GameObject defender)
    {
        if (attacker == null || defender == null)
            return;

        var attackerStats = attacker.GetComponent<CharacterStats>();
        var defenderStats = defender.GetComponent<CharacterStats>();

        if (defenderStats == null)
            return;

        if (defenderStats.isBlocking)
        {
            Debug.Log("block defender stat");
            return;
        }

        var attack = CreateAttack(attackerStats, defenderStats);

        var attackables = defender.GetComponentsInChildren(typeof(IAttackable));

        foreach (IAttackable attackable in attackables)
        {
            attackable.OnAttack(attacker, attack);
        }
    }

    public virtual void ExecuteAttack(GameObject attacker, GameObject defender)
    {
        if (defender == null)
            return;
        if (Vector3.Distance(attacker.transform.position, defender.transform.position) > Range)
        {
            Debug.Log("out of range");

            return;
        }
      
        /*
        if (!attacker.transform.IsFacingTarget(defender.transform))
            return;
        */
        var attackerStats = attacker.GetComponent<CharacterStats>();
        var defenderStats = defender.GetComponent<CharacterStats>();
        bool debug = defender.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(1).IsName("Shield_0M_L_Ready_0");
        if (defenderStats.isBlocking)
        {
            Debug.Log("block" + debug);
            if (!debug)
                Time.timeScale = 0;
            return;
        }

        var attack = CreateAttack(attackerStats, defenderStats);

        var attackables = defender.GetComponentsInChildren(typeof(IAttackable));

        foreach (IAttackable attackable in attackables)
        {
            attackable.OnAttack(attacker, attack);
        }
    }
}
public enum WeaponType { LongRange, LowRange}
