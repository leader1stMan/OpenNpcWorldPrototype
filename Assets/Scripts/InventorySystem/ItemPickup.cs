using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Helper data containers


    public Item Item;
    public int Count;
    public float Value;

    // PickUp components
    Canvas PickUpCanvas;
    MeshFilter PickUpMesh;
    MeshRenderer PickUpRenderer;
    BoxCollider PickUpCollider;

    ItemPickup()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        // Initialise mesh
        PickUpMesh = GetComponentInChildren<MeshFilter>();
        PickUpMesh.mesh = Item.ItemMesh;
        PickUpRenderer = GetComponentInChildren<MeshRenderer>();
        PickUpRenderer.material = Item.ItemMaterial;
        PickUpCollider = GetComponentInChildren<BoxCollider>();
        PickUpCollider.size = GetComponentInChildren<MeshFilter>().mesh.bounds.size;
        PickUpCollider.center = GetComponentInChildren<MeshFilter>().mesh.bounds.center;

        // Initialise canvas
        PickUpCanvas = GetComponentInChildren<Canvas>();
        PickUpCanvas.transform.localPosition = new Vector3(0, 0.5f + PickUpCollider.size.y * PickUpCollider.transform.localScale.y,0);
        PickUpCanvas.GetComponentInChildren<TMPro.TMP_Text>().text = Item.ItemName;
    }

    // Update is called once per frame
    void Update()
    {
        ManageCanvas();
    }

    void ManageCanvas()
    {
        if((Camera.main.transform.position - transform.position).magnitude < 5.0f)
        {
            
        }
        PickUpCanvas.transform.LookAt(Camera.main.transform.position);
    }

    public void OnPickedUp()
    {
        Destroy(gameObject);
    }
}
