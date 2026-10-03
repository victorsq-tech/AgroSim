using System;
using UnityEngine;
using UnityEditor;
namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class FarmFrameValidation
    {
        static FarmFrameValidation() { EditorApplication.delayCall+=Validate; }
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            try {
                var t=Type.GetType("AgroSim.UI.FarmTravelView, Assembly-CSharp");
                var method=t?.GetMethod("CalculateFrame");
                if(method==null) throw new Exception("Full-screen framing calculation missing.");
                foreach(float aspect in new[]{.75f,1.333f,1.777f,2.35f}) foreach(float top in new[]{.825f,1f})
                {
                    var frame=(Vector2)method.Invoke(null,new object[]{aspect,8.8f,3.25f,.2f,top});
                    float bottom=(-3.25f-frame.y)/(2*frame.x)+.5f, upper=(3.25f-frame.y)/(2*frame.x)+.5f;
                    float left=(-8.8f)/(2*frame.x*aspect)+.5f, right=8.8f/(2*frame.x*aspect)+.5f;
                    if(bottom<.2f-.0001f || upper>top+.0001f || left<-.0001f || right>1.0001f) throw new Exception("Field cropped or covered by HUD at aspect "+aspect);
                }
                Debug.Log("AgroSim frame validation PASSED: field fits work/preview area in portrait, 4:3, 16:9 and wide views.");
            } catch(Exception e) { Debug.LogError("AgroSim frame validation FAILED: "+(e.InnerException??e).Message); }
        }
    }
}
