using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // 토글 기능은 2D와 3D가 인터페이스(IToggle)로 묶여있으므로 2D든 3D든 상관없이 사용 가능합니다.
    public class VTSToggleGroup : MonoBehaviour
    {
        protected List<IToggle> toggleList = new List<IToggle>();

        [Header("=== VTSToggleGroup ===")]
        [SerializeField]
        protected bool isWithoutNotify;

        public virtual void Register(IToggle targetToggle)
        {
            toggleList.Add(targetToggle);
        }

        public virtual void SetOffWithout(IToggle targetToggle)
        {
            foreach (IToggle toggle in toggleList)
            {
                if (toggle != targetToggle)
                {
                    if (isWithoutNotify == true)
                    {
                        toggle.SetWithoutNotify(false);
                    }
                    else
                    {
                        toggle.IsOn = false;
                    }
                }
            }
        }
    }
}