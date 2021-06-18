using UnityEngine.Events;
using TMPro;
using UnityEngine;


public class NpcCanvas : MonoBehaviour
{
    public TMP_Text text;

    public Canvas canvas;
    public Camera PlayerCam;
    private void Awake()
    {
        if(text == null)
            text = GetComponentInChildren<TMP_Text>();
        if(canvas == null)
            canvas = GetComponent<Canvas>();
        if (PlayerCam == null)
            PlayerCam = Camera.main;
        updateText();
     
    }
    private void Start()
    {
        var parent = GetComponentInParent<NpcData>();
        if (parent == null)
            enabled = false;

        var data = parent.OnNpcDataInspectorChanged;
        if (data == null)
            data = new UnityEvent();

        data.AddListener(updateText);
    }

    private void Update()
    {
        canvas.transform.LookAt(PlayerCam.transform.position);
    }

    private void updateText()
    {
        text.text = GetComponentInParent<NpcData>().NpcName + "\nThe " + GetComponentInParent<NpcData>().Job.ToString().ToLower();
    }
}
