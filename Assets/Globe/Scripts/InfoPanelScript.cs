using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelScript : MonoBehaviour {

	[SerializeField] private Material infoMaterial;

	[HideInInspector]
	public string url;

	public void openBrowser() {
		Application.OpenURL (url);
	}

	public void closePressed() {

		StartCoroutine(Animations.fade (infoMaterial, 0.35f, new Color (1f, 1f, 1f, 0f), () => {
			Destroy (gameObject);
		}));

	}
}
