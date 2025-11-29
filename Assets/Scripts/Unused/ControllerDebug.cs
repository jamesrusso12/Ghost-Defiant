using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ControllerDebug : MonoBehaviour
{
    public bool showRay = true;
    public float rayLength = 10f;
    public LineRenderer lineRenderer;
    public OVRRaycaster ovrRaycaster;
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector3 rayEndPoint = transform.position + transform.forward * rayLength;
        RaycastResult? uiHit = null;

        if (ovrRaycaster != null)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = new Vector2(Screen.width / 2, Screen.height / 2);

            List<RaycastResult> results = new List<RaycastResult>();
            ovrRaycaster.Raycast(eventData, results);

            if (results.Count > 0)
            {
                rayEndPoint = results[0].worldPosition;
                uiHit = results[0];
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayLength))
        {
            rayEndPoint = hit.point;
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, rayEndPoint);
        }

        if (showRay)
        {
            Debug.DrawRay(transform.position, transform.forward * rayLength, Color.red);
        }

        if (OVRInput.GetDown(clickButton) && uiHit.HasValue)
        {
            ClickUIElement(uiHit.Value);
        }

        if (OVRInput.GetDown(clickButton))
        {
            Debug.Log("Click button pressed");
            if (uiHit.HasValue)
            {
                ClickUIElement(uiHit.Value);
            }
        }

    }

    private void ClickUIElement(RaycastResult uiHit)
    {
        ExecuteEvents.Execute(uiHit.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        Debug.Log("Clicked UI Element: " + uiHit.gameObject.name);
    }
}
