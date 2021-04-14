using UnityEngine;

public class SkeletonAi : EnemyBase
{
    public Animator anim;
    GameObject TheTarget;
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
            anim.SetBool("isIdle", true);
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isBlocking", false);
            TheTarget = null;
        }
        else if (newState == EnemyState.Chasing)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isBlocking", false);
            TheTarget = null;
        }
        else if (newState == EnemyState.Patroling)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", true);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isBlocking", false);
            TheTarget = null;

        }
        else if (newState == EnemyState.Blocking)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isBlocking", true);
          //  print("isblocking");

        }

    }
    public override void Attack(GameObject target)
    {

        anim.SetBool("isBlocking", false);
        anim.SetBool("isIdle", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);
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
}
