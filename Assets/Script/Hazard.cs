using UnityEngine;
using System.Collections;

public class Hazard : MonoBehaviour {
    public enum HazardList{
        Lava,
        Trap,
        ButtonTrap
    }
     
    [SerializeField]
    HazardList hl;

    void TriggerTrap(){
        switch(hl){
            case HazardList.Lava:
                break;
            case HazardList.Trap:
                break;
            case HazardList.ButtonTrap:
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D col){
        if(col.tag.Equals("Player")){
            TriggerTrap();
        }
    }
}
