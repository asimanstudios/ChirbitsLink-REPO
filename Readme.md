# Chirbits

Chirbits is a real-time multiplayer entertainment platform designed to connect people through accessible gaming experiences without the need for a dedicated console.

---

## Overview

The system is composed of two main components:

- A mobile application built with **.NET MAUI (Android)** acting as a smart virtual controller.
- A main client developed in **Unity**, responsible for game execution and core gameplay logic.

Communication between both components is handled using **TCP/WebSocket**, enabling low-latency transmission of touch inputs and sensor data.

Authentication, lobby management, and data persistence are powered by **Firebase Authentication** and **Cloud Firestore**, ensuring secure access and real-time synchronization.

---

## Architecture

Chirbits follows a **three-layer client-server architecture**:

- **Presentation layer**: Mobile app (.NET MAUI)
- **Logic & communication layer**: Unity client + TCP/WebSocket server
- **Data layer**: Firebase (Authentication + Firestore)

---

## Key Features

- Create and join lobbies using 6-digit codes
- Secure user authentication system
- Character selection system
- Minigame voting system
- Real-time score tracking
- Player history and match tracking

---

## Minigames

- **Coin Rush**: Collect coins scattered across the map as fast as possible
- **BombTag**: Hot-potato style game where players avoid being eliminated
- **Hook Collector**: Use a grappling hook mechanic to collect coins strategically

---

## Tech Stack

- Unity (Game client)
- .NET MAUI (Android mobile controller)
- TCP / WebSocket (real-time communication)
- Firebase Authentication
- Cloud Firestore

---

## Objective

The goal of Chirbits is to provide an accessible multiplayer gaming experience from mobile devices, removing the need for dedicated hardware and lowering the entry barrier for real-time interactive games.

---

## Getting Started

### Requirements

- Unity (compatible version used in project)
- .NET MAUI workload installed
- Firebase project configured
- Android device or emulator

### Setup

1. Clone the repository
2. Configure Firebase credentials (Auth + Firestore)
3. Start Unity server/client
4. Build and run the .NET MAUI Android app
5. Connect both using lobby code

---

## Notes

This project demonstrates a complete real-time multiplayer architecture including networking and mobile integration.
