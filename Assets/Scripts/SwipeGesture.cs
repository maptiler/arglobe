using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;


public class SwipeGesture : MonoBehaviour
{
    [SerializeField]
    private GameObject mapParent;

    private Camera cameraRef;

    public float keepSpinning;
    //private float lastGestureTimestamp = 0;

    private bool allowLeft = true;
    private bool allowRight = true;
    private Vector3 leftStartPoint;
    private Vector3 rightStartPoint;


    private void Start()
    {
        cameraRef = Camera.main;
    }

    void Update()
    {
        /*foreach (var item in CoreServices.InputSystem.DetectedControllers)
        {
            MixedRealityPose handPose;
            HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, item.ControllerHandedness, out handPose);
            var controllerVelocity = item.Velocity.normalized;
            var dotProduct = Vector3.Dot(cameraRef.transform.right, controllerVelocity);

            if (!item.IsInPointingPose)
            {
                if (item.ControllerHandedness.IsLeft())
                {
                    if (dotProduct > 0)
                    {
                        //print("Left magnitude: " + item.Velocity.normalized);
                        if (item.Velocity.magnitude > 0.5f)
                        {
                            float dist = Vector3.Distance(handPose.Position, leftStartPoint);
                            if (allowLeft && dist > 0.25f)
                            {
                                if (Mathf.Abs(leftStartPoint.y - handPose.Position.y) < 0.25f)
                                {
                                    keepSpinning -= 50;
                                    keepSpinning = Mathf.Clamp(keepSpinning, -150, 150);
                                    allowLeft = false;
                                    //print("SPIN LEFT - vel: " + item.Velocity.magnitude + " - dist: " + dist);
                                    print("SPIN LEFT");
                                }
                                else
                                {
                                    print("Too different height");
                                }
                            }
                            //print("Left distance: " + Vector3.Distance(handPose.Position, leftStartPoint));
                        }
                        else
                        {
                            allowLeft = true;
                            leftStartPoint = handPose.Position;
                        }
                        //print("Left dot: " + dotProduct);
                    }
                }
                else if (item.ControllerHandedness.IsRight())
                {
                    if (dotProduct < 0)
                    {
                        //print("Righ magnitude: " + item.Velocity.magnitude);
                        if (item.Velocity.magnitude > 0.5f)
                        {
                            float dist = Vector3.Distance(handPose.Position, rightStartPoint);
                            if (allowRight && dist > 0.25f)
                            {
                                if (Mathf.Abs(rightStartPoint.y - handPose.Position.y) < 0.25f)
                                {
                                    keepSpinning += 50;
                                    keepSpinning = Mathf.Clamp(keepSpinning, -150, 150);
                                    allowRight = false;
                                    //print("SPIN RIGHT - vel: " + item.Velocity.magnitude + " - dist: " + dist);
                                    print("SPIN RIGHT");
                                }
                                else
                                {
                                    print("Too different height");
                                }
                            }
                            //print("Right distance: " + Vector3.Distance(handPose.Position, rightStartPoint));
                        }
                        else
                        {
                            allowRight = true;
                            rightStartPoint = handPose.Position;
                        }
                    }
                }
            }
        }*/

        if (Mathf.Abs(keepSpinning) > 0.01f)
        {
            mapParent.transform.Rotate(new Vector3(0f, keepSpinning * 3f, 0f) * Time.deltaTime);
            keepSpinning = keepSpinning > 0 ? keepSpinning - 50 * Time.deltaTime : keepSpinning + 50 * Time.deltaTime;

            if (keepSpinning < 0.1 && keepSpinning > -0.1)
            {
                keepSpinning = 0.0f;
            }
        }




        return;


        /*
            if (item.Velocity.magnitude > 0.25f)
            {
                if (Time.time - lastGestureTimestamp > 0.5f)
                {
                    lastGestureTimestamp = Time.time;
                    if (item.ControllerHandedness.IsLeft())
                    {
                        /// Detect only valid gestures.
                        if (dotProduct > 0)
                        {
                            keepSpinning -= 10;//item.Velocity.magnitude * 500 * Time.deltaTime * -dotProduct;
                            keepSpinning = Mathf.Clamp(keepSpinning, -100, 100);
                            //Debug.LogWarning("START SPINNING RIGHT! " + keepSpinning);
                        }
                    }
                    else if (item.ControllerHandedness.IsRight())
                    {
                        if (dotProduct < 0)
                        {
                            keepSpinning = 10;//item.Velocity.magnitude * 500 * Time.deltaTime * -dotProduct;
                            keepSpinning = Mathf.Clamp(keepSpinning, -100, 100);
                            //Debug.LogWarning("START SPINNING LEFT! " + keepSpinning);
                        }
                    }
                }
            }
        }

        if (Mathf.Abs(keepSpinning) > 0.01f)
        {
            mapParent.transform.Rotate(new Vector3(0f, keepSpinning * 3f, 0f) * Time.deltaTime);
            keepSpinning = keepSpinning > 0 ? keepSpinning - 1 * Time.deltaTime : keepSpinning + 1 * Time.deltaTime;

            if (keepSpinning < 0.1 && keepSpinning > -0.1)
            {
                keepSpinning = 0.0f;
            }
        }
        */
    }
}
