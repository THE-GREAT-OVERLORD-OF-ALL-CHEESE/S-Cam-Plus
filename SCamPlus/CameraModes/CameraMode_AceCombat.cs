using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CameraMode_AceCombat : CameraMode
{
    Vector3 lastOffset;
    Quaternion lastRotation;
    float lastFov = 60;

    public UnityAction<float> aceMinFov_changed;
    public UnityAction<float> aceMaxFov_changed;
    public static MinMax aceFovRange = new MinMax(5, 60);
    public static float aceMaxDistance;

    public UnityAction<float> acePosLerp_changed;
    public static float acePosLerp = 5;

    public UnityAction<float> aceRotLerp_changed;
    public static float aceRotLerp = 5;

    public UnityAction<float> aceZoomLead_changed;
    public static float aceZoomLead = 2;

    public UnityAction<float> aceZoomLerp_changed;
    public static float aceZoomLerp = 0.5f;

    public CameraMode_AceCombat(string name, string shownName) : base(name, shownName)
    {

    }

    public override Settings SpawnSettingsMenu(Settings settings)
    {
        settings.CreateCustomLabel("AceCombat Camera Settings");

        aceMinFov_changed += aceMinFov_Setting;
        settings.CreateCustomLabel("Minimum FOV:");
        settings.CreateFloatSetting("(Default = 5)", aceMinFov_changed, aceFovRange.min, 5, 100);

        aceMinFov_changed += aceMaxFov_Setting;
        settings.CreateCustomLabel("Maximum FOV:");
        settings.CreateFloatSetting("(Default = 60)", aceMaxFov_changed, aceFovRange.max, 5, 100);

        acePosLerp_changed += acePosLerp_Setting;
        settings.CreateCustomLabel("Position Lerp:");
        settings.CreateFloatSetting("(Default = 5)", acePosLerp_changed, acePosLerp, 0, 100);

        aceRotLerp_changed += aceRotLerp_Setting;
        settings.CreateCustomLabel("Rotation Lerp:");
        settings.CreateFloatSetting("(Default = 5)", aceRotLerp_changed, aceRotLerp, 0, 100);

        aceZoomLead_changed += aceZoomLead_Setting;
        settings.CreateCustomLabel("Zoom Lead (in secconds):");
        settings.CreateFloatSetting("(Default = 2)", aceZoomLead_changed, aceZoomLead, 0, 10);

        aceZoomLerp_changed += aceZoomLerp_Setting;
        settings.CreateCustomLabel("Zoom Lerp:");
        settings.CreateFloatSetting("(Default = 0.5f)", aceZoomLerp_changed, aceZoomLerp, 0, 100);

        settings.CreateCustomLabel("");

        return settings;
    }

    public void aceMinFov_Setting(float newval)
    {
        aceFovRange.min = newval;
    }

    public void aceMaxFov_Setting(float newval)
    {
        aceFovRange.max = newval;
    }

    public void acePosLerp_Setting(float newval)
    {
        acePosLerp = newval;
    }

    public void aceRotLerp_Setting(float newval)
    {
        aceRotLerp = newval;
    }

    public void aceZoomLead_Setting(float newval)
    {
        aceZoomLead = newval;
    }

    public void aceZoomLerp_Setting(float newval)
    {
        aceZoomLerp = newval;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {

    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (SCamPlus.player != null)
        {
            Vector3 offset = (SCamPlus.player.transform.position - SCamPlus.GetTargetPos()).normalized * Mathf.Tan((90 - lastFov) * Mathf.Deg2Rad) * 30;
            Vector3 average = (SCamPlus.player.transform.position + SCamPlus.GetTargetPos()) / 2;
            if (offset.magnitude > aceMaxDistance)
            {
                offset = offset.normalized * aceMaxDistance;
            }

            Vector3 targetOffset = offset;
            Quaternion targetRotation = Quaternion.LookRotation(average - SCamPlus.player.transform.position + targetOffset);
            targetOffset += targetRotation * (SCamPlus.GetUpVector() * 10);
            targetRotation = Quaternion.LookRotation(average - (SCamPlus.player.transform.position + targetOffset));

            float distance = ((SCamPlus.player.transform.position + lastOffset) - SCamPlus.GetTargetPos()).magnitude + Vector3.Dot(-offset.normalized, SCamPlus.GetTargetVel() - SCamPlus.player.velocity) * SCamPlus.aceZoomLead;
            float targetFov = Mathf.Clamp(Mathf.Atan2(50, distance) * Mathf.Rad2Deg, SCamPlus.aceFovRange.min, SCamPlus.aceFovRange.max);
            mfdPage.flybyCam.fieldOfView = Mathf.Lerp(SCamPlus.lastFov, targetFov, Time.deltaTime * SCamPlus.aceZoomLerp);

            lastOffset = Vector3.Slerp(lastOffset, targetOffset, Time.deltaTime * SCamPlus.acePosLerp);
            mfdPage.flybyCam.transform.position = SCamPlus.player.transform.position + lastOffset;
            mfdPage.flybyCam.transform.rotation = Quaternion.Slerp(lastRotation, targetRotation, Time.deltaTime * SCamPlus.aceRotLerp);

            lastRotation = mfdPage.flybyCam.transform.rotation;
            lastFov = mfdPage.flybyCam.fieldOfView;

            if (SCamPlus.targetActor != null)
            {
                shownName = "AceCombat";
            }
            else
            {
                shownName = "NoTgt";
            }
        }
        else
        {
            shownName = "NoPlayer";
        }
    }
}
