using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class CodeKey : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip ScanAppearSfx;
    public AudioClip failOpenSfx;
    private AudioSource audioSource;

    [Header("Code Setting")]
    public int codeLength = 4;
    public float resetDelay = 1.5f;

    private List<int> targetCode = new List<int>();
    private List<int> currentInput = new List<int>();

    [Header("UI")]
    public GameObject Scanblock;
    public GameObject codeblock;
    public TextMeshProUGUI codeRandomText; // แสดงรหัส (ถ้าต้องการโชว์)
    public TextMeshProUGUI codeInfroText; // แสดงรหัส (ถ้าต้องการโชว์)

    void Start()
    {
          GenerateRandomCode();
        audioSource = GetComponent<AudioSource>();
        codeInfroText.text = "Input Key Code for scan keycard";
      

        if (codeblock != null)
            codeblock.SetActive(false);
    }

    void GenerateRandomCode()
    {
        targetCode.Clear();

        for (int i = 0; i < codeLength; i++)
        {
            int randomNumber = Random.Range(1, 10);
            targetCode.Add(randomNumber);
        }

        string codeString = string.Join("", targetCode);

        Debug.Log("Target Code: " + codeString);

        if (codeRandomText != null)
            codeRandomText.text = codeString;
    }

    public void PressNumber(int number)
    {
        if (currentInput.Count >= codeLength)
            return;

        currentInput.Add(number);

        Debug.Log("Input: " + string.Join("", currentInput));

        if (currentInput.Count >= codeLength)
        {
            CheckCode();
        }
    }

    void CheckCode()
    {
        bool correct = true;

        for (int i = 0; i < codeLength; i++)
        {
            if (currentInput[i] != targetCode[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Debug.Log("Correct Code!");

            if (Scanblock != null)
                Scanblock.SetActive(true);

            codeInfroText.text = "Correct Code!";
            PlayOneShot(ScanAppearSfx);
            currentInput.Clear();
            StartCoroutine(Resettext());
        }
        else
        {
            Debug.Log("Wrong Code!");
            codeInfroText.text = "Wrong Code!";
            PlayOneShot(failOpenSfx);
            StartCoroutine(ResetAfterFail());
        }
    }
IEnumerator Resettext()
    {
        yield return new WaitForSeconds(resetDelay);
        codeInfroText.text = "Scan keycard and move to the door";

    }
    IEnumerator ResetAfterFail()
    {
        yield return new WaitForSeconds(resetDelay);

        currentInput.Clear();
        codeInfroText.text = "Input Key Code for scan keycard";
        Debug.Log("Input Reset");
    }

    void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}