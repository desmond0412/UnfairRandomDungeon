using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public LevelManager currLevel;
    public LevelManager nextLevel;

    public float changeLevelTime;

    public void SetLevel(LevelManager curr,LevelManager next){
        currLevel = curr;
        nextLevel = next;
    }

    public void NextLevel(){
        if(nextLevel){
            iTween.MoveTo(gameObject,iTween.Hash("x",nextLevel.transform.position.x,
                "y", nextLevel.transform.position.y,
                "time",changeLevelTime));
        }

        SetLevel(nextLevel,nextLevel.nextLevel);
    }
}
