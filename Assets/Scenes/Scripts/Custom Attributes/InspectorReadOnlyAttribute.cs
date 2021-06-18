using UnityEngine;

/// <summary>
/// <br> This is used to mark a serialized variable as Read only in the editor.</br>
/// <br> Note : You need to specify the attribute [SerializeField] or mark the variable as public for this to work </br>
/// </summary>
public class InspectorReadOnlyAttribute : PropertyAttribute { }