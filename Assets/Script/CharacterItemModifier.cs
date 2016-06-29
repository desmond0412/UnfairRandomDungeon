using UnityEngine;
using System.Collections;

public class CharacterItemModifier : MonoBehaviour {
    public enum ItemList{
        None,
        DoubleJump,
        IncreaseMS
    }

    public ItemList activeItem;

    public int additionalJumpCount;
    public float additionalHorizontalSpeed;

    void Update(){
        if(activeItem == ItemList.DoubleJump){
            additionalJumpCount = 1;
        }else{
            additionalJumpCount = 0;
        }

        if(activeItem == ItemList.IncreaseMS){
            additionalHorizontalSpeed = 3.0f;
        }else{
            additionalHorizontalSpeed = 0.0f;
        }
    }

    public void PickUp(ItemList i){
        activeItem = i;
    }

    public void ResetLevel(){
        activeItem = ItemList.None;
    }
}
