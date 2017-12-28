using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour {

	public delegate void AnimationCompletionHandler ();
	private AnimationCompletionHandler _callback;

	public static IEnumerator fade(Material material, float seconds, Color endColor, AnimationCompletionHandler callback) {

		float t = 0f;
		Color startColor = material.color;
		while (t < 1.0f) {

			t += Time.deltaTime * (1.0f/seconds);
			material.color = Color.Lerp (startColor, endColor, t);
			yield return new WaitForEndOfFrame ();
		}

		callback.Invoke ();
	}
}
