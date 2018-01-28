using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoLightBehaviour : MonoBehaviour {
    private Color color;
    private Rigidbody rigid;
    private ParticleSystem[] particles;

    void Start () {
        //ランダムに色を選択
        color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);

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
        particles = GetComponentsInChildren<ParticleSystem>();
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

        //光源に初速（移動、回転）を設定
        rigid = GetComponent<Rigidbody>();
        rigid.AddForce(Camera.main.transform.forward, ForceMode.VelocityChange);
        rigid.angularVelocity = Random.onUnitSphere.normalized;
    }

    void Update () {
        //光源の位置を更新
        PseudoLightingManager.Instance.SetLightPosition(transform.position, color);
    }
}
