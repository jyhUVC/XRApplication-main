using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;
using System;

public class Draw : MonoBehaviour
{

    public Toggle debugToggle;

    private float distance = .3f;

    private int positionCount = 0;

    private float startWidth = 0.005f;
    private float endWidth = 0.005f;
    

    Vector3 prevPointDistance = Vector3.zero;

    float minDistanceBeforeNewPoint = 0.001f;

    LineRenderer currentLineRenderer;
    public Material mat;

    private List<GameObject> lines = new List<GameObject>();

    bool isDrawing;
    bool isDeleting;

    Vector3 colliderSize = new Vector3(0.05f, 0.05f, 0.05f);


    [SerializeField] private GameObject stamp;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
    }
    private void Update()
    {
        if (UIManager.instance.isDrawing)
        {
            DrawOnTouch();
        }
        else if (UIManager.instance.isErasing)
        {
            DeleteOnTouch();
        }
        else if (UIManager.instance.isStamping)
        {
            StampingOnTouch();
        }

    }

    private void StampingOnTouch()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance));
            Anchor drawAnchor = Session.CreateAnchor(new Pose(point, Quaternion.identity));
            GameObject newStamp = GameObject.Instantiate(stamp, drawAnchor.transform.position, drawAnchor.transform.rotation, drawAnchor.transform);
            newStamp.transform.parent = drawAnchor.transform;
        }
    }

    private void DeleteOnTouch()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                Destroy(raycastHit.transform.gameObject);
            }
            
        }
    }
    private void DrawOnTouch()
    {
        int tapCount = Input.touchCount > 1 ? Input.touchCount : 1;

        for (int i = 0; i < tapCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(i).position.x, Input.GetTouch(i).position.y, distance));

            if (touch.phase == TouchPhase.Began)
            {
                Anchor anchor = Session.CreateAnchor(new Pose(touchPosition, Quaternion.identity));
                if (anchor == null)
                {
                }
                else
                {
                }

                AddNewLineRenderer(transform, anchor, touchPosition);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {

                AddPoint(touchPosition);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                currentLineRenderer = null;
                currentLineObject = null;
            }
        }
    }
    public void AddPoint(Vector3 position)
    {
        if (prevPointDistance == null)
            prevPointDistance = position;

        if (prevPointDistance != null && Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= minDistanceBeforeNewPoint)
        {
            prevPointDistance = position;
            positionCount++;

            currentLineRenderer.positionCount = positionCount;

            // index 0 positionCount must be - 1
            currentLineRenderer.SetPosition(positionCount - 1, position);
            BoxCollider collider = currentLineObject.AddComponent<BoxCollider>();
            collider.transform.position = position;
            collider.size = colliderSize;
        }
    }

    GameObject currentLineObject;
    public void AddNewLineRenderer(Transform parent, Anchor anchor, Vector3 position)
    {
        positionCount = 2;
        GameObject go = new GameObject($"LineRenderer");

        go.transform.parent = anchor.transform;
        go.transform.position = position;

        // Collider Setting
        BoxCollider collider = go.AddComponent<BoxCollider>();
        collider.transform.position = position;
        collider.size = colliderSize;

        // Line Setting
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();
        goLineRenderer.material = mat;
        goLineRenderer.widthMultiplier = startWidth;
        goLineRenderer.startWidth = startWidth;
        goLineRenderer.endWidth = endWidth;
        goLineRenderer.startColor = UIManager.instance.startColor;
        goLineRenderer.endColor = UIManager.instance.endColor;
        goLineRenderer.useWorldSpace = true;

        goLineRenderer.SetPosition(0, position);
        goLineRenderer.SetPosition(1, position);

        currentLineObject = go;
        currentLineRenderer = goLineRenderer;
        lines.Add(go);
    }

    public void Width1()
    {
        startWidth = 0.01f;
    }
    public void Width2()
    {
        startWidth = 0.005f;
    }
    public void Width3()
    {
        startWidth = 0.0001f;
    }

}
