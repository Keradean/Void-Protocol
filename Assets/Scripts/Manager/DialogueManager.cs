/*
====================================================================
* DialogueManager.cs - Simple Sequential Dialogue System
====================================================================
* Project: Void Protocol
* Script-Developer: Julian Gomez
* Version: v1.0 - MVP Audio Integration
* 
* [HUMAN-AUTHORED] - Simple trigger-based dialogue concept
* [AI-ASSISTED] - Implementation with SoundManager integration
====================================================================
*/

using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // ... existing AudioClip fields ...

    private void Awake()
    {
        // HINZUFÜGEN:
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // NEUE PUBLIC METHODS:

    public void PlayLandingMessage()
    {
        SoundManager.Instance?.PlayDialogue(landing_01);
    }

    public void PlayTerminalStartMessage()
    {
        SoundManager.Instance?.PlayDialogue(terminalStart_02);
        Invoke(nameof(PlayKeyPointsMessage), 3f);
    }

    private void PlayKeyPointsMessage()
    {
        SoundManager.Instance?.PlayDialogue(keyPointsLocated_03);
    }

    public void PlayTerminalActivated(int terminalIndex)
    {
        // Spider Warning
        if (terminalIndex == 1)
            SoundManager.Instance?.PlayDialogue(spiderIncoming1);
        else if (terminalIndex == 2)
            SoundManager.Instance?.PlayDialogue(spiderIncoming2);

        // Terminal Audio
        SoundManager.Instance?.PlayTerminalActivation(transform.position);

        // Tower Complete nach 30 Sekunden
        StartCoroutine(PlayTowerCompleteDelayed(terminalIndex, 30f));
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

    public void PlayExtractionSequence()
    {
        SoundManager.Instance?.PlayDialogue(countdown_09);
    }
}