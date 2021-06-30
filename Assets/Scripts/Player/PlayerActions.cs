using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using TMPro;

public class PlayerActions : MonoBehaviour
{

    public LayerMask Mask;
    public Camera PlayerCamera;
    public GameObject QuestUiWindow;
    private bool questWindowActive = false;

    //Dialogue    
    public GameObject dialogue_gameobject;
    public KeyCode InteractButton = KeyCode.E;
    public KeyCode EscapeButton = KeyCode.Escape;
    public float InteractionRange;

    public bool _indialogue = false;
    public bool _NpcInDialogue = false;
    private RaycastHit _currenthit;
    private GameObject ConversationStartNpc;

    public bool isInteracting;
    public IInteractWindow openedWindow;

    private void Update()
    {
        if (Input.GetKeyDown(InteractButton) && !isInteracting)
        {
            RaycastHit hit;

            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, InteractionRange, Mask))
            {
                _currenthit = hit;
                Transform Target = hit.transform;
                DialogueManager dialogue = hit.transform.GetComponentInParent<DialogueManager>();

                if (dialogue == null)
                    dialogue = hit.transform.GetComponentInChildren<DialogueManager>();
                if (dialogue == null)
                    return;
                if (dialogue._isdialogue == false)
                {
                    openedWindow = dialogue;
                    isInteracting = true;
                    dialogue_gameobject.SetActive(true);
                    _indialogue = true;
                    Vector3 rot = dialogue.transform.eulerAngles;
                    dialogue.transform.LookAt(transform);
                    dialogue.transform.eulerAngles = new Vector3(rot.x, dialogue.transform.eulerAngles.y, rot.z);
                    dialogue.say(_currenthit.transform.gameObject);
                }
            }
        }
        if (_indialogue == true)
        {
            PressSpeakButton(_currenthit.transform.GetComponentInParent<DialogueManager>());
        }
        if (_NpcInDialogue == true)
        {
            PressSpeakButton(ConversationStartNpc.transform.GetComponentInParent<DialogueManager>());
        }
        QuestWindowToggle();
        if (isInteracting && Input.GetKeyDown(EscapeButton))
        {
            isInteracting = false;
            openedWindow.OnClose();
        }
    }

    public void ReceiveInteraction(GameObject npc) //Recieve interaction from npc
    {
        ConversationStartNpc = npc;
        DialogueManager dialogue = npc.transform.GetComponentInParent<DialogueManager>();

        if (dialogue == null)
            dialogue = npc.transform.GetComponentInChildren<DialogueManager>();
        if (dialogue == null)
            return;
        if (dialogue._isdialogue == false)
        {
            openedWindow = dialogue;
            isInteracting = true;
            dialogue_gameobject.SetActive(true);
            _NpcInDialogue = true;
            Vector3 rot = dialogue.transform.eulerAngles;
            dialogue.transform.LookAt(transform);
            dialogue.transform.eulerAngles = new Vector3(rot.x, dialogue.transform.eulerAngles.y, rot.z);
            dialogue.say(npc.transform.gameObject);
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
            else if (!dialogue.currentSentence.HasPaths())
            {
                if (dialogue.currentSentence.nextSentence != null)
                {
                    dialogue.currentSentence = dialogue.currentSentence.nextSentence;
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
