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

    public IMUData leftIMUData;
    public IMUData rightIMUData;

    // Helper classes

    [Serializable]
    public class IMUData
    {
        public float timestamp;
        public Vector3Serializable position;
        public QuaternionSerializable rotation;
        public Vector3Serializable linearVelocity;
        public Vector3Serializable angularVelocity;
        // Can be calculated from the above
        // public Vector3Serializable linearAcceleration;
    }

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
