using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ha2ne2AI {
    public class ha2ne2AI {
        public const int HEAL_THREASHOLD = 10;
        public const int GOTO_ITEM_THREASHOLD = 5*2;
        public const int HAMMER_THREASHOLD = 10;
        //public const int GET_ITEM_THREASHOLD = 10;


        public static int ManhattanDistance(Point a, Point b) {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public static int[][] JArrayToInt2dArray(JArray jarray) {
            return jarray.Select(j => j.Values<int>().ToArray()).ToArray();
        }

        public static bool IsMovable(Point p,
            Dictionary<string, bool> wall_table,
            Dictionary<string, bool> block_table) {
            return !(wall_table.ContainsKey(p.ToString()) ||
                     block_table.ContainsKey(p.ToString()));
        }

        public static IList<Point> Neighbours(Point p) {
            return new List<Point>{
                        new Point(p.x, p.y + 1),
                        new Point(p.x, p.y - 1),
                        new Point(p.x + 1, p.y),
                        new Point(p.x - 1, p.y)
                    };
        }

        public static List<Path> AddBreakAction(List<Path> path_list, string breakTarget) {
            var i = path_list.FindIndex(path => path.ToString() == breakTarget);
            if (i >= 0) {
                path_list.Insert(i, path_list[i]);
            }
            return path_list;
        }
        public static Path AddBreakAction(Path path, string breakTarget) {
            var first = path;
            //Console.Error.WriteLine($"ConvertToBreakBlockPath: {first}\noriginal: {path.ToList().Select(Util.ToString2).ToString2()}");
            for (; path != null; path = path.prev) {
                if (path.ToString() == breakTarget) {
                    first.f = first.f + 1;
                    path.prev = path.Copy();
                    break;
                }
            }
            //Console.Error.WriteLine($"converted: {first.ToList().Select(Util.ToString2).ToString2()}");
            return first;
        }

        public static Path makePath(
            Point current_pos,
            Func<Point, int> cost_fn,
            Dictionary<string, bool> wall_table,
            Dictionary<string, bool> block_table
            ) {
            Predicate<Path> goal_p = (path) => cost_fn(path.p) == 0;
            Func<Path, IEnumerable<Path>> successor = (path) =>
                Neighbours(path.p).Where(p => IsMovable(p, wall_table, block_table))
                                  .Select(point => new Path(point, path.g + 1, cost_fn(point), path));
            return PathFinder.AStarSearch(new Path(current_pos, 0, cost_fn(current_pos), null), goal_p, successor);
        }

        public static Dictionary<string,bool> makeBoolTable(IEnumerable<string> lst) {
            var table = new Dictionary<string, bool>();
            lst.ForEach(s => table[s] = true);
            return table;
        }

        static Random random;
        private IEnumerator<Point> hoge;
        private IEnumerable<Point> plan;
        private int depth = 0;

        // 壁を壊して階段を降りる時に、階段の近くにアイテムがあると
        // 階段をまたいでアイテムを採りに行こうとしておかしくなるかも
        private void mapAction(JObject data) {
            Point boss_pos = null;

            if (((JArray)data["boss"]).Count() != 0) {
                boss_pos = new Point((int)data["boss"][0][0], (int)data["boss"][0][1]);
            }

            Point current_pos = new Point((int)data["player"]["pos"]["x"], (int)data["player"]["pos"]["y"]);
            var cur_depth = (int)data["player"]["map-level"];

            if (hoge == null || !hoge.MoveNext() || depth != cur_depth) {
                depth = cur_depth;
                if (boss_pos != null) {
                    Func<Point, int> h_fn = (p) => ManhattanDistance(boss_pos, p);
                    var wall_table = makeBoolTable(JArrayToInt2dArray((JArray)data["walls"]).Select(Util.ToString2));
                    var path_to_boss = makePath(current_pos, h_fn, wall_table, new Dictionary<string, bool>());
                    plan = path_to_boss.ToPointList().Skip(1);
                    hoge = plan.GetEnumerator();
                    hoge.MoveNext();
                } else {
                    IEnumerable<string> walls = JArrayToInt2dArray((JArray)data["walls"]).Select(Util.ToString2);
                    IEnumerable<string> blocks = JArrayToInt2dArray((JArray)data["blocks"]).Select(Util.ToString2);

                    Point goal = new Point((int)data["kaidan"][0][0], (int)data["kaidan"][0][1]);
                    Func<Point, int> h_fn = (p) => ManhattanDistance(goal, p);

                    var wall_table = makeBoolTable(walls);
                    var block_table = makeBoolTable(blocks);
                    string breakTargetBlock = "";
                    Path _path = makePath(current_pos, h_fn, wall_table, block_table);
                    Console.Error.WriteLine($"_____ PATH: ({_path.ToList().Count()}) {_path.ToList().ToString2()}");

                    if ((int)data["player"]["hammer"] != 0) {
                        IList<Path> pathList = new List<Path>();
                        foreach (var b in blocks) {
                            var tmp_block_table = makeBoolTable(blocks.Where(b2 => b2 != b));
                            var path1 = makePath(current_pos, h_fn, wall_table, tmp_block_table);
                            path1.targetBlock = b;
                            pathList.Add(path1);//ConvertToBreakBlockPath(path1));
                        }
                        var break_path = pathList.OrderBy(path => path.f).First();
                        if (_path.f > break_path.f + 1 + HAMMER_THREASHOLD) {
                            breakTargetBlock = break_path.targetBlock;
                            _path = break_path;
                        }
                    }
                    Console.Error.WriteLine($"BREAK PATH: ({_path.ToList().Count()}) {_path.ToList().ToString2()}");

                    // アイテムが近くにあるなら取る 結構綺麗に実装できた。
                    block_table = makeBoolTable(blocks.Where(b2 => b2 != _path.targetBlock));
                    var start_to_item_h_functions = JArrayToInt2dArray((JArray)data["items"])
                        .Select(p => new Point(p[0], p[1]))
                        .Select(item_pos => (Func<Point, int>)((p) => ManhattanDistance(p, item_pos)))
                        .OrderBy(item_h_fn => makePath(current_pos, item_h_fn, wall_table, block_table).g);

                    var start_pos = current_pos.Copy();
                    var start_to_goal = _path;
                    List<Path> get_item_path_list = new List<Path>();
                    foreach (var fn in start_to_item_h_functions) {
                        var start_to_item = makePath(start_pos, fn, wall_table, block_table);
                        var item_to_goal = makePath(start_to_item.p, h_fn, wall_table, block_table);
                        if (start_to_goal.f + GOTO_ITEM_THREASHOLD >= start_to_item.f + item_to_goal.f) {
                            get_item_path_list.AddRange(start_to_item.ToList().Skip(1));
                            start_pos = start_to_item.p;
                            start_to_goal = item_to_goal;
                        }
                    }
                    get_item_path_list.AddRange(start_to_goal.ToList().Skip(1));
                    get_item_path_list = AddBreakAction(get_item_path_list, breakTargetBlock);
                    plan = get_item_path_list.Select(path => path.p);
                    hoge = plan.GetEnumerator();
                    hoge.MoveNext();
                }
            }
            Console.Error.WriteLine($"PATH: {plan.ToString2()}");
            Console.Error.WriteLine($"MOVE: {current_pos} TO {hoge.Current}, {current_pos.Diff(hoge.Current)}");
            Console.WriteLine(current_pos.Diff(hoge.Current));
        }

        static bool IsMonsterAlive(JToken monster) {
            return (int)monster["hp"] > 0;
        }

        // STAB MAN
        static void battleAction(JObject data) {
            var alives = ((JArray)data["monsters"]).Where(IsMonsterAlive);
            var slow_attacker =
                alives.Where(m => ((string)m["name"]).Contains("スライム") ||
                                  ((string)m["name"]).Contains("毛の薄いブリガンド")).FirstOrDefault();
            int hp1   = alives.Where(m => ((int)m["hp"]) == 1).Count();
            int target;
            if (slow_attacker != null) {
                target = (int)slow_attacker["number"];
            } else {
                target = (int)alives.OrderBy(m => m["hp"]).First()["number"];
            }
            var act =
                (hp1 > 0) ? "SWING" :
                ((int)data["player"]["hp"] < 10 && (int)data["player"]["heal"] >= 1) ? "HEAL" : $"STAB {target}";
            Console.Error.WriteLine(act);
            Console.WriteLine(act);
        }

        //{"equip":1,
        //"now":{"name":"なし","str":0,"hp":0,"agi":0},
        //"discover":{"name":"もげぞーの剣","str":1,"hp":0,"agi":0}}
        static void equipAction(JObject data) {
            var current = (int)data["now"]["str"] + (int)data["now"]["hp"] + (int)data["now"]["agi"];
            var found   = (int)data["discover"]["str"] + (int)data["discover"]["hp"] + (int)data["discover"]["agi"];

            Console.WriteLine(current < found ? "YES": "NO");
        }
        static void levelupAction(JObject data) {
            Random rand = new Random();
            Console.WriteLine(rand.Next(2) == 1 ? "HP":"AGI");
        }

        public ha2ne2AI() {
            //writer = new StreamWriter(@"log.txt", true);
        }

        public static void Main() {
            String line = "";
            //try {
                random = new Random();
                var ha2ne2AI = new ha2ne2AI();

                Console.WriteLine("ハツネツAI");

                while ((line = Console.ReadLine()) != null) {
                    Console.Error.WriteLine("------------------------------------------------------");
                    Console.Error.WriteLine(line);
                    JObject message = JObject.Parse(line);
                    var d = message as IDictionary<String, JToken>;
                    if (d.ContainsKey("map"))
                        ha2ne2AI.mapAction(message);
                    else if (d.ContainsKey("battle"))
                        battleAction(message);
                    else if (d.ContainsKey("equip"))
                        equipAction(message);
                    else if (d.ContainsKey("levelup"))
                        levelupAction(message);
                    else {
                        Console.Error.WriteLine("Invalid input: {0}", line);
                        Environment.Exit(1);
                    }
                }
            //}
            //catch (Exception e) {
            //    using (StreamWriter sw = new StreamWriter("EXCEPTION.txt"))
            //    {
            //        sw.WriteLine(e.Message);
            //        sw.WriteLine(line);
            //    }
            //}
        }
    }
}