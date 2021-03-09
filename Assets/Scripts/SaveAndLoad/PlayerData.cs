using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public int health;
    public float[] positon;

    public PlayerData(PlayerStats Player)
    {
        level = Player.level;
        health = Player.health;

        positon = new float[3];
        positon[0] = Player.position.x;
        positon[1] = Player.position.y;
        positon[2] = Player.position.z;

    }
}
