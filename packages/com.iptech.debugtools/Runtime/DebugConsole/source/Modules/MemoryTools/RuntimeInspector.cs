#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace IPTech.DebugConsoleService
{
    public static class RuntimeInspector {

        public static Texture[] FindAllTexturesUnder(GameObject gameObject) {
            List<Texture> textures = new List<Texture>();
            Graphic[] tgo = gameObject.GetComponentsInChildren<Graphic>(true);
            for(int i=0;i<tgo.Length;i++) {
                Texture t = tgo[i].mainTexture;
                if(t!=null && !textures.Contains(t)) {
                    textures.Add(t);
                }
            }
            return textures.ToArray();
        }

        public static Texture[] FindAllTextures(bool includeDisabledObjects) {
        #if UNITY_EDITOR
            List<Texture> textures = new List<Texture>();
            Graphic[] graphics = FindComponetsOfType<Graphic>(includeDisabledObjects);
            for(int i=0;i<graphics.Length; i++) {
                Texture tex = graphics[i].mainTexture;
                if(tex!=null && !textures.Contains(tex)) {
                    textures.Add(tex);
                }   
            }
            return textures.ToArray();
        #else
            return Resources.FindObjectsOfTypeAll<Texture>();
        #endif
        }

        public static T[] FindComponetsOfType<T>(bool includeDisabledObject) where T : Component
        {
            List<T> objList = new List<T>();
            if (includeDisabledObject) {
                GameObject[] allGo = GetAllRootGameObjects();
                foreach (GameObject go in allGo) {
                    Transform[] tgo = go.GetComponentsInChildren<Transform> (true).ToArray ();
                    foreach (Transform tr in tgo) {
                        T comp = tr.GetComponent<T>();
                        if (comp!=null && !objList.Contains(comp)) {
                            objList.Add (comp);
                        }
                    }
                }         
            } else {
                #if UNITY_6000_0_OR_NEWER
                objList = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None).ToList();
                #else
                objList = UnityEngine.Object.FindObjectsOfType<T>().ToList();
                #endif
            }
            return objList.ToArray();
        }

        public static GameObject[] GetAllRootGameObjects(){
             GameObject[] tempList = Resources.FindObjectsOfTypeAll<GameObject>();

             List<GameObject> rootList = new List<GameObject>();
             for(int i=0;i<tempList.Length;i++){
                if(!tempList[i].scene.IsValid()) continue;
                 GameObject temp = TraceUpToRoot(tempList[i]);
                 if(temp!=null && !rootList.Contains(temp)) {
                    rootList.Add(temp);
                 }
             }
             return rootList.ToArray();
        }

        private static GameObject TraceUpToRoot(GameObject go) {
            Transform last = go.transform;
            Transform t = last;
            while(t!=null) {
                if(t.gameObject.hideFlags != HideFlags.None) {
                    return null;
                }
                last = t;
                t = t.parent;
            }

            return last.gameObject;
        }
    }
}

#endif

