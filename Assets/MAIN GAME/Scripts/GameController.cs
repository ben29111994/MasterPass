using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine.EventSystems;
using System.Linq;
using VisCircle;
using UnityEngine.PostProcessing;

public class GameController : MonoBehaviour
{
    [Header("Variable")]
    public static GameController instance;
    public int maxLevel;
    public bool isStartGame = false;
    public bool isControl = true;
    bool isDrag = false; 
    int maxPlusEffect = 0;
    bool isVibrate = false;
    Rigidbody ballRigid;
    public float speed;
    public List<Transform> listCommand = new List<Transform>();
    public LayerMask clickMask;
    public LayerMask dragMask;
    MeshRenderer arrow;
    public bool isPass = true;

    [Header("UI")]
    public Slider progress;
    public GameObject winPanel;
    public GameObject losePanel;
    public Text currentLevelTextBig;
    public Text currentLevelText;
    public Text nextLevelText;
    public int currentLevel;
    public Canvas canvas;
    public GameObject startGameMenu;
    public GameObject shopMenu;
    public Transform ballTab;
    public Transform charTab;
    public Text title;
    static int currentBG = 0;
    public InputField levelInput;
    public Image compliment;
    public List<Sprite> listCompliment = new List<Sprite>();
    public Text taskText;
    public GameObject tutorial;
    public GameObject startButton;
    public Text coinText;
    int coin;
    int tempCoin;
    public List<GameObject> listShopChar = new List<GameObject>();
    public List<GameObject> listShopBall = new List<GameObject>();
    public List<Gem> listCoinAnim = new List<Gem>();

    [Header("Objects")]
    public GameObject ball;
    public GameObject plusVarPrefab;
    public GameObject conffeti;
    GameObject conffetiSpawn;
    public List<GameObject> listLevel = new List<GameObject>();
    public List<GameObject> listStop = new List<GameObject>();
    public List<GameObject> listPass = new List<GameObject>();
    public GameObject charStop;
    public List<Color> listBGColor = new List<Color>();
    public GameObject BG;
    public GameObject blast;
    public GameObject indicator;
    public GameObject lastHolder;
    public Coroutine holdBall;
    public bool isHoldBall = false;
    public List<GameObject> listBalls = new List<GameObject>();
    public List<Texture> listChars = new List<Texture>();

    private void OnEnable()
    {
        // PlayerPrefs.DeleteAll();
        Application.targetFrameRate = 60;
        instance = this;
        arrow = GetComponent<MeshRenderer>();
        StartCoroutine(delayStart());
    }

    IEnumerator delayStart()
    {
        yield return new WaitForSeconds(0.001f);
        Camera.main.transform.DOMoveX(-30, 0);
        Camera.main.transform.DOMoveX(0, 1);
        maxLevel = listLevel.Count - 1;
        currentLevel = PlayerPrefs.GetInt("currentLevel");
        currentLevelTextBig.text = "LEVEL " + (currentLevel + 1).ToString();
        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = (currentLevel + 1).ToString();
        progress.value = 0;
        progress.maxValue = 10 + currentLevel;
        taskText.text = progress.value + "/" + progress.maxValue;
        var colorID = Random.Range(0, 5);
        BG.GetComponent<Renderer>().material.color = listBGColor[currentBG];
        currentBG++;
        if (currentBG > listBGColor.Count - 1)
        {
            currentBG = 0;
        }
        listLevel[currentLevel].SetActive(true);
        ball = listLevel[currentLevel].transform.GetChild(0).gameObject;
        ball.transform.parent = null;
        ballRigid = ball.GetComponent<Rigidbody>();
        var getAllBot = listLevel[currentLevel].transform.GetComponentsInChildren<Animator>();
        foreach(var item in getAllBot)
        {
            if(item.CompareTag("Bot"))
            {
                listStop.Add(item.gameObject);
            }
            if(item.CompareTag("BotKeeper"))
            {
                listStop.Add(item.gameObject);
            }
            if(item.CompareTag("Player"))
            {
                listPass.Add(item.gameObject);
            }
        }
        startGameMenu.SetActive(true);
        title.DOColor(new Color32(255, 255, 255, 0), 3);
        isControl = true;
        coin = PlayerPrefs.GetInt("Coin");
        coinText.text = coin.ToString();

        Vector3 ballPos = ball.transform.position;
        ball.SetActive(false);
        var indexSkinBall = PlayerPrefs.GetInt("CurrentBall");
        ball = listBalls[indexSkinBall];
        ball.transform.position = ballPos;
        ball.SetActive(true);
        ballRigid = ball.GetComponent<Rigidbody>();
    }

    //Old control
    //private void Update()
    //{
    //    if (isControl)
    //    {
    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            OnMouseDown();
    //        }
    //    }
    //}

    //void OnMouseDown()
    //{
    //    tutorial.SetActive(false);
    //    startButton.GetComponent<TrignometricScaling>().enabled = true;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, 1000, clickMask))
    //    {
    //        Choose(hit.transform);
    //    }
    //}

    //New control\
    float h, v;
    Vector3 firstP, lastP, dir;
    public Transform target;

    private void Update()
    {
        if (isStartGame && isControl)
        {
            if (isPass)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnMouseDown();
                }

                if (Input.GetMouseButton(0))
                {
                    OnMouseDrag();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    OnMouseUp();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnMouseDownS();
                }

                if (Input.GetMouseButton(0))
                {
                    OnMouseDragS();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    OnMouseUpS();
                }
            }
        }
        else if(!isPass)
        {
            if (charStop.GetComponent<Movement>().anim.GetCurrentAnimatorStateInfo(0).IsName("Running"))
            {
                charStop.GetComponent<Movement>().anim.SetTrigger("Sad");
            }
        }
        //else if (!isStartGame && isControl)
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        ButtonStartGame();
        //        OnMouseDown();
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.D))
        {
            PlayerPrefs.DeleteAll();
        }
    }

    void OnMouseDown()
    {
        if (!isDrag)
        {
            isDrag = true;
            target = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, dragMask))
            {
                firstP = hit.point;
            }
        }
    }

    public RaycastHit[] hit2;
    void OnMouseDrag()
    {
        if (isDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, dragMask))
            {
                lastP = hit.point;
            }
            if (Vector3.Distance(firstP, lastP) > 0.05f)
            {
                //#if UNITY_EDITOR
                //                h = Input.GetAxis("Mouse X");
                //                v = Input.GetAxis("Mouse Y");
                //#endif
                //#if UNITY_IOS
                //                if (Input.touchCount > 0)
                //                {
                //                    h = Input.touches[0].deltaPosition.x / 8;
                //                    v = Input.touches[0].deltaPosition.y / 8;
                //                }
                //#endif
                //dir = new Vector3(h, 0, v);
                //if (dir != Vector3.zero)
                //{
                //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), 5 * Time.deltaTime);
                //}
                // transform.Translate(Vector3.forward * Time.deltaTime * speed);
                dir = lastP - firstP;
                var modDir = dir;
                modDir.y = 0.5f;
                dir = modDir;
                if (dir != Vector3.zero)
                {
                    var targetRot = Quaternion.LookRotation(-dir);
                    var modTargetRot = targetRot;
                    modTargetRot.x = 0;
                    modTargetRot.z = 0;
                    targetRot = modTargetRot;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * 3600);
                }

                if (lastHolder != null)
                {
                    lastHolder.layer = 0;
                    int index = 0;
                    arrow.enabled = true;
                    transform.position = lastHolder.transform.position + dir.normalized * 2;
                    hit2 = null;
                    hit2 = Physics.BoxCastAll(arrow.transform.position, new Vector3(2,1,0.1f), dir, arrow.transform.rotation, 1000, clickMask);

                    //Debug.Log(hit2.Length);
                    if (hit2.Length > 0)
                    {
                        float offset = 360;
                        if(hit2.Length > 1)
                        {
                            for(int i = 1; i < hit2.Length; i++)
                            {
                                float angle = Vector3.Angle(dir - lastHolder.transform.position, hit2[i].transform.position - lastHolder.transform.position);
                                //Debug.Log("Angle: " + angle);
                                if (angle < offset)
                                {
                                    index = i;
                                    offset = angle;
                                }
                            }
                        }
                        //Debug.Log(hit2.transform.name);
                        if (hit2[index].transform.CompareTag("Player"))
                        {
                            target = hit2[index].transform;
                            indicator.transform.position = new Vector3(target.position.x, 0.05f, target.position.z);
                        }

                        if (target != null)
                        {
                            arrow.enabled = false;
                            isDrag = false;
                            Choose(target);
                        }
                    }
                }
            }
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {
            arrow.enabled = false;
            isDrag = false;
            //if(target != null)
            //{
            //    Choose(target);
            //}
        }
    }

    void OnMouseDownS()
    {
        if (!isDrag)
        {
            charStop.GetComponent<Movement>().anim.SetTrigger("Run");
            isDrag = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, dragMask))
            {
                firstP = hit.point;
            }
        }
    }

    void OnMouseDragS()
    {
        if (isDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, dragMask))
            {
                lastP = hit.point;
            }
            if (Vector3.Distance(firstP, lastP) > 0.01f)
            {
#if UNITY_EDITOR
                h = Input.GetAxis("Mouse X");
                v = Input.GetAxis("Mouse Y");
#endif
#if UNITY_IOS
                if (Input.touchCount > 0)
                {
                    h = Input.touches[0].deltaPosition.x / 8;
                    v = Input.touches[0].deltaPosition.y / 8;
                }
#endif
                dir = new Vector3(h, 0, v);
                if (dir != Vector3.zero)
                {
                    charStop.transform.rotation = Quaternion.Lerp(charStop.transform.rotation, Quaternion.LookRotation(dir), 10 * Time.deltaTime);
                }
                charStop.transform.Translate(Vector3.forward * Time.deltaTime * speed);
                if (!charStop.GetComponent<Movement>().anim.GetCurrentAnimatorStateInfo(0).IsName("Running"))
                {
                    charStop.GetComponent<Movement>().anim.SetTrigger("Run");
                }
            }
        }
        else
        {
            if (!charStop.GetComponent<Movement>().anim.GetCurrentAnimatorStateInfo(0).IsName("Sad"))
            {
                charStop.GetComponent<Movement>().anim.SetTrigger("Sad");
            }
        }
    }

    void OnMouseUpS()
    {
        if (isDrag)
        {
            isDrag = false;
            //charStop.GetComponent<Movement>().anim.speed = 2;
            charStop.GetComponent<Movement>().anim.SetTrigger("Tackle");
            //charStop.GetComponent<Rigidbody>().AddForce(charStop.transform.forward * 250);
            //charStop.GetComponent<Rigidbody>().velocity = Vector3.zero;
            charStop.transform.DOKill();
            /*charStop.transform.DOMove(charStop.transform.forward.normalized * 5, 0.85f);*//*.OnComplete(() =>*/
            //{
            //    charStop.GetComponent<Movement>().anim.SetTrigger("Sad");
            //});
            charStop.GetComponent<Rigidbody>().AddForce(charStop.transform.forward * 650);         
            transform.DOMove(transform.position, 0.2f).OnComplete(() =>
            {
                DOTween.To(() => charStop.GetComponent<Rigidbody>().velocity, x => charStop.GetComponent<Rigidbody>().velocity = x, new Vector3(0, 0, 0), 0.5f);
            });
        }
    }

    public void Choose(Transform target)
    {
        if (listCommand.Count > 0 && target != listCommand[listCommand.Count - 1] || listCommand.Count == 0)
        {
            if (isPass && listCommand.Count == 0)
            {
                listCommand.Add(target);
                indicator.SetActive(true);
                indicator.transform.position = new Vector3(target.position.x, 0.05f, target.position.z);
                target.GetComponent<Movement>().Counting();
            }
        }
    }

    public List<Transform> availableList = new List<Transform>();
    void ChooseRandom()
    {
        //float random = 0;
        //yield return new WaitForSeconds(random);
        foreach (var item in listPass)
        {
            if(!listCommand.Contains(item.transform) && !availableList.Contains(item.transform) && item != tempRemove)
            {
                availableList.Add(item.transform);
            }
        }
        float offset = 0;
        int index = 0;
        Transform target;
        if (lastHolder != null)
        {
            for (int i = 0; i < availableList.Count; i++)
            {
                float angle = Mathf.Abs(Vector3.Angle(availableList[i].transform.position - lastHolder.transform.position, charStop.transform.position - lastHolder.transform.position));
                Debug.Log("Angle: " + angle);
                if (angle > offset)
                {
                    index = i;
                    offset = angle;
                }
            }
            target = availableList[index].transform;
        }
        else
        {
            target = availableList[Random.Range(0, availableList.Count - 1)].transform;
        }
        //target = availableList[Random.Range(0, availableList.Count - 1)].transform;
        //yield return new WaitForSeconds(random);
        listCommand.Add(target);
        //indicator.SetActive(true);
        //indicator.transform.position = new Vector3(target.position.x, 0.05f, target.position.z);
        target.GetComponent<Movement>().Counting();
    }

    public void Leveling()
    {
        compliment.gameObject.SetActive(true);
        compliment.sprite = listCompliment[Random.Range(0, listCompliment.Count - 1)];
        compliment.transform.DOKill();
        compliment.transform.localScale = Vector3.zero;
        compliment.transform.DOScale(Vector3.one * 0.5f, 1f).SetEase(Ease.OutBounce).OnComplete(()=> 
        {
            compliment.gameObject.SetActive(false);
        });
        progress.value++;
        if (isPass)
        {
            tempCoin += 10;
        }
        taskText.text = progress.value + "/" + progress.maxValue;
        if (progress.value >= progress.maxValue)
        {
            if (isPass)
                StartCoroutine(Win());
            else
                Lose();
        }
    }

    Transform tempRemove;
    //public bool isTakeBall = true;
    IEnumerator RunCommand()
    {
        //isTakeBall = false;
        bool isStopThis = false;
        while(!isStartGame || listCommand.Count == 0 || /*(charStop != null && (Vector3.Distance(ball.transform.position, charStop.transform.position) > 20 && (listStop.Count > 0 && Vector3.Distance(ball.transform.position, listStop[0].transform.position) > 20)))*/ /*||*/ (!isDrag && listStop.Count == 0))
        {
            if (lastHolder != null && !isHoldBall)
            {
                indicator.SetActive(true);
                indicator.transform.position = new Vector3(lastHolder.transform.position.x, 0.05f, lastHolder.transform.position.z);
                isHoldBall = true;
                holdBall = StartCoroutine(HoldBall());
            }
            yield return null;
        }
        if (isHoldBall)
        {
            StopCoroutine(holdBall);
            ball.transform.DOKill();
            isHoldBall = false;
        }
        //ball.transform.DOMove(listCommand[0].transform.position, 1);
        ballRigid.isKinematic = false;
        var target = listCommand[0].transform.position - ball.transform.position;

        if (!isPass)
        {
            RaycastHit hit2;
            Transform tempTarget;
            if (lastHolder != null)
            {
                arrow.enabled = true;
                transform.position = lastHolder.transform.position + target.normalized * 2;
                if (Physics.Raycast(lastHolder.transform.position, target, out hit2, 1000, clickMask))
                {
                    Debug.Log(hit2.transform.name);
                    if (hit2.transform.CompareTag("Player"))
                    {
                        tempTarget = hit2.transform;
                        indicator.SetActive(true);
                        indicator.transform.position = new Vector3(tempTarget.position.x, 0.05f, tempTarget.position.z);
                        //if (!isPass)
                        //{
                        //    yield return new WaitForSeconds(0.2f);
                        //}
                    }
                    if (hit2.transform.CompareTag("Bot") || hit2.transform.CompareTag("BotKeeper") || hit2.transform.CompareTag("Control"))
                    {
                        //Debug.LogError("No!");
                        //lastHolder = listCommand[0].gameObject;
                        tempRemove = listCommand[0];
                        availableList.Remove(listCommand[0]);
                        listCommand.RemoveAt(0);
                        //yield return new WaitForSeconds(0.5f);
                        if (lastHolder != null && !isHoldBall)
                        {
                            isHoldBall = true;
                            holdBall = StartCoroutine(HoldBall());
                        }
                        if (isStartGame)
                            ChooseRandom();
                        StartCoroutine(RunCommand());
                        isStopThis = true;
                    }
                }
            }
        }

        if (!isStopThis)
        {
            //if (!isPass)
            //{
            //    yield return new WaitForSeconds(0.15f);
            //}
            if (listCommand.Count > 0 && lastHolder != null)
            {
                lastHolder.GetComponent<Movement>().anim.SetTrigger("Pass");
            }
            //if (lastHolder != null)
            //{
            //    var moveBallPos = listCommand[0].transform.position - lastHolder.transform.position;
            //    ball.transform.DOMove(moveBallPos.normalized * 2, 0.1f);
            //}
            //if(!isStartGame)
            //{
            //    isTakeBall = true;
            //}
            //else
            //    isTakeBall = false;
            yield return new WaitForSeconds(0.2f);
            var forceValue = 160;
            if(!isPass)
            {
                forceValue = 130;
            }
            ballRigid.AddForce(target * forceValue);
            if (lastHolder != null)
                lastHolder.layer = 8;
            lastHolder = listCommand[0].gameObject;
            listCommand.RemoveAt(0);
            //while (!isTakeBall)
            //{
            //    yield return null;
            //}
            //Debug.Log(isTakeBall);
            yield return new WaitForSeconds(0.6f);
            if (lastHolder != null)
                lastHolder.GetComponent<Movement>().anim.SetTrigger("Sad");
            if (isStartGame && !isPass)
                ChooseRandom();
            StartCoroutine(RunCommand());
        }
    }

    IEnumerator HoldBall()
    {
        //Debug.Log("Hold");
        ballRigid.velocity = Vector3.zero;
        //ball.transform.parent = lastHolder.GetComponent<Movement>().footLeft.transform;
        lastHolder.GetComponent<Movement>().anim.SetTrigger("HoldLeft");
        ball.transform.DOLocalMove(lastHolder.GetComponent<Movement>().transform.position + lastHolder.GetComponent<Movement>().transform.forward, 0.1f);
        yield return new WaitForSeconds(0.2f);
        ball.transform.DOMoveY(ball.transform.position.y + 1f, 0.2f).SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(0.3f);
        //ball.transform.parent = lastHolder.GetComponent<Movement>().footRight.transform;
        lastHolder.GetComponent<Movement>().anim.SetTrigger("HoldRight");
        ball.transform.DOLocalMove(lastHolder.GetComponent<Movement>().transform.position + lastHolder.GetComponent<Movement>().transform.forward, 0.1f);
        yield return new WaitForSeconds(0.2f);
        ball.transform.DOMoveY(ball.transform.position.y + 1f, 0.2f).SetLoops(2, LoopType.Yoyo);
        yield return new WaitForSeconds(0.3f);
        if (isHoldBall)
        {
            holdBall = StartCoroutine(HoldBall());
        }
    }

    IEnumerator PlusEffect(Vector3 pos)
    {
        maxPlusEffect++;
        if (!UnityEngine.iOS.Device.generation.ToString().Contains("5") && !isVibrate)
        {
            isVibrate = true;
            StartCoroutine(delayVibrate());
            MMVibrationManager.Haptic(HapticTypes.LightImpact);
        }
        var plusVar = Instantiate(plusVarPrefab);
        plusVar.transform.SetParent(canvas.transform);
        plusVar.transform.localScale = new Vector3(1, 1, 1);
        plusVar.transform.position = new Vector3(pos.x + Random.Range(-50, 50), pos.y + Random.Range(-100, -75), pos.z);
        plusVar.GetComponent<Text>().DOColor(new Color32(255, 255, 255, 0), 1f);
        plusVar.SetActive(true);
        plusVar.transform.DOMoveY(plusVar.transform.position.y + Random.Range(50, 90), 0.5f);
        Destroy(plusVar, 0.5f);
        yield return new WaitForSeconds(0.01f);
        maxPlusEffect--;
    }

    IEnumerator delayVibrate()
    {
        yield return new WaitForSeconds(0.2f);
        isVibrate = false;
    }

    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        return parentCanvas.transform.TransformPoint(movePos);
    }

    //public void ButtonStartGame()
    //{
    //    startGameMenu.SetActive(false);
    //    isStartGame = true;
    //    tutorial.SetActive(false);
    //    startButton.GetComponent<TrignometricScaling>().enabled = true;
    //    StartCoroutine(RunCommand());
    //}

    public void ButtonPass()
    {
        var indexSkin = PlayerPrefs.GetInt("CurrentPlayer");
        foreach (var item in listPass)
        {
            item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[indexSkin];
        }

        foreach (var item in listStop)
        {
            item.layer = 0;
            item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        }

        if (currentLevel == 0)
        {
            foreach (var item in listStop)
            {
                item.SetActive(false);
            }
        }

        isPass = true;
        startGameMenu.SetActive(false);
        isStartGame = true;
        StartCoroutine(TutorialDelay());
        StartCoroutine(RunCommand());
    }

    public void ButtonStop()
    {
        var indexSkin = PlayerPrefs.GetInt("CurrentPlayer");
        foreach (var item in listStop)
        {
            item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[indexSkin];
        }

        foreach (var item in listPass)
        {
            item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
        }

        progress.maxValue = 30;
        isPass = false;
        startGameMenu.SetActive(false);
        isStartGame = true;
        StartCoroutine(TutorialDelay());
        StartCoroutine(RunCommand());
        var index = Random.Range(0, listStop.Count - 1);
        charStop = listStop[index];
        listStop.RemoveAt(index);
        if(listStop.Count > 1)
        {
            for(int i = 0; i < listStop.Count; i++)
            {
                listStop[0].SetActive(false);
                listStop.RemoveAt(0);
            }
        }
        foreach(var item in listStop)
        {
            item.GetComponent<Movement>().Setup();
        }
        //charStop.GetComponent<Movement>().enabled = false;
        charStop.tag = "Control";
        charStop.GetComponent<QuickOutline>().enabled = true;
        charStop.GetComponent<Movement>().StopAllCoroutines();
        ChooseRandom();
    }

    IEnumerator TutorialDelay()
    {
        tutorial.SetActive(true);
        if(isPass)
        {
            tutorial.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            tutorial.transform.GetChild(0).gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(2);
        tutorial.transform.DOMoveY(-1000, 1);
        yield return new WaitForSeconds(1);
        tutorial.SetActive(false);
    }

    bool isCheckShop = false;
    public void ButtonShopMenu()
    {
        shopMenu.SetActive(true);
        if (!isCheckShop)
        {
            isCheckShop = true;
            for (int i = 0; i < listShopBall.Count; i++)
            {
                var checkAgain = PlayerPrefs.GetInt("Ball" + i.ToString());
                if (checkAgain == 1)
                {
                    var checkCurrent = PlayerPrefs.GetInt("CurrentBall");
                    if (i == checkCurrent)
                    {
                        listShopBall[i].transform.GetChild(checkAgain).SetAsLastSibling();
                    }
                    else
                    {
                        PlayerPrefs.SetInt("Ball" + i.ToString(), 2);
                        listShopBall[i].GetComponent<ItemManager>().listStatus[checkAgain].SetSiblingIndex(1);
                    }
                }
                else
                {
                    listShopBall[i].transform.GetChild(checkAgain).SetAsLastSibling();
                }
            }

            for (int i = 0; i < listShopChar.Count; i++)
            {
                var checkAgain = PlayerPrefs.GetInt("Player" + i.ToString());
                if (checkAgain == 1)
                {
                    var checkCurrent = PlayerPrefs.GetInt("CurrentPlayer");
                    if (i == checkCurrent)
                    {
                        listShopChar[i].transform.GetChild(checkAgain).SetAsLastSibling();
                    }
                    else
                    {
                        PlayerPrefs.SetInt("Player" + i.ToString(), 2);
                        listShopChar[i].GetComponent<ItemManager>().listStatus[checkAgain].SetSiblingIndex(1);
                    }
                }
                else
                {
                    listShopChar[i].transform.GetChild(checkAgain).SetAsLastSibling();
                }
            }
        }
    }

    public void ExitShop()
    {
        shopMenu.SetActive(false);
    }

    public void CharTabButton()
    {
        charTab.SetAsLastSibling();
    }
    
    public void BallTabButton()
    {
        ballTab.SetAsLastSibling();
    }

    public void WinMethod()
    {
        StopAllCoroutines();
        StartCoroutine(Win());
    }

    IEnumerator Win()
    {
        if (isStartGame)
        {
            isStartGame = false;
            isControl = false;
            isHoldBall = false;
            tutorial.SetActive(false);
            ball.transform.DOKill();
            ball.tag = "Untagged";

            losePanel.SetActive(false);
            conffetiSpawn = Instantiate(conffeti);
            currentLevel++;
            if (currentLevel > maxLevel)
            {
                currentLevel = 0;
            }
            PlayerPrefs.SetInt("currentLevel", currentLevel);

            yield return new WaitForSeconds(1);
            foreach (var item in listStop)
            {
                if (item.GetComponent<Movement>().anim.GetCurrentAnimatorStateInfo(0).IsName("Running"))
                {
                    item.GetComponent<Movement>().enabled = true;
                    item.GetComponent<Movement>().StopAllCoroutines();
                    item.GetComponent<Movement>().anim.SetTrigger("Sad");
                }
            }
            yield return new WaitForSeconds(1);
            winPanel.SetActive(true);
            var nextButton = winPanel.transform.GetChild(1).gameObject;
            nextButton.SetActive(false);
            if (!isPass)
            {
                charStop.GetComponent<Movement>().anim.SetTrigger("Win");
                tempCoin += (int)progress.maxValue * 10;
            }
            var textWinCoin = winPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();
            textWinCoin.text = tempCoin.ToString();
            blast.SetActive(false);
            blast.SetActive(true);
            yield return new WaitForSeconds(1);
            foreach (var item in listCoinAnim)
            {
                item.Move();
            }
            textWinCoin.DOText(" 0 ", 1.2f, true, ScrambleMode.Numerals);
            yield return new WaitForSeconds(1);
            if (!isPass)
            {
                coin += tempCoin;
                PlayerPrefs.SetInt("Coin", coin);
                coinText.text = coin.ToString();
                coinText.transform.parent.DOScale(Vector3.one * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
            else
            {
                coin += tempCoin;
                PlayerPrefs.SetInt("Coin", coin);
                coinText.text = coin.ToString();
                coinText.transform.parent.DOScale(Vector3.one * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
            nextButton.SetActive(true);
        }
    }

    public void Lose()
    {
        if (isStartGame)
        {
            Debug.Log("Lose");
            StopAllCoroutines();
            tutorial.SetActive(false);
            isStartGame = false;
            isControl = false;
            isHoldBall = false;
            ball.transform.DOKill();
            ball.tag = "Untagged";
            foreach (var item in listStop)
            {
                if (item.GetComponent<Movement>().anim.GetCurrentAnimatorStateInfo(0).IsName("Running"))
                {
                    item.GetComponent<Movement>().enabled = true;
                    item.GetComponent<Movement>().StopAllCoroutines();
                    item.GetComponent<Movement>().anim.SetTrigger("Sad");
                }
            }
            StartCoroutine(delayLose());
        }
    }

    IEnumerator delayLose()
    {
        yield return new WaitForSeconds(1);
        losePanel.SetActive(true);
    }

    public void LoadScene()
    {
        StartCoroutine(delayLoadScene());
    }

    IEnumerator delayLoadScene()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        Camera.main.transform.DOMoveX(30, 1);
        yield return new WaitForSeconds(1);
        var temp = conffetiSpawn;
        Destroy(temp);
        SceneManager.LoadScene(0);
    }

    public void OnChangeMap()
    {
        if (levelInput != null)
        {
            int level = int.Parse(levelInput.text.ToString());
            //Debug.Log(level);
            if (level <= maxLevel)
            {
                PlayerPrefs.SetInt("currentLevel", level);
                SceneManager.LoadScene(0);
            }
        }
    }

    public void ButtonNextLevel()
    {
        title.DOKill();
        isStartGame = true;
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            currentLevel = 0;
        }
        PlayerPrefs.SetInt("currentLevel", currentLevel);
        SceneManager.LoadScene(0);
    }

    public void ChoosePlayerButton()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name);
        int check = PlayerPrefs.GetInt("Player" + index.ToString());
        if(check == 0)
        {
            var objectTarget = EventSystem.current.currentSelectedGameObject.GetComponent<ItemManager>().listStatus[0];
            string priceS = objectTarget.transform.GetChild(0).GetComponent<Text>().text;
            int price = int.Parse(priceS);
            if (coin >= price)
            {
                coin -= price;
                PlayerPrefs.SetInt("Coin", coin);
                PlayerPrefs.SetInt("Player" + index.ToString(), 2);
                objectTarget.transform.SetSiblingIndex(0);

                PlayerPrefs.SetInt("CurrentPlayer", index);
                PlayerPrefs.SetInt("Player" + index, 1);

                if (isPass)
                {
                    foreach (var item in listStop)
                    {
                        item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[index];
                    }
                }
                else
                {
                    foreach (var item in listPass)
                    {
                        item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[index];
                    }
                }
            }
        }
        if(check == 2)
        {
            PlayerPrefs.SetInt("CurrentPlayer", index);
            PlayerPrefs.SetInt("Player" + index, 1);

            if (isPass)
            {
                foreach(var item in listStop)
                {
                    item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[index];
                }
            }
            else
            {
                foreach (var item in listPass)
                {
                    item.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.mainTexture = listChars[index];
                }
            }
        }

        for (int i = 0; i < listShopChar.Count; i++)
        {
            var checkAgain = PlayerPrefs.GetInt("Player" + i.ToString());
            if (checkAgain == 1)
            {
                var checkCurrent = PlayerPrefs.GetInt("CurrentPlayer");
                if (i == checkCurrent)
                {
                    listShopChar[i].GetComponent<ItemManager>().listStatus[checkAgain].SetAsLastSibling();
                }
                else
                {
                    PlayerPrefs.SetInt("Player" + i.ToString(), 2);
                    listShopChar[i].GetComponent<ItemManager>().listStatus[checkAgain].SetSiblingIndex(1);
                }
            }
        }
    }

    public void ChooseBallButton()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name);
        int check = PlayerPrefs.GetInt("Ball" + index.ToString());
        if (check == 0)
        {
            var objectTarget = EventSystem.current.currentSelectedGameObject.transform.GetComponent<ItemManager>().listStatus[0];
            string priceS = objectTarget.transform.GetChild(0).GetComponent<Text>().text;
            int price = int.Parse(priceS);
            if (coin >= price)
            {
                coin -= price;
                PlayerPrefs.SetInt("Coin", coin);
                PlayerPrefs.SetInt("Ball" + index.ToString(), 2);
                objectTarget.transform.SetSiblingIndex(0);

                PlayerPrefs.SetInt("CurrentBall", index);
                PlayerPrefs.SetInt("Ball" + index, 1);

                Vector3 ballPos = ball.transform.position;
                ball.SetActive(false);
                var indexSkinBall = PlayerPrefs.GetInt("CurrentBall");
                ball = listBalls[indexSkinBall];
                ball.transform.position = ballPos;
                ball.SetActive(true);
                ballRigid = ball.GetComponent<Rigidbody>();
            }
        }
        if (check == 2)
        {
            PlayerPrefs.SetInt("CurrentBall", index);
            PlayerPrefs.SetInt("Ball" + index, 1);

            Vector3 ballPos = ball.transform.position;
            ball.SetActive(false);
            var indexSkinBall = PlayerPrefs.GetInt("CurrentBall");
            ball = listBalls[indexSkinBall];
            ball.transform.position = ballPos;
            ball.SetActive(true);
            ballRigid = ball.GetComponent<Rigidbody>();
        }

        for (int i = 0; i < listShopBall.Count; i++)
        {
            var checkAgain = PlayerPrefs.GetInt("Ball" + i.ToString());
            if(checkAgain == 1)
            {
                var checkCurrent = PlayerPrefs.GetInt("CurrentBall");
                if(i == checkCurrent)
                {
                    listShopBall[i].GetComponent<ItemManager>().listStatus[checkAgain].SetAsLastSibling();
                }
                else
                {
                    PlayerPrefs.SetInt("Ball" + i.ToString(), 2);
                    listShopBall[i].GetComponent<ItemManager>().listStatus[checkAgain].SetSiblingIndex(1);
                }
            }
        }
    }
}
