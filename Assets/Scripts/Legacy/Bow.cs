using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public GameObject lineGuidePrefab;
    public GameObject arrowPrefab;
    private GameObject arrow;
    private GameObject lineGuide;

    public Transform firePoint; //to be used later as release point of projectile
    public float arrowForce = 20f;

    //bow and bow line trace logic
    public float drawTime = 5f;
    private float drawtimeCounter;
    private bool shotOnce = true;
    private bool linePresent = false;

    void Update()
    {
        BowLogic();
    
    }

    
    //handles when the bow shoots
    //todo: right click to not shoot arrow
    //tl:dr - hold fire, if run out of time or release, shoot
    void BowLogic(){
        //UnityEngine.Debug.Log("bop");

        //if rmb is pressed, put away arrow
        if(Input.GetButton("Fire2")) {
            drawtimeCounter = 0f;
            shotOnce = true;
        }

        if(Input.GetButton("Fire1")) {
            if(drawtimeCounter > 0.0f){
                shotOnce = false;
                Aim();
                drawtimeCounter -= Time.deltaTime;
                
                //instantiate/destroy whenever bow is aimed
                if(!linePresent){
                    lineGuide = Instantiate(lineGuidePrefab, firePoint.position, firePoint.rotation) as GameObject;
                    linePresent = true;
                }

            } 
            
            else if (!shotOnce) {
                Fire();
                shotOnce = true;
            } 
            
            else {
                if(linePresent){
                    Destroy(lineGuide);
                    linePresent = false;
                }
                
            }

        }

        else {
            if(!shotOnce){
                Fire();
                shotOnce = true;
            }
            //reset counter
            drawtimeCounter = drawTime;
            if(linePresent){
                    Destroy(lineGuide);
                    linePresent = false;
                }

        }

    }

    //traces the line
    void Aim() {
        if(linePresent){
            lineGuide.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        }
    }

    //todo: accuracy parameter for direction of arrow shot
    void Fire() {
        arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation) as GameObject;
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * arrowForce, ForceMode.Impulse);

        //UnityEngine.Debug.Log("Shot arrow");
    }

}
