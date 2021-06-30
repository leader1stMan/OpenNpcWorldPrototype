using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield.asset", menuName = "Attack/Shield")]
public class Shield : ScriptableObject
{
    //When equipped, Shield divides character's speed by this number
    public float ShieldDeceleration;
}
