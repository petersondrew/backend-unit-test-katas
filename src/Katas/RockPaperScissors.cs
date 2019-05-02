using System.Collections.Generic;

namespace Katas
{
#pragma warning disable RCS1168 // Parameter name differs from base name.

    /* Acceptance Criteria
     * 
     * Create a class that determines the winner of a round of Rock, Paper, Scissors
     * 
     * Part 1 Requirements:
     *  Paper beats rock
     *  Scissors beats paper
     *  Rock beats scissors
     *  The method should not throw exceptions
     *  
     * Part 2 New Requirements (Not yet implemented and requires refactoring):
     *  Spock smashes scissors and vaporizes rock
     *  Spock is poisoned by lizard and disproven by paper
     *  Lizard poisons Spock and eats paper
     *  Lizard is crushed by rock and decapitated by scissors
     * 
     */
    public enum Action
    {
        Rock,
        Paper,
        Scissors
    }

    public static class Result
    {
        public const string Player1Wins = "Player 1 Wins";
        public const string Player2Wins = "Player 2 Wins";
        public const string Draw = "Draw";
    }

    public interface IMatch
    {
        string Turn(Action player1, Action player2);
    }

    public class RockPaperScissorsMatch : IMatch
    {
        private static readonly Dictionary<Action, Action> Winners = new Dictionary<Action, Action>
        {
            { Action.Rock, Action.Scissors },
            { Action.Scissors, Action.Paper },
            { Action.Paper, Action.Rock },
        };

        public string Turn(Action player1Action, Action player2Action)
        {
            if (player1Action == player2Action)
                return Result.Draw;

            return Winners[player1Action] == player2Action ? Result.Player1Wins : Result.Player2Wins;
        }
    }
}
