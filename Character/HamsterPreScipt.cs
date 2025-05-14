public class HamsterPreScipt : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    private Vector3 lastPosition;
    private Vector3 drinkPosition;

    public GameObject servingItem;

    public bool isWalking = false;

    public ServingState servingState; // 서빙 상태를 enum으로 변경
    public Vector3 startPosition;

    public ClickedDrinkInfo servingInfo = null; // 서빙해야하는 음료, 손님 정보
    public GameObject seletedMachine = null;

    private bool isMakingDrinkCoroutine = false;
    private bool isServingDrinkCoroutine = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        int outsideAreaMask = 1 << NavMesh.GetAreaFromName("outside");
        int walkableAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
        int hamsterAreaMask = 1 << NavMesh.GetAreaFromName("hamster");

        agent.areaMask = outsideAreaMask | walkableAreaMask | hamsterAreaMask;
    }

    private void Start()
    {
        servingState = ServingState.Idle;
        servingInfo = null;
        agent.enabled = false;

        startPosition = transform.position;
        lastPosition = transform.position;
        drinkPosition = Vector3.zero;

        animator.SetTrigger("stay");
    }

    public void CashingAnimator()
    {
        animator.SetTrigger("cashing");
    }

    public void ToutingAnimator()
    {
        animator.SetTrigger("touting");
    }

    void Update()
    {
        if (GetComponent<HamsterBehavior>().currentRole == HamsterState.Serving)
        {
            Serving();
            agent.enabled = true;
            UpdateSpriteDirection();
        }
        else
        {
            agent.enabled = false;
        }
    }

    void Serving()
    {
        void MoveTo(Vector3 target, System.Action onArrived)
        {
            agent.SetDestination(target);

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance &&
                (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
            {
                onArrived?.Invoke();
            }
        }

        if (servingState == ServingState.GoingToDrink) // 음료 가지러 간다
        {
            if (seletedMachine == null)
            {
                if (KitchenManager.Instance.beverage.GetComponent<CoffeePreScript>().isUsing)
                {
                    foreach (var machine in KitchenManager.Instance.coffeeList)
                    {
                        if (!machine.GetComponent<CoffeePreScript>().isUsing)
                        {
                            seletedMachine = machine;
                            machine.GetComponent<CoffeePreScript>().isUsing = true;
                            break;
                        }
                    }

                    if (seletedMachine == null) return;
                }
                else
                {
                    seletedMachine = KitchenManager.Instance.beverage;
                    seletedMachine.GetComponent<CoffeePreScript>().isUsing = true;
                }
            }

            drinkPosition = seletedMachine.transform.position;

            MoveTo(drinkPosition, () =>
            {
                if (!isMakingDrinkCoroutine)
                    StartCoroutine(MakeDrink(HamsterManager.Instance.DRINK_TIME));
            });
        }
        else if (servingState == ServingState.Serving) // 손님에게 전달
        {
            Vector3 customerPosition = servingInfo.customerScriptObject.transform.position;

            MoveTo(customerPosition, () =>
            {
                if (!isServingDrinkCoroutine)
                {
                    servingItem.GetComponent<SpriteRenderer>().sprite = null;
                    StartCoroutine(ServingDrink(0.2f));
                }
            });
        }
        else if (servingState == ServingState.Returning) // 서빙 끝, 원래 자리로 이동
        {
            MoveTo(startPosition, () =>
            {
                servingState = ServingState.Idle;
            });
        }
    }

    public void StopHamsterCoroutine()
    {
        StopAllCoroutines();
        isMakingDrinkCoroutine = false;
        isServingDrinkCoroutine = false;
    }

    IEnumerator MakeDrink(float seconds)
    {
        isMakingDrinkCoroutine = true;
        isWalking = false;

        yield return new WaitForSeconds(seconds);

        servingState = ServingState.Serving;
        if (seletedMachine != null)
        {
            seletedMachine.GetComponent<CoffeePreScript>().isUsing = false;
            seletedMachine = null;
        }

        animator.SetTrigger("serving");
        servingItem.GetComponent<SpriteRenderer>().sprite = servingInfo.drinkObject.GetComponent<SpriteRenderer>().sprite;

        isMakingDrinkCoroutine = false;
    }

    IEnumerator ServingDrink(float seconds)
    {
        isServingDrinkCoroutine = true;
        isWalking = false;

        yield return new WaitForSeconds(seconds);

        // 1. 손님에게 음료 전달
        var customerObj = servingInfo.customerScriptObject;

        if (customerObj.TryGetComponent(out CustomerPreScript customer))
        {
            customer.getDrink = true;
            customer.GetDrink();
        }

        if (customerObj.TryGetComponent(out SpecialCustomerPreScript specialCustomer))
        {
            specialCustomer.getDrink = true;
            specialCustomer.GetDrink();
            specialCustomer.specialStarCount++;
        }

        // 2. 보상 지급
        var drinkBehavior = servingInfo.drinkObject.GetComponent<DrinkBehavior>();
        int rewardGold = drinkBehavior.drinkPrice * drinkBehavior.drinkStar;

        CurrencyManager.Instance.IncreaseGold(rewardGold);

        // 3. 상태 전환 및 마무리
        servingState = ServingState.Returning;
        animator.SetTrigger("walk");
        servingItem.GetComponent<SpriteRenderer>().sprite = null;

        servingInfo = null;
        HamsterManager.Instance.SetServerFloor();

        isServingDrinkCoroutine = false;
    }

    public void RemoveOrder()
    {
        if (servingInfo == null) return;

        var customerObj = servingInfo.customerScriptObject;

        if (customerObj.TryGetComponent(out CustomerPreScript customer))
        {
            customer.getDrink = true;
        }

        if (customerObj.TryGetComponent(out SpecialCustomerPreScript specialCustomer))
        {
            specialCustomer.getDrink = true;
        }

        servingInfo = null;

        HamsterManager.Instance.SetServerFloor();

        isMakingDrinkCoroutine = false;
        isServingDrinkCoroutine = false;

        StopAllCoroutines();
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