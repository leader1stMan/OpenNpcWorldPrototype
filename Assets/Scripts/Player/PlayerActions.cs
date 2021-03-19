using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PlayerActions : MonoBehaviour
{

    public LayerMask Mask;
    public Camera PlayerCamera;
    public GameObject QuestUiWindow;
    private bool questWindowActive = false;
    public Quest quest;
    //Dialogue    
    public GameObject dialogue_gameobject;
    public KeyCode InteractButton = KeyCode.E;
    public KeyCode EscapeButton = KeyCode.Escape;
    public float InteractionRange;

    public PlayerInventory PlayerInventroy;
    public MerchantInventory Shop;

    public bool _indialogue = false;
    private RaycastHit _currenthit;

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
            
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, InteractionRange, Mask))
            {
                _currenthit = hit;
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
                    DialogueManager dialogue = hit.transform.GetComponentInParent<DialogueManager>();
                    if (dialogue == null)
                        dialogue = hit.transform.GetComponentInChildren<DialogueManager>();
                    if (dialogue == null)
                        return;
                    if (dialogue._isdialogue == false)
                    {
                        dialogue_gameobject.SetActive(true);
                        _indialogue = true;
                        Vector3 rot = dialogue.transform.eulerAngles;
                        dialogue.transform.LookAt(transform);
                        dialogue.transform.eulerAngles = new Vector3(rot.x, dialogue.transform.eulerAngles.y, rot.z);
                        dialogue.say(_currenthit.transform.gameObject);
                    }
                }
            }
        }
        if (_indialogue == true)
        {
            PressSpeakButton(_currenthit.transform.GetComponentInParent<DialogueManager>());
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
    private void PressSpeakButton(DialogueManager dialogue)
    {
        var pointer = new PointerEventData(EventSystem.current);
        if (Input.GetMouseButtonDown(0))
        {
            if (dialogue.displayingdialogue)
            {
                dialogue._textSpeed = 0f;
            }
            else if (!dialogue.sentence1.HasPaths())
            {
                if (dialogue.sentence1.nextSentence != null)
                {
                    dialogue.sentence1 = dialogue.sentence1.nextSentence;
                    dialogue.DisplayNextSentence();
                }
                else
                {
                    dialogue.EndDialogue();
                }
            }
            else
            {
                dialogue.displayingdialogue = true;
                dialogue.OptionsActive();
            }
        }
    }
}
