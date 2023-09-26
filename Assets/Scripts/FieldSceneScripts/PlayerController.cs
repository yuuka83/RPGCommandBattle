using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    [SerializeField] private Camera followCamera;

    private float x, z;
    [SerializeField] private float speed = 1.0f;

    private void Start()
    {
        playerAnimator = this.GetComponent<Animator>();
        playerRigidbody = this.GetComponent<Rigidbody>();
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        prevPos = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private Vector3 prevPos;

    public void HandleUpdate()
    {
        // 進行方向への回転
        Vector3 currentPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 deltaPos = currentPos - prevPos;
        // 移動していれば
        if (deltaPos != Vector3.zero)
        {
            // 進行方向の回転
            Quaternion targetQuaternion = Quaternion.LookRotation(deltaPos, Vector3.up);
            transform.rotation = targetQuaternion;
        }
        prevPos = currentPos;
        // 矢印方向への移動
        // 左を押せば-1、右を押せば1、何も押さなければ0 0-1の間で取得
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        transform.position += new Vector3(x, 0, z) * speed * Time.deltaTime;
        //followCamera.transform.position = this.transform.position + new Vector3(0, 1.12f, -3f);

        Vector3 transformAmount = new Vector3(x, 0, z);
        playerAnimator.SetFloat("Forward", transformAmount.magnitude, 0.1f, Time.deltaTime);
        followCamera.transform.position = transform.position - new Vector3(followCamera.transform.forward.x, -0.5f, followCamera.transform.forward.z) * 5;
        // カメラ移動
        if (Input.GetKeyDown(KeyCode.Q))
        {
            followCamera.transform.RotateAround(this.transform.position, Vector3.up, 90);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            followCamera.transform.RotateAround(this.transform.position, Vector3.up, -90);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Destroy(collision.gameObject);
            // ゲームシーンを切り替える
            gameManager.StartBattle();
        }
    }
}