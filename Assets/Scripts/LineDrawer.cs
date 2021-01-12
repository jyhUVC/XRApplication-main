using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;

public class LineDrawer : MonoBehaviour
{
    LineRenderer lineRenderder;

    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float drawDistance = 20f;
    [SerializeField] private int maxPointCount = 50;
    private int drawCount = 0;

    private List<Transform> anchorChildsPool = new List<Transform>();
    [SerializeField] private Transform drawAnchorObject = null;
    [SerializeField] private GameObject cubePrefab = null;

    //Anchor drawAnchor;
    Anchor drawAnchor;

    private List<GameObject> stampOnWorld = new List<GameObject>();
    public Text debugText;

    private void Awake()
    {
        lineRenderder = GetComponent<LineRenderer>();
        lineRenderder.positionCount = 0;
        
        for(int i = 0; i < maxPointCount; ++i)
        {
            GameObject newPointObj = new GameObject("Draw Point");
            newPointObj.transform.SetParent(drawAnchorObject);
            anchorChildsPool.Add(newPointObj.transform);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //DrawLine();
        /*if ()
        {
            SetCube();
        }*/

        // 생성된 stamp looks at camera
        if (stampOnWorld.Count != 0)
        {
            debugText.text = Camera.main.transform.position.ToString();
            for (int i = 0; i < stampOnWorld.Count; i++)
            {
                stampOnWorld[i].transform.LookAt(Camera.main.transform);
            }
        }
    }

    private void SetCube()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, drawDistance));
            Anchor drawAnchor = Session.CreateAnchor(new Pose(point, Quaternion.identity));
            GameObject newCube = GameObject.Instantiate(cubePrefab, drawAnchor.transform.position, drawAnchor.transform.rotation, drawAnchor.transform);
            stampOnWorld.Add(newCube);
            newCube.transform.parent = drawAnchor.transform;
        }
    }

    private void UpdateaAnchor(Anchor drawAnchor, Vector3 position)
    {
        drawAnchor.transform.position = position;
    }

    private void DrawLine()
    {
        if (drawAnchor == null)
            drawAnchor = Session.CreateAnchor(new Pose(Vector3.zero, Quaternion.identity));

        lineRenderder.widthMultiplier = lineWidth;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            drawCount = 0;
            lineRenderder.positionCount = 0;
            lineRenderder.positionCount++;
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, drawDistance));
            UpdateaAnchor(drawAnchor, point);
            anchorChildsPool[drawCount].position = point;
            anchorChildsPool[drawCount].SetParent(drawAnchor.transform);
            lineRenderder.SetPosition(drawCount, anchorChildsPool[drawCount].position);
            drawCount++;
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (drawCount >= maxPointCount)
            {

            }
            else
            {
                lineRenderder.positionCount++;
                Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, drawDistance));
                anchorChildsPool[drawCount].position = point;
                anchorChildsPool[drawCount].SetParent(drawAnchor.transform);
                lineRenderder.SetPosition(drawCount, anchorChildsPool[drawCount].position);
                drawCount++;
            }

        }
    }
    public void RedLine()
    {
        lineRenderder.startColor = Color.red;
        lineRenderder.endColor = Color.red;
    }
    public void YellowLine()
    {
        lineRenderder.startColor = Color.yellow;
        lineRenderder.endColor = Color.yellow;
    }
    public void BlueLine()
    {
        lineRenderder.startColor = Color.blue;
        lineRenderder.endColor = Color.blue;
    }

    public void Width1()
    {
        lineWidth = 0.01f;
    }
    public void Width2()
    {
        lineWidth = 0.005f;
    }
    public void Width3()
    {
        lineWidth = 0.0001f;
    }
}
