using UnityEngine;
using System.Collections;
using Artoncode.Core.Utility;

public class CharacterControl : MonoBehaviour {
    private Rigidbody2D body;
    private Collider2D col;
    private CharacterItemModifier item;
    private Animator anim;

    [Header("Base Properties")]
    public Vector2 velocity;
    public float maxHorizontalSpeed = 5.0f;
    public float needTimeToMaxSpeed = 1.0f;
    public float needTimeToStop = 1.0f;
    public float jumpForce = 10.0f;
    public int maxJumpCount = 1;

    [Header("Active Properties")]
    public float currMaxHorizontalSpeed;
    public int currMaxJumpCount;

    [Header("Runtime Properties")]
    [SerializeField]
    private int numJump = 0;

    [SerializeField]
    private bool isFalling;

    [SerializeField]
    private bool isMoving;

    bool moveLeft,moveRight;
    int horizontalHash,verticalHash,jumpHash;
    float lastJumpTimeFlag;

    void Awake(){
        horizontalHash = Animator.StringToHash("HorizontalSpeed");
        verticalHash = Animator.StringToHash("VerticalSpeed");
        jumpHash = Animator.StringToHash("Jump");
    }

    void Start(){
        body = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        item = GetComponent<CharacterItemModifier>();
        anim = GetComponent<Animator>();

        if(body == null){
            Debug.Log("No Player RigidBody");
        }
    }

    void Update(){
        //Input
        if(Input.GetKey(KeyCode.D) && !moveLeft){
            MoveRight();
        }else{
            moveRight = false;
        }

        if(Input.GetKey(KeyCode.A) && !moveRight){
            MoveLeft();
        }else {
            moveLeft = false;
        }

        if(Input.GetKeyDown(KeyCode.W)){
            Jump();
        }

        UpdateAnimation();
    }

    void UpdateAnimation(){
        if(moveLeft){
            transform.rotation = Quaternion.AngleAxis(180.0f,Vector3.up);
        }

        if(moveRight){
            transform.rotation = Quaternion.AngleAxis(0.0f,Vector3.up);
        }

        anim.SetFloat(horizontalHash,Mathf.Abs(velocity.x));
        anim.SetFloat(verticalHash,velocity.y);
    }

    void FixedUpdate(){
        StatsModifier();

        if(!isMoving){
            velocity.x = GameUtility.changeTowards(velocity.x,0.0f,currMaxHorizontalSpeed / needTimeToStop,Time.fixedDeltaTime);
        }

        body.velocity = new Vector2(velocity.x,body.velocity.y);

        velocity = new Vector2(velocity.x,body.velocity.y);
        lastJumpTimeFlag -= Time.fixedDeltaTime;
    }

    void StatsModifier(){
        currMaxHorizontalSpeed = maxHorizontalSpeed + item.additionalHorizontalSpeed;
        currMaxJumpCount = maxJumpCount + item.additionalJumpCount;
    }

    void LateUpdate(){
        if(!moveLeft && !moveRight){
            isMoving = false;
        }

        if(velocity.y < -0.1f){
            isFalling = true;
        }else{
            isFalling = false;
        }

        if(TouchGround() && lastJumpTimeFlag <= 0.0f){
            numJump = 0;
        }
    }

    void Jump(){
        if(numJump < currMaxJumpCount){
            body.velocity = new Vector2(0.0f,jumpForce);
            numJump ++;
            lastJumpTimeFlag = 0.25f;
        }
    }

    void MoveLeft(){
        velocity.x = GameUtility.changeTowards(velocity.x,-1.0f * currMaxHorizontalSpeed,currMaxHorizontalSpeed / needTimeToMaxSpeed,Time.fixedDeltaTime);

        isMoving = true;
        moveLeft = true;
    }

    void MoveRight(){
        velocity.x = GameUtility.changeTowards(velocity.x,1.0f * currMaxHorizontalSpeed,currMaxHorizontalSpeed / needTimeToMaxSpeed,Time.fixedDeltaTime);

        isMoving = true;
        moveRight = true;
    }

    bool TouchGround(){
        Vector2 ground = new Vector2(col.bounds.min.x + (col.bounds.max.x - col.bounds.min.x)/2.0f,col.bounds.min.y);

        int p = LayerMask.NameToLayer("Player");
        p = 1 << p;

        RaycastHit2D[] hit = Physics2D.RaycastAll(ground,Vector2.down,0.1f,~p);
        foreach(RaycastHit2D h in hit){
            return true;
        }

        return false;
    }

    public void ResetLevel(){
        item.ResetLevel();
    }
    #region utility

    #endregion
}
