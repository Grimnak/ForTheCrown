using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Capstone
{

    public class SaveManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private Save CreateSaveGameObject()
        {
            Save save = new Save();
            //List<GameObject> objects = new List<GameObject>();
            /*GameObject KnightA = GameObject.Find("Knight A");
            GameObject KnightB = GameObject.Find("Knight B");
            objects.Add(KnightA);
            objects.Add(KnightB);
            
            Vector3 knightApos = KnightA.transform.position;
            float[] knightAposfloat = new float[3];
            
            knightAposfloat[0] = knightApos.x;
            knightAposfloat[1] = knightApos.y;
            knightAposfloat[2] = knightApos.z;
            knightAposfloat = Vector3tofloats(knightApos);

            Vector3 knightBpos = KnightB.transform.position;
            float[] knightBposfloat = new float[3];
            knightBposfloat = Vector3tofloats(knightBpos);

            //save.unitPositions.Add(knightAposfloat);
            //save.unitPositions.Add(knightBposfloat);

            int knightAHealth = KnightA.GetComponent<KnightController>().currentHealth;
            int knightBHealth = KnightB.GetComponent<KnightController>().currentHealth;
            save.unitHealths.Add(knightAHealth);
            save.unitHealths.Add(knightBHealth);*/

            var objects = GameObject.FindGameObjectsWithTag("Unit");
            var count = objects.Length;
            foreach(var obj in objects)
            {
                Vector3 unitPos = obj.transform.position;
                float[] unitPosFloat = Vector3tofloats(unitPos);
                save.unitPositions.Add(unitPosFloat);
                int unitHealth = obj.GetComponent<KnightController>().currentHealth;
                save.unitHealths.Add(unitHealth);
            }

            return save;
        }

        private float[] Vector3tofloats(Vector3 pos)
        {
            float[] knightposfloat = new float[3];
            knightposfloat[0] = pos.x;
            knightposfloat[1] = pos.y;
            knightposfloat[2] = pos.z;

            return knightposfloat;
        }

        public void SaveGame()
        {
            Save save = CreateSaveGameObject();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = File.Create(Application.persistentDataPath + "/gamesave.savefile");
            bf.Serialize(stream, save);
            stream.Close();
            Debug.Log("Game Saved.");
        }

        public void LoadGame()
        {
            if(File.Exists(Application.persistentDataPath + "/gamesave.savefile"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.Open(Application.persistentDataPath + "/gamesave.savefile", FileMode.Open);
                Save save = (Save)bf.Deserialize(stream);
                stream.Close();

                float[] KnightApos = save.unitPositions[0];
                float[] KnightBpos = save.unitPositions[1];

                GameObject KnightA = GameObject.Find("Knight A");
                GameObject KnightB = GameObject.Find("Knight B");
                //Vector3 Apos = new Vector3(KnightApos[0], KnightApos[1], KnightApos[2]);
                KnightA.transform.position = new Vector3(KnightApos[0], KnightApos[1], KnightApos[2]);
                KnightB.transform.position = new Vector3(KnightBpos[0], KnightBpos[1], KnightBpos[2]);

                KnightA.GetComponent<KnightController>().currentHealth = save.unitHealths[0];
                KnightB.GetComponent<KnightController>().currentHealth = save.unitHealths[1];

            }
        }
    }
}

