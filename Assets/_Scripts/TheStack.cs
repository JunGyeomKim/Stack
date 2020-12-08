using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TheStack : MonoBehaviour
{
    // Const Value
    private const float BOUND_SIZE = 3.5f;          // ブロックサイズの初期
	private const float MOVING_BOUNDS_SIZE = 3f;    // ブロック移動半径サイズ
	private const float STACK_MOVING_SPEED = 5.0f;  //スタックオブジェクトの移動
	private const float BLOCK_MOVING_SPEED = 3.5f;  // ブロック移動速度
	private const float ERROR_MARGIN = 0.1f;        // 誤差範囲


	private const string BEST_SCORE_KEY = "BestScore";
    private const string BEST_COMBO_KEY = "BestCombo";

	//コピーして引き続き使用するブロック
	public GameObject OriginBlock = null;

	//大きさや位置を検査する以前ブロック
	private Vector3 PrevBlockPosition;

	// 次の生成するブロック
	private Vector3 DesiredPosition;

	//ブロックサイズ
	private Vector2 StackBounds
        = new Vector2(BOUND_SIZE, BOUND_SIZE);

    Transform LastBlock = null;

   
    int StackCount = -1;

	//XとZ、二つの方向を検査するフラグ
	bool IsMovingX = true;

	//ブロックの動きを担当する変数
	float BlockTransition = 0f;

    float SecondaryPosition = 0f;

    int ComboCount = 0;
    int MaxCombo = 0;

    int BestScore = 0;
    int BestCombo = 0;

    public Color PrevColor;
    public Color NextColor;

    //GameOver
    bool IsGameOver = true;


    private void Start()
    {
        
        if (OriginBlock == null)
        {
            Debug.Log("OriginBlock is NULL");
            return;
        }

       
        BestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        BestCombo = PlayerPrefs.GetInt(BEST_COMBO_KEY, 0);


        
        PrevColor = GetRandomColor();
        NextColor = GetRandomColor();

       
        PrevBlockPosition = Vector3.down;
        Spawn_Block();
       
    }

    private void Update()
    {

        if (IsGameOver == true)
        {
            if (Input.GetKeyDown(KeyCode.R)
                || Input.touchCount > 2
                )
                Restart();
            return;
        }

        
        if (Input.GetMouseButtonDown(0))
        {

            if (PlaceBlock() == true)
            {
                Spawn_Block();
            }
            else
            {
                // Game Over
                IsGameOver = true;
                EndEffect();
                CheckScore();
                Debug.Log("GameOver");
                UIManager.Instance.SetScore(
                    BestScore, BestCombo, StackCount, MaxCombo);
            }

        }

        MoveBlock();

        transform.position =
            Vector3.Lerp(transform.position,
            DesiredPosition,
            STACK_MOVING_SPEED * Time.deltaTime);
    }
	//ブロックを生成するコード
	bool Spawn_Block()
    {
        if (LastBlock != null)
            PrevBlockPosition = LastBlock.localPosition;

        GameObject newBlock = null;
        Transform newTrans = null;

       
        newBlock = Instantiate(OriginBlock);

      
        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed!");
            return false;
        }
        
        ColorChange(newBlock);

        newTrans = newBlock.transform;
       
        newTrans.parent = this.transform;

        newTrans.localPosition = PrevBlockPosition + Vector3.up;
		
        newTrans.localRotation = Quaternion.identity;
       
        newTrans.localScale = new Vector3(StackBounds.x, 1, StackBounds.y);

        StackCount++;
       
        DesiredPosition = Vector3.down * StackCount;

        IsMovingX = !IsMovingX;

        BlockTransition = 0f;
       
        LastBlock = newTrans;

        UIManager.Instance.SetScore(StackCount, ComboCount);

        return true;
    }

	//新しいブロックを生成させた後、動きを担当する関数
	void MoveBlock()
    {
        
        BlockTransition += Time.deltaTime * BLOCK_MOVING_SPEED;
     
        float movePosition = Mathf.PingPong(BlockTransition, BOUND_SIZE) - BOUND_SIZE / 2;


		//x軸移動
		if (IsMovingX)
        {
            LastBlock.localPosition =
                new Vector3(movePosition * MOVING_BOUNDS_SIZE,
                StackCount, SecondaryPosition);
        }
		//z軸移動
		else
		{
            LastBlock.localPosition =
                new Vector3(SecondaryPosition,
                StackCount, -movePosition * MOVING_BOUNDS_SIZE);
        }

    }

	//以前ブロックとの位置、大きさチェックするコード
	bool PlaceBlock()
    {
       
        Vector3 lastPosition = LastBlock.transform.localPosition;

        if (IsMovingX)
        {
          
            float deltaX = PrevBlockPosition.x - lastPosition.x;

            bool isNegativeNum = (deltaX < 0) ? true : false;


            deltaX = Mathf.Abs(deltaX);
            
            if (deltaX > ERROR_MARGIN)
            {
                ComboCount = 0;
               
                if (StackBounds.x <= 0)
                {
                    return false;
                }

            
                float middle = (PrevBlockPosition.x + lastPosition.x) / 2;
               
                LastBlock.localScale = new Vector3(StackBounds.x, 1, StackBounds.y);

                Vector3 tempPosition = LastBlock.localPosition;
                tempPosition.x = middle; 

                LastBlock.localPosition = lastPosition = tempPosition;

                float rubbleHalfScale = deltaX / 2;

                CreateRubble(new Vector3(
                    isNegativeNum
                    ? lastPosition.x + StackBounds.x / 2 + rubbleHalfScale
                    : lastPosition.x - StackBounds.x / 2 - rubbleHalfScale
                    , lastPosition.y
                    , lastPosition.z),
                    new Vector3(deltaX, 1, StackBounds.y)
                    );
            }
            else
            {
                LastBlock.localPosition =
                    PrevBlockPosition + Vector3.up;
                ComboCheck();
            }
        }    
        else
        {
            float deltaZ = PrevBlockPosition.z - lastPosition.z;
            bool isNegativeNum = (deltaZ < 0) ? true : false;
            deltaZ = Mathf.Abs(deltaZ);
            if (deltaZ > ERROR_MARGIN)
            {
                ComboCount = 0;
                StackBounds.y -= deltaZ;
                if (StackBounds.y <= 0)
                {
                    return false;
                }

                float middle = (PrevBlockPosition.z +
                    lastPosition.z) / 2;
                LastBlock.localScale = new Vector3(StackBounds.x,
                    1, StackBounds.y);

                Vector3 tempPosition = LastBlock.localPosition;
                tempPosition.z = middle;
                LastBlock.localPosition = lastPosition = tempPosition;

                float rubbleHalfScale = deltaZ / 2;
                CreateRubble(
                    new Vector3(
                      lastPosition.x
                    , lastPosition.y
                    , isNegativeNum
                    ? lastPosition.z + StackBounds.y / 2 + rubbleHalfScale
                    : lastPosition.z - StackBounds.y / 2 - rubbleHalfScale),
                    new Vector3(StackBounds.x, 1, deltaZ)
                    );
            }
            else
            {
                LastBlock.localPosition =
                    PrevBlockPosition + Vector3.up;
                ComboCheck();
            }
        }

       SecondaryPosition =
            (IsMovingX) ? LastBlock.localPosition.x :
            LastBlock.localPosition.z;

        return true;
    }

	//切れた破片生成関数
	void CreateRubble(Vector3 pos, Vector3 scale)
    {
       
        GameObject go = Instantiate(LastBlock.gameObject);
        
        go.transform.parent = this.transform;

        go.transform.localPosition = pos;
       
        go.transform.localScale = scale;
      
        go.transform.localRotation = Quaternion.identity;

        go.AddComponent<Rigidbody>();
       
        go.name = "Rubble";
    }

	//ブロックをよく合わせた際コンボチェック
	void ComboCheck()
    {
        ComboCount++;

        if (MaxCombo < ComboCount)
            MaxCombo = ComboCount;

       
        if ((ComboCount % 5) == 0)
        {
            Debug.Log("5Combo Success!");
           
            StackBounds *= 1.2f;

       
            StackBounds.x =
                (StackBounds.x > BOUND_SIZE) ? BOUND_SIZE : StackBounds.x;
            StackBounds.y =
                (StackBounds.y > BOUND_SIZE) ? BOUND_SIZE : StackBounds.y;
        }
    }

	//ランダムカラーを求めるのに100~250区間である理由はパステルトーンを維持するため
	Color GetRandomColor()
    {
        float r = Random.Range(100f, 250f) / 255f;
        float g = Random.Range(100f, 250f) / 255f;
        float b = Random.Range(100f, 250f) / 255f;

        return new Color(r, g, b);
    }

	//아래 선형보간을 이용해서 색을 조금씩 바꾼다.
	void ColorChange(GameObject go)
    {
        Color applyColor =
            Color.Lerp(
                PrevColor, NextColor,
                (StackCount % 11) / 10f);

        Renderer rn = go.GetComponent<Renderer>();

        if (rn == null)
        {
            Debug.Log("Renderer is NULL!");
            return;
        }

        rn.material.color = applyColor;
        Camera.main.backgroundColor =
            applyColor - new Color(0.1f, 0.1f, 0.1f);

        if (applyColor.Equals(NextColor) == true)
        {
            PrevColor = NextColor;
            NextColor = GetRandomColor();
        }
    }

	//ゲーム終了時の効果
	void EndEffect()
    {
        int childCount = this.transform.childCount;

        for (int i = 1; i < 20; i++)
        {

            if (childCount < i)
                break;

            GameObject go =
                this.transform.GetChild(childCount - i).gameObject;

            if (go.name.Equals("Rubble"))
                continue;

            Rigidbody rigid = go.AddComponent<Rigidbody>();

            rigid.AddForce(
                (Vector3.up * Random.Range(0, 10f)
                + Vector3.right * (Random.Range(0, 10f) - 5f))
                 * 100f
                );
        }
    }

	//スコアチェック
	void CheckScore()
    {
        if (BestScore < StackCount)
        {
            BestScore = StackCount;
            BestCombo = MaxCombo;

            PlayerPrefs.SetInt(BEST_SCORE_KEY, BestScore);
            PlayerPrefs.SetInt(BEST_COMBO_KEY, BestCombo);
        }
    }

	//再起動のために全部初期化してくれる関数
	public void Restart()
    {
        int childCount = this.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }

        IsGameOver = false;
        
        LastBlock = null;

		DesiredPosition = Vector3.zero;

        StackBounds = new Vector2(BOUND_SIZE, BOUND_SIZE);

        StackCount = -1;
       
        IsMovingX = true;
       
        BlockTransition = 0f;
       
        SecondaryPosition = 0f;
       
        ComboCount = 0;
      
        PrevBlockPosition = Vector3.down;

        PrevColor = GetRandomColor();
        NextColor = GetRandomColor();
  
        Spawn_Block();
        Spawn_Block();
    }
}

