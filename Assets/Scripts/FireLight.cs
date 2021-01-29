using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLight : MonoBehaviour
{
    public float noiseScale;
    public float noiseAmp;
    [Range(-.5f, .5f)] public float noiseBias;
    // Update is called once per frame
    private Light[] lights;

    public float intensityRange;
    private float mainIntensity;
    public float displaceRange;
    private Dictionary<Light, Vector3> lightPositions;

    void Start() {
        lights = this.GetComponentsInChildren<Light>();
        lightPositions = new Dictionary<Light, Vector3>();
        mainIntensity = lights[0].intensity;
        foreach(Light l in lights){
            lightPositions.Add(l, l.transform.position);
        }
    }

    void Update()
    {  
        try{
        float noise = noiseAmp * ((Mathf.PerlinNoise(Time.time * noiseScale, 0f)) -.5f);
        float noise2 = noiseAmp * ((Mathf.PerlinNoise(0f, Time.time * noiseScale)) -.5f);
        float noise3 = noiseAmp * ((Mathf.PerlinNoise(Time.time * noiseScale, Time.time * noiseScale)) -.5f);


        float[] noises = {noise - noiseBias, noise2 - noiseBias, noise3 - noiseBias};


        for(int i = 0; i < lights.Length; i++) {
            lights[i].intensity = Mathf.Clamp(lights[i].intensity+noise, mainIntensity-intensityRange, mainIntensity+intensityRange);
            
            float xDisplace = Mathf.Clamp(lights[i].transform.position.x + noises[i%3], lightPositions[lights[i]].x - displaceRange, lightPositions[lights[i]].x + displaceRange);
            float yDisplace = Mathf.Clamp(lights[i].transform.position.y + noises[i%3], lightPositions[lights[i]].y - displaceRange, lightPositions[lights[i]].y + displaceRange);
            float zDisplace = Mathf.Clamp(lights[i].transform.position.z + noises[i%3], lightPositions[lights[i]].z - displaceRange, lightPositions[lights[i]].z + displaceRange);

            lights[i].transform.position = new Vector3(xDisplace, yDisplace, zDisplace);
        
            
        
        }
        } catch { Debug.LogWarning("campfire lights may be missing");}
        
        //transform.position += new Vector3(noise, noise, noise);
    }
}
