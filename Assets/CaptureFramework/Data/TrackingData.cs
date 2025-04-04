using System;
using UnityEngine;


[Serializable]
public class TrackingData
{
    public int frameId;
    public long timestamp;
    public Vector3Serializable headPosition;
    public QuaternionSerializable headRotation;
    public HandData leftHand;
    public HandData rightHand;

    [Serializable]
    public class HandData
    {
        public bool isTracked;
        public Vector3Serializable wristPosition;
        public QuaternionSerializable wristRotation;

        // New skeletal data
        public bool hasSkeletalData;
        public BoneData[] bones;
        // humb, Index, Middle, Ring, Pinky,
        public bool[] fingerPinchStates; // Pinch state per finger
        public float[] fingerPinchStrengths; // Pinch strength per finger
        public float[] fingerPinchConfidence; // Pinch strength confidence per finger

        [Serializable]
        public struct BoneData
        {
            public int id;
            public Vector3Serializable position;
            public QuaternionSerializable rotation;
        }
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