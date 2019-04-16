namespace Katas
{
    /* Acceptance Criteria
     * 
     * Create a class that solves for FizzBuzz
     * 
     * Requirements:
     *  If a number is not evenly divisible by 3 or 5, return the number
     *  If a number is divisible by 3 and 5 evenly, return FizzBuzz
     *  If a number is divisible by only 3 evenly, return Fizz
     *  If a number is divisible by only 5 evenly, return Buzz
     *  The method should not throw exceptions
     * 
     */
    public interface IFizzBuzz
    {
        string Solve(int number);
    }
    public class FizzBuzz : IFizzBuzz
    {
        public string Solve(int number)
        {
            switch (number)
            {
                case int n when n % 3 == 0 && n % 5 == 0:
                    return "FizzBuzz";
                case int n when n % 3 == 0:
                    return "Fizz";
                case int n when n % 5 == 0:
                    return "Buzz";
                default:
                    return number.ToString();
            }
        }
    }
}
