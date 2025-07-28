# GitHub Copilot Instructions for RimWorld Modding Project

This document provides guidance for using GitHub Copilot to assist with the development of a RimWorld mod project built in C#. It covers key topics such as the mod's purpose, features, coding conventions, XML integration, and Harmony patching to enhance collaborative development.

## Mod Overview and Purpose

The aim of this mod is to introduce a Colony Leadership system into RimWorld, enriching gameplay by adding elections, leadership roles, and educational activities. This enhances the social dynamics and decision-making processes within the game by incorporating leadership mechanics that influence colony efficiency and morale.

## Key Features and Systems

1. **Election Mechanics:**
   - Implement a ballot box system (`Building_BallotBox`) to conduct democratic leader elections.
   - Facilitate leadership changes and governance styles using classes like `Democracy` and methods like `ElectLeader`.

2. **Education System:**
   - Utilize structures like `Building_TeachingSpot` and `Building_Chalkboard` to create teaching and learning environments for pawns.
   - Implement lessons and educational outcomes through methods such as `StartLesson` and `LessonTick`.

3. **Leadership Roles & Effects:**
   - Assign leadership roles influencing pawn behavior and colony efficiency, handled by classes like `HediffLeader` and `Need_LeaderLevel`.

4. **Rebellion and Conflict:**
   - Introduce dynamic incidents such as pawn rebellion (`IncidentWorker_Rebellion`) impacting colony stability.

## Coding Patterns and Conventions

- **Class Definitions:**
  Maintain a clear and structured hierarchy using `public`, `internal`, and `sealed` modifiers as needed to control access levels.
  
- **Method Naming:**
  Use descriptive names for methods such as `TryStartGathering`, `AcceptableMapConditionsToStartElection`, and `Notify_PawnsChanged` to ensure code clarity.

- **Encapsulation:**
  Leverage private and public access modifiers to encapsulate functionality and maintain modular code.

## XML Integration

- **Mod Configuration:**
  While detailed XML integration will be handled in the XML files, ensure that any class needing XML configuration has appropriately named fields and properties for easy integration.

- **Error Handling:**
  Given the XML parsing error, ensure robust error handling in C# to manage situations where XML configurations might fail.

## Harmony Patching

- Implement Harmony patches to modify or extend the game's existing methods. For instance, introduce custom leader behavior using Harmony to override or append to game logic involving leadership roles.
- Utilize `Detours` and `DetourAttribute` for safe method redirection and extensions to preserve game stability and mod compatibility.

## Suggestions for Copilot

1. **Code Completion:**
   Use Copilot to assist with code completion for frequently called methods and complex logic, especially in method-rich classes like `GameComponent_ColonyLeadership`.

2. **Error Handling:**
   Leverage Copilot to suggest common error-handling patterns, such as try-catch blocks, particularly when dealing with XML configurations and Harmony patch exceptions.

3. **Refactoring Suggestions:**
   Copilot can offer valuable insights for refactoring redundant or complex logic within methods like `TryLesson` and `ElectLeader`.

4. **Documentation Aid:**
   Utilize Copilot to help draft method summaries and comments to maintain thorough documentation across the codebase.

By following these guidelines, the usage of GitHub Copilot can be optimized for efficient and effective development of the Colony Leadership mod in RimWorld.
 

This instructional guide provides a comprehensive roadmap for contributors utilizing GitHub Copilot, ensuring clarity and consistency throughout the RimWorld mod project.
