using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public struct TransformData
    {
        public TransformData(Vector3 pos, Quaternion rot, Vector3 localScale)
        {
            this.Position = pos;
            this.Rotation = rot;
            this.LocalScale = localScale;
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 LocalScale;

        public void SetTransformData(GameObject obj)
        {
            obj.transform.position = this.Position;
            obj.transform.rotation = this.Rotation;
            obj.transform.localScale = this.LocalScale;
        }
    }
}