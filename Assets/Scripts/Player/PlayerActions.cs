using UnityEngine;
using System;
using System.Threading.Tasks;

public class PlayerActions : MonoBehaviour
{
    public KeyCode InteractButton = KeyCode.E;
    public KeyCode EscapeButton = KeyCode.Escape;

    public LayerMask Mask;
    public float InteractionRange;
    public PlayerInventory PlayerInventroy;
    public MerchantInventory Shop;
    public Camera PlayerCamera;

    public GameObject QuestUiWindow;
    private bool questWindowActive = false;
    public bool isInteracting;
    private void Awake()
    {
        PlayerInventroy = GetComponent<PlayerInventory>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(InteractButton) && !isInteracting)
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit,InteractionRange, Mask))
            {
                Transform Target = hit.transform;

                Shop = Target.GetComponent<MerchantInventory>();

                if (Shop != null)
                {
                    isInteracting = true;
                    PlayerInventroy.InventoryPanel.SetActive(true);
                    PlayerInventroy.ShopAccessed = true;
                    Shop.OpenInventory();

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
        if (isInteracting && Input.GetKeyDown(EscapeButton))
        {
            isInteracting = false;
            PlayerInventroy.InventoryPanel.SetActive(false);
            PlayerInventroy.ShopAccessed = false;
            Shop.CloseInventory();
        }
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
