using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    CharacterControl player;

    public TextMesh textFloor;

    public StartPoint startPoint;
    public EndPoint endPoint;

    public GameObject levelPrefab;
    public LevelManager nextLevel;


    void Start(){
        player = FindObjectOfType<CharacterControl>();

        if(gameObject.name.Substring(gameObject.name.Length - 3).Equals("001")){
            StartLevel();
        }

        textFloor.text = gameObject.name.Substring(gameObject.name.Length - 3);
    }

    public void StartLevel(){
        player.transform.position = startPoint.transform.position;

        //generateNextLevel
        GenerateNextLevel();

        Camera.main.GetComponent<CameraController>().SetLevel(this,nextLevel);
    }

    void GenerateNextLevel(){
        if(nextLevel == null){
            GameObject go = Instantiate(levelPrefab,
                transform.position + Vector3.up * 10.0f,
                Quaternion.identity) as GameObject;

            int i = int.Parse(gameObject.name.Substring(gameObject.name.Length - 3));
            i++;

            if(i<10){
                go.name = "Level00" + i.ToString();
            }else if(i<100){
                go.name = "Level0" + i.ToString();
            }else{
                go.name = "Level" + i.ToString();
            }

            nextLevel = go.GetComponent<LevelManager>();
        }
    }

    public void NextLevel(){
        StartCoroutine(NextLevelCoroutine());
    }

    IEnumerator NextLevelCoroutine(){
        CameraController cc = Camera.main.GetComponent<CameraController>();

        if(cc){
            Camera.main.GetComponent<CameraController>().NextLevel();

            player.gameObject.SetActive(false);

            yield return new WaitForSeconds(cc.changeLevelTime);
            nextLevel.StartLevel();

            player.velocity = Vector2.zero;
            player.gameObject.SetActive(true);
        }
    }
}
