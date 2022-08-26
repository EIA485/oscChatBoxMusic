public struct ConsoleHelper
{
    internal static int MMod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static int MultiChoice(params string[] args)
    {
        int index = 0;
        int length = args.Length;

        int startLeft = Console.CursorLeft;
        int startTop = Console.CursorTop;
        int start = startTop + Math.Min(startLeft, 1);

        bool visible = Console.CursorVisible;
        Console.CursorVisible = false;

        bool clean = false;
        while (true)
        {
            for (int i = 0; i < length; i++)
            {
                Console.SetCursorPosition(0, start + i);

                string s = args[i];
                if (clean)
                {
                    Console.Write(new string(' ', s.Length));
                    continue;
                }

                if (i == index)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.Write(s);
                Console.ResetColor();
            }

            if (clean)
            {
                Console.SetCursorPosition(startLeft, startTop);
                break;
            };

            ConsoleKeyInfo KeyInfo = System.Console.ReadKey(true);
            ConsoleKey Key = KeyInfo.Key;

            if (Key == ConsoleKey.Enter) clean = true;
            if (Key == ConsoleKey.UpArrow) index = MMod(index - 1, length);
            if (Key == ConsoleKey.DownArrow) index = MMod(index + 1, length);
        }

        Console.CursorVisible = visible;

        return index;
    }
}