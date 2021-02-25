using UnityEngine;
using System;
using System.Threading.Tasks;

public class PlayerActions : MonoBehaviour
{
    public KeyCode InteractButton = KeyCode.E;

    public LayerMask Mask;
    public float InteractionRange;
    public Inventory PlayerInventroy;

    public Camera PlayerCamera;

    public GameObject QuestUiWindow;
    private bool questWindowActive = false;
    public Quest quest;

    private void Update()
    {
        if (Input.GetKeyDown(InteractButton))
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit,InteractionRange, Mask))
            {
                Transform Target = hit.transform;

                MerchantShop Shop = Target.GetComponent<MerchantShop>();

                if (Shop != null) 
                {                       
                    StartCoroutine(Shop.Interact());
                    PlayerInventroy.InventoryPanel.SetActive(true);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;

                    Vector3 rot = Target.eulerAngles;
                    Target.LookAt(transform);
                    Target.eulerAngles = new Vector3(rot.x, Target.eulerAngles.y, rot.z);
                }
                else
                {
                    DialogueManager dialogue = Target.GetComponentInParent<DialogueManager>();
                    if (dialogue == null)
                        dialogue = Target.GetComponentInChildren<DialogueManager>();
                    if (dialogue == null)
                        return;
                    Vector3 rot = dialogue.transform.eulerAngles;
                    dialogue.transform.LookAt(transform);
                    dialogue.transform.eulerAngles = new Vector3(rot.x, dialogue.transform.eulerAngles.y, rot.z);
                    dialogue.say("Hello there. How are you");
                }
            }
        }
        QuestWindowToggle();
    }

    private void QuestWindowToggle()
    {

        if (Input.GetKeyDown(KeyCode.X) && questWindowActive == false)
        {

            QuestUiWindow.SetActive(true);
            questWindowActive = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else if (Input.GetKeyDown(KeyCode.X) && questWindowActive == true)
        {
            QuestUiWindow.SetActive(false);
            questWindowActive = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
