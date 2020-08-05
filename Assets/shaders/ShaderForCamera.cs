using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderForCamera : MonoBehaviour
{

    public Material material;
    
    //using material, render the ENTIRE SCREEN OF THE CAMERA 
    //as if its seeing the whole things as the CHOSEN MATERIAL
    //if for ex, an object has said material w/ shader that inverts color, and this code is run,
    //the object in question will invert the color AGAIN, making it seem normal
    //while everything else that DIDNT HAVE THE MATERIAL will invert normally
     
    // void OnRenderImage(RenderTexture src, RenderTexture dest) {

    //     Graphics.Blit(src, dest, material);
    // }
}
