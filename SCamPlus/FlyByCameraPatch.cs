using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(FlybyCameraMFDPage), "Awake")]
class Patch0
{
    [HarmonyPostfix]
    public static void Postfix(FlybyCameraMFDPage __instance)
    {
        SCamPlus.sCam = __instance;
        SCamPlus.sCamTraverse = Traverse.Create(__instance);
        SCamPlus.fovList = (float[])SCamPlus.sCamTraverse.Field("fovs").GetValue();
    }
}


[HarmonyPatch(typeof(FlybyCameraMFDPage), "Start")]
class Patch5
{
    [HarmonyPostfix]
    public static void Postfix(FlybyCameraMFDPage __instance)
    {
        SCamPlus.normalBitmask = __instance.flybyCam.cullingMask;
        SCamPlus.fpvBitmask = __instance.flybyCam.cullingMask;
        SCamPlus.fpvBitmask |= 272629760;
        SCamPlus.headBitmask = __instance.flybyCam.cullingMask;
        SCamPlus.headBitmask |= 272629760;
        SCamPlus.headBitmask ^= 2097152;
        SCamPlus.headBitmask ^= 1048576;
    }
}

[HarmonyPatch(typeof(FlybyCameraMFDPage), "UpdateBehaviorText")]
class Patch1
{
    [HarmonyPrefix]
    public static bool Prefix(FlybyCameraMFDPage __instance)
    {
        SCamPlus.UpdateBehaviourText(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(FlybyCameraMFDPage), "NextMode")]
class Patch2
{
    [HarmonyPrefix]
    public static bool Prefix(FlybyCameraMFDPage __instance)
    {
        if ((bool)SCamPlus.sCamTraverse.Field("randomModes").GetValue())
            return false;

        Debug.Log("Incrementing S-Cam mode");
        SCamPlus.SpectatorBehaviorsPlus currentBehaviour = (SCamPlus.SpectatorBehaviorsPlus)SCamPlus.sCamTraverse.Field("behavior").GetValue();
        currentBehaviour++;
        if ((int)currentBehaviour >= SCamPlus.ammountOfModes) {
            currentBehaviour = 0;
            Debug.Log("Too large, reseting to 0");
        }
        Debug.Log("S-Cam Mode is now " + currentBehaviour.ToString());
        SCamPlus.sCamTraverse.Field("behavior").SetValue((int)currentBehaviour);

        //SCamPlus.sCamTraverse.Method("UpdateBehaviorText");
        SCamPlus.UpdateBehaviourText(__instance);

        if ((bool)SCamPlus.sCamTraverse.Field("flyCamEnabled").GetValue()) {
            if ((int)currentBehaviour <= 6)
            {
                //SCamPlus.sCamTraverse.Method("SetupFlybyPosition").GetValue();
                __instance.EnableCamera();
            }
            else {
                switch (currentBehaviour)
                {
                    case SCamPlus.SpectatorBehaviorsPlus.HUD:
                        __instance.flybyCam.cullingMask = SCamPlus.fpvBitmask;
                        break;
                    case SCamPlus.SpectatorBehaviorsPlus.FreeCam:
                        SCamPlus.position = VTMapManager.WorldToGlobalPoint(__instance.flybyCam.transform.position);
                        break;
                    default:
                        __instance.flybyCam.cullingMask = SCamPlus.normalBitmask;
                        break;
                }
            }
        }
        return false;
    }
}

[HarmonyPatch(typeof(FlybyCameraMFDPage), "LateUpdate")]
class Patch3
{
    [HarmonyPrefix]
    public static bool Prefix(FlybyCameraMFDPage __instance)
    {
        if (!(bool)SCamPlus.sCamTraverse.Field("flyCamEnabled").GetValue())
            return false;
        __instance.flybyCam.nearClipPlane = 0.02f;        
        __instance.flybyCam.fieldOfView = SCamPlus.fovList[(int)SCamPlus.sCamTraverse.Field("fovIdx").GetValue()];

        SCamPlus.SpectatorBehaviorsPlus currentBehaviour = (SCamPlus.SpectatorBehaviorsPlus)SCamPlus.sCamTraverse.Field("behavior").GetValue();
        switch (currentBehaviour) {
            case SCamPlus.SpectatorBehaviorsPlus.TGP:
                if (SCamPlus.tgpMFD != null)
                {
                    __instance.flybyCam.transform.position = SCamPlus.tgpMFD.targetingCamera.transform.position;
                    __instance.flybyCam.transform.rotation = SCamPlus.tgpMFD.targetingCamera.transform.rotation;
                    __instance.flybyCam.fieldOfView = SCamPlus.tgpMFD.targetingCamera.fieldOfView;
                    __instance.behaviorText.text = "TGP";
                }
                else
                {
                    SCamPlus.tgpMFD = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<TargetingMFDPage>();
                    if (__instance.weaponManager.opticalTargeter != null)
                    {
                        __instance.flybyCam.transform.position = __instance.weaponManager.opticalTargeter.sensorTurret.pitchTransform.position;
                        __instance.flybyCam.transform.rotation = __instance.weaponManager.opticalTargeter.sensorTurret.pitchTransform.rotation;
                        __instance.flybyCam.fieldOfView = 60;
                        __instance.behaviorText.text = "TGPOff";
                    }
                    else
                    {
                        __instance.behaviorText.text = "NoTGP";
                    }
                }
                break;
            case SCamPlus.SpectatorBehaviorsPlus.AceCombat:
                if (SCamPlus.player != null)
                {
                    Vector3 offset = (SCamPlus.player.transform.position - SCamPlus.GetTargetPos()).normalized * Mathf.Tan((90 - SCamPlus.lastFov) * Mathf.Deg2Rad) * 30;
                    Vector3 average = (SCamPlus.player.transform.position + SCamPlus.GetTargetPos()) / 2;
                    if (offset.magnitude > SCamPlus.aceMaxDistance) {
                        offset = offset.normalized * SCamPlus.aceMaxDistance;
                    }

                    Vector3 targetOffset = offset;
                    Quaternion targetRotation = Quaternion.LookRotation(average - SCamPlus.player.transform.position + targetOffset);
                    targetOffset += targetRotation * (SCamPlus.GetUpVector() * 10);
                    targetRotation = Quaternion.LookRotation(average - (SCamPlus.player.transform.position + targetOffset));

                    float distance = ((SCamPlus.player.transform.position + SCamPlus.lastOffset) - SCamPlus.GetTargetPos()).magnitude + Vector3.Dot(-offset.normalized, SCamPlus.GetTargetVel() - SCamPlus.player.velocity) * SCamPlus.aceZoomLead;
                    float targetFov = Mathf.Clamp(Mathf.Atan2(50, distance) * Mathf.Rad2Deg, SCamPlus.aceFovRange.min, SCamPlus.aceFovRange.max);
                    __instance.flybyCam.fieldOfView = Mathf.Lerp(SCamPlus.lastFov, targetFov, Time.deltaTime * SCamPlus.aceZoomLerp);

                    SCamPlus.lastOffset = Vector3.Slerp(SCamPlus.lastOffset, targetOffset, Time.deltaTime * SCamPlus.acePosLerp);
                    __instance.flybyCam.transform.position = SCamPlus.player.transform.position + SCamPlus.lastOffset;
                    __instance.flybyCam.transform.rotation = Quaternion.Slerp(SCamPlus.lastRotation, targetRotation, Time.deltaTime * SCamPlus.aceRotLerp);

                    SCamPlus.lastRotation = __instance.flybyCam.transform.rotation;
                    SCamPlus.lastFov = __instance.flybyCam.fieldOfView;

                    if (SCamPlus.targetActor != null)
                    {
                        __instance.behaviorText.text = "AceCombat";
                    }
                    else {
                        __instance.behaviorText.text = "NoTgt";
                    }
                }
                else
                {
                    __instance.behaviorText.text = "NoPlayer";
                }
                break;
            case SCamPlus.SpectatorBehaviorsPlus.HUD:
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
                    //if (!SCamPlus.hudBracket) {
                    //    hudSize *= 0.5f;
                    //}

                    //__instance.flybyCam.fieldOfView = SCamPlus.fovList[(int)SCamPlus.sCamTraverse.Field("fovIdx").GetValue()];

                    __instance.flybyCam.transform.position = SCamPlus.player.transform.TransformPoint(hudPos);
                    __instance.flybyCam.transform.position = __instance.flybyCam.transform.position - SCamPlus.player.transform.forward * hudOffset;

                    //__instance.flybyCam.fieldOfView = 30;
                    //__instance.flybyCam.transform.position = __instance.flybyCam.transform.position - SCamPlus.player.transform.forward * Mathf.Tan((90 - __instance.flybyCam.fieldOfView) * Mathf.Deg2Rad) * hudSize;
                    // __instance.flybyCam.nearClipPlane = Mathf.Max(Mathf.Tan((90 - __instance.flybyCam.fieldOfView) * Mathf.Deg2Rad) * hudSize - 0.3f, 0.02f);
                    __instance.flybyCam.transform.rotation = SCamPlus.player.transform.rotation;
                    __instance.behaviorText.text = "HUD";
                }
                else
                {
                    __instance.behaviorText.text = "NoHUD";
                }
                break;
            case SCamPlus.SpectatorBehaviorsPlus.ExtCam:
                if (SCamPlus.extCamManager != null)
                {
                    Camera currentCam = SCamPlus.extCamManager.cameras[SCamPlus.extCamManager.camIdx];

                    __instance.flybyCam.transform.position = currentCam.transform.position;
                    __instance.flybyCam.transform.rotation = currentCam.transform.rotation;
                    __instance.flybyCam.fieldOfView = currentCam.fieldOfView;

                    __instance.behaviorText.text = "ExtCam";
                }
                else
                {
                    SCamPlus.extCamManager = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<ExternalCamManager>();
                    __instance.behaviorText.text = "NoExtCam";
                }
                break;
            case SCamPlus.SpectatorBehaviorsPlus.FreeCam:
                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    __instance.CycleFovs();
                }

                SCamPlus.rotation.y += Input.GetAxis("Mouse X") * SCamPlus.sensitivity/60 * __instance.flybyCam.fieldOfView;
                SCamPlus.rotation.x += Input.GetAxis("Mouse Y") * -SCamPlus.sensitivity/60 * __instance.flybyCam.fieldOfView;

                SCamPlus.rotation.x = Mathf.Clamp(SCamPlus.rotation.x, -90, 90);
                __instance.flybyCam.transform.eulerAngles = (Vector2)SCamPlus.rotation;

                float speedFactor = 10;
                if (Input.GetKey(KeyCode.LeftShift)) {
                    speedFactor = 1000;
                }
                SCamPlus.position += (__instance.flybyCam.transform.forward * Input.GetAxis("Vertical") + __instance.flybyCam.transform.right * Input.GetAxis("Horizontal")) * Time.deltaTime * speedFactor;
                __instance.flybyCam.transform.position = VTMapManager.GlobalToWorldPoint(SCamPlus.position);
                break;
            default:
                return true;
        }
        if ((bool)SCamPlus.sCamTraverse.Field("previewEnabled").GetValue() && Time.time - SCamPlus.lastRenderTime > 1f / 8f)
        {
            SCamPlus.lastRenderTime = Time.time;
            __instance.flybyCam.enabled = false;
            __instance.flybyCam.targetTexture = __instance.previewRt;
            __instance.flybyCam.Render();
            __instance.flybyCam.targetTexture = null;
            __instance.flybyCam.enabled = true;
        }
        return false;
    }

    [HarmonyPatch(typeof(FlybyCameraMFDPage), "LateUpdate")]
    class Patch4
    {
        [HarmonyPostfix]
        public static void Postfix(FlybyCameraMFDPage __instance)
        {
            if (SCamPlus.upType == SCamPlus.UpType.Stock) {
                return;
            }
            else {
                __instance.flybyCam.transform.LookAt(__instance.flybyCam.transform.position + __instance.flybyCam.transform.forward, SCamPlus.GetUpVector());
                SCamPlus.lastUp = __instance.flybyCam.transform.up;
            }
        }
    }
}
