using UnityEngine;
using DualPantoFramework;
using SpeechIO;
using System.Threading.Tasks;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    private float spawnRange = 9f;
    public GameObject powerupPrefab;
    public int waveNumber = 0;  
    private int enemyCount;
    public bool gameStarted = false;
    private SpeechOut speechOut;

    void Start()
    {
        StartGame();
        speechOut = new SpeechOut();
    }

    async void StartGame() {
        Level room = GameObject.Find("Panto").GetComponent<Level>();
        await room.PlayIntroduction();
        await GameObject.FindObjectOfType<PlayerController>().ActivatePlayer();
        await SpawnPowerup();
        gameStarted = true;
    }

    void OnApplicationQuit()
    {
        speechOut.Stop();
    }

    void Update()
    {
        if (!gameStarted) return;
        enemyCount = FindObjectsOfType<Enemy>().Length;
        if (enemyCount == 0)
        {
            waveNumber++;
            SpawnEnemyWave(waveNumber);
            SpawnPowerup();
        }
    }

    /// challenge: spawn specified numberOfEnemies using Instantiate(...)
    async void SpawnEnemyWave(int numberOfEnemies)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
            if (i == 0)
            {
                await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
            }
        }
    }

    public async void SpawnEnemyWave() {
        await speechOut.Speak("Spawning " + waveNumber + " enemies");
        SpawnEnemyWave(waveNumber);
        waveNumber++;
    }

    private Vector3 GenerateSpawnPosition()
    {
        float randomPosX = Random.Range(-spawnRange, spawnRange);
        float randomPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(randomPosX, 0, randomPosZ);
        return randomPos;
    }
    async public void FindOtherEnemy() {
        GameObject closestEnemy = GetClosestGameObject("Enemy", GameObject.Find("Player").transform.position);
        if (closestEnemy != null)
            await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(closestEnemy);
    }

    GameObject GetClosestGameObject(string tag, Vector3 position) {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;

        float distance = Mathf.Infinity;

        foreach(GameObject go in gos) {
            float currentDistance = Vector3.Distance(go.transform.position, position);
            if (currentDistance < distance)
            {
                closest = go;
                distance = currentDistance;
            }
        }
        return closest;
    }

    async Task SpawnPowerup()
    {
        GameObject powerup = Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
        await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerup);
        await speechOut.Speak("Here is the power up");
    }
}
