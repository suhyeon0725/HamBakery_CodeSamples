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
        // ���¿� ���� �ൿ ó��
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
        // ���� �� �̵�
        float speed = CustomerManager.WalkSpeed * Time.deltaTime;
        Vector3 dir = transform.position.x < 0 ? Vector3.right : Vector3.left;
        transform.Translate(dir * speed);

        if (transform.position.x < CustomerManager.ExitXMin || transform.position.x > CustomerManager.ExitXMax)
            Destroy(gameObject);
    }

    public void ClickEnter()
    {
        // Ŭ�� �� ���� ó��
        if (customerState != CustomerState.Walk) return;
        customerState = CustomerState.Enter;
        animator.SetTrigger("walk");
    }

    void Enter()
    {
        // ���� �������� �̵�
        MoveTo(CustomerManager.Instance.EnterPoint.position, () =>
        {
            if (CustomerManager.Instance.waitCustomers.Count >= CustomerManager.Instance.waitPoint.Count)
                ExitAngry(); // ��⿭�� ���� �� ��� ����
            else
            {
                customerState = CustomerState.Wait;
                CustomerManager.Instance.waitCustomers.Add(gameObject);
            }
        });
    }

    void Wait()
    {
        // ��⿭���� �̵�
        int index = CustomerManager.Instance.waitCustomers.IndexOf(gameObject);
        Vector3 target = CustomerManager.Instance.waitPoint[index].position;
        MoveTo(target, () =>
        {
            animator.SetTrigger("stay");

            // ���� ���̸� �����̽��� �̵�
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
        // �����̽��� �̵��Ͽ� �� ����
        int index = CustomerManager.Instance.selectCustomers.IndexOf(gameObject);
        Vector3 target = CustomerManager.Instance.selectPoint[index].position;

        MoveTo(target, () =>
        {
            if (index == 0)
            {
                selectedBakery = ShowcaseManager.Instance.SelectShowcaseBakery();
                if (selectedBakery == null) ExitAngry(); // ���� ���� �� ����
                else customerState = CustomerState.Order;
            }
        });
    }

    void Order()
    {
        // �ֹ� �������� �̵��Ͽ� �ֹ� ó��
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
                CompleteOrder(); // �ֹ� �Ϸ� ó��
            }
        });
    }

    void CompleteOrder()
    {
        // �ֹ� ó�� �� ����ġ/��� ����
        var bakery = selectedBakery.GetComponent<BakeryBehavior>();
        CurrencyManager.Instance.IncreaseGold(bakery.bakerySellingPrice);
        BakeryManager.Instance.IncreaseBakeryCount(bakery);
        LevelManager.Instance.IncreasePlayerExp(1);

        // �¼� ���ο� ���� ���� �Ǵ� ����
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
        // ���̺�� �̵� �� ���� �� �Ա� ����
        MoveTo(seatPosition.position, () =>
        {
            agent.enabled = false;
            selectedTable.transform.Find("FoodPosition").GetComponent<SpriteRenderer>().sprite =
                selectedBakery.GetComponent<SpriteRenderer>().sprite;

            animator.SetTrigger("eat");
            StartCoroutine(LeaveAfterSeconds(CustomerManager.Instance.DRINK_TIME)); // ���� �� ����
        });
    }

    void TakeOut()
    {
        // ���� �� ����
        agent.enabled = true;
        customerState = CustomerState.Out;
    }

    void Exit()
    {
        // ���� ������ �̵�
        MoveTo(CustomerManager.Instance.ExitPoint.position, () =>
        {
            customerState = CustomerState.Walk;
            agent.enabled = false;
        });
    }

    void ExitAngry()
    {
        // ȭ�� ���·� ����
        gameObject.transform.Find("angry").gameObject.SetActive(true);
        gameObject.transform.Find("face").gameObject.SetActive(false);
        CustomerManager.Instance.waitCustomers.Remove(gameObject);
        animator.SetTrigger("walk");
        customerState = CustomerState.Out;
        agent.enabled = true;
    }

    IEnumerator LeaveAfterSeconds(float seconds)
    {
        // �ð� �� ���̺� ���� ����
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
        // ������ ���� �� �ݹ� ����
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
        if (moveDirection.x > 0.001f)  // ���������� �̵�
        {
            transform.localScale = new Vector3(-0.9f, 0.9f, 1);
        }
        else if (moveDirection.x < -0.001f)  // �������� �̵�
        {
            transform.localScale = new Vector3(0.9f, 0.9f, 1);
        }
        lastPosition = transform.position;  // ���� ��ġ�� ������ ��ġ�� ������Ʈ
    }
}