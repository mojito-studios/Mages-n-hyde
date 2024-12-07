using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class DustEffect : NetworkBehaviour
{


    [SerializeField] private GameObject dustEffect;

    void Start()
    {

    }
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        Transform transform = GetComponent<Transform>();
        if ((transform.rotation.eulerAngles.z <0)|| (transform.rotation.eulerAngles.z > 180))
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = -currentScale.x;
            transform.localScale = currentScale;
        }
        spriteRenderer = dustEffect.GetComponent<SpriteRenderer>();
    }

    void Update()
    {

        float alpha = spriteRenderer.color.a;

        if (alpha <= 0.1)
        {
            Destroy(gameObject);
        }

    }

}
