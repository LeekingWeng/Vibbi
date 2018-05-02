﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GarmentHandler : MonoBehaviour {

	public GameObject clothPiecePrefab;
    public ClothModelHandler cmh;

    public Material garmentMaterial;
    public bool randomizeMaterial;
    public DeformManager deformManager;

    public AttachmentPointsHandler attachMentPointsHandler;

    public List<GameObject> clothPieces = new List<GameObject>();
	public List<GameObject> garmentSeams = new List<GameObject>();

    
	public Color seamColor = Color.red;
	public float seamWidth = 0.01f;


    private List<Material> materials = new List<Material>();

    private void Start()
    {
        materials.Add((Material)AssetDatabase.LoadAssetAtPath("Assets/DeformAssets/Materials/Chevron/Chevron.mat", typeof(Material)));
       // materials.Add((Material)AssetDatabase.LoadAssetAtPath("Assets/DeformAssets/Materials/Leather/Materials/Leather.mat", typeof(Material)));
        materials.Add((Material)AssetDatabase.LoadAssetAtPath("Assets/DeformAssets/Materials/Flannel/Flannel.mat", typeof(Material)));
        materials.Add((Material)AssetDatabase.LoadAssetAtPath("Assets/DeformAssets/Materials/Cloth.mat", typeof(Material)));
       // materials.Add((Material)AssetDatabase.LoadAssetAtPath("Assets/DeformAssets/Materials/ShinyBlack.mat", typeof(Material)));
    }


    // Update is called once per frame
    void Update () {
        HandleInput();

	}

    void HandleInput()
    {
        if (Input.GetButtonUp("Start Simulation"))
        {
            Debug.Log("Start Simulation");
            StartSimulation();
        }

        if (Input.GetButtonUp("Stop Simulation"))
        {
            Debug.Log("Stop Simulation");
            StopSimulation();
        }

        if (Input.GetKeyDown(KeyCode.R)) Reset();
    }

    private List<GameObject> clothModels = new List<GameObject>();

    public void LoadCloth(GameObject clothModel)
    {
        //get position and rotation
        Vector3 position = new Vector3(0, 11, 0); //places clothpiece above character
        Quaternion rotation = Quaternion.AngleAxis(90, new Vector3(1, 0, 0));
        bool bend = false;

        GameObject selectedAttachmentPoint = attachMentPointsHandler.GetSelectedAttachmentPoint();

        
		if(selectedAttachmentPoint != null)
        {
			//Place cloth piece on the selected attachment point
			AttachCloth(selectedAttachmentPoint.transform, out position, out rotation);
            bend = selectedAttachmentPoint.GetComponent<AttachmentPoint>().bendPoint;
        }

        LoadCloth(clothModel, position, rotation, bend);

       
    }

    private void LoadCloth(GameObject clothModel, Vector3 position, Quaternion rotation, bool bend)
    {
        
        if (!clothModels.Contains(clothModel))
        {
            clothModels.Add(clothModel);
        }

        //Create a cloth piece
        GameObject clothPiece = Instantiate(clothPiecePrefab, position, rotation, deformManager.transform.parent);


        //save id
        clothPiece.GetComponent<ClothPieceBehaviour>().id = clothModel.GetComponent<ClothModelBehaviour>().id;

        //save position & rotation of clothpiece
        clothPiece.GetComponent<ClothPieceBehaviour>().originalPosition = position;
        clothPiece.GetComponent<ClothPieceBehaviour>().originalRotation = rotation;
        clothPiece.GetComponent<ClothPieceBehaviour>().isBent = bend;

        //Init cloth piece mesh according to the given cloth model mesh
        var clothModelMesh = clothModel.GetComponent<MeshFilter>().mesh;
        clothPiece.GetComponent<MeshFilter>().sharedMesh = clothModelMesh;
        clothPiece.GetComponent<MeshCollider>().sharedMesh = clothModelMesh;

        //bend
        if (bend)
        {
            clothPiece.GetComponent<Bendable>().Bend();
            clothPiece.GetComponent<MeshCollider>().sharedMesh = clothPiece.GetComponent<MeshFilter>().sharedMesh;
        }

        clothPiece.GetComponent<ClothPieceBehaviour>().initialMesh = clothPiece.GetComponent<MeshCollider>().sharedMesh;


        //Set garment material accordingly
        if (randomizeMaterial)
        {
            //pick random number and load random material on the garment
            int r = Random.Range(0, materials.Count - 1);
            clothPiece.GetComponent<Renderer>().material = materials[r];
        }
        else
        {
            var clothModelFabric = clothModel.GetComponent<Fabricable>();
            var clothPieceFabric = clothPiece.GetComponent<Fabricable>();

            //Clone reference used to update fabric when changed
            clothPieceFabric.clone = clothModelFabric;
            clothModelFabric.clone = clothPieceFabric;

            //Use same material as cloth model
            //clothPieceFabric.materialIndex = clothModelFabric.GetSimulationMaterialIndex();
            clothPieceFabric.SetSimulationMaterial(clothModelFabric.GetSimulationMaterialIndex ());
            //clothPieceFabric.SetSimulationMaterial
        }

        //Keep eventual scaling
        clothPiece.transform.localScale = clothModel.transform.localScale;

        clothPieces.Add(clothPiece);

        cmh.LoadSeamsOfActiveClothPiece(clothPiece); //tell clothmodelhandler to load the seams
    }

    public void UnloadCloth(GameObject clothPiece){

		List<GameObject> connectedSeams = new List<GameObject> ();

		foreach (GameObject seam in garmentSeams) {
			if (seam.GetComponent<GarmentSeamBehaviour> ().firstClothPiece == clothPiece ||
			   seam.GetComponent<GarmentSeamBehaviour> ().secondClothPiece == clothPiece) {
				connectedSeams.Add (seam);
			}
		}

		foreach (GameObject seam in connectedSeams) {
			UnloadSeam (seam);
		}
        

        clothModels.RemoveAt(clothPieces.IndexOf(clothPiece));
        clothPieces.Remove (clothPiece);

		Destroy (clothPiece);
	}

	public void UnloadSeam(GameObject garmentSeam){
        seamModels.RemoveAt(garmentSeams.IndexOf(garmentSeam));
		garmentSeams.Remove (garmentSeam);
		Destroy (garmentSeam);
	}

	public void UnloadAll(){

        //First unload all seams
        foreach (GameObject seam in garmentSeams) {
			Destroy (seam);
		}

		garmentSeams.Clear ();

		//Then unload all cloth pieces
		foreach (GameObject cloth in clothPieces) {
			Destroy (cloth);
		}

		clothPieces.Clear ();
	}

    public void AttachCloth(Transform t, out Vector3 position, out Quaternion rotation)
    {
        position = t.position;
        GameObject tmpGO = new GameObject();
        tmpGO.transform.forward = -t.up;
        rotation = tmpGO.transform.rotation;
        //-t.up
    }

    /*public bool ClothIsLoaded(GameObject cloth){
		var clothModelMesh = cloth.GetComponent<MeshFilter> ().sharedMesh;

		for (int index = 0; index < clothPieces.Count; index++) {
			if (clothPieces[index].GetComponent<MeshFilter> ().sharedMesh.Equals (clothModelMesh)) {
				return true;
			}
		}
		return false;
	}*/
    private List<GameObject> seamModels = new List<GameObject>();
	public void LoadSeam(GameObject seam){
		Debug.Log ("Load Seam");
         if (!seamModels.Contains(seam))
         {
             seamModels.Add(seam);
         }

        var seamBehaviour = seam.GetComponent<SeamBehaviour> ();
		int firstLineMeshIndex = -1;
		int secondLineMeshIndex = -1;
		bool firstMeshFound = false;
		bool secondMeshFound = false;



		for (int index = 0; index < clothPieces.Count; index++) {

            //Check if first cloth is loaded
            //if (clothPieces[index].GetComponent<MeshFilter> ().sharedMesh.Equals (seamBehaviour.GetFirstMesh ())) { // do this with an ID?
            if (clothPieces[index].GetComponent<ClothPieceBehaviour>().id == seamBehaviour.firstClothPieceID)
            {
                //Debug.Log ("Mesh 1 is previously loaded");
				firstLineMeshIndex = index;
				firstMeshFound = true;
			}

			//Check if second cloth is loaded
			if (clothPieces[index].GetComponent<ClothPieceBehaviour>().id == seamBehaviour.secondClothPieceID) {
				//Debug.Log ("Mesh 2 is previously loaded");
				secondLineMeshIndex = index;
				secondMeshFound = true;
			}
		
		}


		if (firstMeshFound && secondMeshFound) {
			List<int> LineVerticeIndices = VibbiMeshUtils.DefineSeamFromLines (seamBehaviour.GetFirstLine (), seamBehaviour.GetSecondLine()); 

			if (LineVerticeIndices.Count <= 0 ) {
				Debug.Log ("Seam edge contains 0 vertices, aborting!");
				return;
			}

			CreateGarmentSeam (firstLineMeshIndex, secondLineMeshIndex, LineVerticeIndices, seam);
		}
	}

	private void CreateGarmentSeam(int firstClothPieceIndex, int secondClothPieceIndex, List<int> lineVerticeIndices, GameObject seam){

		GameObject garmentSeam = new GameObject ("GarmentSeam");
		garmentSeam.transform.parent = transform;

		LineRenderer renderer = garmentSeam.AddComponent<LineRenderer> ();
		renderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		renderer.startColor = seamColor;
		renderer.endColor = seamColor;
		renderer.startWidth = seamWidth;
		renderer.endWidth = seamWidth;

		GarmentSeamBehaviour garmentSeamBehaviour = garmentSeam.AddComponent<GarmentSeamBehaviour> ();
		garmentSeamBehaviour.Init (firstClothPieceIndex, secondClothPieceIndex, lineVerticeIndices, clothPieces[firstClothPieceIndex], clothPieces[secondClothPieceIndex], seam);

		garmentSeams.Add(garmentSeam);

	}

    private IDictionary<int, int> idToPositonInList = new Dictionary<int, int>();
    private bool idsSet = false;
    private int totalNumberOfVertices = 0;
    

    public void StartSimulation()
    {
        attachMentPointsHandler.ShowAttachmentPoints (false);

        foreach (GameObject s in garmentSeams)
        {
            s.GetComponent<GarmentSeamBehaviour>().isSimulationRunning = true;
        }

        foreach(GameObject o in clothPieces)
         {
            Mesh mesh = o.GetComponent<ClothPieceBehaviour>().initialMesh;
            DeformObject deformObject = o.AddComponent<DeformObject>();

            deformObject.originalMesh = mesh;
            //deformObject.material = o.GetComponent<MeshRenderer>().material;
			deformObject.material = o.GetComponent<Fabricable>().GetSimulationMaterial();
            deformObject.AddToSimulation();
        }

        deformManager.Reset();

    }


    private void Reset()
    {
        StopSimulation();
        StartSimulation();
    }
    
    public void StopSimulation()
    {
        deformManager.UnloadDeformables();
        ResetIDs(); //add to reset?

        Vector3[] positions = new Vector3[clothPieces.Count];
        Quaternion[] rotations = new Quaternion[clothPieces.Count];
        bool[] isBended = new bool[clothPieces.Count];

        int index = 0;
       
        for (int i = 0; i < clothModels.Count; i++)
        {
            for (int j = 0; j < clothPieces.Count; j++)
            {
                if (clothModels[i].GetComponent<ClothModelBehaviour>().id == clothPieces[j].GetComponent<ClothPieceBehaviour>().id)
                {
                    positions[index] = clothPieces[j].GetComponent<ClothPieceBehaviour>().originalPosition;
                    rotations[index] = clothPieces[j].GetComponent<ClothPieceBehaviour>().originalRotation;
                    isBended[index] = clothPieces[j].GetComponent<ClothPieceBehaviour>().isBent;
                    index++;
                    break;
                }
            }
        }


        UnloadAll(); //empties garmentSeams & clothPieces
        
        
        for (int i = 0; i < clothModels.Count; i++)
        {
            LoadCloth(clothModels[i], positions[i], rotations[i], isBended[i]);
        }
        
        /*
        foreach (GameObject s in seamModels)
        {
            LoadSeam(s);
        }*/

		attachMentPointsHandler.ShowAttachmentPoints (true);
        
    }

    private void ResetIDs()
    {
        idToPositonInList.Clear();
        totalNumberOfVertices = 0;
    }
    
    public void InitSeams()
    {
        foreach (GameObject seam in garmentSeams)
        {
            GarmentSeamBehaviour gsb = seam.GetComponent<GarmentSeamBehaviour>();
            int id1 = gsb.firstClothPiece.GetComponent<DeformObject>().GetId();
            int id2 = gsb.secondClothPiece.GetComponent<DeformObject>().GetId();
            

            uint[] vertices = new uint[gsb.lineVerticeIndices.Count];
            for (int i = 0; i < gsb.lineVerticeIndices.Count; i = i + 2)
            {
                vertices[i] = (uint)(gsb.lineVerticeIndices[i] + idToPositonInList[id1]);
                vertices[i + 1] = (uint)(gsb.lineVerticeIndices[i+1] + idToPositonInList[id2]);
            }

            deformManager.Sew(id1, id2, vertices, vertices.Length);
        }
    }

    public void setIDs()
    {
        if (clothPieces.Count == 0)
        {
            return;
        }

        //gå baklänges
        for (int i = clothPieces.Count - 1; i > -1; i--)
        {
            int key = clothPieces[i].GetComponent<DeformObject>().GetId();

            if (!idToPositonInList.ContainsKey(key))
            {
                idToPositonInList.Add(key, totalNumberOfVertices); //so that we can get global index when sewing
                totalNumberOfVertices += clothPieces[i].GetComponent<ClothPieceBehaviour>().initialMesh.vertexCount;
            }
                
        }
            
    }

	public void UpdateGarmentSeams(){
		foreach (GameObject o in garmentSeams) {
			o.GetComponent<GarmentSeamBehaviour> ().UpdateIndices ();
		}
	}
		
}
