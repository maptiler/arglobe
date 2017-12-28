﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;

public class GlobeControl : MonoBehaviour {

	[SerializeField] private GameObject mapParent;
	[SerializeField] private GameObject elevator;
	[SerializeField] private GameObject changeButton;
	[SerializeField] private GameObject replaceButton;
	[SerializeField] private GameObject cancelButton;
	[SerializeField] private GameObject infoButton;
	[SerializeField] private GameObject tutorialButton;
	[SerializeField] private GameObject globePositionIndicator;
	[SerializeField] private GameObject canvas;
	[SerializeField] private Material UIMaterial;
	[SerializeField] private Material infoMaterial;
    [SerializeField] private Material topCapMaterial;
    [SerializeField] private Material bottomCapMaterial;

	private GameObject currentGameObject;
	//private GameObject currentLowResGameObject;
	private GameObject sideMenu;

	private ScanningControl scanScript;
	private SpinControl spinScript;

	public static String globeName;

	// Use this for initialization
	void Start () {

		UIMaterial.color = new Color (1f, 1f, 1f, 0f);
		infoMaterial.color = new Color (1f, 1f, 1f, 0f);
        topCapMaterial.color = new Color(1f, 1f, 1f, 0f);
        bottomCapMaterial.color = new Color(1f, 1f, 1f, 0f);

		scanScript = gameObject.GetComponent<ScanningControl> ();
		scanScript.mapParent = mapParent;
		spinScript = gameObject.GetComponent<SpinControl> ();
		spinScript.mapParent = mapParent;

		defaultDetectionAndPlacement ();
	}

	private void defaultDetectionAndPlacement() {

		mapParent.SetActive (false);
		changeButton.SetActive (false);
		replaceButton.SetActive (false);
		tutorialButton.SetActive (false);
		globePositionIndicator.SetActive (false);
		infoButton.SetActive (false);

		StartCoroutine (startFirstScan ());
	}

	IEnumerator startFirstScan() {

		yield return new WaitForSeconds (1);
		scanScript.enable ();
		ScanningControl.finalStep += replaceFinalStep;
	}

	//bool test() {

	//	Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
	//	return GeometryUtility.TestPlanesAABB (planes, currentLowResGameObject.GetComponents<SphereCollider> () [0].bounds);
	//}

	//bool isCameraInside() {

	//	SphereCollider collider = currentLowResGameObject.GetComponents<SphereCollider> () [1];
	//	return collider.bounds.Contains (Camera.main.transform.position);
	//}

	//private void trackGlobePosition() {

	//	if (mapParent.activeSelf == true) {

	//		if (test() == false) {

	//			Vector3 screenPos = Camera.main.WorldToViewportPoint (mapParent.transform.position);

	//			if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1) {
	//				//globePositionIndicator.SetActive (false);
	//			} else {

	//				globePositionIndicator.SetActive (true);

	//				Vector2 onScreenPos = new Vector2(screenPos.x-0.5f, screenPos.y-0.5f)*2; //2D version, new mapping
	//				float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); //get largest offset
	//				onScreenPos = (onScreenPos/(max*2))+new Vector2(0.5f, 0.5f); //undo mapping

	//				RectTransform rect = globePositionIndicator.GetComponent<RectTransform> ();
	//				Quaternion rot = Quaternion.Euler(0f, 0f, 0f);

	//				if (onScreenPos.x == 1) {
	//					float start = 50f;
	//					float end = start - onScreenPos.y*100;
	//					rot *= Quaternion.Euler (0f, 0f, -end);
	//				} else if (onScreenPos.x == 0) {
	//					float start = 50;
	//					float end = start - onScreenPos.y*100;
	//					rot *= Quaternion.Euler (0f, 0f, 180f + end);
	//				} else if (onScreenPos.y == 1) {
	//					float start = 50;
	//					float end = start - onScreenPos.x*100;
	//					rot *= Quaternion.Euler (0f, 0f, 90f + end);
	//				} else if (onScreenPos.y == 0) {
	//					float start = 50;
	//					float end = start - onScreenPos.x*100;
	//					rot *= Quaternion.Euler (0f, 0f, -90f - end);
	//				}


	//				rect.rotation = rot;

	//				rect.anchorMin = onScreenPos;
	//				rect.anchorMax = onScreenPos;
	//			}
	//		} else {
	//			globePositionIndicator.SetActive (false);
	//		}
	//	}
	//}

	// Update is called once per frame
	//void Update () {

 // //      if (currentGameObject != null) {

	//	//	trackGlobePosition ();

	//	//	//Vector3 scale = isCameraInside() == true ? new Vector3(1.505f, 1.505f, 1.505f) : new Vector3(1.484f, 1.484f, 1.484f);
	//	//	//currentLowResGameObject.transform.localScale = scale;
	//	//}
	//}

	public IEnumerator MoveOverSeconds (GameObject objectToMove, Vector3 end, float seconds)
	{
		float elapsedTime = 0;
		Vector3 startingPos = objectToMove.transform.position;
		while (elapsedTime < seconds)
		{
			objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		objectToMove.transform.position = end;
	}

	public void replace() {

		spinScript.disable ();
		scanScript.enable ();
		ScanningControl.finalStep += replaceFinalStep;

		mapParent.SetActive (false);
		changeButton.SetActive (false);
		cancelButton.SetActive (true);
		replaceButton.SetActive (false);
		infoButton.SetActive (false);
		tutorialButton.SetActive (false);
		globePositionIndicator.SetActive (false);
	}

	private void replaceFinalStep(Vector3 position) {

		ScanningControl.finalStep -= replaceFinalStep;
		scanScript.disable ();

		//Debug.Log (position);
		//float distance = Vector3.Distance (Camera.main.transform.position, position);

		//Vector3 upPosition = Camera.main.ViewportToWorldPoint (new Vector3(0.5f, -0.2f, distance));

		//Debug.Log (upPosition);

		//mapParent.transform.position = upPosition;
		//position.y += 0.5f;
		mapParent.transform.position = position;

		mapParent.SetActive (true);

        if (currentGameObject == null) {
			initCall ();
		}
		// fix the animated placing
		//MoveOverSeconds (mapParent, position, 1.5f);

		if (PlayerPrefs.GetInt ("firstRun") == 0) {
			tutorialPressed ();
		} else {
			infoButton.SetActive (true);
			changeButton.SetActive (true);
			replaceButton.SetActive (true);
			tutorialButton.SetActive (true);
			cancelButton.SetActive (false);
			spinScript.enable (false);
		}

//		if (tutorial == false) {
//

//		} else {
//			SpinControl.tutorialDelegate += tutorialOver;
//			spinScript.enable (true);
//		}
	}

	private void tutorialOver() {

		SpinControl.tutorialDelegate -= tutorialOver;
		PlayerPrefs.SetInt ("firstRun", 1);

		infoButton.SetActive (true);
		changeButton.SetActive (true);
		replaceButton.SetActive (true);
		tutorialButton.SetActive (true);
		cancelButton.SetActive (false);
	}

	private void initCall() {
		switchGlobe ("CelestialGlobe1792", false);
	}

	public void tutorialPressed() {

		SpinControl.tutorialDelegate += tutorialOver;
		spinScript.enable (true);

		changeButton.SetActive (false);
		replaceButton.SetActive (false);
		infoButton.SetActive (false);
		tutorialButton.SetActive (false);
		cancelButton.SetActive (false);
	}

	public void cancelPressed() {
		Vector3 position = mapParent.transform.position;
		//position.y -= 0.5f;
		replaceFinalStep (position);
	}

    public void info()
    {

        GameObject infoPanel = (GameObject)Instantiate(Resources.Load("Prefabs/InfoPanel"));

        infoPanel.transform.SetParent(canvas.transform, false);
        infoPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

        GameObject.Find("InfoImage").GetComponent<Image>().sprite = Resources.Load<Sprite>("GlobeImages/" + globeName + "Sprite");

        string[] infoTexts = getInfoTexts();
        GameObject.Find("AuthorValue").GetComponent<Text>().text = infoTexts[0];
        GameObject.Find("DateValue").GetComponent<Text>().text = infoTexts[1];
        GameObject.Find("TitleValue").GetComponent<Text>().text = infoTexts[2];
        infoPanel.GetComponent<InfoPanelScript>().url = infoTexts[3];

        StartCoroutine(Animations.fade(infoMaterial, 0.35f, new Color(1f, 1f, 1f, 1f), () =>
        {

        }));
    }

	private string[] getInfoTexts() {

		//return author, date, title, url

        if (globeName == "CelestialGlobe1792") {
			return new string[] {"Cassini, Giovanni Maria","1792","Globo Terrestre","https://www.davidrumsey.com/luna/servlet/s/8x8xfd"};
		} else if (globeName == "CassiniWorldGlobe1790") {
			return new string[] { "Cassini, Giovanni Maria", "1790", "Globo Celeste", "https://www.davidrumsey.com/luna/servlet/s/070dp5" };
		} else if (globeName == "CelestialGlobe1693") {
			return new string[]  { "Pardies, Ignace Gaston, 1636-1673", "1693", "Globi Coelestis", "https://www.davidrumsey.com/luna/servlet/s/0k75un" };
		} else if (globeName == "WorldGlobe1688") {
			return new string[]  { "Coronelli, Vincenzo, 1650-1718", "1688", "Globo della Terra", "https://www.davidrumsey.com/luna/servlet/s/tx167u" };
		} else if (globeName == "WorldGlobe1492") {
			return new string[]  { "Behaim, Martin, 1459-1507", "1492", "Erdapfel", "https://www.davidrumsey.com/luna/servlet/s/e6yc9y" };
		} else if (globeName == "WorldGlobe1812") {
			return new string[]  { "Pinkerton, John, 1758-1826", "1812", "The World On Mercator's Projection", "https://www.davidrumsey.com/luna/servlet/s/xw0ni7" };
		} else {
			return new string[]  { "Monte, Urbano, 1544-1613", "1587", "Manuscript Wall Map of the World", "https://www.davidrumsey.com/luna/servlet/s/7kzr32" };
		}
	}

	public void menu() {

		replaceButton.SetActive (false);
		infoButton.SetActive (false);
		tutorialButton.SetActive (false);

		sideMenu = (GameObject)Instantiate (Resources.Load("Prefabs/MenuBackground"));

		sideMenu.transform.SetParent (canvas.transform, false);
		sideMenu.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 0f);

		StartCoroutine(Animations.fade (UIMaterial, 0.35f, new Color (1f, 1f, 1f, 1f), () => {
			
		}));
	}

	public void closeMenu() {

		StartCoroutine(Animations.fade (UIMaterial, 0.35f, new Color(1f, 1f, 1f, 0f), () => {
			replaceButton.SetActive(true);
			infoButton.SetActive(true);
			tutorialButton.SetActive(true);
			Destroy(sideMenu);
		}));
	}

	public void switchGlobe(String name, bool withFade) {

		// fix this lag
		if (withFade == true) {
			closeMenu ();
		}

		if (currentGameObject != null) {
			Destroy (currentGameObject);
			currentGameObject = null;
		}

		globeName = name;
		currentGameObject = (GameObject)Instantiate(Resources.Load("Prefabs/"+name));

        CapColors capColors = currentGameObject.GetComponent<CapColors>();

        topCapMaterial.SetColor("_Color", capColors.topCapColor);
        bottomCapMaterial.SetColor("_Color", capColors.bottomCapColor);

		currentGameObject.transform.SetParent(elevator.transform, false);
		currentGameObject.transform.localPosition = new Vector3 (0f, 0f, 0f);
	}
}