Starting as a basic console app to connect via websocket to SC2 and start new game. Evolving to a human managed bot, to displaying intel and aggregated info, to eventually a data driven bot.

Uses Grpc.Tools to auto generate c# gRPC classes and connects over websocket sending binary gRPC messages.

Proto files are downloaded from https://github.com/Blizzard/s2client-proto/tree/master/s2clientprotocol
A future improvement would be a git link to ensure latest are always used.

## Api
Api Project provides access to Hivemind data (intel and aggregated info). [Api folder](/Api/)

## App
App (frontend) displays api data. [App folder](/app/)

# Designing the Bot Object Model
## Game
Game.cs is global singleton process
- Runs websocket threads
- Stores global data in static variables
- Orchestrates managers/queues

## Worker manager
Build workers until 85

Add to Gas
(Later ensure base saturation)

## Connection service
Always read messages, synchronise/Marshall info to main thread
Send a request

## Game starter
Create game from map, as players, join game

## Build queue
Prioritised list of things to build while waiting for resources

	* Scvs while below 85
	* Intitial build order
	* Army/upgrades
	* Expansions


### Repriotise (later versions)
	* Attack/rush
	* Enemy intel

## Building planner
### Making the bot map aware
Break map down into plateaus (area all on same level, such as main base)
Each plateau has a grid system, each block on grid is either
 - free
 - building
 - mineral
 - reserved for pathway
 - enemy
Has a ramp/exit location

On MVP development this ^^ seemed over engineered, so the map was kept as a single 2D array.

## Army manager ( defence positions / attack positions)
## Scout manager
## Intel manager
## Expansion manager
## Army composition manager

## MVP: 1 base, 1 defensive army
Build queue: SCV/depots, rax, marines
SCV manager, building manager, army manager

## RC1 <-- This is Complete!
Attack move at army count

## Rc2
Expand until I press attack

# Solution Architectures
Events system: being attacked, new Intel, etc

OR

Each manager is on its own loop and decision making process with Central over arching goals: expand/rush/attack
Will be easier to add machine learning (in small decision making and goal)
Can start with human input on goals

The 2nd option seemed best. The current version loops every half second, receives new intel (game info) and triggers each manager. If a manager requires an action it is queued in the Build Queue.

# Useful
DebugDraw can highlight things in the game for debugging (where the bot tried to do something). It appears only one drawing is displayed and each new one clears the previous.