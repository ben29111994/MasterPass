using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.IO.IsolatedStorage;

public class Movement : MonoBehaviour
{
    public Color botColor;
    Vector3 look;
    public Rigidbody rigid;
    public GameObject target;
    private Vector3 velocity = Vector3.zero;
    public float speed;
    bool isRun = false;
    bool isTackle = false;
    public Animator anim;
    public GameController gameController;
    public Transform footLeft;
    public Transform footRight;
    public int count;
    public Text countText;
    public GameObject canvasPrefab;
    public int AILevel;
    public float botKeeperRange;
    Vector3 botKeeperPos;
    public Transform zone;
    float randomWait;

    // Start is called before the first frame update
    void OnEnable()
    {
        Setup();
    }

    public void Setup()
    {
        isRun = false;
        anim = GetComponent<Animator>();
        anim.Play("Idle");
        rigid = GetComponent<Rigidbody>();
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        if (CompareTag("Bot"))
        {
            if (AILevel == 1)
                speed = 4;
            if (AILevel == 2)
                speed = 5;
            if (AILevel == 3)
                speed = 6;
            if (AILevel == 4)
                speed = 7;
            if (!GameController.instance.isPass)
            {
                AILevel = 5;
                speed = 3;
            }
            anim.speed = speed / 4;
            StartCoroutine(BotAI());
            var scaleValue = 0.005f + speed / 2000;
            zone.transform.localScale = Vector3.one * scaleValue;
        }
        if (CompareTag("Player"))
        {
            speed = 10;
            //var spawnCanvas = Instantiate(canvasPrefab);
            //spawnCanvas.transform.localPosition = new Vector3(transform.position.x, 0.1f, transform.position.z - 1);
            //countText = spawnCanvas.transform.GetChild(0).GetComponent<Text>();
            count = 0;
            //countText.text = count.ToString();
        }
        if (CompareTag("BotKeeper"))
        {
            if (AILevel == 1)
                speed = 5;
            if (AILevel == 2)
                speed = 6;
            if (AILevel == 3)
                speed = 7;
            if (AILevel == 4)
                speed = 8;
            //if (!GameController.instance.isPass)
            //{
            //    AILevel = 5;
            //    speed = 3;
            //}
            botKeeperPos = transform.position;
            anim.speed = speed / 4;
            StartCoroutine(BotKeeper());
            var scaleValue = 0.005f + speed / 2000;
            zone.transform.localScale = Vector3.one * scaleValue;
        }
        if (CompareTag("Control"))
        {
            speed = 10;
            anim.speed = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!CompareTag("Control"))
        {
            look = GameController.instance.ball.transform.position - transform.position;
            if (look != Vector3.zero)
            {
                var targetRot = Quaternion.LookRotation(look);
                var modTargetRot = targetRot;
                modTargetRot.x = 0;
                modTargetRot.z = 0;
                targetRot = modTargetRot;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * 50 * speed);
            }
            if (GameController.instance.isStartGame && isRun)
            {
                transform.Translate(Vector3.forward * Time.deltaTime * speed);
            }
            if (!GameController.instance.isStartGame && randomWait > 0)
            {
                randomWait = 0;
            }
        }
    }

    IEnumerator BotKeeper()
    {
        anim.SetTrigger("MoveL");
        transform.DOMoveX(botKeeperPos.x + botKeeperRange, botKeeperRange / 2);
        yield return new WaitForSeconds(botKeeperRange / 2 - 0.3f);
        anim.SetTrigger("Sad");
        yield return new WaitForSeconds(0.4f);
        anim.SetTrigger("MoveR");
        transform.DOMoveX(botKeeperPos.x - botKeeperRange, botKeeperRange / 2);
        yield return new WaitForSeconds(botKeeperRange / 2 - 0.3f);
        anim.SetTrigger("Sad");
        yield return new WaitForSeconds(0.4f);
        StartCoroutine(BotKeeper());
    }

    IEnumerator BotAI()
    {
        yield return new WaitForSeconds(1);
        randomWait = Random.Range(1f, 2f); ;
        //randomWait = 1;
        //if(AILevel == 1)
        //{
        //    randomWait = 0;
        //}
        //if (GameController.instance.isStartGame)
        //{
        //    if (isRun)
        //        randomWait = Random.Range(0, 0.5f);
        //    else
        //        randomWait = Random.Range(0.25f, 0.5f);
        //    if (AILevel == 5)
        //    {
        //        randomWait = Random.Range(1f, 2f);
        //    }
        //}
        //else
        //{
        //    anim.SetTrigger("Sad");
        //}
        yield return new WaitForSeconds(randomWait);
        if (GameController.instance.isStartGame)
        {
            int maxValue = 100;
            if (AILevel == 5)
            {
                maxValue = 200;
                int randomRun = Random.Range(0, maxValue);
                if (randomRun < 70)
                {
                    isRun = true;
                    anim.speed = speed / 4;
                    anim.SetTrigger("Run");
                }
                else
                {
                    isRun = false;
                    anim.SetTrigger("Sad");
                }
            }
            //int randomRun = Random.Range(0, maxValue);
            //if (randomRun < 70)
            //{
            else
            {
                isRun = true;
                anim.speed = speed / 4;
                anim.SetTrigger("Run");
            }
            //}
            //else
            //{
            //    isRun = false;
            //    anim.SetTrigger("Sad");
            //}
        }
        else
        {
            anim.SetTrigger("Sad");
        }
        if(!isTackle)
        StartCoroutine(BotAI());
    }

    public void Counting()
    {
        count++;
        //countText.text = count.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            if ((CompareTag("Bot") || CompareTag("BotKeeper")) && !isTackle && GameController.instance.isStartGame)
            {
                isTackle = true;
                isRun = false;
                GameController.instance.isControl = false;
                StopAllCoroutines();
                zone.gameObject.SetActive(false);
                if (GameController.instance.isPass)
                    GameController.instance.Lose();
                else
                    GameController.instance.WinMethod();
                GameController.instance.isHoldBall = false;
                if (GameController.instance.holdBall != null)
                {
                    GameController.instance.StopCoroutine(GameController.instance.holdBall);
                }
                GameController.instance.lastHolder.GetComponent<Movement>().anim.SetTrigger("Fall");
                GameController.instance.lastHolder = null;
                //anim.speed = 2;
                //anim.StopPlayback();
                anim.SetTrigger("Tackle");
                transform.DOKill();
                //var dir = transform.position - GameController.instance.ball.transform.position;
                //transform.DOMove(transform.forward * 5, 0.85f);
                rigid.velocity = Vector3.zero;
                rigid.AddForce(transform.forward * 650);
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.transform.DOKill();
                other.transform.DOMove(transform.position + transform.forward, 0.1f).OnComplete(() =>
                {
                    other.transform.DOKill();
                    other.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
                    //transform.DOMove(transform.position, 1).OnComplete(() =>
                    //{
                    zone.transform.DOScale(0, 0.2f).OnComplete(() =>
                    {
                        DOTween.To(() => rigid.velocity, x => rigid.velocity = x, new Vector3(0, 0, 0), 0.5f);
                        //rigid.AddForce(transform.forward * -750);
                        //anim.speed = 1;
                        //anim.SetTrigger("Win");
                    });
                });
                //transform.DOLocalMoveY(-0.5f, 0.5f).SetLoops(2, LoopType.Yoyo);
            }

            if(CompareTag("Player"))
            {
                //GameController.instance.isTakeBall = true;
                if (GameController.instance.isControl)
                {
                    if (GameController.instance.isStartGame && GameController.instance.lastHolder == transform.gameObject)
                    {
                        other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        var pos = transform.position + transform.forward;
                        pos.y += 0.5f;
                        other.transform.DOMove(pos, 0.2f);
                        count--;
                        if (count < 0)
                            count = 0;
                        //countText.text = count.ToString();
                        GameController.instance.Leveling();
                    }
                    else
                    {
                        //var pos = transform.position + transform.forward;
                        //pos.y += 0.5f;
                        //other.transform.DOMove(pos, 0.2f);
                        GameController.instance.Choose(transform);
                    }
                }
            }

            if(CompareTag("Control"))
            {
                StopAllCoroutines();
                zone.gameObject.SetActive(false);
                //if (GameController.instance.isPass)
                //    GameController.instance.Lose();
                //else
                GameController.instance.WinMethod();
                GameController.instance.isHoldBall = false;
                if (GameController.instance.holdBall != null)
                {
                    GameController.instance.StopCoroutine(GameController.instance.holdBall);
                }
                GameController.instance.lastHolder.GetComponent<Movement>().anim.SetTrigger("Fall");
                GameController.instance.lastHolder = null;
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Tackle"))
                {
                    anim.SetTrigger("Pass");
                }
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.transform.DOKill();
                other.transform.DOMove(transform.position + transform.forward, 0.1f).OnComplete(() =>
                {
                    other.transform.DOKill();
                    other.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
                    //transform.DOMove(transform.position, 2f).OnComplete(() =>
                    //{
                    //    anim.speed = 1;
                    //    //anim.SetTrigger("Win");
                    //});
                });
            }
        }
    }
}
