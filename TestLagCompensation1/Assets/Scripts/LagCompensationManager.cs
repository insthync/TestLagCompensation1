using LiteNetLibManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagCompensationManager : MonoBehaviour
{
    private readonly Dictionary<uint, HitBox[]> HitBoxes = new Dictionary<uint, HitBox[]>();
    public float snapShotInterval = 0.06f;
    public int maxHistorySize = 16;
    public int MaxHistorySize { get { return maxHistorySize; } }
    public static LagCompensationManager Instance { get; private set; }
    private readonly List<HitBox> hitBoxes = new List<HitBox>();
    private float snapShotCountDown = 0f;

    private LiteNetLibGameManager manager;

    private void Awake()
    {
        Instance = this;
        manager = FindObjectOfType<LiteNetLibGameManager>();
    }

    public bool AddHitBoxes(uint objectId, HitBox[] hitBoxes)
    {
        if (HitBoxes.ContainsKey(objectId))
            return false;
        HitBoxes.Add(objectId, hitBoxes);
        return true;
    }

    public bool RemoveHitBoxes(uint objectId)
    {
        return HitBoxes.Remove(objectId);
    }

    public bool SimulateHitBoxes(long connectionId, long targetTime, Action action)
    {
        if (action == null || !BeginSimlateHitBoxes(connectionId, targetTime))
            return false;
        action.Invoke();
        EndSimulateHitBoxes();
        return true;
    }

    public bool SimulateHitBoxesByRtt(long connectionId, Action action)
    {
        if (action == null || !BeginSimlateHitBoxesByRtt(connectionId))
            return false;
        action.Invoke();
        EndSimulateHitBoxes();
        return true;
    }

    public bool BeginSimlateHitBoxes(long connectionId, long targetTime)
    {
        if (!manager.IsServer || !manager.ContainsPlayer(connectionId))
            return false;
        LiteNetLibPlayer player = manager.GetPlayer(connectionId);
        return InternalBeginSimlateHitBoxes(player, targetTime);
    }

    public bool BeginSimlateHitBoxesByRtt(long connectionId)
    {
        if (!manager.IsServer || !manager.ContainsPlayer(connectionId))
            return false;
        LiteNetLibPlayer player = manager.GetPlayer(connectionId);
        long targetTime = manager.ServerTimestamp - player.Rtt;
        return InternalBeginSimlateHitBoxes(player, targetTime);
    }

    private bool InternalBeginSimlateHitBoxes(LiteNetLibPlayer player, long targetTime)
    {
        hitBoxes.Clear();
        foreach (uint subscribingObjectId in player.GetSubscribingObjectIds())
        {
            if (HitBoxes.ContainsKey(subscribingObjectId))
                hitBoxes.AddRange(HitBoxes[subscribingObjectId]);
        }
        long time = manager.ServerTimestamp;
        for (int i = 0; i < hitBoxes.Count; ++i)
        {
            if (hitBoxes[i] != null)
                hitBoxes[i].Rewind(time, targetTime);
        }
        return true;
    }

    public void EndSimulateHitBoxes()
    {
        for (int i = 0; i < hitBoxes.Count; ++i)
        {
            if (hitBoxes[i] != null)
                hitBoxes[i].Restore();
        }
    }

    private void FixedUpdate()
    {
        if (!manager.IsServer)
            return;
        snapShotCountDown -= Time.fixedDeltaTime;
        if (snapShotCountDown > 0)
            return;
        snapShotCountDown = snapShotInterval;
        long time = manager.ServerTimestamp;
        foreach (HitBox[] hitBoxesArray in HitBoxes.Values)
        {
            foreach (HitBox hitBox in hitBoxesArray)
            {
                hitBox.AddTransformHistory(time);
            }
        }
    }
}
