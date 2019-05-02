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
                case "nozama":
                case "ecommerce":
                    NozamaOrder(kataArgs);
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

        private static void NozamaOrder(IEnumerable<string> args)
        {
            var cart = new ShoppingCart();
            var session = new Session(Guid.NewGuid(), cart);
            var warehouse = new Warehouse();
            warehouse.Add(1, 2);
            warehouse.Add(2, 4);
            var nozama = new Nozama(warehouse);
            try
            {
                if (!args.Any())
                {
                    Error.WriteLine("No order specified");
                    return;
                }
                foreach (var arg in args)
                {
                    var item = arg.Split(':');
                    if (item.Length != 2)
                        continue;
                    if (int.TryParse(item[0], out var itemNumber) && int.TryParse(item[1], out var quantity))
                        cart.Add(itemNumber, quantity);
                }
                WriteLine(string.Join(", ", cart.Items));
                nozama.Checkout(session);
                WriteLine("Order successful");
            }
            catch (OrderException e)
            {
                Error.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Error.WriteLine("Error parsing order: {0}", e);
            }
        }
    }
}
