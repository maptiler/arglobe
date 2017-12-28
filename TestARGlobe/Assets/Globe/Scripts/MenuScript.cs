using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MenuScript : MonoBehaviour {

	private GlobeControl globeControl;

	// Use this for initialization
	void Start () {

		globeControl = FindObjectOfType<GlobeControl> ();
	}
	
	public void closePressed() {

		globeControl.closeMenu ();
	}

	public void changeTexture(String name) {

		globeControl.switchGlobe (name, true);
	}
}
