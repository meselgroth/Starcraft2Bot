Starting as a basic console app to connect via websocket to SC2 and start new game. Evolving to a human managed bot, to displaying intel and aggregated info, to eventually a data driven bot.

Uses Grpc.Tools to auto generate c# gRPC classes and connects over websocket sending binary gRPC messages.

Proto files are downloaded from https://github.com/Blizzard/s2client-proto/tree/master/s2clientprotocol
A future improvement would be a git link to ensure latest are always used.

# Object Model
## SCV manager
Build scvs until 85
Add to Gas
(Later ensure base saturation)

## Connection service
Always read messages, synchronise/Marshall info to main thread
Send a request

## Game starter
Create game from map, as players, join game

## Game
Orchestrate managers/queues

## Build queue
Prioritised list of things to build while waiting for resources

	* Scvs while below 85
	* Intitial build order
	* Army/upgrades
	* Expansions


### Reprio
	* Attack/rush
	* Enemy intel

## Building manager
## Army manager ( defence positions / attack positions)
## Scout manager
## Intel manager
## Expansion manager
## Army composition manager

## MVP: 1 base, 1 defensive army
Build queue: SCV/depots, rax, marines
SCV manager, building manager, army manager

## RC1
Attack move at army count

## Rc2
Expand until I press attack

# Solution Architectures
Events system: being attacked, new Intel, etc

OR

Each manager is on its own loop and decision making process with Central over arching goals: expand/rush/attack
Will be easier to add machine learning (in small decision making and goal)
Can start with human input on goals
