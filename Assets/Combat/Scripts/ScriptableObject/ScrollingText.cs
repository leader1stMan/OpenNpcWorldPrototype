using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingText : MonoBehaviour
{
    public float duration = 1f;
    public float speed;

    private TextMesh mesh;
    private float startTime;
    // Start is called before the first frame update
    void Awake()
    {
        mesh = GetComponent<TextMesh>();
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - startTime < duration)
        {
            transform.LookAt(Camera.main.transform);
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        mesh.text = text;
    }

    public void SetColor(Color color)
    {
        mesh.color = color;
    }
}
