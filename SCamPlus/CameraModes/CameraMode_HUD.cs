using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

public class CameraMode_HUD : CameraMode
{
    public CameraMode_HUD(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {
        mfdPage.flybyCam.cullingMask = SCamPlus.fpvBitmask;
    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (SCamPlus.player != null)
        {
            float hudOffset = 0;
            Vector3 hudPos = Vector3.zero;
            switch (SCamPlus.aircrarftType)
            {
                case VTOLVehicles.AV42C:
                    hudPos = new Vector3(0, 1.065f, 0.844f);
                    hudOffset = 0.4f;
                    break;
                case VTOLVehicles.FA26B:
                    hudPos = new Vector3(0, 1.4f, 6.051f);
                    hudOffset = 0.3f;
                    break;
                case VTOLVehicles.F45A:
                    hudPos = new Vector3(0, 1.1f, 6.3f);
                    hudOffset = 0;
                    break;
                default:
                    break;
            }

            mfdPage.flybyCam.transform.position = SCamPlus.player.transform.TransformPoint(hudPos);
            mfdPage.flybyCam.transform.position = mfdPage.flybyCam.transform.position - SCamPlus.player.transform.forward * hudOffset;

            mfdPage.flybyCam.transform.rotation = SCamPlus.player.transform.rotation;
            mfdPage.behaviorText.text = "HUD";
        }
        else
        {
            mfdPage.behaviorText.text = "NoHUD";
        }
    }
}