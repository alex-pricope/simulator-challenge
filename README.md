# Simulator challenge

## What was requested
The challenge was to create a new website/application that will
* Simulate the matches and results of a group of _4 teams_
* A team has it's own _strength_. Re-running the simulation multiple times, we should observe stronger teams on top of the summary.
  * This doesn't mean that the strong teams always win
* Players and other events are not required
* The UI is not important
  * It would be good to show the _group statistics_ and _round statistics_
* I should focus more on
  * Readable code
  * Well-structured, SOLID principles
  * Easy to understand, maintainable, easy to extend

## My solution
I picked a **webapi** solution for this, together with a basic form. The form gathers the user input and makes a _POST_ request 
to the API 

The API also exposes a swagger endpoint where we can see the request models, result JSON, etc.
This is also a good opportunity to show how I design, test, and implement a solution.

![image](https://github.com/user-attachments/assets/43f6dab3-fe19-4341-be2b-b6efac6acc69)


The API also has the following:
* Error middleware so we do not need to handle errors in the controller
* Logging enabled
* Return a custom ErrorDetails for every error (Status and Message)
* DTO separation from models
* Good documentation, versioned, swagger enabled

### Components
The solution has the following components
* The `API` described above
* `SimulationService`
  * Called from the controller, returns simulation results, given a list of teams

## SimulationService
### What does it do?
* Returns a `group result` that contains all the required information
* Takes in an array of `Team`
* Creates the groups with [round-robin]( https://medium.com/coinmonks/sports-scheduling-simplified-the-power-of-the-rotation-algorithm-in-round-robin-tournament-eedfbd3fee8e)
  * No team should play twice in a round
  * Each team should play all other teams
  * For a group of 4 teams, should be 6 matches
* Has a configurable `Draw Probability` (default 25%, double)
* Has a configurable `Goal Factor` (default 30, integer)
* Implements an interface for easy injection, mocking 

### How a simulation work
The simulation is run between `Team A` and `Team B` - each team has its own `Strength`.
The strength influences each team probability to score:

> Given 2 teams, Team A (Strength 80), Team B (Strength 40), Draw: 20%

Team A win probability (without draw): $A = \frac{Strength A}{Strength A + Strength B} = \frac{80}{80+40}=0.666$

Team B win probability (without draw): $B = \frac{Strength B}{Strength A + Strength B} = \frac{40}{80+40} = 0.333$

So the Team A probability (without draw) is `66%` and Team B is `33%`

Now, let's adjust these with the draw percentage

Team A win probability (with draw): $0.666 \times (1-draw$) = $0.666 \times 0.8 = 0.533$

Team B win probability (with draw): $0.333 \times (1-draw$) = $0.333 \times 0.8 = 0.267$

To decide what team wins, the simulator will generate a random (double) number between $[0d, 1d)$
* If the random number falls inside Team A range $[0, 0.533)$ -> `Team A wins`
* If the random number falls inside Team B range $[0.533, (0.533 + 0.267))$ -> `Team B wins`
* If the random number falls outside Team A+B range $[(0.533 + 0.267), 1)$ -> `Draw`

### How does the scoring work?
Each team's strength influences the score.
* A strong team should score more against a weaker team
* Teams that are approximate the same should keep the scoring realistic - no big score differences
* A weaker team will score less against a stronger team
* A strong team doesn't mean it always wins

To achieve this, we take the strength into the score random calculation, after one of the teams won (for example).

> Team A (Strength 80) wins against Team B (Strength 40)

If we want to achieve the above requirements, we need to use the team strength as a baseline when generating a score.
But what would be the upper limit for a strong/weak team?

A team with `80 strength` cannot score `80` goals.

We can divide this by a number and use that. Let's pick `40`

Max score for team A could be: $A = \frac{Strength A}{40} = \frac{80}{40}=2$

Max score for team B could be: $B = \frac{Strength B}{40} = \frac{40}{40}=1$

So, `decreasing` the number, for example, to `20`, will influence the scores toward `higher values`.

Max score for team A _could be_: $A = \frac{Strength A}{20} = \frac{80}{20}=4$

Max score for team B _could be_: $B = \frac{Strength B}{20} = \frac{40}{20}=2$

This is called the `Goal Factor` and it's configurable for the service. The **default value is 30**

The simulator will generate a random number between $WinScore= [1, \frac{WinnerStrength}{Factor})$ and use that as the score for the winning team.

For the losing team, it will generate a random number between $[0, Min(\frac{LoserStrength}{Factor}, WinScore))$.
This is to prevent the losing team from scoring more than the winning team and still use the factor.

On top of that, I added some extra goals to the max possible goals to make the scores a bit more realistic. 

## Libraries
I use the following NuGet libraries
* NUnit to run tests / FluentAssertions to assert
* MOQ
* Serilog

## Integration tests
`Gamebasics.Integration.Tests` contains the integration tests
* **Needs the API to run locally** and the tests to run from IDE or another terminal
* Added tests for the `/simulate/groups` endpoint. 

To run the API locally:
* ```ps
    # run the API on 5054. Navigate to the Gamebasics.Api folder
    dotnet run --urls "http://localhost:5054"
  ```

⚠️ I found that swagger UI caches (at least on my machine) the JSON result in the rendered UI, sometimes re-running the POST for simulations, 
does NOT refresh the results view with the new JSON values. Better to use the UI to see the results.

## Unit tests
* `Gamebasics.Services.Tests`
  * Unit tests for _SimulatorService_ and other components

* For the `SimulatorService` I tried to cover the basics by running at least 1000 simulations sometimes and checking the statistics.

## Error handling
The API has an error middleware where various errors are being caught and transformed into status codes. Also, I return a custom
`ErrorDetails` for these cases.

## Logging
Each API call is logged, showing how long the request took, the status code, etc. 
The logger is also injected in the SimulationService

## Simple UI element
I added a very simple html page that helps with running the simulations. For this to work we need to do the following:

#### Step 1
**Run the API locally** 
```ps
    # run the API on 5054. Navigate to the Gamebasics.Api folder
    dotnet run --urls "http://localhost:5054"
  ```
#### Step 2
Navigate to: http://localhost:5054/index.html 

#### Step 3
* The UI should have the values already present for the teams with `no optional parameters`
* Team A is strongest, Team B is medium and Team C/D are weak 
* Running a simulation should return a flattened result that contains the `group summary` and `rounds summary`
  * If we re-run the simulation many times, we can see the summary change

**Optional Parameters**:
* `DrawProbability` - between 0 and 1 (decimal). This means that 0.43 input translates to 43% probability. 
* `GoalFactor` - between 1 and 100 (integer). The higher the value, the lower to scores so we can simulate different types of games. 
* Both of these have default values in the `SimulatorService` (25% and 30)

**Results**:

![image](https://github.com/user-attachments/assets/74fb8ea0-6a1b-411e-8689-17fa1ca71d9c)

## Limitations
* The Simulation service is very far from a real simulation service, but I think this assignment is not about that
  * I tried to research and experiment a bit
  * My knowledge of probabilities is limited
* Not all code is tested—I did this to save some time
  * I tried to show how I usually write unit tests/integration tests
* I did not want to force the SimulationService to be async just to use the async-await pattern 
  * This could be done with `await Task.Run(() => RunMatchSimulation(teamA, teamB))` inside the service
  * This could be done later if needed

