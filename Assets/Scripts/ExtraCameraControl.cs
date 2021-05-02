using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Capstone
{
    public class ExtraCameraControl : MonoBehaviour
    {
        public GameObject focusTile;
        public bool canMove = true;

        //private GameObject activeTile;
        private Vector3 offset;
        private Vector3 moveInput;
        private Quaternion newRotation;
        private GameObject[] units;
        private bool isAttacking = false;
        private Transform cam;
        [SerializeField] private float panSpeed;
        [SerializeField] private float scrollSpeed = 1;
        [SerializeField] private Vector2 clampRange = new Vector2(-70, 70);
        [SerializeField] private float moveTime = 5;
        public static ExtraCameraControl ecc;

        public static ExtraCameraControl Instance()
        {
            if(ecc == null)
            {
                ecc = new ExtraCameraControl();
            }
            return ecc;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (ecc != null)
            {
                Destroy(gameObject);
                return;
            }
            ecc = this;
            transform.position = new Vector3(LevelGenerator.tileDimension * LevelGenerator.gridLength / 2f, LevelGenerator.tileDimension * Mathf.Max(LevelGenerator.gridLength, LevelGenerator.gridWidth) / 2f, -LevelGenerator.tileDimension * LevelGenerator.gridWidth / 2f);
            newRotation = transform.rotation; 
            offset = transform.position;
            cam = gameObject.GetComponentInChildren<Transform>();
            Cursor.lockState = CursorLockMode.Confined;
            canMove = true;
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if(canMove)
                MoveCam();

            ClampCam();
            if (Input.GetKey(KeyCode.E))
            {
                RotateCam();
                //AdjustOffset();
            }
            cam = gameObject.GetComponentInChildren<Transform>();
            CursorStateSwitch();
        }

        private void CursorStateSwitch()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Cursor.lockState == CursorLockMode.Confined)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Confined;
            }
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

        void AdjustOffset()
        {
            offset = transform.position;
        }

        void MoveCam()
        {
            //Vector3 pos = transform.localPosition;
            //float midHeight = Screen.height / 2;
            //float midWidth = Screen.width / 2;

            moveInput.Set(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            Vector2 mousePos = Input.mousePosition;
            //move right
            if (mousePos.x == Screen.width - 1)
            {
                transform.localPosition += (-transform.forward + transform.right) * panSpeed;
                ClearFocusTile();
            }
            //move left
            if (mousePos.x == 0)
            {
                transform.localPosition += (transform.forward - transform.right) * panSpeed;
                ClearFocusTile();
            }
            //move up
            if (mousePos.y == Screen.height)
            {
                transform.localPosition += (transform.right + transform.forward) * panSpeed;
                ClearFocusTile();
            }
            //move down
            if (mousePos.y == 1)
            {
                transform.localPosition += (-transform.right - transform.forward) * panSpeed;
                ClearFocusTile();
            }

            //pos += moveInput.normalized * panSpeed * Time.deltaTime;
            //transform.position = Vector3.Lerp(transform.position, pos, moveTime);
        }

        void RotateCam()
        {
            ClearFocusTile();
            Vector3 pivot = cam.position + cam.forward * 50f - cam.up * 50f;
            // Debug.Log(transform.position);
            // Debug.Log(cam.position);


            float counter = 1f;
            while (counter > 0)
            {
                transform.RotateAround(pivot, Vector3.up, 1 * Time.deltaTime);
                counter -= Time.deltaTime;
            }
        }

        void ZoomCam()
        {
            Vector3 pos = transform.position;
            pos.y += -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
            transform.position = pos;
        }

        void ClampCam()
        {
            Vector3 pos = transform.position;

            pos.x = Mathf.Clamp(pos.x, clampRange.x, clampRange.y); 
            //pos.y = Mathf.Clamp(pos.y, 20, 50);
            pos.z = Mathf.Clamp(pos.z, clampRange.x, clampRange.y);

            transform.position = pos;
        }

        void ClearFocusTile()
        {
            focusTile = null;
        }
    }
}