using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static readonly string IDLE = "Player_Idle";

    public static readonly string WALK = "Player_Walk";

    public static readonly string RUN = "Player_Run";

    public static readonly string Jump_0 = "Player_Jump_0";

    public static readonly string Jump_1 = "Player_Jump_1";

    public static readonly string Jump_2 = "Player_Jump_2";

    public static readonly string UNARMED_ATTACK = "Unarmed_Attack";

    public static readonly string SWORD_ATTACK = "Sword_Attack";

    public static readonly string SWORD_EQUIP = "Sword_Equip";

    public static readonly string SHIELD_READY = "Shield_0M_L_Ready_0";

    public static readonly string SHIELD_IDLE = "Shield_0M_L_Idle_0";

    public static readonly string SHIELD_HIT = "Shield_0M_L_Hit_0_L";

    string[] LayerPrefixs;

    string[] Layers;
    bool[] Block;

    const int layersNumber = 2;

    public Animator animator;

    public AnimationController(Animator anim)
    {
        animator = anim;
    }

    private void Start()
    {
        Layers = new string[layersNumber];
        Block = new bool[layersNumber];
        LayerPrefixs = new string[] { "LowerBody.", "UpperBody." };
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(string newAnimation, AnimatorLayers layer, bool block = false)
    {
        bool AllLayers = layer == AnimatorLayers.ALL;

        if (AllLayers)
            layer = 0;

        while (true)
        {
            int chosenLayer = (int)layer;

            string animation = LayerPrefixs[chosenLayer] + newAnimation;

            if (Layers[chosenLayer] != animation && !Block[chosenLayer])
            {
                animator.Play(animation);
                Layers[chosenLayer] = animation;

                if (block)
                    StartCoroutine(BlockAnimator(layer, animator.GetCurrentAnimatorStateInfo(chosenLayer).length));
            }
            if (AllLayers)
            {
                if (++layer == AnimatorLayers.ALL)
                    break;
            }
            else
                break;
        }
    }

    IEnumerator BlockAnimator(AnimatorLayers layer, float time)
    {
        Block[(int)layer] = true;
        yield return new WaitForSeconds(time);
        Block[(int)layer] = false;
    }

    public void AttackEvent()
    {
        GetComponentInParent<SkeletonAi>().AttackEvent();
    }
}

public enum AnimatorLayers { DOWN, UP, ALL }