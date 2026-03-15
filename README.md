# Unity Pickup Truck with Local LLM Control

![Project Screenshot](ControlPickupTruckWithLLM/picture.png)

🎥 Demo Video: https://www.youtube.com/watch?v=FbKYlDyITbU

A simple Unity driving project where a pickup truck can be controlled in two ways:

- manually with keyboard input
- through natural language commands sent to a **local LLM server**

Example commands:

- `go forward by 2`
- `turn left 90 degrees`
- `go forward and turn left`
- `go backward and take a right turn`

The game sends the text command to a local model endpoint, gets back structured JSON, and then moves the pickup truck inside Unity.

---

## Features

- Pickup truck driving in Unity
- Keyboard-based movement
- Natural language control using a local LLM
- Chat-style command interface in the game UI
- Support for multi-step commands like move + turn
- Local network connection to a model server running on another machine

---

## Project Overview

This project combines:

- **Unity 6 / URP**
- a **pickup truck vehicle setup**
- a **simple in-game chat UI**
- a **local LLM server** compatible with the OpenAI-style `/v1/chat/completions` API

The Unity game sends the player's text prompt to the local model, parses the model response, and executes the resulting movement commands.

---

## Example Workflow

Player types:

`go forward and turn left`

Model returns JSON like:

```json
{
  "reply": "Done. I moved forward and turned left. Where should I go next?",
  "commands": [
    { "action": "move", "direction": "forward", "distance": 2 },
    { "action": "turn", "direction": "left", "degrees": 90 }
  ]
}