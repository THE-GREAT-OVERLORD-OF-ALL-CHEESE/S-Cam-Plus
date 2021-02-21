using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

public class CameraMode_FreeCam : CameraMode
{

    public CameraMode_FreeCam(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {
        SCamPlus.position = VTMapManager.WorldToGlobalPoint(mfdPage.flybyCam.transform.position);
    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mfdPage.CycleFovs();
        }

        SCamPlus.rotation.y += Input.GetAxis("Mouse X") * SCamPlus.sensitivity / 60f * mfdPage.flybyCam.fieldOfView;
        SCamPlus.rotation.x += Input.GetAxis("Mouse Y") * -SCamPlus.sensitivity / 60f * mfdPage.flybyCam.fieldOfView;

        SCamPlus.rotation.x = Mathf.Clamp(SCamPlus.rotation.x, -90, 90);
        mfdPage.flybyCam.transform.eulerAngles = (Vector2)SCamPlus.rotation;

        float speedFactor = 10;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedFactor = 1000;
        }
        SCamPlus.position += (mfdPage.flybyCam.transform.forward * Input.GetAxis("Vertical") + mfdPage.flybyCam.transform.right * Input.GetAxis("Horizontal")) * Time.deltaTime * speedFactor;
        mfdPage.flybyCam.transform.position = VTMapManager.GlobalToWorldPoint(SCamPlus.position);
        mfdPage.behaviorText.text = "FreeCam";
    }
}