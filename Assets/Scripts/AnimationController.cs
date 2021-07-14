using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    public static readonly string SHILD_UNEQUIP = "Shield_Unequip";

    const int LayersNumber = 3;
    string[] LayerPrefixs;

    string[] Layers;
    bool[] Block;

    public Animator animator;

    public AnimationController(Animator anim)
    {
        animator = anim;
    }

    private void Start()
    {
        Layers = new string[LayersNumber];
        Block = new bool[LayersNumber];

        LayerPrefixs = new string[] { "LowerBody.", "UpperBody.", "Weapon." };

        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Changes animation. 
    /// </summary>
    /// <param name="newAnimation">New animation.</param>
    /// <param name="layer">Animation layer (Upper, Down or All).</param>
    /// <param name="block">If true, blocks chosen layer, so animation can't be changed, before current animation executes</param>
    public void ChangeAnimation(string newAnimation, AnimatorLayers layer, bool block = false)
    {
        foreach (AnimatorLayers value in Enum.GetValues(typeof(AnimatorLayers)))
        {
            if (layer.HasFlag(value) && value != AnimatorLayers.ALL)
            {
                int chosenLayer = ConvertToInt((int)value);
                string animation = LayerPrefixs[chosenLayer] + newAnimation;
                if (Layers[chosenLayer] != animation && !Block[chosenLayer])
                {
                    Layers[chosenLayer] = animation;

                    if (value == AnimatorLayers.WEAPON)
                        animator.Play(animation);
                    else
                        animator.CrossFade(animation, 0.5f);

                    if (block)
                    {
                        StartCoroutine(BlockAnimator(layer, animator.GetCurrentAnimatorStateInfo(ConvertToInt((int)value)).length));
                    }
                }
            }
        }
    }

    public float GetAnimationLength(AnimatorLayers layer)
    {
        return animator.GetCurrentAnimatorClipInfo(ConvertToInt((int)layer)).Length;
    }

    IEnumerator BlockAnimator(AnimatorLayers layer, float time)
    {
        int number = ConvertToInt((int)layer);
        Block[number] = true;
        yield return new WaitForSeconds(time);
        Block[number] = false;
    }

    int ConvertToInt(int number)
    {
        byte[] bytes = { 0, 0, 0, (byte)number };
        return BitConverter.ToInt32(bytes, 0);
    }
}

[Flags]
public enum AnimatorLayers : byte
{
    DOWN = 1,
    UP = 2,
    WEAPON = 4,
    ALL = 7
}