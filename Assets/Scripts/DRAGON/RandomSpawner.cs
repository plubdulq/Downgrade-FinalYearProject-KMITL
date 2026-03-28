using UnityEngine;
using System.Collections;

public class RandomSpawner : MonoBehaviour
{
    public GameObject[] prefab;       // เก็บ Prefab หลายตัว
    public Vector3 spawnArea = new Vector3(10, 0, 10); // พื้นที่สุ่ม
    public float minSpawnTime = 1f;   // เวลาสุ่มต่ำสุด
    public float maxSpawnTime = 3f;   // เวลาสุ่มสูงสุด
    public Transform SpawnPos;
    void Start()
    {

            StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // สุ่มเวลาหน่วง
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // สุ่มตำแหน่ง spawn
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                spawnArea.y,
                Random.Range(-spawnArea.z, spawnArea.z)
            );

            // เลือก prefab แบบสุ่มจาก array
            int randomIndex = Random.Range(0, prefab.Length);
            GameObject obj = prefab[randomIndex];

            // สร้าง Object
            Instantiate(obj, SpawnPos.position,SpawnPos.rotation);
        }
    }
}
