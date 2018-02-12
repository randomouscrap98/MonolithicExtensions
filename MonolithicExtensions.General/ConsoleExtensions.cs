using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonolithicExtensions.Portable;

namespace MonolithicExtensions.General
{
    public static class ConsoleExtensions
    {
        public class Configuration
        {
            public ConsoleColor OutputBG = ConsoleColor.Gray;
            public ConsoleColor OutputFG = ConsoleColor.Black;
            public ConsoleColor SelectBG = ConsoleColor.Green;
            public ConsoleColor SelectFG = ConsoleColor.Black;
            public ConsoleColor ClearColor = ConsoleColor.Black;

            public int ListDisplayCount = 15;
            //public Action<ConsoleKeyInfo> ProcessUnusedKey = null;
        }

        public static class FieldRequestSelections
        {
            public static List<string> Bool() { return new List<string> { "true", "false" }; }
            public static List<string> Enumeration<T>() { return Enum.GetNames(typeof(T)).ToList(); }
        }

        /// <summary>
        /// Many console operations require changing color and location at the same time; use this to accomplish
        /// that quickly.
        /// </summary>
        /// <param name="fg"></param>
        /// <param name="bg"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        public static void QuickSet(ConsoleColor fg, ConsoleColor bg, int left, int top)
        {
            SetColor(fg, bg);
            Console.SetCursorPosition(left, top);
        }

        /// <summary>
        /// Set both console colors at the same time.
        /// </summary>
        /// <param name="fg"></param>
        /// <param name="bg"></param>
        public static void SetColor(ConsoleColor fg, ConsoleColor bg)
        {
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
        }

        public static void TopCascadingDisplayFields(int top, Dictionary<string, string> fields, Configuration config = null)
        {
            var oldbg = Console.BackgroundColor;
            var oldfg = Console.ForegroundColor;
            var oldx = Console.CursorLeft;
            var oldy = Console.CursorTop;

            if (config == null) config = new Configuration();

            Console.SetCursorPosition(0, top);
            var width = fields.Select(x => x.Key.Length).Max();

            foreach(var pair in fields)
            {
                SetColor(config.OutputFG, config.OutputBG);
                Console.Write(" ");
                Console.Write(pair.Key.PadRight(width));
                Console.Write(" ");
                SetColor(config.OutputBG, config.OutputFG);
                Console.WriteLine($" {pair.Value}");
            }

            Console.ReadKey(true);

            var bottom = Console.CursorTop;
            Console.BackgroundColor = config.ClearColor;
            Console.SetCursorPosition(0, top);
            Console.Write(new string(' ', (bottom - top) * Console.WindowWidth));

            QuickSet(oldfg, oldbg, oldx, oldy);
        }

        public static Dictionary<string, string> TopCascadingRequestFields(int top, IList<string> fields, Configuration config = null)
        {
            return TopCascadingRequestFields(top, fields.ToDictionary(x => x, y => (IList<string>)null), config);
        }

        public static Dictionary<string, string> TopCascadingRequestFields(int top, IDictionary<string, IList<string>> fields, Configuration config = null)
        {
            var oldbg = Console.BackgroundColor;
            var oldfg = Console.ForegroundColor;
            var oldx = Console.CursorLeft;
            var oldy = Console.CursorTop;

            if (config == null) config = new Configuration();

            Console.SetCursorPosition(0, top);

            var result = new Dictionary<string, string>();
            var width = fields.Keys.Select(x => x.Length).Max();

            foreach (var field in fields)
            {
                SetColor(config.OutputFG, config.OutputBG);
                Console.Write(" ");
                Console.Write(field.Key.PadRight(width));
                Console.Write(" ");

                if (field.Value == null || field.Value.Count == 0)
                {
                    SetColor(config.OutputBG, config.OutputFG);
                    Console.Write(" ");
                    result.Add(field.Key, Console.ReadLine());
                }
                else
                {
                    int cursorX = Console.CursorLeft + 1;
                    int cursorY = Console.CursorTop;
                    int i = 0;
                    string selection = "";
                    int valueWidth = field.Value.Select(x => x.Length).Max();

                    while(string.IsNullOrWhiteSpace(selection))
                    {
                        QuickSet(config.SelectFG, config.SelectBG, cursorX, cursorY);
                        Console.Write($" {field.Value[i]} ");

                        if (field.Value[i].Length < valueWidth)
                        {
                            SetColor(config.ClearColor, config.ClearColor);
                            Console.Write(new string(' ', (valueWidth - field.Value[i].Length)));
                        }

                        var info = Console.ReadKey(true);

                        if (info.Key == ConsoleKey.Enter)
                            selection = field.Value[i];
                        else if ((info.Key == ConsoleKey.UpArrow || info.Key == ConsoleKey.LeftArrow) && i > 0)
                            i--;
                        else if ((info.Key == ConsoleKey.DownArrow || info.Key == ConsoleKey.RightArrow) && i < field.Value.Count - 1)
                            i++;
                    }

                    result.Add(field.Key, selection);

                    Console.WriteLine();
                }
            }

            var bottom = Console.CursorTop;
            Console.BackgroundColor = config.ClearColor;
            Console.SetCursorPosition(0, top);
            Console.Write(new string(' ', (bottom - top) * Console.WindowWidth));

            QuickSet(oldfg, oldbg, oldx, oldy);

            return result;
        }

        /// <summary>
        /// Draw the given list of items at the given page/count per page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        private static int DrawList<T>(int top, IList<Tuple<T, string>> items, int page, Configuration config, int selected = -1)
        {
            int pages = (int)Math.Ceiling((double)items.Count / config.ListDisplayCount);

            QuickSet(config.OutputFG, config.OutputBG, 0, top);

            if(items.Count == 0)
            {
                selected = -1;
                Console.Write($"No {typeof(T).Name} to display".PadRight(Console.WindowWidth));
            }
            else
            {
                Console.Write($"Page {page + 1} of {pages}".PadRight(Console.WindowWidth));
            }

            SetColor(config.OutputBG, config.OutputFG);//, 0, top);

            for (int i = 0; i < config.ListDisplayCount; i++)
            {
                if (i == selected)
                    SetColor(config.SelectFG, config.SelectBG);
                else if (i == selected + 1)
                    SetColor(config.OutputBG, config.OutputFG);

                var index = i + page * config.ListDisplayCount;
                if (index < items.Count)
                {
                    Console.Write($" {items[index].Item2}".PadRight(Console.WindowWidth));
                }
                else
                {
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }

            return top + config.ListDisplayCount + 1;
        }

        /// <summary>
        /// A menu drawn at <paramref name="top"/> units from the top of the console. Menus can be created recursively
        /// to 'cascade' down. Menus are NOT redrawn: do not clear the console while using this menu system.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="output"></param>
        /// <param name="options"></param>
        public static string TopCascadingListMenu<T>(int top, string output, IDictionary<string, Func<int, T, bool>> options,
            IList<Tuple<T, string>> listItems, Configuration config = null) //where T : null //where T : default()
        {
            var oldbg = Console.BackgroundColor;
            var oldfg = Console.ForegroundColor;
            var oldx = Console.CursorLeft;
            var oldy = Console.CursorTop;

            if (config == null) config = new Configuration();

            int realTop = top;
            int page = 0;
            int selected = 0;
            bool hasList = listItems != null;

            if (listItems == null)
                listItems = new List<Tuple<T, string>>();

            int pages = (int)Math.Ceiling((double)listItems.Count / config.ListDisplayCount);

            if (hasList) realTop = DrawList(top, listItems, page, config, selected);

            QuickSet(config.OutputFG, config.OutputBG, 0, realTop);

            //Make sure output menu title text is broken up nicely (not in the middle of a word) AND
            //that the output takes up entire lines (so backgrounds are set accordingly)
            var lines = output.AutoWordWrap(Console.WindowWidth, "\r").Split("\r".ToCharArray());
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
                Console.Write(line.PadRight(Console.WindowWidth));

            SetColor(config.OutputBG, config.OutputFG);

            var runOptions = new Dictionary<char, Tuple<string, int, int, Func<int, T, bool>>>();
            var runMatching = new Dictionary<char, string>();

            //Now output the menu options and calculate which character needs to be pressed to
            //access each menu option. Furthermore, store some extra metadata for each option so we
            //can redraw parts of the menu for selection.
            foreach (var pair in options)
            {
                bool found = false;

                if (Console.CursorLeft + pair.Key.Length + 2 > Console.WindowWidth)
                    Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));

                foreach (char c in pair.Key)
                {
                    var key = c.ToString().ToLower().First();
                    if (!found && !runOptions.ContainsKey(key))
                    {
                        found = true;
                        var identifier = $"[{c}]";
                        Console.Write(identifier);
                        runOptions.Add(key, Tuple.Create(identifier, Console.CursorLeft - 3, Console.CursorTop, pair.Value));
                        runMatching.Add(key, pair.Key);
                    }
                    else
                    {
                        Console.Write(c);
                    }
                }

                Console.Write(' ');

                if (!found) throw new InvalidOperationException($"Cannot create menu! Option {pair.Value} has no usable letters!");
            }

            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
            var bottom = Console.CursorTop;
            string result = "";

            while (true)
            {
                T selectedItem = hasList && listItems.Count > selected && selected >= 0 ? listItems[selected].Item1 : default(T);

                var key = Console.ReadKey(true);

                if (runOptions.ContainsKey(key.KeyChar))
                {
                    var option = runOptions[key.KeyChar];
                    QuickSet(config.SelectFG, config.SelectBG, option.Item2, option.Item3);
                    Console.Write(option.Item1);
                    QuickSet(oldfg, oldbg, 0, bottom);
                    if (option.Item4(bottom, selectedItem))
                    {
                        result = runMatching[key.KeyChar];
                        break;
                    }
                    QuickSet(config.OutputBG, config.OutputFG, option.Item2, option.Item3);
                    Console.Write(option.Item1);
                    Console.SetCursorPosition(0, bottom);
                }
                else if (hasList && listItems.Count > 0)
                {
                    bool redrawList = true;

                    if (key.Key == ConsoleKey.DownArrow && selected < config.ListDisplayCount - 1)
                        selected++;// = (selected + 1) % config.ListDisplayCount;
                    else if (key.Key == ConsoleKey.UpArrow && selected > 0)//selected < config.ListDisplayCount - 1)
                        selected--;// = (config.ListDisplayCount + selected - 1) % config.ListDisplayCount;
                    else if (key.Key == ConsoleKey.RightArrow && page < pages - 1)
                        page++;// = (page + 1) % pages;
                    else if (key.Key == ConsoleKey.LeftArrow && page > 0)
                        page--;// = (pages + page - 1) % pages;
                    else
                        redrawList = false;

                    if (selected + page * config.ListDisplayCount >= listItems.Count)
                        selected = (listItems.Count % config.ListDisplayCount) - 1;

                    if (redrawList)
                        DrawList(top, listItems, page, config, selected);
                }
            }

            Console.BackgroundColor = config.ClearColor;
            Console.SetCursorPosition(0, top);
            Console.Write(new string(' ', (bottom - top) * Console.WindowWidth));

            QuickSet(oldfg, oldbg, oldx, oldy);
            return result;
        }

        public static string TopCascadingMenu(int top, string output, IDictionary<string, Func<int, bool>> options,
            Configuration config = null) //where T : default()
        {
            return TopCascadingListMenu(top, output,
                options.ToDictionary(x => x.Key, x => new Func<int, object, bool>((t, o) => x.Value(t))),
                null, config);
        }

        public static bool TopCascadingConfirm(int top, string output, Configuration config = null)
        {
            return TopCascadingMenu(top, output, new Dictionary<string, Func<int, bool>>() {
                { "Yes", (i) => true },
                { "No", (i) => true}
            }, config) == "Yes";
        }
        //public static void TopListMenu<T>(int top, string output, Dictionary<T, string> items, int displayCount,
        //    Dictionary<string, Func<int, T, bool>> menuOptions, MenuConfiguration config = null)
        //{
        //    if (config == null)
        //        config = new MenuConfiguration();
        //}
    }
}
