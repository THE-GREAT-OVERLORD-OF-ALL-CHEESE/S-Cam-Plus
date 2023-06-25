using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerVisibilityManager : MonoBehaviour
{
    public Camera camera;
    public List<Component> renderComonents = new List<Component>();

    private void Start()
    {
        Debug.Log("Added player visibility manager.");

        GetVisibilityComponents();
        //SetVisibility(false);

        Camera.onPreRender += OnCameraPreRender;
        Camera.onPostRender += OnCameraPostRender;
    }

    private void GetVisibilityComponents()
    {
        GetVisibilityComponents(GetComponentInChildren<RudderFootAnimator>(true).gameObject);
        GetVisibilityComponents(GetComponentInChildren<CamRigRotationInterpolator>(true).gameObject);
    }

    private void GetVisibilityComponents(GameObject go)
    {
        renderComonents.AddRange(go.GetComponentsInChildren<MeshRenderer>(true));
        renderComonents.AddRange(go.GetComponentsInChildren<SkinnedMeshRenderer>(true));
    }

    public void OnCameraPreRender(Camera cam)
    {
        if (cam == FlybyCameraMFDPage.instance.flybyCam)
        {
            SetVisibility(false);
        }
    }

    public void OnCameraPostRender(Camera cam)
    {
        if (cam == FlybyCameraMFDPage.instance.flybyCam)
        {
            SetVisibility(true);
        }
    }

    private void SetVisibility(bool visibility)
    {
        foreach (Component renderComponent in renderComonents)
        {
            if (renderComponent is Renderer renderer)
            {
                renderer.enabled = visibility;
            }
        }
    }

    private void OnDestroy()
    {
        Camera.onPreRender -= OnCameraPreRender;
        Camera.onPostRender -= OnCameraPostRender;
    }
}