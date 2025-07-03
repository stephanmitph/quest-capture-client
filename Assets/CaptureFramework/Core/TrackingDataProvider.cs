
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using PassthroughCameraSamples;
using UnityEngine;

public class TrackingDataProvider : MonoBehaviour
{
    [Header("Hand Tracking")]
    [SerializeField] private OVRHand leftOVRHand;
    [SerializeField] private OVRHand rightOVRHand;
    [SerializeField] private OVRSkeleton leftOVRSkeleton;
    [SerializeField] private OVRSkeleton rightOVRSkeleton;

    public TrackingData CaptureTrackingData(int frameId)
    {
        TrackingData data = new TrackingData();

        data.frameId = frameId;
        data.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Pose headPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
        data.headPosition = new TrackingData.Vector3Serializable(headPose.position);
        data.headRotation = new TrackingData.QuaternionSerializable(headPose.rotation);

        data.leftHand = CaptureHandData(leftOVRHand, leftOVRSkeleton, OVRInput.Controller.LHand);
        data.rightHand = CaptureHandData(rightOVRHand, rightOVRSkeleton, OVRInput.Controller.RHand);

        data.leftIMUData = CaptureIMUData(OVRInput.Controller.LTouch);
        data.rightIMUData = CaptureIMUData(OVRInput.Controller.RTouch);

        return data;
    }

    private TrackingData.IMUData CaptureIMUData(OVRInput.Controller controller)
    {
        TrackingData.IMUData imuData = new TrackingData.IMUData();

        imuData.isTracked = OVRInput.GetControllerPositionTracked(controller);
        if (imuData.isTracked)
        {
            imuData.position = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerPosition(controller));
            imuData.rotation = new TrackingData.QuaternionSerializable(OVRInput.GetLocalControllerRotation(controller));
            imuData.linearVelocity = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerVelocity(controller));
            imuData.angularVelocity = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerAngularVelocity(controller));
            // imuData.linearAcceleration = new TrackingData.Vector3Serializable(OVRInput.GetLocalControllerAcceleration(controller));
        }

        return imuData;
    }

    private TrackingData.HandData CaptureHandData(OVRHand ovrHand, OVRSkeleton ovrSkeleton, OVRInput.Controller controller)
    {
        TrackingData.HandData handData = new TrackingData.HandData();

        handData.isTracked = ovrHand != null && ovrHand.IsTracked;
        if (handData.isTracked)
        {
            handData.position = new TrackingData.Vector3Serializable(ovrHand.transform.position);
            handData.rotation = new TrackingData.QuaternionSerializable(ovrHand.transform.rotation);
        }

        handData.hasSkeletalData = false;
        if (handData.isTracked && ovrSkeleton != null && ovrSkeleton.IsInitialized && ovrSkeleton.Bones.Count > 0)
        {
            handData.hasSkeletalData = true;

            int boneCount = ovrSkeleton.Bones.Count;
            handData.bones = new TrackingData.HandData.BoneData[boneCount];
            handData.fingerPinchStates = new bool[5];
            handData.fingerPinchStrengths = new float[5];
            handData.fingerPinchConfidence = new float[5];

            for (int i = 0; i < boneCount; i++)
            {
                OVRBone bone = ovrSkeleton.Bones[i];
                Vector3 position = bone.Transform.localPosition;
                Quaternion rotation = bone.Transform.localRotation;

                handData.bones[i] = new TrackingData.HandData.BoneData
                {
                    id = i,
                    position = new TrackingData.Vector3Serializable(position),
                    rotation = new TrackingData.QuaternionSerializable(rotation)
                };
            }
            for (int i = 0; i < 5; i++)
            {
                handData.fingerPinchStates[i] = ovrHand.GetFingerIsPinching((OVRHand.HandFinger)i);
                handData.fingerPinchStrengths[i] = ovrHand.GetFingerPinchStrength((OVRHand.HandFinger)i);
                handData.fingerPinchConfidence[i] = (float)ovrHand.GetFingerConfidence((OVRHand.HandFinger)i);
            }
        }
        else
        {
            handData.hasSkeletalData = false;
            handData.bones = new TrackingData.HandData.BoneData[0];
            handData.fingerPinchStrengths = new float[0];
            handData.fingerPinchConfidence = new float[0];
        }

        return handData;
    }
}