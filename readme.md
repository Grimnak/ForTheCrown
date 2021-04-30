# For The Crown!

For The Crown! is a semi-realistic medieval turn-based strategy game in which players choose their armies and fight against a friend or the AI.

### Table of Contents

* **How to Play**
* **Game Modes**
* **Units**
* **Structures**

---

### How to Play

For The Crown! is primarily controlled with the Mouse. The Mouse is responsible for nearly all inputs in the game.

**Menus**

- Left Click to trigger buttons
- Escape to step out of menus, also can be pressed to activate menu in the main game scene
- Left Click and drag 2D sprites to various containers. Dragging a 2D sprite during the game scene will spawn that unit into the game board.
- After a map has been selected, the player will enter an army selection menu. Units have different costs which will be realized when these units are added to the player's army. Dragging these 2D sprites from the top container to the lower container will add them to the player's army. You may drag them back to the original container to refund your choice.

**Controlling your Army**

- Left Click a unit to view its action options. If you're in the Move Phase, then the active tiles will represent a unit's movement options. If you're in the Attack Phase, then the active tiles will represent a unit's attack options.
- Right Click an active tile to issue a respective command to a selected unit. If in the Move Phase, then the unit will move to the right-clicked tile. If in the Attack Phase, then the unit will attempt to attack that tile.

**Combat UI**

- During the Game Scene, you will notice two menus popping in and out during regular combat. The left menu corresponds to whatever your mouse is hovering over. When your mouse hovers over something with information to provide, then the Hover Combat Menu will display the relevant information. The right menu corresponds to whatever you have selected with a left click. If you've clicked and are hovering the same unit, both menus will display the same information.
- When you've left clicked your unit, in addition to seeing action options via tile feedback, you'll notice a menu in the bottom-right of the screen. That menu manages the Special Attack button. Special Attacks are elaborated on more later in this guide. In regards to Combat UI, pressing that button will activate a Unit's special attack, if a unit is able to use its special attack.
- The Combat UI menus also contain a familiar concept in Strategy games: unit veterancy. Whenever a unit performs an action or is involved in an event, it will earn promotion points. When a unit has a certain number of promotion points, it will be able to promote via the selected Combat UI panel. Choosing the top promotion option will boost the unit's stats, while choosing the bottom promotion option will heal the unit. The final promotion that a unit receives will include a unique stat buff to augment that unit's overall design.

**Camera**

- During the Game Scene, you will have the ability to control the camera. By moving the mouse to the edge of the game window, you will move the camera in that direction, relative to the camera's position in the game world. Additionally, the mouse is locked to the game window unless you tab out of the game.
- The camera also has the ability to zoom in/out and rotate. To zoom, use the scroll wheel. To rotate, press E.
- When an attack event occurs, a camera transition will zoom in on the event.

---

### Game Modes

For The Crown! offers 3 different Game Modes: Training, Endless, and Online Multiplayer.

**Training**

- The goal of this game mode is to expose players to new unit types in a controlled environment. This game mode is supposed to be as easier than other modes for players to learn how to play the game.
- The game map remains on the default game map and is not selectable.
- Each of the four training missions features progressively more diverse AI armies.

**Endless**

- The goal of this game mode is to test the players against a scaling enemy AI. The player will observe that the enemy AI's army infinitely regenerates after each "wave" is defeated.
- Each time the player defeats a "wave" of AI units, a new "wave" will enter the field after the player advances the game state. After the "wave" is defeated, the player may place new units in a fashion that is a combination of both unit placement before the game starts and army selection prior to transitioning to the game board.
- As the "waves" progress, new units will generate, in addition to more powerful, promoted variants of these units until the player is eventually overwhelmed.

**Online Multiplayer**

- The goal of this game mode is for players to play against each other anywhere in the world in a 1 versus 1 fashion. Once players select the Multiplayer option from the Main Menu, they will log into Photon servers. If Photon appears to be unresponsive, try restarting the game first before reporting an issue. Players will know they are logged in if they are able to enter a name into the Multiplayer Menu's text input field near the top. Players will be able to either Host a game or Join a game. When a player selects the Host option, they will be taken to a new menu where they will enter a name for their lobby and then create the lobby. When a player selects the Join option, they will be able to see all joinable lobbies. Once 2 players are in a lobby, the game will be able to start. When the lobby host starts the game, they will be able to select a map and then start the match. From here, the game plays as normal.
- When a player leaves the match, the other player will also leave the match.
- When it is not a player's turn, they will still be able to observe the opponent's actions but be unable to control their units.

---

### Units

For The Crown! offers 5 unique unit types: Knight, Archer, Cleric, Horseman, and Siege.  Units may earn promotions as they engage in combat to improve their effectiveness.

**Knight**

- The game's iconic unit; designed to be sturdy with average damage, minimal attack range, average cost and average movement range.
- Special Ability: Raise your blade. Reduce damage received by **50%** for the following turn. Consumes attack.

**Archer**

- The main ranged unit; designed to be squishy with low damage, large attack range, average cost and average movement range.
- Special Ability: Fire an explosive arrow on your next attack, dealing damage in a 3x3 Area of Effect.

**Cleric**

- A healer; designed to be squishy with minimal damage, minimal attack range, high cost and average movement range. May choose to attack an enemy unit or heal a damaged ally during combat.
- Special Ability: Invoke divine power. The next heal is instead an Area of Effect around the Cleric.

**Horseman**

- A mounted unit; designed to be average health pool with average damage, minimal attack range, high cost and maximum movement range.
- Special Ability: Reach forward, temporarily increasing attack range by **1** for the remainder of turn combat.

**Siege**

- A catapult; designed to be squishy with high damage, maximum attack range, high cost and minimal movement range. **Attacks take one full turn to resolve**.
- Special Ability: Pivot the beam back even further; grants infinite attack range. **Starts on cooldown**.

---

### Structures

For The Crown! offers 3 different neutral structures that affect gameplay decisions and take up board space: the Castle (resembles a Rook from Chess), the Tower (made of wood) and the Well. There is technically a 4th structure that is simply a pile of junk. Additionally, **Ranged** units may attack through structures.

In order to occupy a structure, a player must have the greatest number of units adjacent to that structure.

**Castle**

- Once occupying this structure, the player's army receives a boost to each **Melee** unit's movement range by 1 tile.

**Tower**

- Once occupying this structure, the player's army receives a boost to each **Ranged** unit's attack range by 1 tile.

**Well**

- Once occupying this structure, all adjacent units in the occupying army receive a turn-based heal.

**Junk**

- The 4th structure. Represented by a piles of inanimate objects, this structure simply takes up space on the game board. Units must traverse around this structure.

# Personnel
---
#### Team Members
- Will Bartlett
- Eric Henderson
- Shengyu Jin
- Dylan Klingensmith
- Garrett Morse
- Longfei Yu
- Kyle Ziman

#### Instructor
- Dr. Roger Crawfis
