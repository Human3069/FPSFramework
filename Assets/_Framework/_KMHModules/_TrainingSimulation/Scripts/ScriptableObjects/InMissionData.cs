using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class InMissionData : ScriptableObject
    {
        private const string LOG_FORMAT = "<color=white><b>[InMissionData]</b></color> {0}";

        [SerializedDictionary("MissionType", "StartPointUID")]
        [SerializeField]
        protected SerializedDictionary<VTSManager.MissionType, string> StartPointUIDDic = new SerializedDictionary<VTSManager.MissionType, string>();
        [SerializeField]
        protected string FallbackPointUID;

        public virtual void MoveCameraToStartPoint()
        {
            UIDObject uidObj = GetStartPoint();
            Transform targetCameraT = CameraEx.Display_0_Camera.transform;

            if (uidObj._BaseObj is Utill_CameraPath)
            {
                Utill_CameraPath path = uidObj._BaseObj as Utill_CameraPath;

                targetCameraT.position = path.pointList[0].position;
                targetCameraT.rotation = path.pointList[0].rotation;
            }
            else
            {
                targetCameraT.position = uidObj.transform.position;
                targetCameraT.rotation = uidObj.transform.rotation;
            }
        }

        public virtual UIDObject GetStartPoint()
        {
            string targetUid;
            VTSManager.MissionType missionType = (VTSManager.MissionType)VTSManager.Instance._MissionType;

            if (StartPointUIDDic.ContainsKey(missionType) == true)
            {
                targetUid = StartPointUIDDic[missionType];
            }
            else
            {
                targetUid = FallbackPointUID;
            }

            return UIDManager.Instance.GetUIDObject(targetUid);
        }
    }
}