using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitBox : MonoBehaviour
{
    [System.Serializable]
    public struct TransformHistory
    {
        public long Time { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    [SerializeField]
    protected float damageRate = 1f;

    public Collider CacheCollider { get; private set; }
    public Rigidbody CacheRigidbody { get; private set; }
    public Collider2D CacheCollider2D { get; private set; }
    public Rigidbody2D CacheRigidbody2D { get; private set; }
    public int Index { get; private set; }

    protected bool isSetup;
    protected Vector3 defaultLocalPosition;
    protected Quaternion defaultLocalRotation;
    protected List<TransformHistory> histories = new List<TransformHistory>();

#if UNITY_EDITOR
    [Header("Rewind Debugging")]
    public Color debugHistoryColor = new Color(0, 1, 0, 0.25f);
    public Color debugRewindColor = new Color(0, 0, 1, 0.5f);
    private Vector3? debugRewindPosition;
    private Quaternion? debugRewindRotation;
    private Vector3? debugRewindCenter;
    private Vector3? debugRewindSize;
#endif

    private void Awake()
    {
        CacheCollider = GetComponent<Collider>();
        if (CacheCollider)
        {
            CacheRigidbody = gameObject.GetComponent<Rigidbody>();
            CacheRigidbody.useGravity = false;
            CacheRigidbody.isKinematic = true;
#if UNITY_EDITOR
            debugRewindCenter = CacheCollider.bounds.center - transform.position;
            debugRewindSize = CacheCollider.bounds.size;
#endif
            return;
        }
        CacheCollider2D = GetComponent<Collider2D>();
        if (CacheCollider2D)
        {
            CacheRigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            CacheRigidbody2D.gravityScale = 0;
            CacheRigidbody2D.isKinematic = true;
#if UNITY_EDITOR
            debugRewindCenter = CacheCollider2D.bounds.center - transform.position;
            debugRewindSize = CacheCollider2D.bounds.size;
#endif
        }
    }

    public virtual void Setup(int index)
    {
        isSetup = true;
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
        Index = index;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (debugRewindCenter.HasValue &&
            debugRewindSize.HasValue)
        {
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            foreach (TransformHistory history in histories)
            {
                Matrix4x4 transformMatrix = Matrix4x4.TRS(history.Position + debugRewindCenter.Value, history.Rotation, debugRewindSize.Value);
                Gizmos.color = debugHistoryColor;
                Gizmos.matrix = transformMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            if (debugRewindPosition.HasValue &&
                debugRewindRotation.HasValue)
            {
                Matrix4x4 transformMatrix = Matrix4x4.TRS(debugRewindPosition.Value + debugRewindCenter.Value, debugRewindRotation.Value, debugRewindSize.Value);
                Gizmos.color = debugRewindColor;
                Gizmos.matrix = transformMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
            Gizmos.matrix = oldGizmosMatrix;
        }
        Handles.Label(transform.position, name + "(HitBox)");
    }
#endif

    internal void Rewind(long currentTime, long rewindTime)
    {
        TransformHistory beforeRewind = default;
        TransformHistory afterRewind = default;
        for (int i = 0; i < histories.Count; ++i)
        {
            if (beforeRewind.Time > 0 && beforeRewind.Time <= rewindTime && histories[i].Time >= rewindTime)
            {
                afterRewind = histories[i];
                break;
            }
            else
            {
                beforeRewind = histories[i];
            }
            if (histories.Count - 1 == i)
            {
                afterRewind = new TransformHistory()
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Time = currentTime,
                };
            }
        }
        long durationToRewindTime = rewindTime - beforeRewind.Time;
        long durationBetweenRewindTime = afterRewind.Time - beforeRewind.Time;
        float lerpProgress = (float)durationToRewindTime / (float)durationBetweenRewindTime;
        transform.position = Vector3.Lerp(beforeRewind.Position, afterRewind.Position, lerpProgress);
        transform.rotation = Quaternion.Slerp(beforeRewind.Rotation, afterRewind.Rotation, lerpProgress);
#if UNITY_EDITOR
        debugRewindPosition = transform.position;
        debugRewindRotation = transform.rotation;
#endif
    }

    internal void Restore()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = defaultLocalRotation;
    }

    public void AddTransformHistory(long time)
    {
        if (histories.Count == LagCompensationManager.Instance.MaxHistorySize)
            histories.RemoveAt(0);
        histories.Add(new TransformHistory()
        {
            Time = time,
            Position = transform.position,
            Rotation = transform.rotation,
        });
    }
}
