using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public DayAndNightControl timeController;

    public int waitSecondsRealtime;

    private List<GameObject> characters = new List<GameObject>();


    public void Interact(PlayerActions player)
    {
        player.SetSleepPanelState();
    }

    public void ChooseSleep(float amount, PlayerActions id)
    {
        if (((timeController.currentTime + (float)1 / (float)24) * amount) <= 1)
        {
            timeController.currentTime += ((float)1 / (float)24) * amount;
        }
        else
        {
            int days = Mathf.RoundToInt(amount / 24);
            amount -= 24 * days;

            timeController.currentDay += days;
            timeController.currentTime += amount;
        }

        StartCoroutine(Sleep(id));
    }

    IEnumerator Sleep(PlayerActions id)
    {

        id.gameObject.GetComponentInChildren<FirstPersonAIO>().canJump = false;
        id.gameObject.GetComponentInChildren<FirstPersonAIO>().playerCanMove = false;

        yield return new WaitForSecondsRealtime(waitSecondsRealtime);
        
        id.gameObject.GetComponentInChildren<FirstPersonAIO>().canJump = true;
        id.gameObject.GetComponentInChildren<FirstPersonAIO>().playerCanMove = true;
    }
}
