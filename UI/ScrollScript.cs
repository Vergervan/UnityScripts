using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollScript : MonoBehaviour, IDragHandler
{
    float currentPos = 0;
    float prePos = 0;
    bool isDraged = false;
    public float margin = 15f;
    private float defaultMargin = 0f;
    public float Value = 0;
    void Start()
    {
        defaultMargin = gameObject.GetComponent<RectTransform>().rect.width / 2;
    }
    void Update()
    {
        float width = gameObject.transform.parent.GetComponent<RectTransform>().rect.width;
        Value = (gameObject.GetComponent<RectTransform>().localPosition.x + (width/2) - margin - defaultMargin) / (width-(margin+defaultMargin)*2);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDraged = true;
        float width = gameObject.transform.parent.GetComponent<RectTransform>().rect.width / 2;
        gameObject.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x, gameObject.transform.position.y, 0);
        float x = Clamp(gameObject.GetComponent<RectTransform>().localPosition.x, -1 * width + defaultMargin + margin, width - defaultMargin - margin);
        gameObject.GetComponent<RectTransform>().localPosition = new Vector3(x, 0);
    }

    public float Clamp(float x, float min, float max)
    {
        if (x > max) return max;
        else if (x < min) return min;
        return x;
    }

}
