# Void Protocol

> Ein 3D-Sci-Fi-First-Person-Survival-Shooter auf einem lebensfeindlichen Planeten, entwickelt mit Unity 6 im Rahmen des Praxisprojekts (PIP) an der SRH Fachschulen GmbH.

![Unity Version](https://img.shields.io/badge/Unity-6000.0.56f1-black)
![Render](https://img.shields.io/badge/Pipeline-URP-blue)
![Status](https://img.shields.io/badge/Status-Playable%20Build-green)

---

## 📋 Projektinfo

| | |
|---|---|
| **Genre** | 3D First-Person Survival-Shooter |
| **Engine** | Unity 6000.0.56f1 |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Input** | Unity Input System (Action Maps: `Player` / `UI`) |
| **Kamera** | Cinemachine |
| **KI-Navigation** | Unity AI Navigation (NavMesh) |
| **Arbeitstitel** | „Space Colony Game" |
| **Kurs** | PIP, SRH Fachschulen |

---

## 🎮 Worum geht's

Deine Kapsel schlägt auf einem fremden, sauerstoffarmen Planeten ein. Deine **Sauerstoffreserve tickt permanent herunter**. Bewegst du dich nicht weiter und füllst du keine Tanks nach, kostet dich das am Ende das Leben. Über ein Funk-Terminal bekommst du deinen Auftrag: Finde die verteilten **Terminals und Türme** auf der Karte und aktiviere sie. Jede Aktivierung lockt Wellen von **Spinnen** an, gegen die du dich mit deinen Waffen wehren musst. Sind alle Ziele aktiviert, startet ein **Countdown zur Extraktion**. Schaffst du es rechtzeitig zurück zum Landepunkt, entkommst du dem Planeten.

Erzählt wird das Ganze über eine durchgehende **Sprach-Dialog-Ebene**: Landung, Terminalstart, Spinnenwarnungen, Turm-Fortschritt, Countdown und Abflug begleiten den Missionsverlauf.

---

## ✨ Features

- ✅ First-Person-Controller (CharacterController) mit Gehen, Sprinten und Springen
- ✅ Ressourcen-Survival mit Health, Stamina und Oxygen (drainen und auffüllen)
- ✅ Sauerstoff-System mit Schaden bei leerem Tank
- ✅ Waffensystem mit mehreren Waffen, Munition, Nachladen und Waffenwechsel
- ✅ Raycast-Shooting mit Impact- und Damage-Effekten sowie Muzzle Flash
- ✅ Gegner-KI (Spinnen) über eine Finite State Machine: Patrol, Wander, Chase, Attack
- ✅ Gegner-Spawner mit **Object Pooling**
- ✅ Interaktionssystem: Terminals aktivieren, Pickups für Ammo, Health und Oxygen
- ✅ Missions-System mit Timer (`MissionManager`, ScriptableObject-Missionen)
- ✅ Sprachgesteuerte Story-Beats (`DialogueManager` und `SoundManager`)
- ✅ Tile-basiertes Navigations-UI (Richtung und Distanz zum nächsten Ziel)
- ✅ EXP- und Level-System
- ✅ Haupt- und Startmenü mit Szenenwechsel

---

## 🕹️ Steuerung

Gesteuert wird über das **Unity Input System** (Action Map `Player`). Standard-Belegung:

| Aktion | Input |
|---|---|
| Bewegung | `WASD` |
| Umsehen | `Maus` |
| Springen | `Leertaste` |
| Sprinten | `Shift` |
| Interagieren (Terminal / Pickup) | `E` |
| Schießen | `Linke Maustaste` |
| Nachladen | `R` |
| Nächste Waffe | `Mausrad hoch` / `Next` |
| Vorherige Waffe | `Mausrad runter` / `Previous` |
| Pause | `ESC` |

> Da alles über das Input System läuft, lassen sich sämtliche Bindings (auch Controller) im `Actions`-Asset anpassen.

---

## 🧩 Systeme & Architektur

### Spieler (`Player/`)

Alle Werte liegen zentral in einem **`PlayerStats`-ScriptableObject** (Level, Health, Stamina, Oxygen, EXP).

| Script | Aufgabe |
|---|---|
| `PlayerController.cs` | Bewegung, Kamera, Springen, Sprinten, Schießen, Interaktion |
| `InputManager.cs` | Kapselt das Input System (Player- und UI-Action-Maps) |
| `PlayerHealth.cs` | `TakeDamage()`, Tod, Death-Screen (`IDamageable`) |
| `PlayerStamina.cs` | Ausdauer verbrauchen (Sprint) und regenerieren |
| `PlayerOxygen.cs` | Sauerstoff-Drain, Schaden bei leerem Tank |
| `PlayerExp.cs` | EXP sammeln und Level-Up berechnen |
| `PlayerInteraction.cs` | Raycast auf `Interactable`, Highlight und Prompt |

### Waffen (`Weapon/`)

- **`Weapon.cs`**: Datencontainer je Waffe mit Range, Damage, Feuerrate (`TimeBtwShots`), `AutoFire`, `ClipSize`, `CurrentAmmo`, `RemainingAmmo` und Muzzle Flare
- **`WeaponsManager.cs`**: hält das Waffen-Array, verwaltet die aktuelle Waffe, feuert per **Raycast**, steuert das Muzzle-Flare-Timing, unterscheidet Impact- und Damage-Effekt (je nach `IDamageable`-Treffer) und regelt Nachladen und Waffenwechsel

### Gegner-KI (`Enemy/`)

Im Projekt stecken zwei Ansätze nebeneinander:

**Finite State Machine (`Enemy/FSM/`):**

```
EnemyBrain ──> CurrentState (FSMState)
                 ├── Actions:    ActionPatrol · ActionWander · ActionChase · ActionAttack
                 └── Decisions:  DetectPlayer · DecisionAttackPlayer
```

- `EnemyBrain.cs` hält die States, wechselt per String-ID und dreht den Gegner zum Spieler (Slerp)
- Die Decisions liefern `true` oder `false` und lösen so die Transition in den passenden State aus

**Direkter Controller (`EnemyController.cs`):** eine alternative Logik mit Patrol-Waypoints, Chase, Strafing und Nahkampf-Attacke inklusive Cooldown.

**Weitere:** `EnemyHealth.cs` (Leben, Tod, Rückgabe an den Pool) und `EnemyEXP.cs` (gibt beim Tod EXP an den Spieler).

### Spawner (`Enemy/Spawner/Spawner.cs`)

Erzeugt Gegner über ein **`ObjectPool<EnemyHealth>`** an zufälligen Spawn-Punkten, bis `spawnStop` erreicht ist. Scharfgeschaltet wird er von aktivierten Terminals. Das Object Pooling vermeidet GC-Spikes während der Gegnerwellen.

### Missionen (`Manager/MissionManager/`)

- **`Mission.cs`**: abstrakte ScriptableObject-Basis (`StartMission`, `UpdateMission`, `MissionCompleted`)
- **`MissionTimer.cs`**: konkrete Mission mit Countdown
- **`MissionManager.cs`**: Singleton, startet und aktualisiert die aktuelle Mission
- **`MissionsEnd.cs`**: Trigger am Extraktionspunkt

### Interaktion (`Extra/Interaction/`)

- **`Interactable.cs`**: Basis mit Highlight-Material und Prompt-Text
- **`InteractTerminal.cs`**: startet die Mission und aktiviert den zugehörigen Enemy-Spawner
- **Pickups:** `PickUpAmmo`, `PickUpHealth`, `PickUpOxygenTank`

### Audio & Narrative

- **`DialogueManager.cs`**: spielt die Story-Beats (Landing, Terminal, Spinnenwarnungen, Turm-Completions, Countdown, Abflug) über den `SoundManager`
- **`SoundManager.cs`**: zentrale Sound- und Voice-Ausgabe (Combat, Movement, Interaction, Voice Dialogue)

### Navigation (`World/GroundTiles/`)

`TileManager` und `TileNavigationUI` berechnen Richtung (8 Himmelsrichtungen) und Distanz zum nächsten Schlüssel-Ziel und zeigen beides im HUD an. Das funktioniert als schlanker Kompass, der den Spieler zum nächsten Ziel führt.

---

## 🗺️ Szenen

| Szene | Rolle |
|---|---|
| `Assets/Scenes/StartMenue.unity` | Start- und Hauptmenü |
| `Assets/Scenes/MainScene.unity` | Hauptspiel (Planetenoberfläche) |

Das Menü lädt über `MainMenu.cs` mit `SceneManager.LoadScene("MainScene")`.

---

## 📂 Projektstruktur

```
Assets/
├── Scripts/
│   ├── Player/        PlayerController, InputManager, Player, PlayerStats (SO),
│   │                  PlayerHealth, PlayerStamina, PlayerOxygen, PlayerExp,
│   │                  PlayerInteraction
│   ├── Weapon/        Weapon, WeaponsManager
│   ├── Enemy/         EnemyBrain, EnemyController, EnemyHealth, EnemyEXP
│   │   ├── FSM/       FSMState, FSMAction, FSMDecision, FSMTransition
│   │   │   ├── Actions/   ActionPatrol, ActionWander, ActionChase, ActionAttack
│   │   │   └── Decisions/ DetectPlayer, DecisionAttackPlayer
│   │   └── Spawner/   Spawner (Object Pool)
│   ├── Manager/       GameManager, UIManager, SoundManager, DialogueManager
│   │   └── MissionManager/ Mission, MissionManager, MissionTimer, MissionsEnd
│   ├── Extra/         Singleton<T>, IDamageable
│   │   └── Interaction/ Interactable, InteractTerminal, PickUpAmmo/Health/OxygenTank
│   ├── Waypoint/      Waypoint (+ Editor)
│   ├── World/         GroundTiles: TileManager, TileNavigationUI, TileSystemSetup
│   └── MainMenu/      MainMenu
├── ScriptableObjects/ Player, Missions
├── Art/              3D Models (Enemy, Weapons, Interaction), Shader, World, UI, VFX
├── Sounds/           Combat, Movement, Interaction, VoiceDialogue, HoverCraft
├── Prefabs/          Avatar, Weapons, Enemy, PickUps, World, Effects, Interact
├── Actions/          Input System Assets
└── Scenes/           StartMenue, MainScene
```

---

## 🛠️ Technischer Überblick

- **Generisches Singleton** (`Extra/Singleton<T>.cs`) für die Manager
- **`IDamageable`-Interface** für Spieler und Gegner, damit das Schadensmodell einheitlich bleibt
- **ScriptableObjects** für `PlayerStats` und Missionen, sodass sich das Balancing im Editor erledigen lässt
- **Object Pooling** (`UnityEngine.Pool`) für die Gegner
- **Finite State Machine** für eine modulare Gegner-KI
- **Unity Input System** mit getrennten Player- und UI-Action-Maps
- **URP, Cinemachine und AI Navigation (NavMesh)**

---

## 🚀 Installation & Start

1. Repository klonen:
   ```bash
   git clone https://github.com/Keradean/Void-Protocol.git
   ```
2. Unity Hub öffnen, dann **Add project from disk**
3. Unity-Version **6000.0.56f1** auswählen
4. Projekt öffnen und die Szene **`Assets/Scenes/StartMenue.unity`** laden
5. Play drücken, dann im Menü auf **Play Game**

**Debug:** `L` setzt im Spiel die Spieler-Stats zurück (Entwickler-Hilfe).

---

## 🎨 Assets & Credits

- **Engine-Pakete:** Unity URP, Cinemachine, AI Navigation, Input System, TextMesh Pro
- **Audio:** Voice-Dialogue-, Combat-, Movement- und Interaction-Sounds (`Assets/Sounds`)
- **3D-Modelle und VFX:** Gegner, Waffen, Interaktionsobjekte, Partikel (unter anderem Sandstorm)

Die Autorenschaft ist in den Script-Headern dokumentiert:
- **Script-Entwicklung:** Dennis De Col
- **Audio-Integration (Konzept):** Julian Gomez *(siehe `PlayerController.cs`)*

> Einige Skripte, darunter `PlayerController`, `Player` und `InputManager`, stammen aus dem Vorgängerprojekt *„The Last Refuge / 3D Interactive"* und wurden hier weiterverwendet.

*Alle verwendeten Assets sind lizenzfrei oder entsprechend lizenziert.*

---

## 📜 Lizenz

MIT License, siehe [`LICENSE.md`](LICENSE.md).
Entwickelt im Rahmen des Praxisprojekts (PIP) an der SRH Fachschulen GmbH.

**Entwickler:** Dennis De Col · **GitHub:** https://github.com/Keradean/Void-Protocol
</content>
