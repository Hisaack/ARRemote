using System.Collections;
using UnityEngine;
using agora_gaming_rtc;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARFoundation;

public class RemoteDrawer : MonoBehaviour
{
    // Start is called before the first frame update
    IRtcEngine rtcEngine;

    Camera arCam;     // the AR Camera
    Camera renderCam; // the Renderer Camera, space of 3D objects
    Camera viewCam; // the viewer of the projected quad, acting camera since AR Camera projects into a RenderTexture

    [SerializeField] Transform referenceObject = null;

    [SerializeField] GameObject DrawPrefab = null;

    public float DotScale = 0.15f;

    private GameObject anchorGO;
    private Color DrawColor = Color.black;
  
    void Start()
    {
        rtcEngine = IRtcEngine.QueryEngine();
        if (rtcEngine != null)
        {
            rtcEngine.OnStreamMessage += HandleStreamMessage;
        }

        CamStart();
    }


    /// <summary>
    ///    The delegate function to handle message sent from Audience side
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="streamId"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    void HandleStreamMessage(uint userId, int streamId, string data, int length)
    {
        if (data.Contains("color"))
        {
           StartCoroutine(CoProcessDrawingData(data));
        }
       
        if (data.Contains("clear"))
        {
            Destroy(anchorGO);
        }
      

        Debug.LogWarning("Main Camera pos = " + Camera.main.transform.position);
    }

   
    /// <summary>
    ///  Do the drawing async
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// 
    IEnumerator CoProcessDrawingData(string data)
    {

        try
        {
            DrawmarkModel dm = JsonUtility.FromJson<DrawmarkModel>(data);
            DrawColor = dm.color;

            Vector2 previousVector_1 = Vector3.zero;
            Vector2 previousVector_2 = Vector3.zero;
            Vector2 previousVector_3 = Vector3.zero;

            List<Vector2> Iteration1 = new List<Vector2>();
            List<Vector2> Iteration2 = new List<Vector2>();
            List<Vector2> Iteration3 = new List<Vector2>();

            for (int i = 0; i < dm.points.Count; i++)
            {
                Vector2 middl = (previousVector_1 + dm.points[i]) / 2;
                if (i != 0)
                    Iteration1.Add(middl);
                Iteration1.Add(dm.points[i]);

                previousVector_1 = dm.points[i];
            }



            for (int i = 0; i < Iteration1.Count; i++)
            {
                Vector2 middl = (previousVector_2 + Iteration1[i]) / 2;
                if (i != 0)
                    Iteration2.Add(middl);
                Iteration2.Add(Iteration1[i]);

                previousVector_2 = Iteration1[i];
            }
            //for (int i = 0; i < Iteration2.Count; i++)
            //{
            //    Vector2 middl = (previousVector_3 + Iteration2[i]) / 2;
            //    if (i != 0)
            //        Iteration3.Add(middl);
            //    Iteration3.Add(Iteration2[i]);

            //    previousVector_3 = Iteration2[i];
            //}


            foreach (Vector2 pos in Iteration2)
            {
                DrawDot(pos);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        yield return null;

    }

    int dotCount = 0;
    /// <summary>
    ///   
    /// </summary>
    /// <param name="pos">Screen Position</param>
    void DrawDot(Vector2 pos)
    {
        if (anchorGO == null)
        {
            anchorGO = new GameObject();
            anchorGO.transform.SetParent(referenceObject.transform.parent);
            anchorGO.transform.position = Vector3.zero;
            anchorGO.transform.localScale = Vector3.one;
            anchorGO.name = "DrawAnchor";
        }


        // DeNormalize the position and adjust to passed camera
        Vector3 location = DeNormalizedPosition(pos, renderCam);

        GameObject go = GameObject.Instantiate(DrawPrefab, location, Quaternion.identity);
        go.transform.SetParent(anchorGO.transform);
        go.transform.localScale = DotScale * Vector3.one;
        go.layer = (int)CameraLayer.IGNORE_RAYCAST;
        go.name = "dot " + dotCount;
        dotCount++;
        Debug.LogFormat("{0} pos:{1} => : {2} ", go.name, pos, location);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                mat.color = DrawColor;
            }
        }
    }

    /// <summary>
    ///    Provide a ViewPort Position (0,0) = bottom left and (1,1) top right
    /// return world position for the current camera
    /// </summary>
    /// <param name="vector2"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    Vector3 DeNormalizedPosition(Vector3 vector2, Camera camera)
    {
        Vector3 pos = new Vector3(vector2.x, vector2.y);

        pos = camera.ViewportToScreenPoint(pos);

        // Consider using the referenceObject for position calculation
        // Vector3 deltaPos = camera.transform.position - referenceObject.position;

        pos = new Vector3(pos.x, pos.y, 8);

        return camera.ScreenToWorldPoint(pos);

    }

    // Use this for initialization
    void CamStart()
    {
        renderCam = GameObject.Find("RenderCamera").GetComponent<Camera>();
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
        viewCam = GameObject.Find("ViewCamera").GetComponent<Camera>();
    }
}
