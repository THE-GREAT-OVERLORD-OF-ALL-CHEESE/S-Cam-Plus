using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CameraMode_AceCombat : CameraMode
{
    public CameraMode_AceCombat(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {

    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (SCamPlus.player != null)
        {
            Vector3 offset = (SCamPlus.player.transform.position - SCamPlus.GetTargetPos()).normalized * Mathf.Tan((90 - SCamPlus.lastFov) * Mathf.Deg2Rad) * 30;
            Vector3 average = (SCamPlus.player.transform.position + SCamPlus.GetTargetPos()) / 2;
            if (offset.magnitude > SCamPlus.aceMaxDistance)
            {
                offset = offset.normalized * SCamPlus.aceMaxDistance;
            }

            Vector3 targetOffset = offset;
            Quaternion targetRotation = Quaternion.LookRotation(average - SCamPlus.player.transform.position + targetOffset);
            targetOffset += targetRotation * (SCamPlus.GetUpVector() * 10);
            targetRotation = Quaternion.LookRotation(average - (SCamPlus.player.transform.position + targetOffset));

            float distance = ((SCamPlus.player.transform.position + SCamPlus.lastOffset) - SCamPlus.GetTargetPos()).magnitude + Vector3.Dot(-offset.normalized, SCamPlus.GetTargetVel() - SCamPlus.player.velocity) * SCamPlus.aceZoomLead;
            float targetFov = Mathf.Clamp(Mathf.Atan2(50, distance) * Mathf.Rad2Deg, SCamPlus.aceFovRange.min, SCamPlus.aceFovRange.max);
            mfdPage.flybyCam.fieldOfView = Mathf.Lerp(SCamPlus.lastFov, targetFov, Time.deltaTime * SCamPlus.aceZoomLerp);

            SCamPlus.lastOffset = Vector3.Slerp(SCamPlus.lastOffset, targetOffset, Time.deltaTime * SCamPlus.acePosLerp);
            mfdPage.flybyCam.transform.position = SCamPlus.player.transform.position + SCamPlus.lastOffset;
            mfdPage.flybyCam.transform.rotation = Quaternion.Slerp(SCamPlus.lastRotation, targetRotation, Time.deltaTime * SCamPlus.aceRotLerp);

            SCamPlus.lastRotation = mfdPage.flybyCam.transform.rotation;
            SCamPlus.lastFov = mfdPage.flybyCam.fieldOfView;

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
