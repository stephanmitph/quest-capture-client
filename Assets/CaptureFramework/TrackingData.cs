using System;
using UnityEngine;

[Serializable]
public class TrackingData
{
    public long timestamp;
    public Vector3Serializable headPosition;
    public QuaternionSerializable headRotation;
    public HandData leftHand;
    public HandData rightHand;

    [Serializable]
    public class HandData
    {
        public bool isTracked;
        public Vector3Serializable position;
        public QuaternionSerializable rotation;
        public float pinchStrength;
    }

    [Serializable]
    public struct Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }

    [Serializable]
    public struct QuaternionSerializable
    {
        public float x, y, z, w;

        public QuaternionSerializable(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }
    }
}