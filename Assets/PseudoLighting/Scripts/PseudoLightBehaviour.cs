using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Unity.SharingWithUNET;

public class PseudoLightBehaviour : NetworkBehaviour
{
    //サーバが設定した初期値をクライアント側に反映するため[SyncVar]とし、
    //オブジェクトが各クライアントで生成されたタイミングで参照する。
    [SyncVar]
    public Color color;
    [SyncVar]
    public Vector3 localVelocity;
    [SyncVar]
    public Vector3 localAngularVelocity;

    private Rigidbody rb;

    void Start () {
        Debug.Log("Start");
        if (SharedCollection.Instance == null)
        {
            Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
            Destroy(this);
            return;
        }

        //サーバがオブジェクト生成時にlocalPositionを初期位置に設定しているので
        //localPositionを維持したまま、Parentを設定する。
        transform.SetParent(SharedCollection.Instance.transform, false);

        //速度の反映
        rb = GetComponentInChildren<Rigidbody>();
        rb.velocity = transform.parent.TransformDirection(localVelocity);
        rb.angularVelocity = transform.parent.TransformVector(localAngularVelocity);

        //光源（Cube）のマテリアルに色を反映
        var rs = GetComponentsInChildren<Renderer>();
        if (rs != null)
        {
            foreach (var r in rs)
            {
                r.material.color = color;
            }
        }

        //光源（Particle）のマテリアルに色を反映
        var particles = GetComponentsInChildren<ParticleSystem>();
        if (particles != null)
        {
            foreach (var p in particles)
            {
                ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient();
                gradient.color = color;
                gradient.mode = ParticleSystemGradientMode.Color;
                var main = p.main;
                main.startColor = gradient;
            }
        }
    }

    void Update () {
        //光源の位置を更新
        PseudoLightingManager.Instance.SetLightPosition(transform.position, color);
    }
}
