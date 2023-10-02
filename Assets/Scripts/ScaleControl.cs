using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ScaleControl : MonoBehaviour
{
	[SerializeField] private GraphicRaycaster GR;
    
	private bool shouldReceiveTouches = true;
	private float maxScale = 2000f;
	private float minScale = 20f;
	private float currentScale = 1;


    private void Awake()
    {
		currentScale = LevelManagerGlobe.Instance.MapParent.transform.parent.localScale.x;
	}

    void Update()
	{
		if (Input.touchCount > 0)
		{
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

			if (Input.touchCount == 2)
			{
				Vector2 touchZeroPrevPos = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
				Vector2 touchOnePrevPos = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

				float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag) / 1;

				currentScale -= deltaMagnitudeDiff;
				currentScale = Mathf.Clamp(currentScale, minScale, maxScale);

				//Debug.LogWarning("SCAALE " + currentScale);
				LevelManagerGlobe.Instance.MapParent.transform.parent.localScale = new Vector3(currentScale, currentScale, currentScale);
			}
		}
	}
}