using UnityEngine;
using UnityEngine.Events;

public class NpcData : MonoBehaviour
{
    [SerializeField]
    protected string npcName;
    public string NpcName { get { return npcName; } }

    [SerializeField]
    protected int age;
    public int Age { get { return age; } }

    [SerializeField]
    protected Job job;
    public Job Job { get { return job; } }



    [SerializeField]
    protected Transform home;

    [SerializeField]
    protected Transform work;

    [InspectorReadOnly][SerializeField]
    private NpcStates _currentState;


    protected NpcStates currentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            _currentState = value;
        }
    }
    

    [HideInInspector]
    public UnityEvent OnNpcDataInspectorChanged;
    private void OnValidate()
    {
        if (OnNpcDataInspectorChanged == null)
            OnNpcDataInspectorChanged = new UnityEvent();
        OnNpcDataInspectorChanged.Invoke();
    }

   

}

