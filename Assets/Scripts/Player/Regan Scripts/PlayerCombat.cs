using System.Collections; 
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{    
    public Animator anim;
    public AttackDefinition Attack;
    public CharacterStats stats;
    public float attackCooldown = 0f;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;
    }
    public void AttackTarget(GameObject target)
    {
        if (attackCooldown <= 0)
        {
            if (stats.GetWeapon() != null)
            {
                if (stats.GetWeapon().type == WeaponType.LongRange)
                {
                    stats.GetWeapon().ExecuteAttack(gameObject, gameObject.transform.position + new Vector3(0, 0.4f, 0), gameObject.transform.rotation, LayerMask.NameToLayer("Player Projectile"));
                }
                else
                {
                    stats.GetWeapon().ExecuteAttack(gameObject, target);
                }
            }
            else
            {
                var attack = Attack.CreateAttack(stats, target.GetComponent<CharacterStats>());

                var attackables = target.GetComponentsInChildren(typeof(IAttackable));

                foreach (IAttackable attackable in attackables)
                {
                    attackable.OnAttack(gameObject, attack);
                }
            }
            if (stats.GetWeapon() != null)
                attackCooldown = stats.GetWeapon().Cooldown;
            else
                attackCooldown = Attack.Cooldown;
        }
    }

   /* void playerBlock()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isBlock = true;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            isBlock = false;
        }
    }
    
    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        anim.SetBool("isAttacking", false);
    }
   */
}
