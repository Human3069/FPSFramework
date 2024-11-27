using geniikw.DataRenderer2D;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public partial class UI_Line : UIDataMesh , ISpline
    {
        public Spline line;
        Spline ISpline.Line
        {
            get
            {
                return line;
            }
        }

        protected IEnumerable<IMesh> m_Drawer = null;
        protected override IEnumerable<IMesh> DrawerFactory
        {
            get
            {
                return m_Drawer ?? (m_Drawer = LineBuilder.Factory.Normal(this, transform).Draw());
            }
        }

        protected override void Start()
        {
            base.Start();

            line.owner = this;
            line.EditCallBack += GeometyUpdateFlagUp;
        }
        
        public static UI_Line CreateLine(Transform parent = null)
        {
            GameObject _obj = new GameObject("UILine");

            if (parent != null)
            {
                _obj.transform.SetParent(parent);
            }
            _obj.transform.localPosition = Vector3.zero;

            UI_Line line = _obj.AddComponent<UI_Line>();
            line.line = Spline.Default;
            line.Start();

            return line;
        }
    }
    
}