Keegan Rush  - CharonBot

***TO BUILD***
Builds on a Windows machine meeting the requirements of the 2015 Entelect R100k Challenge. To compile, execute compile.bat located in the root folder.

***STRATEGY***
A rather simple bot built off of the basic bot. It essentially ignores the enemy player and concentrates on shooting as many aliens as possible. When executed, it runs through a list of priorities to get the best move for the current situation.

The priorities are broken down into the following methods:

- TryDodge: attempts to move out of the way of any incoming bullets or missiles

If there is nothing to dodge, we go to:

- TryShoot: Does predictions of all alien positions in the amount of rounds it would take for a fired missile to reach them. If an alien can be shot, and it would not be more desirable to wait a few rounds in order to shoot an alien in a closer row, we shoot.

If shooting would not be desirable, we go to:

- TryBuild: Builds either a missile controller or an alien factory if the situation and location are ideal.

If building would not be desirable, we go to:

- TryPlace: Attempts to move the bot to a different position so that it does not get stuck behind shields for the entire game. Moves the bot out into the open.

If there is no reason to move the bot, we go to:

- TryShift: Attempts to move the bot into a position where it would be able to sooner shoot an alien, rather than wait in the same position for aliens to get closer.

If, after this chain of decision making, no move has been found, the bot does nothing.