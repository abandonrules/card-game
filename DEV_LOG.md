# Card Game
A multiplayer strategy game inspired by Final Fantasy's Triple Triad mini-game and the classic board game, Reversi.

## Development Log
**April 12th, 2017**  
![](https://thumbs.gfycat.com/FancyGrimBarasinga-size_restricted.gif)

- *Programming*
    - +Prototype of playable demo
    - +Networking framework implemented using PUN
    - +Added new scene to test matchmaking system
        - Players are matched randomly
    - + Added turn system to determine current turn number, remaining time in turn, current player's turn synchronized between players
    - +Initial setup of game has been changed to fit network framework (spawning 2 players, dealing of cards, synchronized card info, etc.)
- *Discussion*
    - Should a turn be ended manually after 1) Placing a card? or 2) Pressing a button?

**March 21st, 2017**  
![](https://thumbs.gfycat.com/JovialFarGardensnake-size_restricted.gif)
![](https://thumbs.gfycat.com/SoggyQueasyDungbeetle-size_restricted.gif)

- *Programming*
    - +Introduced two various implementations of a player turn
    - +Mobile testing done through XCode (iPad)
    - +Added card and board behavior based on player interaction
- *Discussion*
    - Decision to use Player Cycle 2
    - Idea of switching between floating/static numbers of cards when rotating

**March 5th, 2017**
- Discussed new idea of mixing in (Reversi) with initial proposal, re-adjusting gameplay and player mechanics

**February 17th, 2017**
- Discussed initial proposal of game, gameplay design and mechanics (similar to Triple Triad)
- Primary focus targeted as a mobile app with PC as secondary target
