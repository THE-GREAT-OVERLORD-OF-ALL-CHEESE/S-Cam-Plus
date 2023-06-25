using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

public class CameraMode_SmoothZoom : CameraMode
{
    public Actor targetActor;
    private float targetChangeTimer;
    public float trackMinTime = 0.5f;

    private float coolDownTimer;
    private bool coolDown = false;
    public float trackMaxTime = 5;
    public float coolDownLength = 10;

    private float targetFov;
    private float lastFov = 60;

    public float trackingFOV = 10;
    public float trackingRange = 2000;

    public CameraMode_SmoothZoom(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {
        mfdPage.flybyCam.cullingMask = SCamPlus.headBitmask;
        mfdPage.flybyCam.transform.parent = VRHead.instance.transform.parent;
    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        UpdateLookTarget();
        if (targetActor != null)
        {
            Vector3 offset = targetActor.position - VRHead.instance.transform.position;
            Quaternion lookQuat = Quaternion.LookRotation(offset, VRHead.instance.transform.up);
            mfdPage.flybyCam.transform.rotation = Quaternion.Slerp(mfdPage.flybyCam.transform.rotation, lookQuat, mfdPage.smoothLookRate * Time.deltaTime);

            targetFov = Mathf.Clamp(Mathf.Atan2(targetActor.physicalRadius * 8, offset.magnitude) * Mathf.Rad2Deg, SCamPlus.aceFovRange.min, SCamPlus.aceFovRange.max);
        }
        else {
            mfdPage.flybyCam.transform.localRotation = Quaternion.Slerp(mfdPage.flybyCam.transform.localRotation, VRHead.instance.transform.localRotation, mfdPage.smoothLookRate * Time.deltaTime);
            targetFov = SCamPlus.fovList[(int)SCamPlus.sCamTraverse.Field("fovIdx").GetValue()];
        }

        lastFov = Mathf.Lerp(lastFov, targetFov, mfdPage.smoothLookRate * Time.deltaTime);
        mfdPage.flybyCam.fieldOfView = lastFov;
        mfdPage.flybyCam.transform.localPosition = Vector3.Lerp(mfdPage.flybyCam.transform.localPosition, VRHead.instance.transform.localPosition, mfdPage.smoothLookRate * Time.deltaTime);
        mfdPage.behaviorText.text = "SmoothZoom";
    }

    private void UpdateLookTarget() {
        if (coolDown)
        {
            targetActor = null;
            coolDownTimer -= Time.deltaTime;
            if (coolDownTimer < 0)
            {
                coolDownTimer = trackMaxTime;
                coolDown = false;
            }
            return;
        }

        if (TargetManager.instance != null && SCamPlus.player != null)           
        {
            Actor newTarget = null;

            float distance = trackingRange;
            foreach (Actor actor in TargetManager.instance.allActors)
            {
                if (actor != null)
                {
                    Vector3 offset = actor.transform.position - VRHead.instance.transform.position;
                    if (actor.role == Actor.Roles.Air || actor.role == Actor.Roles.Missile || actor == SCamPlus.targetActor) {
                        if (actor != SCamPlus.player && Vector3.Angle(VRHead.instance.transform.forward, offset) < trackingFOV && offset.magnitude < distance)
                        {
                            distance = offset.magnitude;
                            newTarget = actor;
                        }
                    }
                }
            }

            if (newTarget != targetActor)
            {
                targetChangeTimer += Time.deltaTime;
                if (targetChangeTimer > trackMinTime)
                {
                    targetActor = newTarget;
                    targetChangeTimer = 0;
                }
            }
            else
            {
                targetChangeTimer = 0;
            }

            if (targetActor != null)
            {
                coolDownTimer -= Time.deltaTime;
                if (coolDownTimer < 0)
                {
                    coolDownTimer = coolDownLength;
                    coolDown = true;
                }
            }
            else
            {
                coolDownTimer += Time.deltaTime;
                coolDownTimer = Mathf.Min(coolDownTimer, trackMaxTime);
            }
        }
    }
}