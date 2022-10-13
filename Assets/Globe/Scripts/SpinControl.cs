using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpinControl : MonoBehaviour {

	public delegate void TutorialOver();
	public static event TutorialOver tutorialDelegate;

	[HideInInspector] public GameObject mapParent;
	[SerializeField] private GraphicRaycaster GR;
	[SerializeField] GameObject rotationAnimationObject;
	[SerializeField] GameObject zoomAnimationObject;
	[SerializeField] GameObject canvas;
    [SerializeField] SphereCollider globeSphereCollider;

	private Touch initTouch;
    private float keepSpinning = 0.0f;
	private bool shouldReceiveTouches = true;
	private float currentScale = 1.0f;
	private float maxScale = 20f;
	private float minScale = 0.2f;
	private float tutorialScaleStart = 0.0f;
    private bool canSpin = true;

	// 0 - no tutorial
	// 1 - tutorial start
	// 2 - tutorial second step

	private int tutorialStep = 0;
	private GameObject chatBubble;
	private float oldEulAngles;
    private int swipeDirection = 1;
	
	void Update () {

		if (keepSpinning != 0.0f) {

			mapParent.transform.Rotate (new Vector3(0f, keepSpinning*3f, 0f) * Time.deltaTime);
			keepSpinning = keepSpinning > 0 ? keepSpinning - 0.1f : keepSpinning + 0.1f;

			if (keepSpinning < 0.1 && keepSpinning > -0.1) {
				keepSpinning = 0.0f;
			}
		}

		if (Input.touchCount > 0) {

			keepSpinning = 0.0f;

			Touch touchZero = Input.touches [0];

			if (shouldReceiveTouches == false) {
				if (touchZero.phase == TouchPhase.Ended) {
					shouldReceiveTouches = true;
				}
				return;
			}

			PointerEventData ped = new PointerEventData(null);
			ped.position = touchZero.position;
			List<RaycastResult> results = new List<RaycastResult>();

			GR.Raycast(ped, results);
			if (results.Count > 0) {

				if (touchZero.phase == TouchPhase.Began) {
					shouldReceiveTouches = false;
				}
				return;
			}

            if (Input.touchCount == 1 && canSpin == true && (tutorialStep == 0 || tutorialStep == 1)) {

				if (touchZero.phase == TouchPhase.Began) {
                    swipeDirection = isCameraInside() == true ? -1 : 1;
					initTouch = touchZero;

				} else if (touchZero.phase == TouchPhase.Moved) {

					float deltaX = initTouch.position.x - touchZero.position.x;
                    mapParent.transform.Rotate (new Vector3(0f, deltaX*4f * swipeDirection, 0f) * Time.deltaTime);
					initTouch = touchZero;

				} else if (touchZero.phase == TouchPhase.Ended) {

                    keepSpinning = initTouch.deltaPosition.x * -1 * swipeDirection;
					initTouch = new Touch ();
				}
			}
			else if (Input.touchCount == 2 && (tutorialStep == 0 || tutorialStep == 2)) {

                canSpin = false;
				Touch touchOne = Input.GetTouch (1);

				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				float deltaMagnitudeDiff = (prevTouchDeltaMag - touchDeltaMag)/1000;


				currentScale -= deltaMagnitudeDiff;

				if (currentScale < minScale) {
					currentScale = minScale;
				}
				else if (currentScale > maxScale) {
					currentScale = maxScale;
				}

				mapParent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
			}
		}
        else {
            canSpin = true;
        }

		if (tutorialStep == 2 && Mathf.Abs(currentScale - tutorialScaleStart) >= 0.12f ) {
			Handheld.Vibrate ();
			tutorialStep = 0;
			zoomAnimationObject.SetActive (false);

			chatBubble.GetComponentInChildren<Text>().text = "Excellent! This is everything.\n Remember you can replace the globe anytime\nas well as change it's texture.";

			StartCoroutine (finishTutorial());
		}

		if (tutorialStep == 1) {

			float rotValue = Mathf.Abs (oldEulAngles - mapParent.transform.rotation.y);

            if ( rotValue > 0.4f) {
				Handheld.Vibrate ();
				tutorialStep = 2;
				rotationAnimationObject.SetActive (false);
				zoomAnimationObject.SetActive (true);
				tutorialScaleStart = currentScale;
				chatBubble.GetComponentInChildren<Text>().text = "Easy right? Now try to zoom in and out.";
			}
		}
	}

	IEnumerator finishTutorial() {

		yield return new WaitForSecondsRealtime (5f);

		if (chatBubble != null) {
			Destroy (chatBubble);
			chatBubble = null;
		}

		tutorialDelegate ();
	}

	bool isCameraInside() {
        return globeSphereCollider.bounds.Contains(Camera.main.transform.position);
	}

	public void disable() {
		keepSpinning = 0.0f;
		enabled = false;
	}

	public void enable(bool tut) {
		tutorialStep = tut == false ? 0 : 1;
		keepSpinning = 0.0f;
		enabled = true;

		if (tutorialStep == 1) {

			chatBubble = (GameObject)Instantiate(Resources.Load("Prefabs/ChatBubble"));
            chatBubble.transform.SetParent(canvas.transform, false);

			chatBubble.GetComponentInChildren<Text>().text = "Now lets try spinning the Globe around.\nSwipe your finger on the screen and see what happens.";
			chatBubble.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (20f, -20f);

			rotationAnimationObject.SetActive (true);

			oldEulAngles = mapParent.transform.rotation.y;
		}
	}

	public bool GetShouldReceiveTouches() {
		return shouldReceiveTouches;
	}
}
