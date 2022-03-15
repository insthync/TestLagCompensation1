using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : LiteNetLibBehaviour
{
    public Text textHit;
    public Material matHistory;
    public Material matHit;
    private GameObject historyObj;
    private GameObject hitObj;

    void Update()
    {
        if (IsClient && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                textHit.text = hit.transform.ToString();
                InstantiateHit(hit);
            }
            RPC(RpcRaycast, ray.origin, ray.direction);
        }
    }
    
    [ServerRpc]
    private void RpcRaycast(Vector3 origin, Vector3 direction)
    {
        LagCompensationManager.Instance.BeginSimlateHitBoxesByRtt(ConnectionId);
        InstantiateHistory();
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit))
        {
            textHit.text = hit.transform.ToString();
            InstantiateHit(hit);
        }
        LagCompensationManager.Instance.EndSimulateHitBoxes();
    }

    void InstantiateHistory()
    {
        if (historyObj != null)
            Destroy(historyObj);
        var obj = Instantiate(MovingObject.Instance.transform, MovingObject.Instance.transform.position, MovingObject.Instance.transform.rotation);
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
        historyObj = obj.gameObject;
    }

    void InstantiateHit(RaycastHit hit)
    {
        if (hitObj != null)
            Destroy(hitObj);
        var obj = Instantiate(hit.transform.gameObject, hit.transform.position, hit.transform.rotation);
        var renderer = obj.GetComponent<Renderer>();
        renderer.material = matHit;
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        hitObj = obj.gameObject;
    }
}
