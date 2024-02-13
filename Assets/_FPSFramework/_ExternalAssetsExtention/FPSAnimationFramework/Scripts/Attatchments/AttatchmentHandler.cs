using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class AttatchmentHandler : MonoBehaviour
    {
        [SerializeField]
        protected BaseSight _selectedSight;
        public BaseSight SelectedSight
        {
            get
            {
                return _selectedSight;
            }
        }

        protected void Awake()
        {
            SelectedSight.gameObject.SetActive(true);
        }
    }
}