using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace SemesterAlgos {
    class Program {
        static void Main(string[] args) {
            /*
             * Владелец премиальной карты «Аэрофлот» для поддержания своего статуса должен совершить за неделю
             * все возможные на данный период перелеты по городам России (кроме Москвы и Санкт-Петербурга).
             * Причем ему надо посетить и такие «непопулярные» города,
             * для которых может быть рейс Екатеринбург – Петропавловск-Камчатский,
             * но не быть обратного рейса. А поскольку наш клиент проживает в Челябинске,
             * то начнет и закончит свой маршрут он в Челябинске.
             */
            /*Graph<string> aeroflotMap = new Graph<string>();
            aeroflotMap.Nodes.Add(new Node<string>("Москва")); //0
            aeroflotMap.AddIncidentNode(new Node<string>("Санкт-Петербург"), new []{0}); //1
            aeroflotMap.AddIncidentNode(new Node<string>("Екатеринбург"), new []{0}); //2
            aeroflotMap.AddIncidentNode(new Node<string>("Челябинск"), new []{2}); //3
            aeroflotMap.AddIncidentNode(new Node<string>("Омск"), new []{2}); //4 
            aeroflotMap.AddIncidentNode(new Node<string>("Курган"), new []{2, 3}); //5
            aeroflotMap.AddIncidentNode(new Node<string>("Петропавловск-Камчатский"), new []{4, 10}); //6
            aeroflotMap.Nodes.Add(new Node<string>("Крым")); //7
            aeroflotMap.DirectNodeTo(7, 0);
            aeroflotMap.AwesomePrint();
            Console.ReadKey();
            Console.Clear();
            aeroflotMap.SimplePrint();
            Console.ReadKey();*/
            ProgramCLI cli;
            if (args.Length > 0)
                cli = new ProgramCLI(args.First(x => x.EndsWith(".cg")));
            else
                cli = new ProgramCLI();
            cli.Run();
        }
    }

    class FileReader {
        public FileReader(string filename) {
            //вот тут можно и поспать...
            //TODO реализовать чтение файла и придумать формат
        }
    }
    
    public class ProgramCLI {
        private Graph<string> aeroflotMap = new Graph<string>();

        public ProgramCLI() { }

        public ProgramCLI(string filename) {
            //TODO new FileReader?
        }
        
        public void Run() {
            Console.WriteLine("Вывести справку по командам - команда 0");
            bool isRunning = true;
            while (isRunning)
            {
                Console.Write("Введите команду: ");
                string input = Console.ReadLine();
                int result = 0;
                if (int.TryParse(input?[0].ToString(), out result))
                {
                    string[] data = input?.Split(' ');
                    switch (result)
                    {
                        case 0:
                            if (data.Length < 2)
                                ShowManual();
                            else
                                ShowManual(data[1]);
                            break;
                        case 1:
                            if(data.Length < 2) { 
                                ShowMistakeMessage();
                                continue;
                            }
                            aeroflotMap.Nodes.Add(new Node<string>(data[1]));
                            Console.WriteLine($"Добавлено: {data[1]}");
                            break;
                        case 2:
                            if(data.Length < 4) {
                                ShowMistakeMessage();
                                continue;
                            }
                            AddElementToGraph(data[1], data[2], data[3]);
                            break;
                        case 3:
                            if(data.Length < 2) {
                                ShowMistakeMessage();
                                continue;
                            }
                            aeroflotMap.Delete(data[1]);
                            Console.WriteLine($"Нас покинул: {data[1]}");
                            break;
                        case 7:
                            if(data.Length < 2) {
                                Console.WriteLine("Укажите способ вывода графа на экран вторым параметром\n" +
                                                  "Подробнее: команда 0 7");
                            } else if (data[1] == "1")
                                aeroflotMap.AwesomePrint();
                            else if (data[1] == "2")
                                aeroflotMap.SimplePrint();
                            else 
                                Console.WriteLine("Другого способа нет");
                            break;
                        case 8:
                            Console.Clear();
                            break;
                        case 9:
                            isRunning = false;
                            Console.WriteLine("Покедова :)");
                            break;
                        default:
                            Console.WriteLine("Казалось бы всё хорошо, но ты всё равно что-то делаешь не так");
                            break;
                    }
                }
                else
                    Console.WriteLine("Не прокатило. Проверьте, что всё делаете правильно");
            }
        }

        private void ShowManual(string s = "-1") {
            if (int.TryParse(s, out int page)) {
                switch (page) {
                    case -1:
                        Console.WriteLine("0: Вывести эту справку\n" +
                                          "\t0 [n]: Вывести справку по команде n\n" +
                                          "1 [a]: Добавить элемент\n" +
                                          "2 [m] [a] [b]: Связать элемент\n" +
                                          "3 [a]: Удалить элемент\n" +
                                          "7 [m]: Вывод графа на экран\n" +
                                          "8: Очистка консоли\n" +
                                          "9: Выход");
                        break;
                    case 1:
                        Console.WriteLine("1 [a]: Добавить элемент, не связывая его с другими");
                        break;
                    case 2:
                        Console.WriteLine("2 [m] [a] [b]: Связать элемент\n" +
                                          "\t2 [1] [a] [b]: Режим 1 - Связать индексы a и b в обе стороны (a<->b)\n" +
                                          "\t2 [2] [a] [b]: Режим 2 - Связать индексы из a в b (a->b)");
                        break;
                    case 3:
                        Console.WriteLine("3 [a]: Удалить элемент вместе со всеми связями, независимо от направления");
                        break;
                    case 7:
                        Console.WriteLine("7: Вывод графа на экран\n" +
                                          "\t7 [1]: Вывод графа в графическом виде\n" +
                                          "\t7 [2]: Вывод графа в текстовом виде");
                        break;
                    case 8:
                        Console.WriteLine("8: Очистить рабочее поле консоли");
                        break;
                    case 9:
                        Console.WriteLine("9: Завершение работы программы");
                        break;
                }
            }
        }

        private void AddElementToGraph(string mode, string index1, string index2) {
            if(int.TryParse(mode, out int imode) &&
                int.TryParse(index1, out int iindex1) &&
                int.TryParse(index2, out int iindex2)) {
                int result = 0;
                switch (imode) {
                    case 1:
                        result = aeroflotMap.ConnectNodesByIndex(iindex1, iindex2);
                        break;
                    case 2:
                        result = aeroflotMap.DirectNodeTo(iindex1, iindex2);
                        break;
                    default:
                        result = 1;
                        break;
                }
                if(result != 0) Console.WriteLine("Упс.. Вероятно, вы указали неверный индекс");
            }
        }

        private void ShowMistakeMessage() {
            Console.WriteLine("Шалишь... Учи матчасть перед использованием...");
        }
    }
    
    public class GraphPrinter {

        public static void PrintPoints() {
            int i = 0;
            foreach (Point point in GetConsoleCoords(5)) {
                Console.SetCursorPosition(point.X, point.Y);
                Console.Write(i++);
            }
            Console.SetCursorPosition(0, 0);
        }
        public static void DrawLine(Point from, Point to, char symbol) {
            int dx = Math.Abs(to.X - from.X), sx = from.X < to.X ? 1 : -1;
            int dy = Math.Abs(to.Y - from.Y), sy = from.Y < to.Y ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            while(true) {
                Console.SetCursorPosition(from.X, from.Y);
                Console.Write(symbol);
                if (from.X == to.X && from.Y == to.Y) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; from.X += sx; }
                if (e2 < dy) { err += dx; from.Y += sy; }
            }
        } //Отрисовка прямых по Брезенхэму
        public static Point GetConsoleCoordsForPlate(Point nodeCoord, string plate) {
            int half = plate.Length / 2;
            if (plate.Length % 2 == 0)
                return new Point(nodeCoord.X - half > 0 ? nodeCoord.X - half : 0, nodeCoord.Y);
            else
                return new Point(nodeCoord.X - half + 1 > 0 ? nodeCoord.X - half + 1 : 0, nodeCoord.Y);
        }
        public static List<Point> GetConsoleCoords(int cornersCount) {
            double bestRadius = Console.WindowHeight - 2;
            Point resolutionFactor = new Point(2, 1);
            var e = GetNodesCoords(cornersCount, bestRadius);
            return e.Select(p => new Point(p.X * resolutionFactor.X, p.Y * resolutionFactor.Y)).ToList();
        } //Координаты для консолей с учётом их разрешения
        public static List<Point> GetNodesCoords(int cornersCount, double radius) {
            List<Point> result = new List<Point>();
            result.Add(new Point((int) radius/2, 0));
            for (int i = 1; i < cornersCount; i++) {
                Point delta = FromPolarToDecart(GetAngleInDegrees(cornersCount)*(i-1)*2,
                    GetSideLength(cornersCount, radius/2));
                Point last = result.Last();
                result.Add(new Point(Math.Abs(last.X + delta.X), Math.Abs(last.Y + delta.Y)));
            }
            return result;
        } //Список точек на отрисовку
        private static Point FromPolarToDecart(double angle, double radius) {
            double x = radius * Math.Cos(GetRadians(angle));
            double y = radius * Math.Sin(GetRadians(angle));
            return new Point((int) Math.Round(x), (int) Math.Round(y));
        } //Получение расстояния через углы (полярно)
        private static double GetSideLength(int cornersCount, double radius) { //Получение длины стороны n-угольника
            return 2*radius * Math.Sin(GetRadians(GetAngleInDegrees(cornersCount)));
        }
        private static double GetAngleInDegrees(int cornersCount) { //Получение угла в градусах для n-угольника
            return 180.0 / cornersCount;
        }
        private static double GetRadians(double degrees) {
            return (Math.PI * degrees) / 180;
        } //Перевод в радианы
        
    }

    public class Graph<T> {
        public readonly List<Node<T>> Nodes = new List<Node<T>>();

        public void SimplePrint() {
            foreach (Node<T> node in Nodes) {
                Console.WriteLine(node.ToString());
            }
        }
        public void AwesomePrint() {
            Console.Clear();
            AllocateNodesCoords();
            char edgeSymbol = '.';
            foreach (Node<T> node in Nodes) {
                foreach (Node<T> incidentNode in node.IncidentNodes) {
                    GraphPrinter.DrawLine(node.ConsoleCoords, incidentNode.ConsoleCoords, edgeSymbol);
                }
            }
            Func<Node<T>, string> format = (n) => $"[{n.Index.ToString()}]: {n.Value.ToString()}";
            foreach (Node<T> node in Nodes) {
                Point pos = GraphPrinter.GetConsoleCoordsForPlate(node.ConsoleCoords, format(node));
                Console.SetCursorPosition(pos.X, pos.Y);
                Console.Write(format(node));
            }
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write("Для прожолжения нажмите любую клавишу...");
            Console.ReadKey();
            Console.Clear();
        }
        public int AddIncidentNode(Node<T> node, int[] incidentIndexes) {
            Nodes.Add(node);
            return ConnectNodesByIndex(node.Index, incidentIndexes);
        }
        public int ConnectNodesByIndex(int first, int[] incidentIndexes) {
            int result = 0;
            foreach (int index in incidentIndexes)
                if (ConnectNodesByIndex(first, index) == 1)
                    result = 1;
            return result;
        }
        public int ConnectNodesByIndex(int first, int second) {
            return DirectNodeTo(first, second)*DirectNodeTo(second, first);
        }
        public int DirectNodeTo(int from, int to) {
            Node<T> nodeFrom, nodeTo;
            try {
                nodeFrom = Nodes.First(x => x.Index == from);
                nodeTo = Nodes.First(x => x.Index == to);
            }
            catch (Exception e) {
                return 1;
            }
            nodeFrom.IncidentNodes.Add(nodeTo);
            return 0;
        }
        private void AllocateNodesCoords() {
            int nodesCount = Nodes.Count;
            var e = GraphPrinter.GetConsoleCoords(nodesCount);
            for (int i = 0; i < nodesCount; i++) {
                Nodes[i].ConsoleCoords = e[i];
            }
        }

        public void Delete(string s) {
            throw new NotImplementedException();
        }
    }
    public class Node<T> {
        private static int _maxIndex;
        public int Index { get; }
        public Point ConsoleCoords { get; set; }
        
        public readonly List<Node<T>> IncidentNodes = new List<Node<T>>();
        public T Value { get; }
        
        public Node(T value) {
            Value = value;
            this.Index = _maxIndex++;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{this.Index}]: \"{this.Value.ToString()}\" < ");
            sb.Append(String.Join(", ", IncidentNodes.Select(x => x.Index)));
            sb.Append(" >");
            return sb.ToString();
        }
    }
}