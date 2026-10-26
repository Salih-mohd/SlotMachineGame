\# 🎰 Slot Machine Game



A 2D slot machine game developed using \*\*Unity 6 and C#\*\*, featuring three animated reels, configurable betting, symbol-based payouts, interactive lever controls, winning effects, and arcade-style audio.



The project focuses on modular gameplay architecture, reusable components, smooth animations, and Web browser compatibility.



\## Features



\- \*\*Three-Reel Slot Machine:\*\* Three vertically spinning reels with three visible symbols each.

\- \*\*Betting System:\*\* Choose between 10G, 50G, and 100G bets.

\- \*\*Credit Management:\*\* Start with 500G, deduct credits when spinning, and award credits for winning combinations.

\- \*\*Random Outcomes:\*\* Each spin generates random symbol outcomes.

\- \*\*Winning System:\*\* Matching all three center-row symbols awards credits based on the symbol's payout multiplier.

\- \*\*Smooth Reel Animations:\*\* Acceleration, speed-matched deceleration, staggered stopping, and landing bounce.

\- \*\*Interactive Lever:\*\* Two-sprite animated lever replaces the traditional SPIN button.

\- \*\*Winning Effects:\*\* Flying WIN text effects implemented using LeanTween and object pooling.

\- \*\*Animated Credit Display:\*\* Smooth credit counting with a visual shake effect on wins.

\- \*\*Audio System:\*\* Background music, spinning sounds, reel-stop effects, winning audio, and UI click sounds.

\- \*\*Web Build:\*\* Playable in a modern desktop Web browser.



\## Gameplay Rules



The player starts with \*\*500G credits\*\*.



1\. Select a bet amount: 10G, 50G, or 100G.

2\. Click the slot machine lever to spin.

3\. The selected bet is deducted from the credit balance.

4\. All three reels spin and stop sequentially.

5\. The game evaluates the center horizontal payline.

6\. If all three center symbols match, the player receives the corresponding payout.



\### Payout Table



| Symbol Combination | Payout Multiplier |

|---|---:|

| SEVEN × 3 | 10× Bet |

| BELL × 3 | 5× Bet |

| BAR × 3 | 3× Bet |

| CHERRY × 3 | 2× Bet |



\*\*Example:\*\* Matching three SEVEN symbols with a 50G bet awards 500G.



Only the center horizontal payline is evaluated.



\## Controls



| Control | Action |

|---|---|

| Slot Machine Lever | Start a spin |

| BET Button | Open or close bet selection |

| 10G / 50G / 100G | Select bet amount |



Spinning is disabled while a spin is in progress or when the player has insufficient credits for the selected bet.



\## Technologies Used



\- Unity 6

\- C#

\- Unity UI (uGUI)

\- TextMeshPro

\- ScriptableObjects

\- LeanTween

\- Unity Animator

\- Unity AudioSource

\- Web platform build support



\## Project Architecture



The game uses separate components for gameplay, visual effects, audio, and UI.



| Script | Responsibility |

|---|---|

| `SlotMachineController` | Coordinates betting, spinning, outcomes, and rewards |

| `ReelController` | Controls reel symbols, movement, recycling, and stopping |

| `SymbolData` | Stores symbol identifiers, sprites, and payout values |

| `SlotOutcomeGenerator` | Generates random reel outcomes |

| `WinEvaluator` | Checks matching symbols on the center payline |

| `PayoutCalculator` | Calculates winning payouts |

| `WalletManager` | Manages credits, deductions, and rewards |

| `BetSelectionUI` | Handles bet selection |

| `SlotGameUI` | Updates and animates the credit display |

| `SlotEffectsController` | Coordinates WIN text spawning |

| `WinEffectAnchor` | Defines WIN text spawn positions and directions |

| `WinTextEffect` | Animates individual WIN text objects |

| `WinEffectPool` | Reuses WIN text objects through object pooling |

| `SlotAudioController` | Handles music and gameplay sound effects |



\### Implementation Highlights



\*\*ScriptableObject-driven symbols\*\*



Symbol information is stored separately from the reel logic, allowing symbol sprites and payout multipliers to be configured through the Unity Inspector.



\*\*Object pooling\*\*



Reel symbols are created once and recycled during spinning. WIN text effects use a dedicated object pool to avoid repeated runtime instantiation and destruction.



\*\*Smooth animation\*\*



Reels accelerate smoothly and use speed-matched deceleration to stop at predetermined outcomes. The stopping system preserves center-symbol alignment and includes configurable landing feedback.



\*\*Event-driven UI\*\*



Gameplay events and balance-change notifications coordinate UI updates and animations without placing all responsibilities in a single script.



\## Running the Project



\### Option 1 — Unity Editor



1\. Clone or download this repository.

2\. Open \*\*Unity Hub\*\*.

3\. Select \*\*Add project from disk\*\*.

4\. Choose the project folder.

5\. Open the project using a compatible \*\*Unity 6\*\* editor.

6\. Open the main slot machine scene from the `Assets` folder.

7\. Press \*\*Play\*\*.



Unity will import the required packages and assets when opening the project.



\### Option 2 — Web Build



The repository includes a browser build inside the `WebGLBuild` directory.



To run it:



1\. Open the Unity project.

2\. Go to \*\*File → Build Profiles → Web\*\*.

3\. Select \*\*Build And Run\*\*.

4\. Choose the `WebGLBuild` output directory.

5\. Unity will launch the game in a browser using a local server.



Alternatively, host the contents of `WebGLBuild` using a local HTTP server or compatible Web hosting service.



\*\*Note:\*\* Opening `index.html` directly through the `file://` protocol may not work correctly.



\## Development Notes



This project was developed as a Unity slot machine assignment, with an emphasis on maintainable code, reusable systems, and a polished gameplay experience.



The project source and Web build are included in this repository.



