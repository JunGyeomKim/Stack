using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UI_STATE
{
    NONE,
    HOME,
    GAME,
    SCORE,
    MAX
}

public class UIManager : MonoBehaviour
{
    static UIManager instance;
    public static UIManager Instance
    {
        // get; set;
        get
        {
            return instance;
        }
    }


    UI_STATE CurrState = UI_STATE.HOME;

    public TheStack StackScript = null;

    // HOME UI
    Transform HomeUI = null;
    UIButton StartBtn = null;

    // GAME UI
    Transform GameUI = null;
    UILabel ScoreLabel = null;
    Transform ComboRoot = null;
    UILabel ComboCount = null;

    // SCORE UI
    Transform ScoreUI = null;

    private void Awake()
    {
        instance = this;

        HomeUI = this.transform.Find("HomeUI");
        GameUI = this.transform.Find("GameUI");
        ScoreUI = this.transform.Find("ScoreUI");

        if (HomeUI == null || GameUI == null || ScoreUI == null)
        {
            Debug.Log("UI Transform is NULL" +
                HomeUI + ", " + GameUI + ", " + ScoreUI);
            return;
        }

        //StartBtn = this.transform.FindChild(
        //    "HomeUI/StartBtn").GetComponent<UIButton>();

        // Not Exception
        //StartBtn = 
        //    HomeUI.FindChild("StartBtn").GetComponent<UIButton>();
        ////            null              .GetComponent
        //// -> Null Reference Error

        // Exception
        Transform tempTrans = null;

        tempTrans = HomeUI.Find("StartBtn");
        if(tempTrans == null)
        {
            Debug.Log("Not Founded StartBtn!");
            return;
        }

        StartBtn = tempTrans.GetComponent<UIButton>();
        
        Init();
    }

    void Init()
    {
        // HomeUI
        //StartBtn.onClick.Add(
        //    new EventDelegate(this, "OnClickedStartBtn"));

        EventDelegate.Add(StartBtn.onClick,
           new EventDelegate(this, "OnClickedStartBtn"));

        // GameUI
        ScoreLabel = 
            GameUI.Find("Score/ScoreLabel").GetComponent<UILabel>();
        ComboRoot = GameUI.Find("Combo");
        ComboCount = 
            ComboRoot.Find("ComboCount").GetComponent<UILabel>();

        // ScoreUI
        EventDelegate.Add(
ScoreUI.Find("QuitBtn").GetComponent<UIButton>().onClick
            , new EventDelegate(this, "OnClickedQuitBtn"));

        EventDelegate.Add(
ScoreUI.Find("ReStartBtn").GetComponent<UIButton>().onClick
            , new EventDelegate(this, "OnClickedReTry"));

        ChangeState(UI_STATE.HOME);
    }

    // 게임오버
    public void SetScore(int bestS, int bestC, int s, int c)
    {
        ScoreUI.Find(
            "BestScore/Score/ScoreCount"
            ).GetComponent<UILabel>().text = bestS.ToString();
        ScoreUI.Find(
            "BestScore/Combo/ComboCount"
            ).GetComponent<UILabel>().text = bestC.ToString();
        ScoreUI.Find(
            "CurrScore/Score/ScoreCount"
            ).GetComponent<UILabel>().text = s.ToString();
        ScoreUI.Find(
            "CurrScore/Combo/ComboCount"
            ).GetComponent<UILabel>().text = c.ToString();

        ChangeState(UI_STATE.SCORE);
    }


    public void ChangeState(UI_STATE state)
    {
        HomeUI.gameObject.SetActive(false);
        GameUI.gameObject.SetActive(false);
        ScoreUI.gameObject.SetActive(false);

        switch (state)
        {
            case UI_STATE.HOME:
                HomeUI.gameObject.SetActive(true);
                break;

            case UI_STATE.GAME:
                GameUI.gameObject.SetActive(true);
                break;

            case UI_STATE.SCORE:
                ScoreUI.gameObject.SetActive(true);
                break;           
        }
        CurrState = state;
    }




    public void OnClickedStartBtn()
    {
        StackScript.Restart();
        ChangeState(UI_STATE.GAME);
    }

    public void OnClickedReTry()
    {
        StackScript.Restart();
        ChangeState(UI_STATE.GAME);
    }

    public void OnClickedQuitBtn()
    {
        Application.Quit();
    }





    public void SetScore(int score, int combo)
    {
        ScoreLabel.text = score.ToString();

        if(combo == 0)
        {
            ComboRoot.gameObject.SetActive(false);
        }
        else
        {
            if(ComboRoot.gameObject.activeSelf == false)
                ComboRoot.gameObject.SetActive(true);

            ComboCount.text = combo.ToString();
        }
    }

}
