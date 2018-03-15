﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GuiLabs.Undo;

public class BoundaryLineBehaviour : MonoBehaviour{

	public Transform first;
	public Transform second;

	public Vector3 unitVector;

	protected InteractionStateManager interactionStateManager;

	void Awake(){
		interactionStateManager = Component.FindObjectOfType<InteractionStateManager> ();
	}

	// Update is called once per frame
	void Update () {
		if (first == null || second == null)
			return;
		UpdateLine ();

	}

	void OnMouseUp(){
		if (Input.GetKey (KeyCode.A) || interactionStateManager.currentState == InteractionStateManager.InteractionState.ADDPOINT) {
			RaycastHit hit;

			if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 30f, LayerMask.GetMask("ModelPlane"))) {
				gameObject.transform.GetComponentInParent<BoundaryPointsHandler> ().AddPoint(gameObject, hit.point);
			}



		}else if(Input.GetKey(KeyCode.U) || interactionStateManager.currentState == InteractionStateManager.InteractionState.UNFOLDCLOTH){
			gameObject.transform.GetComponentInParent<BoundaryPointsHandler> ().Unfold(gameObject);
		}
	}

	void UpdateLine(){
		var start = first.transform.position;
		var end = second.transform.position;

		var parentScaleCompensation = transform.parent.transform.localScale.x;
		var offset = end - start;
		var scale = new Vector3(offset.magnitude/parentScaleCompensation, 1, 1);
		var position = start;

		//Save unit vector for other uses
		unitVector = offset.normalized;

		transform.position = position;
		transform.right = offset;
		transform.localScale = scale;
	}
}
