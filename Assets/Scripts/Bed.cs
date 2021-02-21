using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    public DayAndNightControl timeController;

    public int waitSecondsRealtime;

    private List<GameObject> characters = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponentInParent<PlayerActions>() || !other.GetComponentInParent<FirstPersonAIO>())
            return;

        characters.Add(other.GetComponentInParent<PlayerActions>().gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.GetComponentInChildren<PlayerActions>() || !other.GetComponentInChildren<FirstPersonAIO>())
            return;

        other.GetComponentInChildren<PlayerActions>().bedInNear = this;

        if (Input.GetKeyDown(other.GetComponentInChildren<PlayerActions>().InteractButton))
            other.GetComponentInChildren<PlayerActions>().InteractWithBed();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponentInChildren<PlayerActions>())
            return;

        other.GetComponentInChildren<PlayerActions>().bedInNear = null;

        other.GetComponentInChildren<PlayerActions>().SetSleepPanelState(false);

        characters.Remove(other.GetComponentInParent<PlayerActions>().gameObject);
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
        foreach (var item in characters)
        {
            if (item.GetComponentInParent<PlayerActions>() == id)
            {
                item.GetComponentInChildren<FirstPersonAIO>().canJump = false;
                item.GetComponentInChildren<FirstPersonAIO>().playerCanMove = false;
                break;
            }
        }

        yield return new WaitForSecondsRealtime(waitSecondsRealtime);

        foreach (var item in characters)
        {
            if (item.GetComponentInParent<PlayerActions>() == id)
            {
                item.GetComponentInChildren<FirstPersonAIO>().canJump = true;
                item.GetComponentInChildren<FirstPersonAIO>().playerCanMove = true;
                break;
            }
        }
    }
}
