using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;


public class HandSwipe : MonoBehaviour
{
    [SerializeField]
    private TrackedHandJoint trackedHandJoint = TrackedHandJoint.Palm;

    [SerializeField]
    private Handedness trackedHandedness = Handedness.Left;

    [SerializeField]
    private SwipeGesture swiperRef;

    [SerializeField]
    private Camera cameraRef;


    private MixedRealityPose handPose;
    private Vector3 lastHandPosition;

    private Vector3 handStartPoint;
    private bool allowSwipe;


    void Update()
    {
        HandJointUtils.TryGetJointPose(trackedHandJoint, trackedHandedness, out handPose);

        Vector3 moveDir = handPose.Position - lastHandPosition;

        float direction = Vector3.Dot(cameraRef.transform.right, moveDir.normalized);
        //print("DIR: " + direction);

        if (trackedHandedness == Handedness.Left)
        {
            if (direction > 0)
            {
                CheckForSwipe(moveDir, direction);
            }
        }
        else if (trackedHandedness == Handedness.Right)
        {
            if (direction < 0)
            {
                CheckForSwipe(moveDir, direction);
            }
        }

        lastHandPosition = handPose.Position;
    }

    void CheckForSwipe(Vector3 moveDir, float direction)
    {
        float vel = moveDir.magnitude * Time.deltaTime * 100;
        //print("VEL: " + vel);
        if (vel > 0.15f)
        {
            //print("High velocity!");
            float dist = Vector3.Distance(handPose.Position, handStartPoint);
            if (allowSwipe && dist > 0.15f)
            {
                if (Mathf.Abs(handStartPoint.y - handPose.Position.y) < 0.25f)
                {
                    swiperRef.keepSpinning -= 50 * (direction > 0 ? 1 : -1);
                    swiperRef.keepSpinning = Mathf.Clamp(swiperRef.keepSpinning, -150, 150);
                    allowSwipe = false;
                    print("SPIN");
                }
                else
                {
                    print("Too different height");
                }
            }
        }
        else
        {
            allowSwipe = true;
            handStartPoint = handPose.Position;
        }
    }
}
