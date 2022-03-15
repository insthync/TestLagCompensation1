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

    void Start()
    {
        hitBoxes = GetComponentsInChildren<HitBox>();
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            hitBoxes[i].Setup(i);
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;
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
