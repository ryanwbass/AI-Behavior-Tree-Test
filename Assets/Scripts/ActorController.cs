using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CircleCollider2D))]
public class ActorController : MonoBehaviour
{

    private CircleCollider2D circleCollider;
    private GameObject actor;

    public float moveSpeed;

    bool[,] walkableMap;
    PathFind.Grid gameGrid;
    public PathFind.Point currentPoint;
    public PathFind.Point targetPoint;
    private float startTime;
    private float journeyLength;
    public List<PathFind.Point> path;

    public Vector2Int lastTarget;

    public List<Vector2Int> unvisitedShops;
    public int nextShop;
    public bool avoidedPedestrian;
    public bool waitingForPedToPass;

    // Use this for initialization
    void Start()
    {
        actor = gameObject;

        walkableMap = GameManager.instance.walkableMap;
        gameGrid = new PathFind.Grid(walkableMap.GetLength(0), walkableMap.GetLength(1), walkableMap);

        SetCurrentPoint(transform.position);
        targetPoint = new PathFind.Point(currentPoint.x, currentPoint.y);
        avoidedPedestrian = false;
        waitingForPedToPass = false;
        unvisitedShops = new List<Vector2Int>(GameManager.instance.shops);
        unvisitedShops.Remove(new Vector2Int(currentPoint.x, currentPoint.y));
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x >= 50)
        {
            print("Success");   
            Destroy(this.gameObject);
            GameManager.instance.actorIsAlive = false;
        }
        for(int p = 0; p < GameManager.instance.pedestrians.Count; p++)
        {
            Vector3 currentPedestrianPos = GameManager.instance.pedestrians[p].transform.position;
            float distBetweenActorAndPed = Vector3.Distance(currentPedestrianPos, transform.position);
            if (distBetweenActorAndPed < 2 && IsOutsideShops())
            {
                print("Failure");
                Destroy(this.gameObject);
                GameManager.instance.actorIsAlive = false;
            }
        }

        DecideWhatToDo();

        

    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector2(currentPoint.x, currentPoint.y), moveSpeed * Time.deltaTime);
    }

    void SetCurrentPoint(Vector2 newPos)
    {
        currentPoint = new PathFind.Point((int)newPos.x, (int)newPos.y);
    }

    void TakeNextPathStep()
    {
        currentPoint = path[0];
        path.RemoveAt(0);
    }

    private void DecideWhatToDo()
    {
        if (avoidedPedestrian)
        {
            if (FinishedPath())
            {
                Vector2Int currentPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);
                if (unvisitedShops.Contains(currentPos)) ;
                {
                    unvisitedShops.Remove(currentPos);
                }
                moveSpeed = 6;
                SetTarget(lastTarget);
                SetNewPath(targetPoint);
                avoidedPedestrian = false;
                return;
            }
            if (!FinishedPathStep())
            {
                Move();
                return;
            }
            if (FinishedPathStep() && !FinishedPath())
            {
                TakeNextPathStep();
                return;
            }
        }

        for (int p = 0; p < GameManager.instance.pedestrians.Count; p++)
        {
            Vector3 currentPedestrianPos = GameManager.instance.pedestrians[p].transform.position;
            float distBetweenActorAndPed = Vector3.Distance(currentPedestrianPos, transform.position);
            float difInYVals = Mathf.Abs(currentPedestrianPos.y - transform.position.y);
            if (distBetweenActorAndPed < 6 && !avoidedPedestrian && IsOutsideShops() && difInYVals < 2)
            {
                moveSpeed = 8;
                SetTarget(GetClosestShop());
                SetNewPath(targetPoint);
                avoidedPedestrian = true;
                return;
            }
            if (distBetweenActorAndPed < 4 && !avoidedPedestrian && IsOutsideShops() && difInYVals < 3)
            {
                moveSpeed = 8;
                Vector2Int closestShop = GetClosestShop();
                SetTarget(closestShop);
                SetNewPath(targetPoint);
                avoidedPedestrian = true;
                return;
            }
            if (distBetweenActorAndPed < 3 && IsOutsideShops() && difInYVals >= 3)
            {
                return;
            }
            if (distBetweenActorAndPed <=3 && !IsOutsideShops())
            {
                return;
            }


        }

        if (FinishedPath() && !avoidedPedestrian)
        {
            if (!VisitedAllShops())
            {
                MoveToNextShop();
                return;
            }
            else
            {
                MoveToEnd();
                return;
            }
        }
        if (FinishedPathStep() && !FinishedPath())
        {
            TakeNextPathStep();
            return;
        }

        if (!FinishedPathStep())
        {
            Move();
            return;
        }
    }

    void MoveToNextShop()
    {
        PickNextShop();
        SetNewPath(targetPoint);
    }

    void PickNextShop()
    {
        nextShop = GetRandomShop();
        SetTarget(unvisitedShops[nextShop]);
        unvisitedShops.RemoveAt(nextShop);
    }

    void MoveToEnd()
    {
        int randEndPoint = Random.Range(5, 15);
        SetTarget(new Vector2Int(50, randEndPoint));
        SetNewPath(targetPoint);
    }

    Vector2Int GetClosestShop()
    {
        Vector2Int closestShop = new Vector2Int(100, 100);
        for (int s = 0; s < GameManager.instance.shops.Count; s++)
        {
            if (Vector3.Distance(transform.position, new Vector3(closestShop.x, closestShop.y)) > Vector3.Distance(transform.position, new Vector3(GameManager.instance.shops[s].x, GameManager.instance.shops[s].y)))
            {
                closestShop = GameManager.instance.shops[s];
            }
        }
        return closestShop;
    }

    int GetRandomShop()
    {
        return Random.Range(0, unvisitedShops.Count);
    }

    void SetTarget(Vector2Int newTarget)
    {
        lastTarget = new Vector2Int(targetPoint.x, targetPoint.y);
        targetPoint.Set(newTarget.x, newTarget.y);
    }
    
    void SetNewPath(PathFind.Point target)
    {
        path = PathFind.Pathfinding.FindPath(gameGrid, currentPoint, target, true); 
    }

    bool FinishedPathStep()
    {
        return transform.position == new Vector3(currentPoint.x, currentPoint.y);
    }

    bool VisitedAllShops()
    {
        return unvisitedShops.Count <= 0;
    }

    bool FinishedPath()
    {
        return currentPoint == targetPoint;
    }

    bool IsOutsideShops()
    {
        return transform.position.y > 4 && transform.position.y < 16;
    }
}