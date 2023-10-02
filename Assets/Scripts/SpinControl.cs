using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class SpinControl : MonoBehaviour
{
	[SerializeField] private GraphicRaycaster GR;
    [SerializeField] SphereCollider globeSphereCollider;
    
	private Touch initTouch;
    private float keepSpinning = 0.0f;
	private bool shouldReceiveTouches = true;
	private int swipeDirection = 1;


    void Update()
	{
		if (keepSpinning != 0.0f)
		{
			LevelManagerGlobe.Instance.MapParent.transform.Rotate(new Vector3(0f, keepSpinning * 3f, 0f) * Time.deltaTime);
			keepSpinning = keepSpinning > 0 ? keepSpinning - 0.1f : keepSpinning + 0.1f;

			if (keepSpinning < 0.1 && keepSpinning > -0.1)
			{
				keepSpinning = 0.0f;
			}
		}

		if (Input.touchCount > 0)
		{
			keepSpinning = 0.0f;

			if (shouldReceiveTouches == false)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					shouldReceiveTouches = true;
				}
				return;
			}

			PointerEventData ped = new PointerEventData(null);
			ped.position = Input.GetTouch(0).position;
			List<RaycastResult> results = new List<RaycastResult>();

			GR.Raycast(ped, results);
			if (results.Count > 0)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					shouldReceiveTouches = false;
				}
				return;
			}

			if (Input.touchCount == 1)
			{
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					swipeDirection = IsCameraInside() == true ? -1 : 1;
					initTouch = Input.GetTouch(0);
				}
				else if (Input.GetTouch(0).phase == TouchPhase.Moved)
				{
					float deltaX = initTouch.position.x - Input.GetTouch(0).position.x;
					LevelManagerGlobe.Instance.MapParent.transform.Rotate(new Vector3(0f, deltaX * 4f * swipeDirection, 0f) * Time.deltaTime);
					initTouch = Input.GetTouch(0);
				}
				else if (Input.GetTouch(0).phase == TouchPhase.Ended)
				{
					keepSpinning = initTouch.deltaPosition.x * -1 * swipeDirection;
					initTouch = new Touch();
				}
				//Debug.LogWarning("ROTATING");
			}
		}
	}

	bool IsCameraInside() 
	{
        return globeSphereCollider.bounds.Contains(LevelManagerGlobe.Instance.cameraRef.transform.position);
	}
}