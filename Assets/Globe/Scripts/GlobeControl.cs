﻿using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;


public class GlobeControl : MonoBehaviour {

	[SerializeField] private GameObject mapParent;
	[SerializeField] private GameObject elevator;
	//[SerializeField] private GameObject changeButton;
	//[SerializeField] private GameObject replaceButton;
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

		cancelPressed();
	}

	private void defaultDetectionAndPlacement() {

		mapParent.SetActive (false);
		//changeButton.SetActive (false);
		//replaceButton.SetActive (false);
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
		//changeButton.SetActive (false);
		cancelButton.SetActive (true);
		//replaceButton.SetActive (false);
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
			//changeButton.SetActive (true);
			//replaceButton.SetActive (true);
			tutorialButton.SetActive (true);
			cancelButton.SetActive (false);
			spinScript.enable (false);
		}
	}

	private void tutorialOver() {

		SpinControl.tutorialDelegate -= tutorialOver;
		PlayerPrefs.SetInt ("firstRun", 1);

		infoButton.SetActive (true);
		//changeButton.SetActive (true);
		//replaceButton.SetActive (true);
		tutorialButton.SetActive (true);
		cancelButton.SetActive (false);
	}

	private void initCall() {
		switchGlobe ("CelestialGlobe1792", false);
	}

	public void tutorialPressed() {

		SpinControl.tutorialDelegate += tutorialOver;
		spinScript.enable (true);

		//changeButton.SetActive (false);
		//replaceButton.SetActive (false);
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

		switch (globeName)
		{
			case "CelestialGlobe1792":
				return new string[] { "Cassini, Giovanni Maria", "1792", "Globo Terrestre", "https://www.davidrumsey.com/luna/servlet/s/8x8xfd" };
			case "CassiniWorldGlobe1790":
				return new string[] { "Cassini, Giovanni Maria", "1790", "Globo Celeste", "https://www.davidrumsey.com/luna/servlet/s/070dp5" };
			case "CelestialGlobe1693":
				return new string[] { "Pardies, Ignace Gaston, 1636-1673", "1693", "Globi Coelestis", "https://www.davidrumsey.com/luna/servlet/s/0k75un" };
			case "WorldGlobe1688":
				return new string[] { "Coronelli, Vincenzo, 1650-1718", "1688", "Globo della Terra", "https://www.davidrumsey.com/luna/servlet/s/tx167u" };
			case "WorldGlobe1492":
				return new string[] { "Behaim, Martin, 1459-1507", "1492", "Erdapfel", "https://www.davidrumsey.com/luna/servlet/s/e6yc9y" };
			case "WorldGlobe1812":
				return new string[] { "Pinkerton, John, 1758-1826", "1812", "The World On Mercator's Projection", "https://www.davidrumsey.com/luna/servlet/s/xw0ni7" };
			default:
				return new string[] { "Monte, Urbano, 1544-1613", "1587", "Manuscript Wall Map of the World", "https://www.davidrumsey.com/luna/servlet/s/7kzr32" };
		}
	}

	public void menu() {

		//replaceButton.SetActive (false);
		infoButton.SetActive (false);
		tutorialButton.SetActive (false);

		sideMenu = (GameObject)Instantiate (Resources.Load("Prefabs/MenuBackground"));
		sideMenu.transform.SetParent (canvas.transform, false);
		sideMenu.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0f, 0f);

		StartCoroutine(Animations.fade (UIMaterial, 0.35f, new Color (1f, 1f, 1f, 1f), () => {}));
	}

	public void closeMenu() {

		StartCoroutine(Animations.fade (UIMaterial, 0.35f, new Color(1f, 1f, 1f, 0f), () => {
			//replaceButton.SetActive(true);
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