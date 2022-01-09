using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class StackGroup : MonoBehaviour
{
    [SerializeField] private float maxWidth;
    [SerializeField] private float spacing;

    [SerializeField] private RectTransform rect;
    private void Start()
    {
        rect = transform.GetComponent<RectTransform>();
    }
    void Update()
    {
        float offset = 0f;
        foreach(var obj in transform.GetComponentsInChildren<RectTransform>())
        {
            if (obj == rect || obj.parent != rect) continue;
            obj.anchoredPosition = new Vector2(obj.sizeDelta.x/2, 0 - offset - (offset == 0f ? 0f : spacing));
            obj.sizeDelta = new Vector2(maxWidth, obj.sizeDelta.y);
            offset += obj.sizeDelta.y;
        }
    }
}
