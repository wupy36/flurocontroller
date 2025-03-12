using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XRay : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material baseMaterial;

    public Material XRayMaterial;
    public UnityEvent onXRay;
    public UnityEvent offXRay;

    private int baselayer;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        baseMaterial = meshRenderer.material;
        baselayer = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Camera.main.transform.position - transform.position, out hit))
        {
            if (hit.collider.CompareTag("Player") == false)
            {
                gameObject.layer = 16;
                meshRenderer.material = XRayMaterial;
                onXRay.Invoke();
            }
            else
            {
                gameObject.layer = baselayer;
                meshRenderer.material = baseMaterial;
                offXRay.Invoke();
            }
        }
    }
}
