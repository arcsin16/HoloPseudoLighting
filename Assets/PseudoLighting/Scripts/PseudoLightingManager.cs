using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

public class PseudoLightingManager : Singleton<PseudoLightingManager>{

    public GameObject prefab;

    public Material material;

    private int count = 0;
    private Vector4[] posArray;
    private Color[] colorArray;

    void Start () {
        InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;

        posArray = new Vector4[32];
        colorArray = new Color[32];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnLightSource();
        }
    }

    private void LateUpdate()
    {
        //光源のバッファの設定が終わってから、Shaderに光源位置、色を設定する。
        material.SetInt("_Count", count);
        material.SetVectorArray("_PositionArray", posArray);
        material.SetColorArray("_ColorArray", colorArray);

        count = 0;
    }

    private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
    {
        SpawnLightSource();
    }

    /// <summary>
    /// バッファに光源の位置と色を設定する。
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="color"></param>
    public void SetLightPosition(Vector3 pos, Color color)
    {
        if (count < posArray.Length-1)
        {
            posArray[count] = new Vector4(pos.x, pos.y, pos.z, 0.5f);
            colorArray[count] = color;
            count++;
        }
    }

    /// <summary>
    /// 光源を生成する
    /// </summary>
    private void SpawnLightSource()
    {
        var trans = Camera.main.transform;
        var obj = Instantiate(prefab, trans.position + trans.forward, Quaternion.identity);

        Destroy(obj, 8.0f);
    }

}
