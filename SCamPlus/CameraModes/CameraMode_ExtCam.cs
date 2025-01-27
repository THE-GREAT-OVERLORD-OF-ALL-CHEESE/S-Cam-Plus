using UnityEngine;
using VTOLAPI;

namespace CheeseMods.SCamPlus.CameraModes
{
    public class CameraMode_ExtCam : CameraMode
    {
        public CameraMode_ExtCam(string name, string shownName) : base(name, shownName)
        {
            this.name = name;
            this.shownName = shownName;
        }

        public override void LateUpdate(FlybyCameraMFDPage mfdPage)
        {
            if (SCamPlus.extCamManager != null)
            {
                Camera currentCam = SCamPlus.extCamManager.cameras[SCamPlus.extCamManager.camIdx];

                mfdPage.flybyCam.transform.position = currentCam.transform.position;
                mfdPage.flybyCam.transform.rotation = currentCam.transform.rotation;
                mfdPage.flybyCam.fieldOfView = currentCam.fieldOfView;

                mfdPage.behaviorText.text = "ExtCam";
            }
            else
            {
                SCamPlus.extCamManager = VTAPI.GetPlayersVehicleGameObject().GetComponentInChildren<ExternalCamManager>();
                mfdPage.behaviorText.text = "NoExtCam";
            }
        }
    }
}