using UnityEngine;
using System;


public class MenuScript : MonoBehaviour {
	[SerializeField]
	private GlobeControl globeControl;


	public void closePressed() {

		globeControl.closeMenu ();
	}

	public void changeTexture(String name) {

		globeControl.switchGlobe (name, true);
	}
}