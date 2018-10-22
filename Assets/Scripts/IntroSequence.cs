using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSequence : MonoBehaviour {

	private bool isLeftRockOn = false;
	private bool isRightRockOn = false;
	private bool connectionOn = false;
    private bool inIntro = true;

	[Header("Objects")]
	public GameObject rock1;
	public GameObject rock2;
	public GameObject rope;
	
	private IEnumerator RockFadeIn(GameObject rock){
		MeshRenderer renderer = rock.GetComponent<MeshRenderer>();
		float startTime = Time.time;
		while(Time.time < startTime + 2f){
			renderer.material.SetFloat("_AlphaOffset", 1.0f * (Time.time - startTime) / 2f);
			yield return null;
		}	
	}

	public IEnumerator RopeFadeIn(float _duration){
		Debug.Log ("Started FadeIn Coroutine");
		MeshRenderer renderer = rope.GetComponent<MeshRenderer>();
        renderer.enabled = true;
		float startTime = Time.time;
		while(Time.time < startTime + _duration){
			Color color = renderer.material.GetColor ("_MainColor");
			color.a = 1.0f * (Time.time - startTime) / _duration;
			renderer.material.SetColor("_MainColor", color);
			yield return null;
		}
	}

	private IEnumerator RopeFadeOut(float _duration) {
		MeshRenderer renderer = rope.GetComponent<MeshRenderer>();
		float startTime = Time.time;
		while(Time.time < startTime + _duration){
			Color colorTex = renderer.material.GetColor ("_MainColor");
			colorTex.a = 1.0f - ((Time.time - startTime)/ _duration);
			renderer.material.SetColor("_MainColor", colorTex);
			yield return null;
		}
		Color color = renderer.material.GetColor ("_MainColor");
		color.a = 0.0f;
		renderer.material.SetColor("_MainColor", color);
	}

    private void Start()
    {
        rope.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown (KeyCode.A)) {
			if (!isLeftRockOn) {
                isLeftRockOn = true;
				StartCoroutine (RockFadeIn (rock2));
			} 
		} else if (Input.GetKeyDown (KeyCode.D)) {
			if (!isRightRockOn) {
                isRightRockOn = true;
				StartCoroutine (RockFadeIn (rock1));
			}
		} else if (Input.GetKeyDown (KeyCode.S)) {
			if (!connectionOn) {
				connectionOn = true;
				StartCoroutine (RopeFadeIn (0.5f));
			} else {
                connectionOn = false;
				StartCoroutine (RopeFadeOut (0.5f));
		}
	}

}
}
