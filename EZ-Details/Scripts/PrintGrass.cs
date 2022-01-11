using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ezVariabeln : ScriptableObject
{
    public List<GameObject> dTos;
}

public class tt : EditorWindow
{
    public bool Draw;
    public float Radius = 5;
    public float DrawAmmount = 1;

    public int DrawIndex = 0;
    public List<GameObject> details = new List<GameObject>();
    public List<GameObject> randomDetails = new List<GameObject>();


    bool delete, drawing;
    public bool canBake, randomHeight, randomScale,randomDetail;
    public float rHmin = 0.5f, rHmax = 2, rHminV = 1, rHmaxV = 1;
    public float rSmin = 0.5f, rSmax = 2, rSminV = 1, rSmaxV = 1;

    ezVariabeln ezv;
    List<Vector3> positionBuffer = new List<Vector3>();
    //GUI STUFF
   
    Vector2 scroll = Vector2.zero;
    Rect lastRect;
    public GameObject p;
    RaycastHit hit;
    Texture2D back,paintIcon,paintHover,paintActive,bakeicon,bakehover,bakenotactiv,deleteicon, deletenotactiv, deletehover,select,helpicon,helphover;
    static Texture2D detailicon;

    public static bool EZdebug = false;

    [MenuItem("AzeS/EZ Detail Painter")]
    public static void OpenDetailPaint()
    {
        detailicon = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/DetailIcon.png", typeof(Texture2D)) as Texture2D;
        GUIContent wcon = new GUIContent();
        wcon.text = "EZ Detail Painter";
        wcon.image = detailicon;
        EditorWindow w = GetWindow<tt>();
        w.maxSize = new Vector2(300, 425);
        w.minSize = new Vector2(300, 425);
        w.titleContent = wcon;
        
    }

    

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
        delete = false;
        AssetPreview.SetPreviewTextureCacheSize(100);

        Object eass = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Scripts/ezVariabel.asset", typeof(ezVariabeln)) as ezVariabeln;

        ezv = eass as ezVariabeln; 

        if(ezv == null)
        {
            ScriptableObject ezasset = ScriptableObject.CreateInstance<ezVariabeln>();
            AssetDatabase.CreateAsset(ezasset, "Assets/EZ-Details/Scripts/ezVariabel.asset");
        }
        else
        {
            details = ezv.dTos;
        }

        back = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dbg.png", typeof(Texture2D)) as Texture2D;
        paintIcon = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dpoff.png", typeof(Texture2D)) as Texture2D;
        paintActive = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dpOn.png", typeof(Texture2D)) as Texture2D;
        paintHover = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dphover.png", typeof(Texture2D)) as Texture2D;
        bakeicon = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dbicon.png", typeof(Texture2D)) as Texture2D;
        bakehover = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dbhover.png", typeof(Texture2D)) as Texture2D;
        bakenotactiv = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/dbna.png", typeof(Texture2D)) as Texture2D;
        deleteicon = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/ddicon.png", typeof(Texture2D)) as Texture2D;
        deletehover = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/ddhover.png", typeof(Texture2D)) as Texture2D;
        deletenotactiv = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/ddna.png", typeof(Texture2D)) as Texture2D;
        select = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/elsc.png", typeof(Texture2D)) as Texture2D;
        helpicon = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/helpicon.png", typeof(Texture2D)) as Texture2D;
        helphover = AssetDatabase.LoadAssetAtPath("Assets/EZ-Details/Textures/helphover.png", typeof(Texture2D)) as Texture2D;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;

    }

    private void OnDestroy()
    {
        if(p != null)
        {
            BakeMesh(p);
        }

        if(ezv != null)
        {
            ezv.dTos = details;
        }

        SceneView.duringSceneGui -= OnScene;

    }
    

    void OnScene(SceneView scene)
    {
        canBake = (p != null && p.transform.childCount > 1) ? true : false;
        Event e = Event.current;

        
        if (!Draw)
        {
            drawing = false;
            return;
        }

        Selection.activeGameObject = null;

        Vector3 mousePos = e.mousePosition;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
        mousePos.x *= ppp;

        Ray ray = scene.camera.ScreenPointToRay(mousePos);

        if (drawing && !delete)
        {

            for (int i = 0; i < DrawAmmount; i++)
            {
                Vector3 sp = setPosition(hit);
                if (p == null)
                {
                    p = new GameObject();
                    p.transform.position = hit.point;
                    p.name = "DetailsParent";
                    p.AddComponent<MeshFilter>();
                    p.AddComponent<MeshRenderer>();

                }

                GameObject go = Instantiate((randomDetail ? randomDetails[Random.Range(0, randomDetails.Count)] : details[DrawIndex]), sp, Quaternion.identity);
                RaycastHit rh;
                if (Physics.Raycast(go.transform.position, -Vector3.up, out rh))
                {
                    go.transform.position = rh.point;
                    go.transform.rotation = Quaternion.FromToRotation(Vector3.up, rh.normal);
                    if (randomScale)
                    {
                        float r = Random.Range(rSminV, rSmaxV);
                        go.transform.localScale = new Vector3(r, r, r);
                    }
                    else if (randomHeight)
                    {
                        float r = Random.Range(rHminV, rHmaxV);
                        go.transform.localScale = new Vector3(go.transform.localScale.x, r, go.transform.localScale.z);

                    }
                }
                go.transform.parent = p.transform;
            }


        }
        else if (delete)
        {
            try
            {
                if (drawing)
                {
                    if (p != null)
                    {
                        for (int i = 0; i < p.transform.childCount; i++)
                        {
                            float dis = Vector3.Distance(hit.point, p.transform.GetChild(i).transform.position);
                            if (dis <= Radius)
                            {
                                if (!randomDetail)
                                {
                                    if (i >= p.transform.childCount) break;

                                    if (p.transform.GetChild(i).gameObject.name.ToLower() == (details[DrawIndex].name + "(Clone)").ToLower())
                                    {
                                        DestroyImmediate(p.transform.GetChild(i).gameObject);
                                    }
                                    if (p.transform.childCount <= 0)
                                    {
                                        DestroyImmediate(p);
                                        break;
                                    }
                                }
                                else
                                {

                                    if (i >= p.transform.childCount) break;

                                    for (int v = 0; v < randomDetails.Count; v++)
                                    {
                                        if (p.transform.GetChild(i).transform.gameObject.name.ToLower() == (randomDetails[v].name + "(Clone)").ToLower())
                                        {
                                            DestroyImmediate(p.transform.GetChild(i).gameObject);
                                        }
                                    }

                                    if (p.transform.childCount <= 0)
                                    {
                                        DestroyImmediate(p);
                                        break;
                                    }
                                }

                            }
                        }
                    }
                }
            }catch(System.Exception ex)
            {
                if(EZdebug)Debug.LogWarning(ex.StackTrace);
            }
        }


        if (Physics.Raycast(ray, out hit))
        {
            Handles.color = (delete ? new Color(1, 0, 0, 0.5f) : new Color(0, 0.2f, 1f, 0.5f));
            Handles.DrawSolidDisc(hit.point, hit.normal, Radius);
            Handles.color = (delete ? Color.red : Color.blue);
            Handles.DrawWireDisc(hit.point, hit.normal, Radius, 4);
            float m = Radius / 2;
            Handles.DrawWireDisc(hit.point, hit.normal, m, 2);
            Handles.DrawWireDisc(hit.point, hit.normal, m + (m - (m / 2)), 2);
            Handles.DrawWireDisc(hit.point, hit.normal, m - (m / 4), 1);
            Handles.DrawWireDisc(hit.point, hit.normal, m + (m / 2), 2);
            Handles.DrawWireDisc(hit.point, hit.normal, m / 2 - (m / 3), 1);

           
            Handles.color = (delete ? new Color(1, 0.5f, 0, 1) : Color.cyan);
            var d = hit.point.normalized - hit.normal;
            d += hit.normal * 10;
            Handles.DrawDottedLine(hit.point, hit.point + d, 6);
        }


        if (e.button != 1 && e.button != 2)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            Event evt = Event.current;

            switch (evt.GetTypeForControl(controlId))
            {

                case EventType.MouseDown:

                    //Important, must set hotControl to receive MouseUp outside window!

                    GUIUtility.hotControl = controlId;
                    if (evt.button == 0) drawing = true;
                    evt.Use();
                    break;

                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    if (evt.button == 0) drawing = false;
                    evt.Use();
                    break;
                case EventType.KeyDown:
                    GUIUtility.hotControl = controlId;
                    if (evt.keyCode == KeyCode.LeftShift)
                    {
                        delete = true;
                    }
                    evt.Use();
                    break;
                case EventType.KeyUp:
                    GUIUtility.hotControl = 0;
                    if (evt.keyCode == KeyCode.LeftShift)
                    {
                        delete = false;
                    }
                    evt.Use();
                    break;

            }
        }


    }

    public Vector3 setPosition(RaycastHit hit)
    {
        Vector3 sp = new Vector3(hit.point.x + (Random.insideUnitCircle * Radius).x, hit.point.y + 20, hit.point.z + (Random.insideUnitCircle * Radius).y);

        if (!positionBuffer.Contains(sp))
        {
            sp = new Vector3(hit.point.x + (Random.insideUnitCircle * Radius).x, hit.point.y + 20, hit.point.z + (Random.insideUnitCircle * Radius).y);
        }
        positionBuffer.Add(sp);
        return sp;
    }

    public void BakeMesh(GameObject gg)
    {

        GameObject g = Instantiate(p);
        g.name = "BaketDetailMesh";

        Vector3 oldPos = g.transform.position;
        Quaternion oldRot = g.transform.rotation;
        g.transform.position = Vector3.zero;
        g.transform.rotation = Quaternion.identity;

        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer ||
                !meshFilter.sharedMesh ||
                meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                continue;
            }

            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                if (materialArrayIndex == -1)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count - 1;
                }
                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // Get / Create mesh filter & renderer
        MeshFilter meshFilterCombine = g.GetComponent<MeshFilter>();
        MeshRenderer meshRendererCombine = g.GetComponent<MeshRenderer>();


        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        // Destroy other meshes
        foreach (Mesh oldMesh in meshes)
        {
            oldMesh.Clear();
            DestroyImmediate(oldMesh);
        }

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;
        int i = 0;
        int v = g.transform.childCount;
        while (i < v)
        {
            DestroyImmediate(g.transform.GetChild(0).transform.gameObject);
            i++;
        }


        g.transform.position = oldPos;
        g.transform.rotation = oldRot;
        DestroyImmediate(p);
        p = null;
        positionBuffer.Clear();
    }

    private int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }
        return -1;
    }

    public Texture2D dyeTexture(Color _color)
    {
        Texture2D t = new Texture2D(10, 10);

        for (int i = 0; i < 10; i++)
        {
            for (int y = 0; y < 10; y++)
            {
                t.SetPixel(i, y, _color);
            }
        }
        t.Apply();
        return t;
    }
    private GameObject dragObject()
    {
        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            // To consume drag data.
            DragAndDrop.AcceptDrag();

            // GameObjects from hierarchy.
            if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length > 0)
            {
                return DragAndDrop.objectReferences[0] as GameObject;
            }

            // Unity Assets including folder.
            else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {

                return DragAndDrop.objectReferences[0] as GameObject;
            }

        }
        return null;
    }

    private void OnGUI()
    {
        try
        {
            
            GUI.DrawTexture(new Rect(0, 0, 300, 425), back);

            Event e = Event.current;
           
            if (e.type == EventType.ScrollWheel)
            {
                scroll.x += e.delta.y * 4;
                e.Use();
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift)
            {
                delete = true;
                e.Use();
            }
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftShift)
            {
                delete = false;
                e.Use();
            }

            GUIStyle gs = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 15,
                richText = true,
                normal = new GUIStyleState()
                {
                    background = !Draw ? paintIcon : paintActive
                },
                hover = new GUIStyleState()
                {
                    background = paintHover
                },
                active = new GUIStyleState()
                {
                    background = !Draw ? paintIcon : paintActive
                }
            };
            GUIContent gsc = new GUIContent();
            gsc.tooltip = "Draw Button press to switch draw mode";
            gsc.text = "";
            GUIStyle gsr = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = new GUIStyleState()
                {
                    background = p == null ? deletenotactiv : deleteicon
                },
                hover = new GUIStyleState()
                {
                    background = p == null ? deletenotactiv : deletehover
                },
                active = new GUIStyleState()
                {
                    background = p == null ? deletenotactiv : deleteicon
                }
            };
            GUIContent gsrc = new GUIContent();
            gsrc.tooltip = "Delete all details from not baket detail mesh";
            gsrc.text = "";
            GUIStyle gsg = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = new GUIStyleState()
                {
                    background = canBake ? bakeicon : bakenotactiv
                },
                hover = new GUIStyleState()
                {
                    background = canBake ? bakehover : bakenotactiv
                },
                active = new GUIStyleState()
                {
                    background = canBake ? bakeicon : bakenotactiv
                }
            };
            GUIContent gsgc = new GUIContent();
            gsgc.tooltip = "Bake all details in a detail mesh";
            gsgc.text = "";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(1);
            if (GUILayout.Button(gsc, gs, GUILayout.MaxWidth(65), GUILayout.MaxHeight(55), GUILayout.MinHeight(55), GUILayout.MinWidth(65)))
            {
                if (details.Count > 0) Draw = !Draw;
                else if (EditorUtility.DisplayDialog("No Details Found", "You need to add a Detail object as prefab just drag it from project window or hierarchy and drop it in side the Detail Window", "Okay"))
                {

                }
            }

            EditorGUILayout.Space(5);
            if (p != null)
            {
                if (GUILayout.Button(gsrc, gsr, GUILayout.MaxWidth(65), GUILayout.MaxHeight(55), GUILayout.MinHeight(55), GUILayout.MinWidth(65)))
                {
                    if (EditorUtility.DisplayDialog("Do you want realy delete all?", "All not baket details will removed from you´r scene", "Remove", "Cancel"))
                    {
                        DestroyImmediate(p);
                    }
                }
            }
            else if(GUILayout.Button(gsrc, gsr, GUILayout.MaxWidth(65), GUILayout.MaxHeight(55), GUILayout.MinHeight(55), GUILayout.MinWidth(65))) { }

            EditorGUILayout.Space(5);
            if (canBake)
            {
                if (GUILayout.Button(gsgc, gsg, GUILayout.MaxWidth(65), GUILayout.MaxHeight(55), GUILayout.MinHeight(55), GUILayout.MinWidth(65)))
                {
                    Debug.Log("Bake Detail Mesh");
                    BakeMesh(p);
                }
            }
            else if(GUILayout.Button(gsgc, gsg, GUILayout.MaxWidth(65), GUILayout.MaxHeight(55), GUILayout.MinHeight(55), GUILayout.MinWidth(65))) { }
            
            EditorGUILayout.Space(1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            
            if (details == null || details.Count == 0)
            {
                EditorGUILayout.HelpBox("Drag and drop your Details prefabs direct in", MessageType.Info);
            }


            Radius = EditorGUILayout.Slider(Radius, 2f, 25f);


        DrawAmmount = EditorGUILayout.IntField("Draw Ammount", (int)DrawAmmount);

        if (randomHeight && randomScale)
        {
            EditorGUILayout.HelpBox("If random Scale active the painter will ignore random height", MessageType.Warning);
        }

        randomHeight = EditorGUILayout.Toggle("Random Height", randomHeight);

        if (randomHeight)
        {
            float x = rHminV;
            float y = rHmaxV;
            EditorGUILayout.BeginHorizontal();
            GUIStyle rl = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
                normal =
                {
                    textColor = Color.white
                }

            };
            EditorGUILayout.LabelField(rHminV.ToString(), rl, GUILayout.MaxWidth(40));
            rHmin = EditorGUILayout.FloatField(rHmin, GUILayout.MaxWidth(40));
            EditorGUILayout.MinMaxSlider(ref x, ref y, rHmin, rHmax);
            rHmax = EditorGUILayout.FloatField(rHmax, GUILayout.MaxWidth(40));
            rl.alignment = TextAnchor.MiddleLeft;
            
            EditorGUILayout.LabelField(rHmaxV.ToString(), rl, GUILayout.MaxWidth(40));
            x = (float)System.Math.Round(x, 2);
            y = (float)System.Math.Round(y, 2);
            rHminV = x;
            rHmaxV = y;
            EditorGUILayout.EndHorizontal();

        }

        randomScale = EditorGUILayout.Toggle("Random Scale", randomScale);

        if (randomScale)
        {
            float x = rSminV;
            float y = rSmaxV;
            EditorGUILayout.BeginHorizontal();
            GUIStyle rll = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
                normal =
                {
                    textColor = Color.white
                }

            };

            EditorGUILayout.LabelField(rSminV.ToString(), rll, GUILayout.MaxWidth(40));
            rSmin = EditorGUILayout.FloatField(rSmin, GUILayout.MaxWidth(40));
            EditorGUILayout.MinMaxSlider(ref x, ref y, rSmin, rSmax);
            rSmax = EditorGUILayout.FloatField(rSmax, GUILayout.MaxWidth(40));
            rll.alignment = TextAnchor.MiddleLeft;
            
            EditorGUILayout.LabelField(rSmaxV.ToString(), rll, GUILayout.MaxWidth(40));
            x = (float)System.Math.Round(x, 2);
            y = (float)System.Math.Round(y, 2);
            rSminV = x;
            rSmaxV = y;
            EditorGUILayout.EndHorizontal();

        }

            randomDetail = EditorGUILayout.Toggle("Random Detail", randomDetail);

            if (!randomDetail && randomDetails.Count > 0) randomDetails.Clear();

            GUIStyle gsa = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.LowerCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 20,
                richText = true,
                border = new RectOffset(10, 0, 10, 15),
                
            };

            GUILayout.Label("Selectet Detail : <color=green>" + (details.Count < DrawIndex + 1 ? "none" : details[DrawIndex].name) + "</color>", gsa,GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth),GUILayout.MaxHeight(25));
            
            

            scroll = EditorGUILayout.BeginScrollView(scroll, true, false, GUILayout.MinHeight(80));
            scroll.y = 0;

            EditorGUILayout.BeginHorizontal();

            
          
        GUIStyle bs = new GUIStyle();
        bs.padding.left = 5;
        bs.padding.right = 5;
        bs.padding.top = 5;
        bs.padding.bottom = 5;
        bs.fixedWidth = 70;
        bs.fixedHeight = 70;

        GUIContent bc = new GUIContent();
        
           

            for (int i = 0; i < details.Count; i++)
            {

                Texture2D t = null;
                int brk = 0;
                if (details != null && details.Count > 0)
                {
                    while (t == null && brk < 120)
                    {
                        t = AssetPreview.GetAssetPreview(details[i]);
                        brk++;
                    }
                    if (t == null) t = AssetPreview.GetMiniTypeThumbnail(details[i].GetType());
                }

                bc.image = t;
                if (!AssetPreview.IsLoadingAssetPreviews())
                {


                    if (GUILayout.Button(t, bs))
                    {
                        if (!randomDetail)
                        {
                            if (Event.current.button == 1)
                            {
                                if (EditorUtility.DisplayDialog("Warning", "Remove Detail Element From List", "Remove", "Cancel"))
                                {

                                    details.RemoveAt(i);
                                    DrawIndex = 0;

                                }
                            }
                            else DrawIndex = i;
                        }
                        else
                        {
                            if (Event.current.button == 1)
                            {
                                if (randomDetails.Contains(details[i]))
                                {
                                    randomDetails.Remove(details[i]);
                                   
                                }
                            }
                            else
                            {

                                if (!randomDetails.Contains(details[i]))
                                {
                                    randomDetails.Add(details[i]);
                                    
                                }
                            }
                        }
                    }
                    lastRect = GUILayoutUtility.GetLastRect();
                    
                    if (i == DrawIndex && !randomDetail)
                    { 
                        GUI.DrawTexture(new Rect(lastRect.x + lastRect.width - 25, lastRect.height - 25, 20, 20), select);
                    }

                    if (randomDetail)
                    {
                        for (int g = 0; g < randomDetails.Count; g++)
                        {
                            if (details[i] == randomDetails[g])
                            {
                                GUI.DrawTexture(new Rect(lastRect.x + lastRect.width - 25, lastRect.height - 25, 20, 20), select);

                            }
                        }
                    }
                }
            }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

            if (!randomDetail && randomDetails.Count > 0) randomDetails.Clear();
            if (randomDetail && randomDetails.Count < 2)
            {
                GUIStyle rms = new GUIStyle(EditorStyles.helpBox)
                {
                    fontStyle = FontStyle.Bold,
                    normal = {
                        textColor = Color.green
                    },
                    stretchWidth = true
                    
                };
                GUIContent rmc = new GUIContent(EditorGUIUtility.IconContent("console.infoicon"));
                rmc.text = "Select all details to print randomly";
                EditorGUILayout.LabelField(rmc, rms);
                
                //EditorGUILayout.HelpBox("Select all details to print randomly", MessageType.Info);
            }

            GUIStyle hsy = new GUIStyle()
            {
                normal={
                    background = helpicon
                },
                hover =
                {
                    background=helphover
                }
            };

            if(GUI.Button(new Rect(275, 400, 25,25), "", hsy))
            {
                if(EditorUtility.DisplayDialog("Controll Details", 
                    "1.)Add Details from your project window or hierachy in the Details Window\n" +
                    "2.)Select the Draw icon (upper left icon) to change in the Draw mode\n" +
                    "3.)With left mousebutton you draw the selected Detail\n" +
                    "4.)With left shift and left mousebutton you remove the selected Detail" +
                    "5.)To remove Detail from list just right click on it\n", "simple"))
                {
                    Debug.Log("1.)Add Details from your project window or hierachy in the Details Window\n" +
                    "2.)Select the Draw icon (upper left icon) to change in the Draw mode\n" +
                    "3.)With left mousebutton you draw the selected Detail\n" +
                    "4.)With left shift and left mousebutton you remove the selected Detail" +
                    "5.)To remove Detail from list just right click on it\n");
                }
            }

            GameObject dg = dragObject();
            if (dg != null) details.Add(dg);
            Repaint();
        }
        catch (System.Exception e)
        {
          if(EZdebug) Debug.Log(e.StackTrace);
        }
    }



}

