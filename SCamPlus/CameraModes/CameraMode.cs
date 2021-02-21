using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CameraMode
{
    public string name;
    public string shownName;

    protected CameraMode(string name, string shownName) {
        this.name = name;
        this.shownName = shownName;
    }

    public virtual void Start(FlybyCameraMFDPage mfdPage) {
    
    }

    public virtual void LateUpdate(FlybyCameraMFDPage mfdPage)
    {

    }
}
