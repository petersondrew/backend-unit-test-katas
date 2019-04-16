using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Katas.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var kataArgs = args.Skip(1);
            switch (args[0].ToLower())
            {
                case "fizzbuzz":
                    FizzBuzz(kataArgs);
                    break;
                case "rockpaperscissors":
                case "rps":
                case "roshambo":
                    RockPaperScissors(kataArgs);
                    break;
                default:
                    Error.WriteLine("Unknown Kata");
                    break;
            }
        }

        private static void FizzBuzz(IEnumerable<string> args)
        {
            var fizzBuzz = new FizzBuzz();
            foreach (var arg in args)
            {
                if (int.TryParse(arg, out var parsed))
                    WriteLine(fizzBuzz.Solve(parsed));
            }
        }

        private static void RockPaperScissors(IEnumerable<string> args)
        {
            var rps = new RockPaperScissorsMatch();
            foreach (var arg in args)
            {
                var turn = arg.Split(',');
                if (turn.Length != 2)
                    continue;
                if (Enum.TryParse<Action>(turn[0], true, out var player1)
                    && Enum.TryParse<Action>(turn[1], true, out var player2))
                    WriteLine(rps.Turn(player1, player2));
            }
        }
    }
}
