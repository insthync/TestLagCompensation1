using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : LiteNetLibBehaviour
{
    public Text textHit;
    public Text textRtt;
    public Material matHistory;
    public Material matPresent;
    public Material matHit;
    public LineRenderer lineRenderer;
    private GameObject historyObj;
    private GameObject presentObj;
    private GameObject hitObj;

    private void Update()
    {
        if (IsClient && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (hitObj != null)
                Destroy(hitObj);
            if (Physics.Raycast(ray, out hit))
            {
                textHit.text = hit.transform.root.ToString();
                InstantiateHit(hit);
            }
            lineRenderer.SetPositions(new Vector3[]
            {
                ray.origin,
                ray.origin + ray.direction * 100f,
            });
            RPC(RpcRaycast, ray.origin, ray.direction);
        }
        textRtt.text = IsClient ? Manager.Rtt.ToString() : Manager.GetPlayer(ConnectionId).Rtt.ToString();
    }

    [ServerRpc]
    private void RpcRaycast(Vector3 origin, Vector3 direction)
    {
        if (hitObj != null)
            Destroy(hitObj);
        InstantiatePresent();
        LagCompensationManager.Instance.BeginSimlateHitBoxesByRtt(ConnectionId);
        InstantiateHistory();
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit))
        {
            textHit.text = hit.transform.root.ToString();
            InstantiateHit(hit);
        }
        lineRenderer.SetPositions(new Vector3[]
        {
            origin,
            origin + direction * 100f,
        });
        LagCompensationManager.Instance.EndSimulateHitBoxes();
    }

    void InstantiatePresent()
    {
        if (presentObj != null)
            Destroy(presentObj);
        var obj = Instantiate(MovingObject.Instance.transform);
        obj.position = MovingObject.Instance.transform.position + Vector3.back * 0.1f;
        obj.rotation = MovingObject.Instance.transform.rotation;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = matPresent;
        }
        obj.GetComponent<MovingObject>().enabled = false;
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        obj.name += "(Present)";
        presentObj = obj.gameObject;
    }

    void InstantiateHistory()
    {
        if (historyObj != null)
            Destroy(historyObj);
        var obj = Instantiate(MovingObject.Instance.transform);
        obj.position = MovingObject.Instance.transform.position + Vector3.back * 0.2f;
        obj.rotation = MovingObject.Instance.transform.rotation;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = matHistory;
        }
        obj.GetComponent<MovingObject>().enabled = false;
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        obj.name += "(Rewinded)";
        historyObj = obj.gameObject;
    }

    void InstantiateHit(RaycastHit hit)
    {
        if (hitObj != null)
            Destroy(hitObj);
        var obj = Instantiate(hit.transform);
        obj.position = hit.transform.position + Vector3.forward * 0.1f;
        obj.rotation = hit.transform.rotation;
        var renderer = obj.GetComponent<Renderer>();
        renderer.material = matHit;
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        obj.name += "(Hit)";
        hitObj = obj.gameObject;
    }
}
