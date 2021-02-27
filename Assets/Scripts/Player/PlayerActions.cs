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
    public float InteractionRange;

    public bool _indialogue = false;
    private RaycastHit _currenthit;
    private void Update()
    {
        if (Input.GetKeyDown(InteractButton) && !_indialogue)
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, InteractionRange, Mask))
            {
                _currenthit = hit;
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
        if (_indialogue == true)
        {
            PressSpeakButton(_currenthit.transform.GetComponentInParent<DialogueManager>());
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
