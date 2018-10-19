using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript;
using TouchScript.Pointers;


public class TouchController : MonoBehaviour {
    private void OnEnable()
    {
        // Add Events
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.PointersPressed += PointersPressedHandler;
            TouchManager.Instance.PointersUpdated += PointersUpdatedHandler;
            TouchManager.Instance.PointersReleased += PointersReleasedHandler;
        }
    }

    private void OnDisable()
    {
        // Delete Events
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.PointersPressed -= PointersPressedHandler;
            TouchManager.Instance.PointersUpdated -= PointersUpdatedHandler;
            TouchManager.Instance.PointersReleased -= PointersReleasedHandler;
        }
    }

    void PointersPressedHandler(object sender, PointerEventArgs e)
    {
        foreach (var pointer in e.Pointers)
        {
            var position = GetPointerPosition(pointer);
        }
    }

    void PointersUpdatedHandler(object sender, PointerEventArgs e)
    {
        foreach (var pointer in e.Pointers)
        {
            var position = GetPointerPosition(pointer);
        }
    }

    void PointersReleasedHandler(object sender, PointerEventArgs e)
    {
        foreach (var pointer in e.Pointers)
        {

        }
    }

    public Vector3 GetPointerPosition(Pointer pointer)
    {
        Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(pointer.Position);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.collider.name);
            position = hit.point;
        }

        return position;
    }
}
