/// Hint: Commenting or uncommenting in VS
/// On Mac: CMD + SHIFT + 7
/// On Windows: CTRL + K and then CTRL + C

using UnityEngine;
using DualPantoFramework;
using SpeechIO;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRb;
    public float speed = 10f;
    public GameObject focalPoint;
    public bool hasPowerup;
    private float powerupStrength = 15f;
    public int powerupTime = 7;
    public GameObject powerupIndicator;
    private SpeechIn speech;
    private SpeechOut speechOut;
    private bool movementFrozen;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        //ActivatePlayer();
        speech = new SpeechIn(onSpeechRecognized);
        speech.StartListening(new string[]{"help", "resume"});
        speechOut = new SpeechOut();
    }

    async void onSpeechRecognized(string command) {
        if (command == "resume" && movementFrozen) {
            ResumeAfterPause();
        } else if (command == "help" && !movementFrozen) {
            ToggleMovementFrozen();
            var powerups = GameObject.FindGameObjectsWithTag("Powerup");
            if (powerups.Length > 0) {
                await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(powerups[0]);
            }
        }
    }

    void ToggleMovementFrozen() {
        playerRb.constraints = movementFrozen ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Rigidbody>().constraints = movementFrozen
                                           ? RigidbodyConstraints.None
                                           : RigidbodyConstraints.FreezeAll;
        }
        movementFrozen = !movementFrozen;
    }

    async void ResumeAfterPause() {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            await GameObject.Find("Panto").GetComponent<LowerHandle>().SwitchTo(enemy);
        }
        ToggleMovementFrozen();
    }

    public async Task ActivatePlayer()
    {
        UpperHandle upperHandle = GameObject.Find("Panto").GetComponent<UpperHandle>();
        await upperHandle.SwitchTo(gameObject);
        upperHandle.FreeRotation(); 
    }

    void Update()
    {
        if(!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        powerupIndicator.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
    }

    void FixedUpdate()
    {
        if(!GameObject.FindObjectOfType<SpawnManager>().gameStarted) return;
        //float forwardInput = Input.GetAxis("Vertical");
        //playerRb.AddForce(focalPoint.transform.forward * forwardInput * speed);
        PantoMovement();
    }

    void PantoMovement()
    {
        UpperHandle upperHandle =  GameObject.Find("Panto").GetComponent<UpperHandle>();
        float rotation = upperHandle.GetRotation();
        transform.eulerAngles = new Vector3(0, rotation, 0);
        playerRb.velocity = speed * transform.forward;
    }

    async void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
        {
            hasPowerup = true;
            powerupIndicator.gameObject.SetActive(true);
            Destroy(other.gameObject);
            CancelInvoke("PowerupCountdown"); // if we previously picked up an powerup
            Invoke("PowerupCountdown", powerupTime);
            await speechOut.Speak("You got the power up");
            GameObject.FindObjectOfType<SpawnManager>().SpawnEnemyWave();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        /// challenge: when collision has tag "Enemy" and we have a powerup
        /// get the enemyRigidbody and push the enemy away from the player
        if (other.CompareTag("Enemy") && hasPowerup)
        {
            Rigidbody enemyRigidbody = other.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.transform.position - transform.position;
            enemyRigidbody.AddForce(awayFromPlayer.normalized * powerupStrength, ForceMode.Impulse);
        }
    }

    void PowerupCountdown()
    {
        hasPowerup = false;
        powerupIndicator.gameObject.SetActive(false);
    }

    void OnApplicationQuit() {
        speechOut.Stop();
    }
}
