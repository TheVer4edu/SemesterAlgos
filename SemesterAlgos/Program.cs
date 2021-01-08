using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

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
            
            ProgramCLI cli;
            if (args.Length > 0) {
                cli = new ProgramCLI(args.First(x => x.EndsWith(".cg")));
                cli.ExecuteFile();
            }
            else
                cli = new ProgramCLI();
            cli.Run();
        }
    }
    class FileReader : IEnumerable<string> {
        private string fileName;
        private StreamReader sr;

        public FileReader(string fileName) {
            this.fileName = fileName;
        }

        public FileReader OpenFile() {
            if (fileName == null || !File.Exists(fileName)) {
                Console.WriteLine($"File {fileName} not found");
            }
            else {
                try {
                    sr = new StreamReader(fileName);
                }
                catch (Exception e) {
                    Console.WriteLine($"Unable to access file {fileName}");
                }
            }

            return this;
        }

        public IEnumerator<string> GetEnumerator()
        {
            string line;
            if(sr == null) yield break;
            while ((line = sr.ReadLine()) != null) {
                string material = line.Trim(' ');
                if(material.Length == 0 || material[0] == '#') continue;
                if (material.Contains('#'))
                    material = material.Substring(0, material.IndexOf('#', StringComparison.Ordinal));
                yield return material;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    public class ProgramCLI {
        private Graph<string> aeroflotMap = new Graph<string>();
        private List<Graph<string>> bestRoutes = new List<Graph<string>>();
        private FileReader _fileReader;

        public ProgramCLI() { }

        public ProgramCLI(string filename) {
            _fileReader = new FileReader(filename);
        }

        public int ExecuteFile() {
            if (_fileReader == null)
                return -1;
            foreach (string command in _fileReader.OpenFile())
                this.PerformCommand(command);
            return 0;
        }
        
        public void Run() {
            Console.WriteLine("Вывести справку по командам - команда 0");
            bool isRunning = true;
            while (isRunning)
            {
                Console.Write("Введите команду: ");
                string input = Console.ReadLine();
                int result = PerformCommand(input);
                isRunning = result != int.MinValue;
            }
        }

        public int PerformCommand(string input) {
            if (input.Length == 0) return -1;
            if (int.TryParse(input[0].ToString(), out int result))
            {
                string[] data = input.Split(' ').Where(x => x != "").ToArray();
                switch (result)
                {
                    case 0:
                        if (data.Length < 2)
                            ShowManual();
                        else
                            ShowManual(data?[1]);
                        break;
                    case 1:
                        if(data.Length < 2) { 
                            ShowMistakeMessage();
                            return -1;
                        }
                        string value = String.Join(" ", data.Skip(1).ToArray());
                        aeroflotMap.AddNode(new Node<string>(value));
                        Console.WriteLine($"Добавлено: {value}");
                        break;
                    case 2:
                        if(data?.Length != 4) {
                            ShowMistakeMessage();
                            return -1;
                        }
                        AddElementToGraph(data[1], data[2], data[3]);
                        break;
                    case 3:
                        if(data.Length == 2 && int.TryParse(data[1], out int removableIndex)) 
                            aeroflotMap.Delete(removableIndex);
                        else {
                            ShowMistakeMessage();
                            return -1;
                        }
                        Console.WriteLine($"Нас покинул: {data[1]}");
                        break;
                    case 4:
                        switch (data.Length) {
                            case 1:
                                FindRoute();
                                break;
                            case 2:
                                FindRoute(mode: data[1]);
                                break;
                            case 3: 
                                FindRoute(data[1], data[2]);
                                break;
                            case 4:
                                FindRoute(data[1], data[2], data[3]);
                                break;
                            case 5:
                                FindRoute(data[1], data[2], data[3], data[4]);
                                break;
                            case 6:
                                FindRoute(data[1], data[2], data[3], data[4], data[5]);
                                break;
                            default:
                                ShowMistakeMessage();
                                break;
                        }
                        break;
                    case 7:
                        if(data?.Length != 2)
                            Console.WriteLine("Укажите способ вывода графа на экран вторым параметром\n" +
                                              "Подробнее: команда 0 7");
                        else switch (data[1]) {
                            case "1":
                                aeroflotMap.AwesomePrint();
                                break;
                            case "2":
                                aeroflotMap.SimplePrint();
                                break;
                            default:
                                Console.WriteLine("Другого способа нет");
                                break;
                        }
                        break;
                    case 8:
                        Console.Clear();
                        break;
                    case 9:
                        Console.WriteLine("Покедова :)");
                        return int.MinValue;
                    default:
                        Console.WriteLine("Казалось бы всё хорошо, но ты всё равно что-то делаешь не так");
                        break;
                }
            }
            else
                Console.WriteLine("Не прокатило. Проверьте, что всё делаете правильно");
            return result;
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
                                          "4: Работа с маршрутами\n" +
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
                    case 4:
                        Console.WriteLine("4: То же, что и '4 1' \n" +
                                          "4 [-1] [a] [b]: Найти полные маршруты из a в b и зарегистрировать их. ВНИМАНИЕ! ЭТОТ МЕТОД НЕОПТИМАЛЕН! ИСПОЛЬЗУЙТЕ '4 0'\n" +
                                          "4 [0] [a] [b]: Оптимальный поиск полных маршрутов из a в b и их регистрация\n" +
                                          "4 [0] [a] [b] [l]: Найти полные маршруты из a в b до l проб и зарегистрировать их\n" +
                                          "4 [0] [a] [b] [l] [f]: Найти маршруты из a в b до l проб и зарегистрировать их. Если f не равно нулю, будут обработаны только полные пути\n" +
                                          "4 [1]: Вывести номера зарегистрированных маршрутов\n" +
                                          "4 [2] [n]: Отрисовать маршрут с индексом n\n" +
                                          "4 [3] [n]: Вывести текстовое обозначение маршрута n");
                        break;
                    case 7:
                        Console.WriteLine("7: Вывод графа на экран\n" +
                                          "\t7 [1]: Вывод графа в псевдографическом виде\n" +
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

        private void FindRoute(string mode = "1", string index = "-1", string index2 = "-1", string count = "-1", string visitAll = "1") {
            if (int.TryParse(mode, out int imode) &&
                int.TryParse(index, out int iindex) && 
                int.TryParse(index2, out int iindex2) && 
                int.TryParse(count, out int icount) && 
                int.TryParse(visitAll, out int ivisitAll)) {
                Stopwatch sw = new Stopwatch();
                switch (imode) {
                    case -1:
                        sw.Start();
                        bestRoutes = aeroflotMap.FindBestRoutes(iindex, iindex2); //неоптимальный алгоритм   
                        sw.Stop();
                        Console.Clear();
                        Console.WriteLine($"[{sw.ElapsedMilliseconds / 1000d}s]Найдено маршрутов из {iindex} в {iindex2}: {bestRoutes.Count}");
                        sw.Reset();
                        break;
                    case 0:
                        sw.Start();
                        bestRoutes = aeroflotMap.FindBestRoutes(iindex, iindex2, ivisitAll != 0, icount); //оптимальный алгоритм   
                        sw.Stop();
                        Console.Clear();
                        Console.WriteLine($"[{sw.ElapsedMilliseconds / 1000d}s]Найдено маршрутов из {iindex} в {iindex2}: {bestRoutes.Count}");
                        sw.Reset();
                        break;
                    case 1:
                        if(bestRoutes.Count > 0) 
                            Console.WriteLine($"Существуют маршруты, диапазон [0; {bestRoutes.Count - 1}]");
                        else 
                            Console.WriteLine("Таких маршрутов не бывает");
                        break;
                    case 2:
                        if(iindex < bestRoutes.Count)
                            bestRoutes[iindex].AwesomePrint();
                        else
                            Console.WriteLine("Такого маршрута ещё не придумали");
                        break;
                    case 3:
                        if(iindex < bestRoutes.Count)
                            bestRoutes[iindex].SimplePrint();
                        else
                            Console.WriteLine("Такого маршрута ещё не придумали");
                        break;
                }
            } 
        }

        private void ShowMistakeMessage() {
            Console.WriteLine("Шалишь... Учи матчасть перед использованием...");
        }
    }
    public class GraphPrinter {
        public static void DrawEdgeLine(Point from, Point to, char symbol, bool doubleSide = false) {
            Point start = from;
            int dx = Math.Abs(to.X - from.X), sx = from.X < to.X ? 1 : -1;
            int dy = Math.Abs(to.Y - from.Y), sy = from.Y < to.Y ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            while(true) {
                Console.SetCursorPosition(from.X, from.Y);
                if (Math.Abs(from.X - to.X) == 1 || Math.Abs(from.Y - to.Y) == 1) Console.ForegroundColor = ConsoleColor.Red;
                if (doubleSide && (Math.Abs(start.X - from.X) == 1 || Math.Abs(start.Y - from.Y) == 1)) Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(symbol);
                Console.ResetColor();
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
        public static List<Point> GetConsoleCoords(int cornersCount, int flipAngle = 0) {
            double bestRadius = Console.WindowHeight - 2;
            Point resolutionFactor = new Point(2, 1);
            var e = GetNodesCoords(cornersCount, bestRadius, flipAngle);
            return e.Select(p => new Point(p.X * resolutionFactor.X, p.Y * resolutionFactor.Y)).ToList();
        } //Координаты для консолей с учётом их разрешения
        public static List<Point> GetNodesCoords(int cornersCount, double radius, int flipAngle = 0) {
            List<Point> result = new List<Point>();
            result.Add(new Point((int) radius/2, 0));
            for (int i = 1; i < cornersCount; i++) {
                Point delta = FromPolarToDecart(GetAngleInDegrees(cornersCount)*(i-1)*2 + flipAngle,
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
    public class Graph<T> : IComparable<Graph<T>>, IEquatable<Graph<T>> where T: IComparable<T> {
        public readonly List<Node<T>> Nodes = new List<Node<T>>();
        public void SimplePrint() {
            foreach (Node<T> node in Nodes) {
                Console.WriteLine(node.ToString());
            }
        }
        public void AwesomePrint() {
            Console.Clear();
            AllocateNodesCoords(0);
            char edgeSymbol = ':';
            foreach (Node<T> node in Nodes) {
                foreach (Node<T> incidentNode in node.IncidentNodes) {
                    bool doubleConnected = node.IncidentNodes.Contains(incidentNode)
                                           && incidentNode.IncidentNodes.Contains(node);
                    GraphPrinter.DrawEdgeLine(node.ConsoleCoords, incidentNode.ConsoleCoords, edgeSymbol, doubleConnected);
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
        public void AddNode(T value) {
            this.AddNode(new Node<T>(value));
        }
        public void AddNode(Node<T> node) {
            this.Nodes.Add(node);
        }
        public int AddIncidentNode(Node<T> node, int[] incidentIndexes) {
            AddNode(node);
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
        public int DirectNodeTo(int from, int to, bool showMessage = true) {
            if (from == to)
                return -2;
            Node<T> nodeFrom, nodeTo;
            try {
                nodeFrom = Nodes.First(x => x.Index == from);
                nodeTo = Nodes.First(x => x.Index == to);
            }
            catch (Exception e) {
                return 1;
            }
            nodeFrom.IncidentNodes.Add(nodeTo);
            if(showMessage) Console.WriteLine($"Путь из \"{nodeFrom.Value.ToString()}\" в \"{nodeTo.Value.ToString()}\"");
            return 0;
        }
        private void AllocateNodesCoords(int flipAngle = 0) {
            if (Nodes.All(x => x.ConsoleCoords != Point.Empty))
                return;
            int nodesCount = Nodes.Count;
            List<Point> consoleCoords = GraphPrinter.GetConsoleCoords(nodesCount, flipAngle);
            for (int i = 0; i < nodesCount; i++) {
                Nodes[i].ConsoleCoords = consoleCoords[i];
            }
        }
        public void Delete(int index) {
            foreach (Node<T> node in Nodes)
                node.IncidentNodes.RemoveAll(x => x.Index == index);
            Nodes.RemoveAll(x => x.Index == index);
        }
        public List<int> FindSameNodes(T value) {
            List<int> result = new List<int>();
            foreach (Node<T> node in Nodes)
                if(node.Value.CompareTo(value) == 0)
                    result.Add(node.Index);
            return result;
        }
        public List<Graph<T>> FindBestRoutes(int from = 0, int to = 0, bool visitAllNodes = true, int limit = -1) {
            List<Graph<T>> result = new List<Graph<T>>();
            List<int[]> allPermutations = FindAllRoutes(from, limit).Select(x => x.ToArray()).ToList();
            if(from != to) 
                allPermutations = allPermutations.Where(x => x.Last() == to).ToList();
            double curPermutation = 0, fullCount = allPermutations.Count, percent = 0;
            foreach (int[] permutation in allPermutations) {
                percent = Math.Round(curPermutation++ / fullCount, 2) * 100d;
                Console.WriteLine($"Performing: {percent}%");
                Graph<T> graph = CreateRouteChainFromIndexes(permutation);
                var lastReal = Nodes.First(x => x.Index == graph.Nodes.Last().Index).IncidentNodes;
                if(from == to &&
                   lastReal.Any(x => x.Index == to))
                    graph.DirectNodeTo(graph.Nodes.Last().Index, from, false);
                if(!visitAllNodes || graph.Nodes.Count == this.Nodes.Count)  {
                    if(from == to && graph.Nodes.Last().IncidentNodes.Contains(graph.Nodes.First()))
                        result.Add(graph);
                    else if(from != to)
                        result.Add(graph);
                }
            }
            result = result.ToList();
            return result;
        }
        public List<Graph<T>> FindBestRoutes(int from = 0, int to = 0) {
            List<Graph<T>> result = new List<Graph<T>>();
            int[] allIndexes = Nodes.Select(x => x.Index).ToArray();
            CorrectExistingMap(allIndexes, from);
            List<int[]> allPermutations = new List<int[]>();
            MakePermutations(allIndexes, allPermutations, 1);
            if(from != to) 
                allPermutations = allPermutations.Where(x => x.Last() == to).ToList();
            Node<T> startNode = Nodes.First(x => x.Index == from);
            double curPermutation = 0, fullCount = allPermutations.Count;
            foreach (int[] permutation in allPermutations) {
                Console.WriteLine($"Performing: {Math.Round(curPermutation++ / fullCount, 2) * 100d}%");
                Graph<T> graph = CreateRouteChainFromIndexes(permutation);
                var e = Nodes.First(x => x.Index == graph.Nodes.Last().Index).IncidentNodes;
                if(from == to &&
                   e.Any(x => x.Index == to))
                    graph.DirectNodeTo(graph.Nodes.Last().Index, from);
                if(graph.Nodes.Count == Nodes.Count)  {
                    if(from == to && graph.Nodes.Last().IncidentNodes.Contains(graph.Nodes.First()))
                        result.Add(graph);
                    else if(from != to)
                        result.Add(graph);
                }
            }
            result = result.ToList();
            return result;
        }
        private Graph<T> CreateRouteChainFromIndexes(int[] indexes) {
            int from = indexes[0];
            Node<T> startNode = Nodes.First(x => x.Index == from);
            Graph<T> graph = new Graph<T>();
            graph.AddNode(startNode.Copy());
            foreach (int i in indexes) {
                if(i == from) continue;
                var last = Nodes.First(x => x.Index == graph.Nodes.Last().Index);
                var current = Nodes.First(x => x.Index == i);
                if (last.IncidentNodes.Contains(current)) {
                    graph.AddNode(current.Copy());
                    graph.DirectNodeTo(last.Index, current.Index, false);
                }
                else break;
            }
            return graph;
        }
        public List<List<int>> FindAllRoutes(int from = 0, int limit = -1) {
            List<int> visited = new List<int>();
            List<List<int>> resultMap = new List<List<int>>();
            Node<T> fromNode = Nodes.First(x => x.Index == from);
            VisitIncidents(fromNode, visited, resultMap, limit);
            return resultMap;
        }
        private void VisitIncidents(Node<T> node, List<int> visited, List<List<int>> maps, int limit = -1) {
            if(limit > -1 && maps.Count >= limit) return;
            visited.Add(node.Index);
            foreach (Node<T> incidentNode in node.IncidentNodes) {
                if(visited.Contains(incidentNode.Index))
                    continue;
                VisitIncidents(incidentNode, visited, maps, limit);
            }
            if(!maps.Any(x => isSublist(x, visited)))
                maps.Add(visited.Select(x => x).ToList());
            visited.Remove(node.Index);
        }
        private bool isSublist(List<int> parent, List<int> sub) {
            return FindSubarrayStartIndex(parent.ToArray(), sub.ToArray()) != -1;
        }
        public static int FindSubarrayStartIndex(int[] array, int[] subArray)
        {
            for (var i = 0; i < array.Length - subArray.Length + 1; i++)
                if (ContainsAtIndex(array, subArray, i))
                    return i;
            return -1;
        }
        public static bool ContainsAtIndex(int[] array, int[] subarray, int index) {
            for(int i = 0; i < Math.Min(array.Length, subarray.Length); i++) {
                if(array[index + i] != subarray[i]) return false; 	
            }
            return true;
        }
        private void CorrectExistingMap(int[] map, int from) {
            int index = Array.IndexOf(map, from);
            if(index == 0) return;
            int value = map[0];
            map[0] = map[index];
            map[index] = value;
        }
        private void MakePermutations(int[] permutation, List<int[]> allPermutations, int position) {
            if (position == permutation.Length) {
                int[] nextPermutation = new int[permutation.Length];
                Array.Copy(permutation, nextPermutation, permutation.Length);
                allPermutations.Add(nextPermutation);
                return;
            }
            for (int i = 0; i < permutation.Length; i++) {
                int index = Array.IndexOf(permutation, i, 0, position);
                if (index != -1) 
                    continue;
                permutation[position] = permutation[i];
                MakePermutations(permutation, allPermutations, position + 1);
            }
        }
        public override int GetHashCode() {
            int result = 31;
            unchecked {
                foreach (Node<T> node in Nodes) {
                    result += node.GetHashCode();
                }
            }
            return result;
        }
        public int CompareTo(Graph<T> other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }
        public bool Equals(Graph<T> other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.GetHashCode(), other.GetHashCode());
        }
    }
    public class Node<T> : ICloneable, IComparable<Node<T>> {
        private static int _maxIndex;
        public int Index { get; }
        public Point ConsoleCoords { get; set; }
        
        public readonly List<Node<T>> IncidentNodes = new List<Node<T>>();
        public T Value { get; }
        public Node(T value) {
            if(value == null) throw new ArgumentException("Value should not be null");
            Value = value;
            this.Index = _maxIndex++;
        }
        private Node(T value, int index, Point consoleCoords) {
            if(value == null) throw new ArgumentException("Value should not be null");
            Value = value;
            this.Index = index;
            this.ConsoleCoords = consoleCoords;
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{this.Index}]: \"{this.Value.ToString()}\" < ");
            sb.Append(String.Join(", ", IncidentNodes.Select(x => x.Index)));
            sb.Append(" >");
            return sb.ToString();
        }
        public object Clone() {
            return this.Copy();
        }

        public override int GetHashCode() {
            int result = 31;
            unchecked {
                result += 31 * Index;
                result += 31 * Value.ToString().Sum(x => (int) x);
                result += 31 * ConsoleCoords.X;
                result += 31 * ConsoleCoords.Y;
                result += 31 * this.ToString().Sum(x => (int) x);
            }
            return result;
        }

        public Node<T> Copy() {
            return new Node<T>(this.Value, this.Index, ConsoleCoords);
        }

        public int CompareTo(Node<T> other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }
    }
}