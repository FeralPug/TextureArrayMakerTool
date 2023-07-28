using UnityEngine;
using UnityEditor;

public class TextureArrayTool : EditorWindow
{
    Resolutions selectedXResolution;
    Resolutions selectedYResolution;
    ArraySize arraySize;
    ArraySize lastArraySize;
    TextureFormat textureFormat;
    bool generateMipMaps;

    bool sameXAndYResolution;

    int textureXResolution;    
    int textureYResolution;    
    int numberOfTextures;
    
    Texture2D[] texture2Ds;

    Vector2 scrollPosition;

    bool hasResolution;

    bool hasArraySize;
    bool hasTextures;
    bool hasTextureFormat;

    [MenuItem("Tools/TexureArray/CreateTexture2DArray")]
    public static void OpenWindow()
    {
        TextureArrayTool textureArrayTool = GetWindow<TextureArrayTool>();

        textureArrayTool.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Set the desired texture resolution, array size, texture format, and textures.", MessageType.None);

        using(var check = new EditorGUI.ChangeCheckScope())
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            using (new GUILayout.HorizontalScope())
            {
                sameXAndYResolution = GUILayout.Toggle(sameXAndYResolution, "Same X & Y Res");

                if (sameXAndYResolution)
                {
                    selectedXResolution = (Resolutions)EditorGUILayout.EnumPopup("Texture Resolution", selectedXResolution);
                }
                else
                {
                    selectedXResolution = (Resolutions)EditorGUILayout.EnumPopup("Texture X Resolution", selectedXResolution);
                    selectedYResolution = (Resolutions)EditorGUILayout.EnumPopup("Texture Y Resolution", selectedYResolution);
                }     
            }
            
            arraySize = (ArraySize)EditorGUILayout.EnumPopup("Size of Array", arraySize);
            textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("TextureFormat", textureFormat);
            generateMipMaps = GUILayout.Toggle(generateMipMaps, "Generate MipMaps");

            if (check.changed)
            {
                if(lastArraySize != arraySize)
                {
                    texture2Ds = null;
                }
                lastArraySize = arraySize;
            }

            if (CheckArraySettings())
            {
                numberOfTextures = (int)arraySize;

                if (sameXAndYResolution)
                {
                    textureXResolution = (int)selectedXResolution;
                    textureYResolution = (int)selectedXResolution;
                }
                else
                {
                    textureXResolution = (int)selectedXResolution;
                    textureYResolution = (int)selectedYResolution;
                }

                if (texture2Ds == null)
                {
                    texture2Ds = new Texture2D[numberOfTextures];
                }

                using (new GUILayout.VerticalScope())
                {
                    for (int i = 0; i < texture2Ds.Length; i++)
                    {
                        texture2Ds[i] = TextureField($"Texture {i + 1}", texture2Ds[i]);
                    }
                }

                hasTextures = true;

                for(int i = 0; i < texture2Ds.Length; i++)
                {
                    if(texture2Ds[i] == null)
                    {
                        hasTextures = false;
                        break;
                    }
                }           
            }

            if(hasResolution && hasArraySize && hasTextures)
            {
                if (GUILayout.Button("Ceate and Save TextureArray Asset"))
                {
                    string absolutePath = EditorUtility.SaveFilePanel("Save File", "Assets", "NewTextureArray" + ".asset", "asset");
                    string assetPath = "";

                    string[] splitPath = absolutePath.Split('/');
                    bool foundAssetsFolder = false;

                    for (int i = 0; i < splitPath.Length; i++)
                    {
                        if (!foundAssetsFolder && splitPath[i] == "Assets")
                        {
                            foundAssetsFolder = true;
                        }

                        if (foundAssetsFolder && !splitPath[i].EndsWith(".asset"))
                        {
                            assetPath += splitPath[i] + "/";
                        }
                        else if (foundAssetsFolder && splitPath[i].EndsWith(".asset"))
                        {
                            assetPath += splitPath[i];
                        }

                    }

                    if (assetPath != "")
                    {

                        Texture2DArray texArray = new Texture2DArray(textureXResolution, textureYResolution, numberOfTextures, textureFormat, generateMipMaps);

                        var currentActiveRT = RenderTexture.active;
                        var rt = RenderTexture.GetTemporary(textureXResolution, textureYResolution, 0);
                        var tempTexture2D = new Texture2D(textureXResolution, textureYResolution);
                        var rect = new Rect(0, 0, textureXResolution, textureYResolution);

                        for (int i = 0; i < texture2Ds.Length; i++)
                        {
                            if (texture2Ds[i].height != textureYResolution || texture2Ds[i].width != textureXResolution)
                            {
                                Graphics.Blit(texture2Ds[i], rt);
                                RenderTexture.active = rt;
                                tempTexture2D.ReadPixels(rect, 0, 0);
                                texArray.SetPixels(tempTexture2D.GetPixels(), i);
                            }
                            else
                            {
                                texArray.SetPixels(texture2Ds[i].GetPixels(), i);
                            }

                        }

                        RenderTexture.active = currentActiveRT;
                        RenderTexture.ReleaseTemporary(rt);

                        texArray.Apply();

                        AssetDatabase.CreateAsset(texArray, assetPath);

                        AssetDatabase.SaveAssets();
                    }

                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    Texture2D TextureField(string name, Texture2D texture)
    {
        return (Texture2D)EditorGUILayout.ObjectField(name, texture, typeof(Texture2D), false);
    }

    bool CheckArraySettings()
    {
        if(arraySize != ArraySize.None)
        {
            hasArraySize = true;
        }

        if (sameXAndYResolution)
        {
            if(selectedXResolution != Resolutions.None)
            {
                hasResolution = true;
            }
        }
        else
        {
            if (selectedXResolution != Resolutions.None && selectedYResolution != Resolutions.None)
            {
                hasResolution = true;
            }
        }

        if(textureFormat != 0)
        {
            hasTextureFormat = true;
        }

        if(hasArraySize && hasResolution && hasTextureFormat)
        {
            return true;
        }

        return false;
    }

    public enum Resolutions
    {
        None,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
    }

    public enum ArraySize
    {
        None,
        _1 = 1,
        _2 = 2,
        _3 = 3,
        _4 = 4,
        _5 = 5,
        _6 = 6,
        _7 = 7,
        _8 = 8
    }

}
