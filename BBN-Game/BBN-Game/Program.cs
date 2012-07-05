using System;

namespace BBN_Game
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BBNGame game = new BBNGame())
            {
                game.Run();
            }
        }
    }
}

