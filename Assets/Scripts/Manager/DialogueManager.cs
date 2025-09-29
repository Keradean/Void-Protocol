/*
====================================================================
* DialogueManager.cs - Simple Sequential Dialogue System (Fixed)
====================================================================
* Project: Void Protocol
* Script-Developer: Julian Gomez
* Version: v1.1 - Compiler fixes + serialized clips
* 
* [HUMAN-AUTHORED] - Simple trigger-based dialogue concept
* [AI-ASSISTED] - Implementation with SoundManager integration
* [AI-ASSISTED] - v1.1: Declare missing fields, add using, tidy API
====================================================================
*/

using System.Collections;          // needed for IEnumerator, WaitForSeconds
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Intro / Narrative Beats")]
    [SerializeField] private AudioClip landing_01;
    [SerializeField] private AudioClip terminalStart_02;
    [SerializeField] private AudioClip keyPointsLocated_03;

    [Header("Spider Warnings")]
    [SerializeField] private AudioClip spiderIncoming1;
    [SerializeField] private AudioClip spiderIncoming2;

    [Header("Tower Completions")]
    [SerializeField] private AudioClip tower1Complete;
    [SerializeField] private AudioClip tower2Complete;
    [SerializeField] private AudioClip tower3Complete;

    [Header("Mission / Extraction")]
    [SerializeField] private AudioClip countdown_09;
    [SerializeField] private AudioClip exitPlanet_10;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ---- Public API ----

    public void PlayLandingMessage()
    {
        SoundManager.Instance?.PlayDialogue(landing_01);
    }

    public void PlayTerminalStartMessage()
    {
        SoundManager.Instance?.PlayDialogue(terminalStart_02);
        Invoke(nameof(PlayKeyPointsMessage), 3f);
    }

    public void PlayTerminalActivated(int terminalIndex)
    {
        // Spider warning based on which terminal was activated
        if (terminalIndex == 1)
            SoundManager.Instance?.PlayDialogue(spiderIncoming1);
        else if (terminalIndex == 2)
            SoundManager.Instance?.PlayDialogue(spiderIncoming2);

        // Terminal SFX (positional) — handled by SoundManager
        SoundManager.Instance?.PlayTerminalActivation(transform.position);

        // After 30s: tower complete VO line
        StartCoroutine(PlayTowerCompleteDelayed(terminalIndex, 30f));
    }

    public void PlayExtractionSequence()
    {
        SoundManager.Instance?.PlayDialogue(countdown_09);
    }

    // ---- Internals ----

    private void PlayKeyPointsMessage()
    {
        SoundManager.Instance?.PlayDialogue(keyPointsLocated_03);
    }

    private IEnumerator PlayTowerCompleteDelayed(int index, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (index == 1)
            SoundManager.Instance?.PlayDialogue(tower1Complete);
        else if (index == 2)
            SoundManager.Instance?.PlayDialogue(tower2Complete);
        else if (index == 3)
        {
            SoundManager.Instance?.PlayDialogue(tower3Complete);
            Invoke(nameof(PlayExitMessage), 2f);
        }
    }

    private void PlayExitMessage()
    {
        SoundManager.Instance?.PlayDialogue(exitPlanet_10);
    }
}
