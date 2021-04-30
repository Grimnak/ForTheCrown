using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Author:  Longfei Yu
 * Last Edited By:  Longfei Yu
 * Date Created:  3-08-2021
 * Description:  This script controls the camera tracking a character from 3rd person view.
 * Filename:  ThirdPersonView.cs
 */

public class ThirdPersonView : MonoBehaviour
{
    public Vector3 offset = new Vector3(1, 3, -3);
    public float lookAtOffset = 3f;

    private bool isAttacking = false;
    private GameObject[] units;
    private Cinemachine.CinemachineVirtualCamera cvc;
    // Start is called before the first frame update
    void Start()
    {
        cvc = gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        AttackDetection();
    }

    private void AttackDetection()
    {
        units = GameObject.FindGameObjectsWithTag("Unit");
        foreach (GameObject u in units)
        {
            Animator animator = u.GetComponent<Animator>();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("AttackPrep") || animator.GetCurrentAnimatorStateInfo(0).IsName("Special") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") 
                || animator.GetCurrentAnimatorStateInfo(0).IsName("HealStart") || animator.GetCurrentAnimatorStateInfo(0).IsName("HealEnd"))
            {
                gameObject.transform.position = animator.transform.position - animator.transform.forward * 6 + animator.transform.up * 3 + animator.transform.right * 2;
                GameObject tmp = new GameObject();
                Transform t = tmp.transform;
                t.SetPositionAndRotation(animator.transform.position, animator.transform.rotation);
                t.position += new Vector3(0, lookAtOffset, 0);
                cvc.LookAt = t;
                Debug.Log("is attacking");
                Destroy(tmp, 0.1f);
                break;
            }
        }

    }

}
