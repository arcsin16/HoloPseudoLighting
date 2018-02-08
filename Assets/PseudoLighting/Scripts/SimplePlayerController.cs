using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SharingWithUNET;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(sendInterval = 0.033f)]
public class SimplePlayerController : NetworkBehaviour,IInputClickHandler {

    public GameObject bullet;

    /// <summary>
    /// The transform of the shared world anchor.
    /// </summary>
    private Transform sharedWorldAnchorTransform;

    private NetworkDiscoveryWithAnchors networkDiscovery;

    [SyncVar]
    private Vector3 localPosition;

    [SyncVar]
    private Quaternion localRotation;

    void Awake()
    {
        networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
    }

    // Use this for initialization
    void Start()
    {
        if (SharedCollection.Instance == null)
        {
            Debug.LogError("This script required a SharedCollection script attached to a gameobject in the scene");
            Destroy(this);
            return;
        }

        if (isLocalPlayer)
        {
            //イベントリスナーの登録
            InputManager.Instance.AddGlobalListener(gameObject);
        }

        //位置合わせの基準となるアンカーのtransformを親に設定する
        sharedWorldAnchorTransform = SharedCollection.Instance.gameObject.transform;
        transform.SetParent(sharedWorldAnchorTransform);
    }

    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            //イベントリスナーの削除
            InputManager.Instance.RemoveGlobalListener(gameObject);
        }
    }

    void Update()
    {
        //他プレイヤーの位置反映
        if (!isLocalPlayer)
        {
            //CmdTransformで通知された他のプレイヤーのlocalPosition, localRotationを反映する。
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, 0.3f);
            transform.localRotation = localRotation;
            return;
        }

        // 自プレイヤーの位置更新
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;

        localPosition = transform.localPosition;
        localRotation = transform.localRotation;

        // サーバへ位置更新のコマンドを送信する
        CmdTransform(localPosition, localRotation);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (isLocalPlayer)
        {
            //AirTapで弾を発射する
            CmdFire();
        }
    }

    //[Command]属性が付き、Cmdから始まるメソッドはサーバで実行される処理
    //クライアントがこれらのメソッドを呼び出すと、クライアント側では何も処理されず
    //サーバ側で処理が呼び出される。

    //姿勢更新はマイフレーム発生するので、
    //channel=1(QosType.UnreliableFragmented)として速度優先する。
    [Command(channel = 1)]
    public void CmdTransform(Vector3 postion, Quaternion rotation)
    {
        //クライアント -> サーバにクライアントのローカルプレイヤーの
        //位置と姿勢（position, rotation)がコマンドの引数として通知される。

        //サーバは[SyncVar]属性付きのlocalPosition、localRotationに値を保存
        //することで、サーバ -> 全クライアントに反映する。
        localPosition = postion;
        localRotation = rotation;
    }

    /// <summary>
    /// Called on the host when a bullet needs to be added. 
    /// This will 'spawn' the bullet on all clients, including the 
    /// client on the host.
    /// </summary>
    [Command]
    void CmdFire()
    {
        Vector3 bulletDir = transform.forward;
        Vector3 bulletPos = transform.position + bulletDir * 1.5f;
        var color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);

        var localPos = sharedWorldAnchorTransform.InverseTransformPoint(bulletPos);
        var localDir = sharedWorldAnchorTransform.InverseTransformVector(bulletDir);

        //サーバー側からアンカーを起点とした相対座標で位置、姿勢を共有する
        //クライアント側ではオブジェクトが生成された際に、相対座標を元に実際の位置、姿勢を反映する
        //→PseudoLightBehaviour::Startを参照
        GameObject nextBullet = (GameObject)Instantiate(bullet, localPos, Quaternion.Euler(localDir));

        //速度、色などはNetworkServer.Spawnしてもクライアント側には反映されないので
        //[SyncVar]なフィールドを定義して共有しておいて、PseudoLightBehaviour::Startで反映する。
        var behaviour = nextBullet.GetComponentInChildren<PseudoLightBehaviour>();
        behaviour.color = color;
        behaviour.localVelocity = localDir * 1.0f;
        behaviour.localAngularVelocity = Random.onUnitSphere;

        //ホストクライアントを含む全クライアントにオブジェクトを生成する。
        //※ここで生成するオブジェクトは、UNETSharingStageのRegistered Spawnable Prefabに登録しておくこと。
        NetworkServer.Spawn(nextBullet);

        //8秒後に弾を破棄する
        //サーバでDestroyされると、全クライアントに生成された弾も破棄される
        Destroy(nextBullet, 8.0f);
    }


}
