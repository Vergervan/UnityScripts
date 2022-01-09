using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class TextBackground : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObject;
    [SerializeField] private float realHeight;
    [SerializeField] private RectTransform rect;
    [SerializeField, HideInInspector] private string _textBox;
    public string Text
    {
        get => _textBox;
        set
        {
            _textBox = value;
            textObject.text = value;
            textObject.ForceMeshUpdate(true);
        }
    }
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    void Update()
    {
        realHeight = textObject.text.Length > 0 ? textObject.textBounds.size.y : 0f;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, realHeight);
    }
}
