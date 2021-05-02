using UnityEngine;
using System.Collections;

public class SkeletonAi : EnemyBase
{
    public AnimationController controller;

    GameObject TheTarget;

    protected override void Start()
    {
        controller = GetComponentInChildren<AnimationController>();
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
        
        if (CurrentState != EnemyState.Idle)
        {
            if (CurrentState == EnemyState.Attacking)
            {
                transform.LookAt(currentTarget);
                transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            }
            else
            {
                Vector3 rot = transform.eulerAngles;
                transform.eulerAngles = new Vector3(rot.x, transform.eulerAngles.y, rot.z);
            }
        }

    }

    protected override void ManageStateChange(EnemyState oldState, EnemyState newState)
    {
        base.ManageStateChange(oldState, newState);
        if (newState == EnemyState.Idle)
        {
            controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);
            TheTarget = null;
        }
        else if (newState == EnemyState.Chasing)
        {
            controller.ChangeAnimation(AnimationController.RUN, AnimatorLayers.ALL);
            TheTarget = null;
        }
        else if (newState == EnemyState.Patroling)
        {
            controller.ChangeAnimation(AnimationController.WALK, AnimatorLayers.ALL);
            TheTarget = null;

        }
        else if (newState == EnemyState.Blocking)
        {
          Debug.Log("shield up");
          controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP);
        }
    }

    public override void Attack(GameObject target)
    {
        controller.ChangeAnimation(AnimationController.SWORD_ATTACK, AnimatorLayers.UP);
        //Idk if this is a good way of damaging
        TheTarget = target;
       // 
    }

    public void AttackEvent()
    {
        if (TheTarget != null)
        {
            stats.GetWeapon().ExecuteAttack(gameObject, TheTarget);
        }
    }

    public override void DealDamage()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, stats.GetWeapon().Range, WhatCanThisEnemyAttack);
        if (cols.Length <= 0)
            return;
        foreach (Collider col in cols)
        {
            if (col.transform == this.transform)
                continue;
        }
    }

    public void ShieldHit()
    {
        Debug.Log("anim");
        controller.ChangeAnimation(AnimationController.SHIELD_HIT, AnimatorLayers.UP);
        StartCoroutine(ShieldHitAnim());
    }

    IEnumerator ShieldHitAnim()
    {
        yield return new WaitForSeconds(0.5f);
        controller.ChangeAnimation(AnimationController.SHIELD_READY, AnimatorLayers.UP);
    }
}
