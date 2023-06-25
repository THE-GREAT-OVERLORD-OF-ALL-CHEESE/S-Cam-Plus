using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(EjectionSeat), "Start")]
class EjectionSeat_Start
{
    [HarmonyPostfix]
    public static void Postfix(EjectionSeat __instance)
    {
        if (SCamPlus.playerVisibility == false) {
            __instance.gameObject.AddComponent<PlayerVisibilityManager>();
        }
    }
}

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

        SCamPlus.sCamTraverse.Field("behavior").SetValue(0);//baha update broke loading this value, so im fixing it
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

        SCamPlus.currentBehaviour++;
        if ((int)SCamPlus.currentBehaviour > 6 + SCamPlus.cameraModes.Count) {
            SCamPlus.currentBehaviour = 0;
            Debug.Log("Too large, reseting to 0");
        }
        Debug.Log("S-Cam Mode is now " + SCamPlus.currentBehaviour.ToString());

        //SCamPlus.sCamTraverse.Method("UpdateBehaviorText");
        SCamPlus.UpdateBehaviourText(__instance);

        if ((bool)SCamPlus.sCamTraverse.Field("flyCamEnabled").GetValue()) {
            if ((int)SCamPlus.currentBehaviour <= 6)
            {
                //SCamPlus.sCamTraverse.Method("SetupFlybyPosition").GetValue();
                SCamPlus.sCamTraverse.Field("behavior").SetValue((int)SCamPlus.currentBehaviour);//we can only update this value to be within the range the game normally expects, or it causes issues when loading saves
                __instance.EnableCamera();
            }
            else {
                __instance.flybyCam.cullingMask = SCamPlus.normalBitmask;
                __instance.cameraModel.SetActive(false);

                SCamPlus.cameraModes[(int)SCamPlus.currentBehaviour - 7].Start(__instance);
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


        if ((int)SCamPlus.currentBehaviour > 6) {
            SCamPlus.cameraModes[(int)SCamPlus.currentBehaviour - 7].LateUpdate(__instance);

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
        else {
            return true;
        }
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

    [HarmonyPatch(typeof(FlybyCameraMFDPage), "OnQuickload")]
    class Patch5
    {
        [HarmonyPrefix]
        public static bool PreFix(FlybyCameraMFDPage __instance, ConfigNode qsNode)
        {
            ConfigNode node = qsNode.GetNode("SpectatorCamera");
            if (node != null && node.GetValue<bool>("flyCamEnabled"))
            {
                __instance.EnableCamera();
            }
            return false;
        }
    }
}
