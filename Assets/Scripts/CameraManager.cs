/* 
 * Author:  Eric Henderson
 * Last Edited By:  Longfei Yu
 * Date Created:  1-21-2021
 * Description:  This script handles the camera movements as the player performs actions and activates tiles.
 * Filename:  CameraManager.cs
 */

using UnityEngine;


namespace Capstone
{

    public class CameraManager : MonoBehaviour
    {
        public GameObject focusTile;

        //private GameObject activeTile;
        private Vector3 offset;
        private Vector3 moveInput;
        private GameObject[] units;
        private bool isAttacking = false;
        private Cinemachine.CinemachineStateDrivenCamera csdc;
        [SerializeField] private float scrollSpeed = 200;
        [SerializeField] private float panSpeed;
        [SerializeField] private float moveTime;


        // Set a placeholder game object until an active or selected tile is correctly determined.  Also, grab the camera's startup offset.
        private void Start()
        {
            //transform.position = new Vector3(LevelGenerator.tileDimension * LevelGenerator.gridLength / 2f, LevelGenerator.tileDimension * Mathf.Max(LevelGenerator.gridLength, LevelGenerator.gridWidth) / 2f, -LevelGenerator.tileDimension * LevelGenerator.gridWidth / 2f);
            offset = transform.position;
            csdc = gameObject.GetComponent<Cinemachine.CinemachineStateDrivenCamera>();
        }

        // Update camera positions after other runtime movements.
        private void LateUpdate()
        {
            AttackDetection();
            ZoomCam();
            if(GameLogicManager.Instance.isReadyToPlay == true)
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, 0, 0.01f);
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
                    csdc.m_AnimatedTarget = animator;
                    isAttacking = true;
                    //Lock the camera to prevent camera from moving
                    ExtraCameraControl.Instance().canMove = false;
                    break;
                }
                isAttacking = false;
            }

            if (!isAttacking)
            {
                Invoke("SetAnimatedTargetToNull", 2f);
            }
        }

        void SetAnimatedTargetToNull()
        {
            if (csdc == null) return;
            csdc.m_AnimatedTarget = null;
            //unlock camera
            ExtraCameraControl.Instance().canMove = true;
        }

        // Move the camera to appropriately center on the user's selected tile (or active tile, if no selected tile exists).
        private void CenterCamera()
        {
            //GameObject[] candidateTiles = GameObject.FindGameObjectsWithTag("Active Tile");
            //float minimumDistanceToCamera = Mathf.Infinity;
            Vector3 pos = transform.position;

            // If there is a focus tile, then center camera on it.
            if (focusTile != null)
            {
                Vector3 focusPosition = focusTile.transform.position + offset;

                if (pos != focusPosition)
                {
                    pos = Vector3.Lerp(pos, focusPosition, Time.deltaTime);
                }
            }
            /*
            // If there is no focus tile, find the closest active tile and center the camera on it.  Theoretically, there should only ever be one active tile, but the built-in Unity OnMouseExit() method isn't completely reliable.
            else if (candidateTiles.Length > 0)
            {
                foreach (GameObject candidateTile in candidateTiles)
                {
                    float distance = Vector3.Distance(transform.position, candidateTile.transform.position);

                    if (distance < minimumDistanceToCamera)
                    {
                        activeTile = candidateTile;
                        minimumDistanceToCamera = distance;
                    }
                }

                // Adjust camera position assuming there is an active tile (and no currently selected tile).
                Vector3 tmp = activeTile.transform.position + offset;
                pos = Vector3.Lerp(transform.position, new Vector3(tmp.x, pos.y, tmp.z), Time.deltaTime);
            }
            // If there is no focus tile and there was never an activated tile, simply center camera at the default origin position.
            else
            {
                pos = Vector3.Lerp(transform.position, new Vector3(offset.x, pos.y, offset.z), Time.deltaTime);
            }
            */
            transform.position = pos;
        }

        void ZoomCam()
        {
            Vector3 pos = transform.localPosition;
            // prevent camera from moving along z axis while already reached the zoom limit
            if (pos.y > -10 && pos.y < 20)
                pos.z += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
            pos.y -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
            float nor = Input.GetAxis("Mouse X");
            //Debug.Log(nor);
            /*
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
                pos.x += scrollSpeed * Time.deltaTime * nor;
            */
            transform.localPosition = pos;

            //Clamp the y value
            Vector3 pos2 = transform.position;
            pos2.y = Mathf.Clamp(pos2.y, 20, 50);
            transform.position = pos2;
        }

        void MoveCam()
        {
            Vector3 pos = transform.position;
            float midHeight = Screen.height / 2;
            float midWidth = Screen.width / 2;

            moveInput.Set(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            Vector2 mousePos = Input.mousePosition;
            //move right
            if (mousePos.x > Screen.width * 0.9f && mousePos.x <= Screen.width)
            {
                moveInput.x = 1 - (mousePos.y - midHeight) / midHeight;
                moveInput.z = 1 + (mousePos.y - midHeight) / midHeight;
                ClearFocusTile();
            }
            //move left
            if (mousePos.x < Screen.width * 0.1f && mousePos.x >= 0)
            {
                moveInput.z = -1 + (mousePos.y - midHeight) / midHeight;
                moveInput.x = -1 - (mousePos.y - midHeight) / midHeight;
                ClearFocusTile();
            }
            //move up
            if (mousePos.y > Screen.height * 0.9f && mousePos.y <= Screen.height)
            {
                moveInput.x = -1 + (mousePos.x - midWidth) / midWidth;
                moveInput.z = 1 + (mousePos.x - midWidth) / midWidth;
                ClearFocusTile();
            }
            //move down
            if (mousePos.y < Screen.height * 0.1f && mousePos.y >= 0)
            {
                moveInput.x = 1 + (mousePos.x - midWidth) / midWidth;
                moveInput.z = -1 + (mousePos.x - midWidth) / midWidth;
                ClearFocusTile();
            }


            pos += moveInput.normalized * panSpeed * Time.deltaTime;
            //transform.position = Vector3.Lerp(transform.position, pos, moveTime);
        }


        void ClearFocusTile()
        {
            focusTile = null;
        }

    }
}