using System;
using UnityEditor;
using UnityEngine;
namespace EcoGuardian.Editor
{
    public static class ProjectValidation
    {
        [MenuItem("Eco Guardian/Validate Asset Imports")]
        public static void Validate()
        {
            foreach(string name in new[]{"thorn","frost","bloom","bomb","poison","ancient","kenney_130","kenney_245","kenney_250"})
                if(Resources.Load<Texture2D>("Art/"+name)==null)throw new Exception("Missing artwork: "+name);
            foreach(string name in new[]{"forest","hit","plant","wave"})
                if(Resources.Load<AudioClip>("Audio/"+name)==null)throw new Exception("Missing audio: "+name);
            if(Resources.Load<Font>("Fonts/GuardianSans")==null)throw new Exception("Missing Vietnamese font");
            Debug.Log("Eco Guardian: all required asset imports resolved.");
        }
    }
}
