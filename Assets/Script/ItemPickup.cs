using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour {
    public CharacterItemModifier.ItemList item;

    void OnTriggerEnter2D(Collider2D col){
        if(col.tag.Equals("Player")){
            col.GetComponent<CharacterItemModifier>().PickUp(item);
            Destroy(this.gameObject);
        }
    }
}
