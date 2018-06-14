using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimDoneCheck : MonoBehaviour {

	public GameManager man;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void AnimDone(){
		man.NextStage(1);
	}
}
