﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabricable : Behaviour {

	public Material[] materials;
	public Material[] simulationMaterials;

	private int materialIndex = 0;

	//Clone used for bidirectionality
	public Fabricable clone;

	void OnMouseUp(){
		

		if (Input.GetKey (KeyCode.Alpha1)) {

			UpdateState (1);

		}

		if (Input.GetKey (KeyCode.Alpha2)) {

			UpdateState (2);

		}

		if (Input.GetKey (KeyCode.Alpha3)) {

			UpdateState (3);

		}

		if (Input.GetKey (KeyCode.Alpha4)) {

			UpdateState (4);

		}

		if (Input.GetKey (KeyCode.Alpha0)) {

			UpdateState (0);
		}
	}

	public int GetSimulationMaterialIndex(){
		return materialIndex;
	}

	public Material GetMaterial(){
		return materials [materialIndex];
	}

	public Material GetSimulationMaterial(){
		return simulationMaterials[materialIndex];
	}

	public void SetSimulationMaterial(int index){
		UpdateState (index);
	}

	void UpdateState(int index){
		var newIndex = index != this.materialIndex;
		if (newIndex) {

			UpdateMaterial (index);

			//Update clone if it exist
			if (clone) {
				clone.UpdateMaterial (index);
			}

		}
	}

	void UpdateMaterial(int index){
		//Use the index
		GetComponent<Renderer> ().material = materials[index];

		//Store the index
		materialIndex = index;
	}
}
