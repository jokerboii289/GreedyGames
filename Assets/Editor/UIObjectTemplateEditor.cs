using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class UIObjectTemplateEditor : EditorWindow  // for now only predefined templates only working
{
    //for single templpate creation
    private string templateName;
    private Vector3 templatePos;
    private Vector2 size;
    private string parentName;
    private enum TemplateType
    {
        Text, Button, Image, Canvas, EmptyObjects
    }
    private TemplateType templateType;


    private TextAsset templateJSON;


    [MenuItem("Tools/UI Object Template Generator")]
    public static void ShowWindow()
    {
        GetWindow<UIObjectTemplateEditor>("UI Template Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Object Template Generator", EditorStyles.boldLabel);

        templateJSON = (TextAsset)EditorGUILayout.ObjectField("Template JSON", templateJSON, typeof(TextAsset), false);

        if (GUILayout.Button("Generate Template"))
        {
            if (templateJSON != null)
            {
                string json = templateJSON.text;
                UITemplate template = JsonUtility.FromJson<UITemplate>(json);

                // Use template data to instantiate UI objects
                GenerateTemplate(template);
            }
            else
            {
                Debug.LogError("Template JSON not assigned.");
            }
        }

        //adding individual template 
        GUILayout.Label("Individual Template Generator", EditorStyles.boldLabel);

        templateName = EditorGUILayout.TextField("Name For The Template", templateName);
        //templateType = EditorGUILayout.TextField("Specify Type",templateType);
        parentName = EditorGUILayout.TextField("Parent Name", parentName);
        size = EditorGUILayout.Vector2Field("width and height", size);
        templatePos = EditorGUILayout.Vector2Field("Template position", templatePos);
        templateType = (TemplateType)EditorGUILayout.EnumPopup("Specify Type", templateType);


        if (GUILayout.Button("Generate single Template"))
        {
            GameObject newTemplate = new GameObject(templateName);
            SetPropeties(newTemplate, templatePos, size);

            switch (templateType.ToString())
            {
                case "Text":
                    newTemplate.AddComponent<Text>();
                    break;
                case "Button":
                    newTemplate.AddComponent<Button>();
                    break;
                case "Image":
                    newTemplate.AddComponent<Image>();
                    break;
                case "EmptyObject":
                    break;
                case "Canvas":
                    newTemplate.AddComponent<Canvas>();
                    break;
                default:
                    break;
            }
            var parentObject = GameObject.Find(parentName);
            if (parentObject != null)
                newTemplate.transform.SetParent(parentObject.transform);

            UITemplate uiTemplate = JsonUtility.FromJson<UITemplate>(templateJSON.text);

            RecursionToFindParent(parentObject.name, uiTemplate.objects);

            //RecursionToFindParentAndAddChild(parentObject.name, uiTemplate.objects);

            Debug.Log("<color=Green>Generate: </color>success");

        }
    }

    private void GenerateTemplate(UITemplate template)
    {
        foreach (UIObject uiObject in template.objects)
        {
            GameObject canvas = CreateCanvas(uiObject);

            CreateUIObjectsRecursively(uiObject.children, canvas.transform);
        }
    }

    private void CreateUIObjectsRecursively(List<UIObject> uiObjects, Transform parentTransform)
    {
        foreach (UIObject uiObj in uiObjects)
        {
            GameObject obj;

            if (uiObj.type == "Image")
                obj = CreateImage(uiObj);
            else if (uiObj.type == "Text")
                obj = TextField(uiObj);
            else if (uiObj.type == "Button")
                obj = CreateButtons(uiObj);
            else if (uiObj.type == "EmptyObject")
                obj = CreateEmptyObject(uiObj);
            else
            {
                Debug.LogWarning("<color=red>Error: </color>Unsupported UIObject type: " + uiObj.type);
                continue;
            }

            obj.transform.SetParent(parentTransform);

            // Recursively process children
            if (uiObj.children != null && uiObj.children.Count > 0)
            {
                CreateUIObjectsRecursively(uiObj.children, obj.transform);
            }
        }
    }


    private GameObject CreateCanvas(UIObject uiobject) // Canvas
    {
        var canvas = new GameObject(uiobject.type);
        canvas.AddComponent<Canvas>();

        var myCanvas = canvas.GetComponent<Canvas>();
        //types of rendermode
        if (uiobject.properties.renderMode == "ScreenSpaceOverlay")
            myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        else
            myCanvas.renderMode = RenderMode.WorldSpace;

        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        Debug.Log("canvas created first");
        return canvas;
    }

    // Textfield
    private GameObject TextField(UIObject uiobject)//text fields 
    {
        var obj = new GameObject(uiobject.properties.name);
        Text text = obj.AddComponent<Text>();
        SetPropeties(obj, new Vector3(uiobject.position.x, uiobject.position.y, 0),
            new Vector2(uiobject.size.width, uiobject.size.height));

        return obj;
    }

    private GameObject CreateButtons(UIObject uiobject)//buttons
    {
        var obj = new GameObject(uiobject.properties.name);
        Button button = obj.AddComponent<Button>();
        SetPropeties(obj, new Vector3(uiobject.position.x, uiobject.position.y, 0),
            new Vector2(uiobject.size.width, uiobject.size.height));
        obj.AddComponent<Image>();
        return obj;
    }

    private GameObject CreateImage(UIObject uiobject)//For BG
    {
        var obj = new GameObject(uiobject.properties.name);

        Image image = obj.AddComponent<Image>();
        SetPropeties(obj, new Vector3(uiobject.position.x, uiobject.position.y, 0),
            new Vector2(uiobject.size.width, uiobject.size.height));
        return obj;
    }

    private GameObject CreateEmptyObject(UIObject uiobject)//create Empty Object
    {
        var obj = new GameObject(uiobject.properties.name);
        SetPropeties(obj, new Vector3(uiobject.position.x, uiobject.position.y, 0),
            new Vector2(uiobject.size.width, uiobject.size.height));
        return obj;
    }

    private void SetPropeties(GameObject newTemplate, Vector3 pos, Vector2 size)//we can add other properties like color,rotation etc
    {
        var rect = newTemplate.GetComponent<RectTransform>();
        if (rect == null)
            rect = newTemplate.AddComponent<RectTransform>();
        rect.localPosition = pos;
        rect.sizeDelta = size;
    }


    //Add TemplateData

    //Find the parent UIObject in the template
    private void RecursionToFindParent(string parentName, List<UIObject> uiObjects)
    {
        foreach (UIObject uiObject in uiObjects)
        {
            if (uiObject.properties.name == (parentName))
            {
                //data
                AddTheNewTemplate(uiObject);
                Debug.Log("<color=orange>DataEntered: </color>New template data added to JSON file.");

                break;//break out of loop
            }

            if (uiObject.children != null && uiObject.children.Count > 0)
            {
                RecursionToFindParent(parentName, uiObject.children);
            }
        }
    }


    void AddTheNewTemplate(UIObject uiObject)
    {
        string jsonPath = @"D:\Git_Repo\GreedyGames\Assets\Data\data.txt"; // jason path

        string jsonString = File.ReadAllText(jsonPath);

        UITemplate template = JsonUtility.FromJson<UITemplate>(jsonString);

        // Create new template data
        UIProperties newProperties = new UIProperties
        {
            text = "New Text",
            value = 42,
            name = "NewTemplate"
        };

        UIPosition newPosition = new UIPosition
        {
            x = 100,
            y = 200
        };

        UISize newUiSize = new UISize
        {
            width = 150,
            height = 75
        };

        UIObject newUIObject = new UIObject
        {
            type = "Text",
            properties = newProperties,
            position = newPosition,
            size = newUiSize,
            children = new List<UIObject>()
        };

        uiObject.children.Add(newUIObject);

        //template.objects.Add(newUIObject);

        // Serialize the modified template back to JSON
        string modifiedJson = JsonUtility.ToJson(template, true);
        File.WriteAllText(jsonPath, modifiedJson);
    }
}
