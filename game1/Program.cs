using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;



namespace Game
{
    public class MenuTest
    {
        string Menu()
        {
            Console.Clear();
            Console.WriteLine("=== Главное меню ===");
            Console.WriteLine("1. Новая игра");
            Console.WriteLine("2. Найстройки игры");
            Console.WriteLine("3. Выход");
            Console.WriteLine("====================");
            Console.Write("Введите номер действия: ");

            string userInput = Console.ReadLine();

            // Ждем, пока игрок нажмет любую клавишу для продолжения
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return userInput;
        }
    }


    class Program
    {
        

        private const int ScreenWidth = 150;
        private const int ScreenHeight = 60;

        private const int MapWidth = 32;
        private const int MapHeight = 32;

        private const double Fov = Math.PI / 3;
        private const double Depth = 16;

        private static double _playerX = 2;
        private static double _playerY = 2;
        private static double _playerA = 0;
        private static bool OnFps;
        private static bool OnMap ;
        
        private static readonly StringBuilder Map = new StringBuilder();

        private static readonly char[] Screen = new char[ScreenWidth * ScreenHeight];
        
        public static async Task Main(string[] args)
        {
            Label:
            Console.Clear();
            
            Console.WriteLine("=== Главное меню ===");
            Console.WriteLine("1. Новая игра");
            Console.WriteLine("2. Найстройки игры");
            Console.WriteLine("3. Выход");
            Console.WriteLine("====================");
            Console.Write("Введите номер действия: ");
            

            string userInput = Console.ReadLine();
            if (userInput == "2")
            {
                OnFps = true;
                OnMap = true;
                Console.Clear();
            Label2:
                Console.WriteLine("=== Опции ===");
                Console.WriteLine("1. включить | выключить отображение карты");
                Console.WriteLine("2. Включить|выключить отображение фпс");
                Console.WriteLine("3. Назад");
                Console.WriteLine("==============");
                Console.Write("Введите номер действия: ");

                string userInputOption = Console.ReadLine();

                switch (userInputOption)
                {
                    case "1":
                        
                        OnMap = false;
                        goto Label2;
                        // Ваш код для изменения настроек звука
                        break;
                    case "2":
                        
                        OnFps = false;
                        goto Label2;
                        // Ваш код для изменения языка
                        break;
                    case "3":
                        goto Label;
                        break;
                    default:
                        Console.WriteLine("Неверный номер действия");
                        break;
                }

                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey();



            }

            if (userInput == "1")
            {
                Console.SetWindowSize(ScreenWidth, ScreenHeight);
                Console.SetBufferSize(ScreenWidth, ScreenHeight);
                Console.CursorVisible = false;


                InitMap();

                DateTime dateTimeFrom = DateTime.Now;
                 



                while (true)
                {

                    DateTime dateTimeTo = DateTime.Now;
                    double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                    dateTimeFrom = DateTime.Now;

                    if (Console.KeyAvailable)
                    {

                        ConsoleKey consoleKey = Console.ReadKey(intercept: true).Key;

                        switch (consoleKey)
                        {

                            case ConsoleKey.A:
                                _playerA += elapsedTime * 5;
                                break;
                            case ConsoleKey.D:
                                _playerA -= elapsedTime * 5;
                                break;
                            case ConsoleKey.W:
                                {
                                    _playerX += Math.Sin(_playerA) * 10 * elapsedTime;
                                    _playerY += Math.Cos(_playerA) * 10 * elapsedTime;

                                    if (Map[(int)_playerY * MapWidth + (int)_playerX] == '#')
                                    {
                                        _playerX -= Math.Sin(_playerA) * 10 * elapsedTime;
                                        _playerY -= Math.Cos(_playerA) * 10 * elapsedTime;
                                    }
                                    break;

                                }
                            case ConsoleKey.S:
                                {

                                    _playerX -= Math.Sin(_playerA) * 10 * elapsedTime;
                                    _playerY -= Math.Cos(_playerA) * 10 * elapsedTime;

                                    if (Map[(int)_playerY * MapWidth + (int)_playerX] == '#')
                                    {
                                        _playerX += Math.Sin(_playerA) * 10 * elapsedTime;
                                        _playerY += Math.Cos(_playerA) * 10 * elapsedTime;
                                    }
                                    break;

                                }

                                 
                        }
                      
                            InitMap();
                        
                        
                    }
                    //Ray casting

                    var rayCastingTasks = new List<Task<Dictionary<int, char>>>();

                    for (int x = 0; x < ScreenWidth; x++)
                    {
                        int x1 = x;
                        rayCastingTasks.Add(item: Task.Run(function: () => CastRay(x1)));
                    }

                    Dictionary<int, char>[] rays = await Task.WhenAll(rayCastingTasks);

                    foreach (Dictionary<int, char> dictionary in rays)
                    {
                        foreach (int key in dictionary.Keys)
                        {
                            Screen[key] = dictionary[key];
                        }



                    }
                    //Статс
                    if (OnFps == true)
                    {
                        Debug.WriteLine(OnFps);
                        char[] stats = $"X: {_playerX}, Y: {_playerY}, A: {_playerA}, FPS: {(int)(1 / elapsedTime)}"
                       .ToCharArray();
                        stats.CopyTo(array: Screen, index: 0);
                    }


                    //map
                    if (OnMap == true)
                    {
                        for (int x = 0; x < MapWidth; x++)
                        {
                            for (int y = 0; y < MapHeight; y++)
                            {
                                Screen[(y + 1) * ScreenWidth + x] = Map[y * MapWidth + x];
                            }
                        }
                        //player
                        

                    }
                    Screen[(int)(_playerY + 1) * ScreenWidth + (int)_playerX] = 'C';
                    Console.SetCursorPosition(left: 0, top: 0);
                    Console.Write(buffer: Screen);


                }

                
            }

           
            

            if (userInput == "3")
            {
                Console.WriteLine("Выход из игры");
                Environment.Exit(0);
            }




            }

        
      

        public static Dictionary<int, char> CastRay(int x)
        {
            var result = new Dictionary<int, char>();
            double rayAngle = _playerA + Fov / 2 - x * Fov / ScreenWidth;

            double rayX = Math.Sin(rayAngle);
            double rayY = Math.Cos(rayAngle);

            double distanceTowall = 0;
            bool hitWall = false;
            bool isBounds = false;

            while (!hitWall && distanceTowall < Depth)
            {

                distanceTowall += 0.1;

                int testX = (int)(_playerX + rayX * distanceTowall);
                int testY = (int)(_playerY + rayY * distanceTowall);

                if (testX < 0 || testX >= Depth + _playerX || testY < 0 || testY >= Depth + _playerY)
                {

                    hitWall = true;
                    distanceTowall = Depth;
                }
                else
                {
                    char testCell = Map[testY * MapWidth + testX];

                    if (testCell == '#')
                    {
                        hitWall = true;

                        var boundsVectorList = new List<(double module, double cos)>();

                        for (int tx = 0; tx < 2; tx++)
                        {
                            for (int ty = 0; ty < 2; ty++)
                            {
                                double vx = testX + tx - _playerX;
                                double vy = testY + ty - _playerY;

                                double vectormodule = Math.Sqrt(vx * vx + vy * vy);
                                double cosAngle = rayX * vx / vectormodule + rayY * vy / vectormodule;

                                boundsVectorList.Add((vectormodule, cosAngle));
                            }

                        }

                        boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                        double boundAngle = 0.03 / distanceTowall;

                        if (Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                            Math.Acos(boundsVectorList[1].cos) < boundAngle)
                            isBounds = true;
                    }
                    else
                    {
                        Map[testY * MapWidth + testX] = '0';
                    }

                }

            }

            int celling = (int)(ScreenHeight / 2d - ScreenHeight * Fov / distanceTowall);
            int floor = ScreenHeight - celling;

            char wallShade;

            if (isBounds)
                wallShade = '|';

            else if (distanceTowall <= Depth / 4d)
                wallShade = '\u2588';
            else if (distanceTowall < Depth / 3d)
                wallShade = '\u2593';
            else if (distanceTowall < Depth / 2d)
                wallShade = '\u2592';
            else if (distanceTowall < Depth)
                wallShade = '\u2591';
            else
                wallShade = ' ';

            for (int y = 0; y < ScreenHeight; y++)
            {


                if (y <= celling)
                {
                    result[y * ScreenWidth + x] = ' ';

                }
                else if (y > celling && y <= floor)
                {

                    result[y * ScreenWidth + x] = wallShade;
                }
                else
                {
                    char floorShade;
                    double b = 1 - (y - ScreenHeight / 2d) / (ScreenHeight / 2d);

                    if (b < 0.25)
                        floorShade = '@';
                    else if (b < 0.5)
                        floorShade = '/';
                    else if (b < 0.75)
                        floorShade = '-';
                    else if (b < 0.9)
                        floorShade = '.';
                    else
                        floorShade = ' ';

                    result[y * ScreenWidth + x] = floorShade;
                }
            }



            return result;

        }

        private static void InitMap()
        {
            Map.Clear();
            Map.Append("################################");
            Map.Append("...............................#");
            Map.Append("......#############...##########");
            Map.Append("#.....#.................#.....##");
            Map.Append("#....############.....###.###.##");
            Map.Append("#......#.......#.......#.....###");
            Map.Append("##########..###.#...############");
            Map.Append("#...#.......................###");
            Map.Append("#.###.###.#############.#...#.##");
            Map.Append("#.#...#...#.#...#.....#....#...#");
            Map.Append("#.#####.###.#.#.#####.#....#.#.#");
            Map.Append("#.#.....#...#...#.#............#");
            Map.Append("###.###########.#.#.#####.######");
            Map.Append("#.....#.....#...#...#.....#....#");
            Map.Append("#.#####.#####.#####.#########..#");
            Map.Append("#.#...#.#.....#.....#...#....###");
            Map.Append("###.#.#.#.#########.#.###....###");
            Map.Append("#...#.#.#...........#...#...####");
            Map.Append("#.#.#.#########.#.#.###.###....#");
            Map.Append("#.#.#...#.......#.#...#...###..#");
            Map.Append("#.#####.#.#.#####.###.#.#.#...##");
            Map.Append("#.......#.#.#.......#.#.#.#....#");
            Map.Append("#.#######.#.#####.#.#.#.#.#....#");
            Map.Append("#.#.....#...#...#.#.#.#.########");
            Map.Append("#########.###.#.#.#####.##...###");
            Map.Append("#.............#.....#.....#.####");
            Map.Append("#.###########.#####.#####.###.##");
            Map.Append("#.#.........#.............#...##");
            Map.Append("#.#########.#########.###.###.##");
            Map.Append("#...........#..................#");
            Map.Append("#######..#################..####");
            Map.Append("###......#################..####");
            Map.Append("#####................#####......");


        }

    }
}