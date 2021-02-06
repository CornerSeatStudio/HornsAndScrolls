using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudPointer : MonoBehaviour
{

    private ObjectiveHandler objHandler; 
    private PlayerHandler player;
    public GameObject pointer;
    private Image sprite;
    private RectTransform pointerRectTransform;
    public float posMod = 200f;
  
    void Start(){
        objHandler = FindObjectOfType<ObjectiveHandler>();
        player = FindObjectOfType<PlayerHandler>();
        sprite = pointer.GetComponent<Image>();
        pointerRectTransform = pointer.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

        if(objHandler.CurrObjective != null){
            Vector3 toPosition = objHandler.CurrObjective.transform.position;
            Vector3 fromPosition = player.transform.position;
            fromPosition.y = 0f; toPosition.y = 0f;
            Vector3 dir = (toPosition - fromPosition).normalized; //get a vector that points from fromPos to toPos
            float angle = (Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360;
            pointerRectTransform.localEulerAngles = new Vector3(0, 0, angle);

            Vector3 tpsp = Camera.main.WorldToScreenPoint(objHandler.CurrObjective.transform.position);
            bool offScreen = tpsp.x <= 0 || tpsp.x >= Screen.width || tpsp.y <= 0 || tpsp.y >= Screen.height;
            
            if(offScreen){
                sprite.enabled = true;

                // Debug.Log(dir);
                pointerRectTransform.localPosition = new Vector3(dir.x * posMod, dir.z * posMod, 0f);

            } else {
                sprite.enabled = false;
            }
        }
    }
}
