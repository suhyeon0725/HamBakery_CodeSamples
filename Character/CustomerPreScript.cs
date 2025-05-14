public class CustomerPreScript : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    public CustomerState customerState;
    public GameObject selectedBakery;
    public GameObject selectedTable;
    Transform seatPosition;

    private bool isOrdering;
    private float orderElapsed;

    private Vector3 lastPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        int outsideAreaMask = 1 << NavMesh.GetAreaFromName("outside");
        int walkableAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");

        agent.areaMask = outsideAreaMask | walkableAreaMask;
    }

    void Start()
    {
        agent.enabled = false;
        animator.SetTrigger("walk");
    }

    void Update()
    {
        // 상태에 따라 행동 처리
        switch (customerState)
        {
            case CustomerState.Walk: Walk(); break;
            case CustomerState.Enter: Enter(); break;
            case CustomerState.Wait: Wait(); break;
            case CustomerState.Select: Select(); break;
            case CustomerState.Order: Order(); break;
            case CustomerState.Seat: Seat(); break;
            case CustomerState.TakeOut: TakeOut(); break;
            case CustomerState.Out: Exit(); break;
        }

        UpdateSpriteDirection();
    }

    void Walk()
    {
        // 입장 전 이동
        float speed = CustomerManager.WalkSpeed * Time.deltaTime;
        Vector3 dir = transform.position.x < 0 ? Vector3.right : Vector3.left;
        transform.Translate(dir * speed);

        if (transform.position.x < CustomerManager.ExitXMin || transform.position.x > CustomerManager.ExitXMax)
            Destroy(gameObject);
    }

    public void ClickEnter()
    {
        // 클릭 시 입장 처리
        if (customerState != CustomerState.Walk) return;
        customerState = CustomerState.Enter;
        animator.SetTrigger("walk");
    }

    void Enter()
    {
        // 입장 지점으로 이동
        MoveTo(CustomerManager.Instance.EnterPoint.position, () =>
        {
            if (CustomerManager.Instance.waitCustomers.Count >= CustomerManager.Instance.waitPoint.Count)
                ExitAngry(); // 대기열이 가득 찬 경우 퇴장
            else
            {
                customerState = CustomerState.Wait;
                CustomerManager.Instance.waitCustomers.Add(gameObject);
            }
        });
    }

    void Wait()
    {
        // 대기열에서 이동
        int index = CustomerManager.Instance.waitCustomers.IndexOf(gameObject);
        Vector3 target = CustomerManager.Instance.waitPoint[index].position;
        MoveTo(target, () =>
        {
            animator.SetTrigger("stay");

            // 가장 앞이면 쇼케이스로 이동
            if (index == 0 && ShowcaseManager.Instance.HasBakery())
            {
                customerState = CustomerState.Select;
                CustomerManager.Instance.selectCustomers.Add(gameObject);
                CustomerManager.Instance.waitCustomers.Remove(gameObject);
                animator.SetTrigger("select");
            }
        });
    }

    void Select()
    {
        // 쇼케이스로 이동하여 빵 선택
        int index = CustomerManager.Instance.selectCustomers.IndexOf(gameObject);
        Vector3 target = CustomerManager.Instance.selectPoint[index].position;

        MoveTo(target, () =>
        {
            if (index == 0)
            {
                selectedBakery = ShowcaseManager.Instance.SelectShowcaseBakery();
                if (selectedBakery == null) ExitAngry(); // 선택 실패 시 퇴장
                else customerState = CustomerState.Order;
            }
        });
    }

    void Order()
    {
        // 주문 지점으로 이동하여 주문 처리
        MoveTo(CustomerManager.Instance.OrderPoint.position, () =>
        {
            if (!isOrdering)
            {
                animator.SetTrigger("stay");
                isOrdering = true;
                orderElapsed = 0;
            }

            float total = HamsterManager.Instance.cashingSpeed;
            orderElapsed += Time.deltaTime;
            HamsterManager.Instance.cashingSlider.fillAmount = orderElapsed / total;

            if (orderElapsed >= total)
            {
                HamsterManager.Instance.cashingSlider.gameObject.SetActive(false);
                CompleteOrder(); // 주문 완료 처리
            }
        });
    }

    void CompleteOrder()
    {
        // 주문 처리 및 경험치/골드 지급
        var bakery = selectedBakery.GetComponent<BakeryBehavior>();
        CurrencyManager.Instance.IncreaseGold(bakery.bakerySellingPrice);
        BakeryManager.Instance.IncreaseBakeryCount(bakery);
        LevelManager.Instance.IncreasePlayerExp(1);

        // 좌석 여부에 따라 착석 또는 포장
        selectedTable = InteriorManager.Instance.GetAvailableTable();
        if (selectedTable != null)
        {
            seatPosition = selectedTable.transform.Find("SeatPosition");
            selectedTable.GetComponent<TableBehavior>().isFull = true;
            customerState = CustomerState.Seat;
        }
        else
        {
            customerState = CustomerState.TakeOut;
            animator.SetTrigger("takeout");
        }

        CustomerManager.Instance.selectCustomers.Remove(gameObject);
        agent.enabled = true;
    }

    void Seat()
    {
        // 테이블로 이동 후 착석 및 먹기 시작
        MoveTo(seatPosition.position, () =>
        {
            agent.enabled = false;
            selectedTable.transform.Find("FoodPosition").GetComponent<SpriteRenderer>().sprite =
                selectedBakery.GetComponent<SpriteRenderer>().sprite;

            animator.SetTrigger("eat");
            StartCoroutine(LeaveAfterSeconds(CustomerManager.Instance.DRINK_TIME)); // 먹은 뒤 퇴장
        });
    }

    void TakeOut()
    {
        // 포장 후 퇴장
        agent.enabled = true;
        customerState = CustomerState.Out;
    }

    void Exit()
    {
        // 매장 밖으로 이동
        MoveTo(CustomerManager.Instance.ExitPoint.position, () =>
        {
            customerState = CustomerState.Walk;
            agent.enabled = false;
        });
    }

    void ExitAngry()
    {
        // 화난 상태로 퇴장
        gameObject.transform.Find("angry").gameObject.SetActive(true);
        gameObject.transform.Find("face").gameObject.SetActive(false);
        CustomerManager.Instance.waitCustomers.Remove(gameObject);
        animator.SetTrigger("walk");
        customerState = CustomerState.Out;
        agent.enabled = true;
    }

    IEnumerator LeaveAfterSeconds(float seconds)
    {
        // 시간 후 테이블 비우고 퇴장
        yield return new WaitForSeconds(seconds);

        if (selectedTable != null)
        {
            selectedTable.GetComponent<TableBehavior>().isFull = false;
            selectedTable.transform.Find("FoodPosition").GetComponent<SpriteRenderer>().sprite = null;
        }

        agent.enabled = true;
        animator.SetTrigger("walk");
        customerState = CustomerState.Out;
    }

    void MoveTo(Vector3 target, System.Action onArrived)
    {
        // 목적지 도달 시 콜백 실행
        if (!agent.enabled) agent.enabled = true;

        agent.SetDestination(target);
        animator.SetTrigger("walk");

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
        {
            onArrived?.Invoke();
        }
    }

    void UpdateSpriteDirection()
    {
        Vector3 moveDirection = transform.position - lastPosition;
        if (moveDirection.x > 0.001f)  // 오른쪽으로 이동
        {
            transform.localScale = new Vector3(-0.9f, 0.9f, 1);
        }
        else if (moveDirection.x < -0.001f)  // 왼쪽으로 이동
        {
            transform.localScale = new Vector3(0.9f, 0.9f, 1);
        }
        lastPosition = transform.position;  // 현재 위치를 마지막 위치로 업데이트
    }
}