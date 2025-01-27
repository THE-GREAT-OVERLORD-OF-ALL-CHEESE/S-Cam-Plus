using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using VTOLAPI;

namespace CheeseMods.SCamPlus.CameraModes
{
    public class CameraMode_SmoothTGP : CameraMode
    {
        public CameraMode_SmoothTGP(string name, string shownName) : base(name, shownName)
        {
            this.name = name;
            this.shownName = shownName;
        }

        public override void Start(FlybyCameraMFDPage mfdPage)
        {
            SCamPlus.tgpMFD = VTAPI.GetPlayersVehicleGameObject().GetComponentInChildren<TargetingMFDPage>();
        }

        public override void LateUpdate(FlybyCameraMFDPage mfdPage)
        {
            if (SCamPlus.tgpMFD != null)
            {
                mfdPage.flybyCam.transform.position = SCamPlus.tgpMFD.targetingCamera.transform.position;

                mfdPage.flybyCam.transform.rotation = Quaternion.Slerp(SCamPlus.lastRotation, SCamPlus.tgpMFD.targetingCamera.transform.rotation, Time.deltaTime * 1);
                SCamPlus.lastRotation = mfdPage.flybyCam.transform.rotation;

                mfdPage.flybyCam.fieldOfView = SCamPlus.tgpMFD.targetingCamera.fieldOfView;
                mfdPage.behaviorText.text = "SmoothTGP";
            }
            else
            {
                SCamPlus.tgpMFD = VTAPI.GetPlayersVehicleGameObject().GetComponentInChildren<TargetingMFDPage>();
                if (mfdPage.weaponManager.opticalTargeter != null)
                {
                    mfdPage.flybyCam.transform.position = mfdPage.weaponManager.opticalTargeter.sensorTurret.pitchTransform.position;
                    mfdPage.flybyCam.transform.rotation = mfdPage.weaponManager.opticalTargeter.sensorTurret.pitchTransform.rotation;
                    mfdPage.flybyCam.fieldOfView = 60;
                    mfdPage.behaviorText.text = "TGPOff";
                }
                else
                {
                    mfdPage.behaviorText.text = "NoTGP";
                }
            }
        }
    }
}