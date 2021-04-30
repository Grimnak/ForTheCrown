using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

public class SFB_CharacterEquip : MonoBehaviour {

    static GameObject target;                                                   // The target to match
    private static string rootBoneName = "BoneRoot";                            // Name of the root bone structure [IMPORTANT AND MAY BE DIFFERENT FOR YOUR MODEL!]
    private static SkinnedMeshRenderer targetRenderer;                          // Renderer we are targetting
    private static string subRootBoneName;                                      // Bone name
    private static GameObject thisBoneRoot;                                     // Current bone root
    
    private static Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

    [MenuItem("Window/Infinity PBR/Character Equip")]                           // Provides a menu item [Removed Shortcut Oct 30, 2020]
    static void EquipCharacter()
    {
        if (!Selection.activeGameObject)
        {
            Debug.Log("No Object Selected!");
            return;
        }

        target = Selection.activeGameObject;
        boneMap = new Dictionary<string, Transform>();

        // Make sure there is a proper root bone structure. We will look through all the children, looking
        // for the rootBoneName and a SkinnedMeshRenderer. The bone structure in that object will
        // populate our dictionary.
        bool isValidObject = IsValidObject(target.transform);

        // If this isn't a valid object, then end the process
        if (!isValidObject)
        {
            Debug.LogWarning("Warning: This was not a valid object (no root bone called " + rootBoneName + " was found!");
            return;
        }

        // Now search for all equipment that needs to be equipped.
        foreach (Transform child in target.transform)
        {
            bool isEquipmentObject = false;                            
            SkinnedMeshRenderer childSkinnedMeshRenderer = null;              

            // Search the children of this child, looking for the rootBoneName, which indicates it is a valid
            // equipment object.
            foreach (Transform subChild in child.transform)
            {
                if (IsThisObjectValid(subChild.name))
                {
                    thisBoneRoot = subChild.gameObject;                 // This is the rootBoneObject, so save it as such
                    isEquipmentObject = true;
                }
                    
                if (subChild.GetComponent<SkinnedMeshRenderer>())       // If the subChild has a skinnedMeshRenderer
                {
                    // If the subChild has a SkinnedMeshRenderer, assign the renderer, bone name, and target;
                    childSkinnedMeshRenderer = subChild.gameObject.GetComponent<SkinnedMeshRenderer>();
                    subRootBoneName = childSkinnedMeshRenderer.rootBone.name;
                    targetRenderer = childSkinnedMeshRenderer;
                }
            }

            // If this object is something that can be equipped and it has a SkinnedMeshRenderer
            if (isEquipmentObject && childSkinnedMeshRenderer)
            {
                Debug.Log(child.name + " is valid equipment!");
            
                Transform[] boneArray = childSkinnedMeshRenderer.bones;         // Set the boneArray to be all the bones from the subChildRenderer
                for (int i = 0; i < boneArray.Length; i++)              // For each bone...
                {
                    string boneName = boneArray[i].name;                // Get the bone name
                    if (boneMap.ContainsKey(boneName))                  // if the dictionary for the target bones contains this bone name...
                    {
                        boneArray[i] = boneMap[boneName];               // Set the array to match the bone from the Dictionary
                    }
                }

                childSkinnedMeshRenderer.bones = boneArray;                     // Assing the boneArray to the bones
                foreach (Transform bone in targetRenderer.bones)        // For each bone...
                {
                    if (bone.name == subRootBoneName)                   // Is the bone name the same as subRootBoneName?
                    {
                        childSkinnedMeshRenderer.rootBone = bone;               // Assign the root bone to this
                    }
                }

                // Update in late october 2020 -- adding "if" statement  to fix the bug that suddenly appeared.
                Debug.Log("Child: " + child.gameObject.name);
                if (IsThisAPrefab(child.gameObject))
                    UnpackThisPrefab(child.gameObject);
            
           
                DestroyImmediate(thisBoneRoot, true);                   // Destroy the bones of the subChildRenderer
            }
        }

        /* NOTE: In a previous version I had a prefab created automatically. I'm removing this as (1) it's not really
         * hard at all to create a prefab by draggin, (2) I think there's little point to making lots of prefabs with
         * different versions, and (3) one less thing to support...coding isn't my strong suit! :D
         */

        // Finally, we will remove "Dummy001" from the objects.
        Transform[] allChildren = target.GetComponentsInChildren<Transform>();   // Get all children of the target
        foreach (Transform child in allChildren)                        // And for each one...
        {
            if (child.gameObject.name == "Dummy001")                    // If the name is Dummy001
            {
                if (IsThisAPrefab(child.gameObject))                    // [Oct 30, 2020] Honestly not sure if this is needed here.
                    UnpackThisPrefab(child.gameObject);
                DestroyImmediate(child.gameObject);                     // Destroy it
            }
        }
        
        Debug.Log("Character Equip Complete!!");
        
        
    }

    // [Oct 30, 2020] This will return true if we found an object that matches the rootBoneName, and will also populate
    // the bones dictionary with bones from the object.
    static bool IsValidObject(Transform target)
    {
        bool isValid = false;
        foreach (Transform childCheck in target)                  
        {
            if (IsThisObjectValid(childCheck.name))
                isValid = true;
            AddBonesToDictionary(childCheck);
        }

        return isValid;
    }

    static void AddBonesToDictionary(Transform transform)
    {
        if (transform.GetComponent<SkinnedMeshRenderer>())               // If this has a skinnedMeshRenderer
        {
            targetRenderer = transform.GetComponent<SkinnedMeshRenderer>();  // Set targetRenderer
            Debug.Log("Set Target Renderer: " + targetRenderer.name);
            foreach (Transform bone in targetRenderer.bones)            // For each bone...
            {
                boneMap[bone.name] = bone;                              // Add the bone to the dictionary
            }
        }
    }

    // [Oct 30, 2020] Moved this simple check out, as we may need to add more to it in the future.
    static bool IsThisObjectValid(string name)
    {
        if (name == rootBoneName)
            return true;
        return false;
    }
    
    // [Oct 30, 2020] Moved this and the following method out, as they currently work, but I'm not 100% sure if this
    // is the best way of doing it. This may break in the future, not sure. Moved it out here to make it easier to fix.
    static bool IsThisAPrefab(GameObject gameObject)
    {
        if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            return true;
        return false;
    }

    // [Oct 30, 2020] See note above IsThisAPrefab()
    static void UnpackThisPrefab(GameObject gameObject)
    {
        PrefabUtility.UnpackPrefabInstance(gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    }
}