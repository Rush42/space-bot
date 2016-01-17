# Entelect R100k Challenge (2015)
This repository contains my entry into the **2015 Entelect R100k Challenge**.

More info on the challenge can be found below.
* [http://www.htxt.co.za/2015/06/15/this-years-entelect-100k-challenge-is-all-about-ai-and-space-invaders/](http://www.htxt.co.za/2015/06/15/this-years-entelect-100k-challenge-is-all-about-ai-and-space-invaders/)
* [http://mybroadband.co.za/news/software/55383-r100k-challenge-puts-sa-programmers-to-the-test.html](http://mybroadband.co.za/news/software/55383-r100k-challenge-puts-sa-programmers-to-the-test.html)
* [http://www.entelect.co.za/News/DisplayNewsItem.aspx?niid=49806](http://www.entelect.co.za/News/DisplayNewsItem.aspx?niid=49806)

## How To Run the Game
When running the game, Player 1 will be my bot (seen at the bottom) that is contained in this repository, and Player 2 will be a sample bot.

Open the **Game Harness** folder and run **SpaceInvadersDuel.exe** to watch the two bots battle to the death!

## (Optional) Set Up From Scratch
The game has been pre-configured to run easily, but if you would like to do it yourself, you can follow these steps:

1. Download the [Game Harness](https://github.com/EntelectChallenge/2015-SpaceInvaders-TestHarness/releases/download/1.0.6/2015-TestHarness-1.0.6-Windows.zip) from Entelect. This is what will run the match. We'll drop the AI bot into this to make decisions which it will then pass on to the Harness for game execution.
2. Clone this repository and run *compile.bat* to compile the project. You will need VS 2013 or later.
3. Look for the compiled bot in either **BasicBot\bin\Debug** or **BasicBot\bin\Release**
4. Copy (Ctrl+C) the compiled bot files.
5. Open the harness that you downloaded in step one. Replace all contents in the **player1** folder with the files from the previous step (Ctrl+V)
6. Run **SpaceInvadersDuel.exe** and enjoy the match.