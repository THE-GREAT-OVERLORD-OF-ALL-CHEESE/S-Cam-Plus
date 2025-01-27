namespace CheeseMods.SCamPlus.CameraModes
{
    public class CameraMode
    {
        public string name;
        public string shownName;

        protected CameraMode(string name, string shownName)
        {
            this.name = name;
            this.shownName = shownName;
        }

        public virtual void Start(FlybyCameraMFDPage mfdPage)
        {

        }

        public virtual void LateUpdate(FlybyCameraMFDPage mfdPage)
        {

        }
    }
}