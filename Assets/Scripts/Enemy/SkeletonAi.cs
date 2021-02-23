using UnityEngine;

public class SkeletonAi : EnemyBase
{
    public Animator anim;
    
    protected override void Update()
    {
        base.Update();
        
        if (CurrentState != EnemyState.Idle)
        {           
            Vector3 rot = transform.eulerAngles;
            transform.LookAt(agent.steeringTarget);
            transform.eulerAngles = new Vector3(rot.x, transform.eulerAngles.y, rot.z);            
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

        }
        else if (newState == EnemyState.Chasing)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);

        }
        else if (newState == EnemyState.Patroling)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", true);
            anim.SetBool("isAttacking", false);

        }
    }
    public override void Attack(GameObject target)
    {


        anim.SetBool("isIdle", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);
        //Idk if this is a good way of damaging
        stats.GetWeapon().ExecuteAttack(gameObject, target);
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
