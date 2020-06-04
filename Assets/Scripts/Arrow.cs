using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 //copy paste from https://answers.unity.com/questions/462907/how-do-i-stop-a-projectile-cold-when-colliding-wit.html
//todo: tag arrow so they cant stick to each other

public class Arrow : MonoBehaviour {
    [SerializeField] public float arrowDisappearTime = 5f;
     private Quaternion q;
     private Vector3 v3;
     private bool hasHit = false;
     Transform MyTransform;
     Rigidbody MyRigidbody;

     void Start () {            
         MyTransform = this.transform;
         MyRigidbody = this.GetComponent<Rigidbody>();
         MyRigidbody.AddForce(MyTransform.forward, ForceMode.VelocityChange);            
     }
 
     void OnCollisionEnter(Collision col)
     {        
         MyRigidbody.isKinematic = true;    
         hasHit = true;

        //destroy after arrowDisappeartime
        Destroy(gameObject, arrowDisappearTime);
     }
     
     void LateUpdate() {
         if (hasHit) {
             transform.position = v3;
             transform.rotation = q;
             hasHit = false;
         }
         else {
             v3 = transform.position;
             q = transform.rotation;
         }
     }
 }
