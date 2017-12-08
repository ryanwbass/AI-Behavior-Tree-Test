    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CircleCollider2D))]
public class PedestrianController : MonoBehaviour {

    private CircleCollider2D circleCollider;
    private GameObject pedestrian;

    public float moveSpeed;

    public enum Action { MoveToEnd, MoveFoward, MoveBack, SwitchLanes, ShiftInLane, Wait, InreaseMoveSpeed, DecreaseMoveSpeed, NUMOFACTIONS};
    public Action currentAction;
    public Vector2 targetPos;
    List<Action> actions;
    bool waiting;
    int timeToWait;
    float timeWaited;
    int lane;
    int posInLane;

    public void Init(int _lane, int _posInLane)
    {
        pedestrian = gameObject;

        actions = new List<Action>();
        targetPos = (Vector2)transform.position;
        waiting = false;
        timeWaited = 0;
        lane = _lane;
        posInLane = _posInLane;
        GeneratePattern();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x >= 50)
        {
            GameManager.instance.pedestrians.Remove(this.gameObject);
            Destroy(this.gameObject);
        }

        if((Vector2)transform.position == targetPos && !waiting)
        {
            ProcessNextAction();
        }
        else if((Vector2)transform.position != targetPos && !waiting)
        {
            Move();
        }
        else if (waiting)
        {
            timeWaited = timeWaited + Time.deltaTime;

            if(timeWaited > timeToWait)
            {
                waiting = false;
            }
        }
    }

    void ProcessNextAction()
    {
        if(actions.Count == 0)
        {
            actions.Add(Action.MoveToEnd);
        }
        currentAction = actions[0];
        actions.RemoveAt(0);
        switch (currentAction)
        {
            case Action.MoveToEnd:
                targetPos = new Vector2(50, transform.position.y);
                return;
            case Action.MoveFoward:
                targetPos = (Vector2)transform.position + Vector2.right * Random.Range(3, 5);
                return;
            case Action.MoveBack:
                targetPos = (Vector2)transform.position + Vector2.left * Random.Range(3, 5);
                return;
            case Action.Wait:
                waiting = true;
                timeToWait = Random.Range(1, 5);
                timeWaited = 0;
                return;
            case Action.InreaseMoveSpeed:
                moveSpeed = moveSpeed + Random.Range(1, 4);
                if (moveSpeed > 10)
                {
                    moveSpeed = 10;
                }
                return;
            case Action.DecreaseMoveSpeed:
                moveSpeed = moveSpeed - Random.Range(1, 4);
                if(moveSpeed <= 2)
                {
                    moveSpeed = 3;
                }
                return;
            case Action.ShiftInLane:
                float shiftAmount = Random.Range(-1.0f, 1.0f);
                targetPos = (Vector2)transform.position + Vector2.up * shiftAmount;
                return;
            case Action.SwitchLanes:
                int upOrDown = Random.Range(0, 1);

                if(upOrDown == 0)
                {
                    if(posInLane == 0)
                    {
                        targetPos = (Vector2)transform.position + Vector2.down * 2;
                    }
                    else
                    {
                        targetPos = (Vector2)transform.position + Vector2.down * 3;
                    }
                }
                else
                {
                    if (posInLane == 0)
                    {
                        targetPos = (Vector2)transform.position + Vector2.up * 3;
                    }
                    else
                    {
                        targetPos = (Vector2)transform.position + Vector2.up * 2;
                    }
                }
                return;
        }
        
    }

    void Move()
    {
        if(targetPos.y >= 5 && targetPos.y <= 15 && targetPos.x >= 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            targetPos = transform.position;
        }
        
    }

    void GeneratePattern()
    {
        actions.Add(Action.MoveFoward);
        int patternLength = Random.Range(4, 10);
        while(actions.Count < patternLength)
        {
            Action newAction = (Action)Random.Range(1, (int)Action.NUMOFACTIONS);
            actions.Add(newAction);
        }
        actions.Add(Action.MoveToEnd);
    }
}
