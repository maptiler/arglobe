using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereControl : MonoBehaviour {

	public bool isVisible = false;

	public void OnBecameVisible() {
		isVisible = true;
	}

	public void OnBecameInvisible() {
		isVisible = false;
	}
}
