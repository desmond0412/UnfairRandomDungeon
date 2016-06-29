using UnityEngine;
using System.Collections;

public class EndPoint : MonoBehaviour {
    [SerializeField]
    LevelManager lm;

    Collider2D col;

	void Start () {
        col = GetComponent<Collider2D>();
	}
	
    void OnTriggerEnter2D(Collider2D col){
        if(col.tag.Equals("Player")){
            lm.NextLevel();
        }
    }
}
