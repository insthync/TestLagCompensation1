using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : LiteNetLibBehaviour
{
    public float moveSpeed = 5;
    public float bounds = 3f;
    private HitBox[] hitBoxes;
    private bool moveLeft;

    public static MovingObject Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            return;
        Instance = this;
    }

    private void Start()
    {
        hitBoxes = GetComponentsInChildren<HitBox>();
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            hitBoxes[i].Setup(i);
        }
        LagCompensationManager.Instance.AddHitBoxes(ObjectId, hitBoxes);
    }

    private void Update()
    {
        if (moveLeft)
        {
            transform.position += Vector3.left * Time.deltaTime * moveSpeed;
            if (transform.position.x < -bounds)
                moveLeft = false;
        }
        else
        {
            transform.position += Vector3.right * Time.deltaTime * moveSpeed;
            if (transform.position.x > bounds)
                moveLeft = true;
        }
        if (IsServer)
            RPC(RpcUpdatePosition, transform.position);
    }

    [AllRpc]
    private void RpcUpdatePosition(Vector3 position)
    {
        if (IsServer)
            return;
        transform.position = position;
    }
}
