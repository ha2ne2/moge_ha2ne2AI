using Microsoft.VisualStudio.TestTools.UnitTesting;
using ha2ne2AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ha2ne2AI.Tests {
    [TestClass()]
    public class PathFinderTests {

        [TestMethod()]
        public void ShowMap() {
            var wall_table = new Dictionary<string, bool>();
            var json = "{\"map\":1,\"player\":{\"hp\":32,\"maxhp\":49,\"str\":70,\"maxstr\":70,\"agi\":45,\"maxagi\":45,\"level\":10,\"exp\":112,\"heal\":4,\"hammer\":1,\"map-level\":100,\"buki\":[\"ラグナロク\",40,0,15],\"pos\":{\"x\":5,\"y\":9}},\"blocks\":[[10,10],[9,10],[8,10],[7,10],[6,10],[5,10],[4,10],[3,10],[2,10],[1,10],[0,10],[10,9],[9,9],[8,9],[7,9],[6,9],[4,9],[3,9],[2,9],[1,9],[0,9],[10,8],[0,8],[10,7],[9,7],[8,7],[7,7],[6,7],[4,7],[3,7],[2,7],[1,7],[0,7],[10,6],[9,6],[8,6],[7,6],[6,6],[4,6],[3,6],[2,6],[1,6],[0,6],[10,5],[9,5],[8,5],[7,5],[6,5],[4,5],[3,5],[2,5],[1,5],[0,5],[10,4],[9,4],[8,4],[7,4],[6,4],[4,4],[3,4],[2,4],[1,4],[0,4],[10,3],[9,3],[8,3],[7,3],[3,3],[2,3],[1,3],[0,3],[10,2],[9,2],[8,2],[7,2],[3,2],[2,2],[1,2],[0,2],[10,1],[9,1],[8,1],[7,1],[6,1],[5,1],[4,1],[3,1],[2,1],[1,1],[0,1],[10,0],[9,0],[8,0],[7,0],[6,0],[5,0],[4,0],[3,0],[2,0],[1,0],[0,0]],\"walls\":[],\"items\":[[1,8]],\"boss\":[[5,2]],\"kaidan\":[],\"events\":[[9,8]],\"ha2\":[]}";
            var data = JObject.Parse(json);

            Point current_pos = new Point((int)data["player"]["pos"]["x"], (int)data["player"]["pos"]["y"]);

            ha2ne2AI.JArrayToInt2dArray(((JArray)data["walls"])).Select(p => p.ToString2())
                .ForEach(s => wall_table[s] = true);

            ha2ne2AI.JArrayToInt2dArray(((JArray)data["blocks"])).Select(p => p.ToString2())
                .ForEach(s => wall_table[s] = true);

            for (int y = 0; y < 20; y++) {
                for (int x = 0; x < 20; x++) {
                    Console.Write(wall_table.ContainsKey(new int[] { x, y }.ToString2()) ? "X" : " ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod()]
        public void AStarSearchTest() {
            //var wall_table = new Dictionary<string, bool>();
            //var json = "{\"map\":1,\"player\":{\"hp\":30,\"maxhp\":30,\"str\":30,\"maxstr\":30,\"agi\":30,\"maxagi\":30,\"level\":1,\"exp\":0,\"heal\":2,\"hammer\":5,\"map-level\":1,\"buki\":[\"なし\",0,0,0],\"pos\":{\"x\":3,\"y\":5}},\"blocks\":[[6,9],[8,8],[6,8],[4,8],[2,8],[1,8],[8,7],[6,7],[4,7],[8,6],[7,6],[6,6],[4,6],[3,6],[2,6],[8,5],[4,5],[2,5],[8,4],[6,4],[5,4],[4,4],[2,4],[8,3],[4,3],[8,2],[7,2],[6,2],[4,2],[3,2],[2,2],[1,2]],\"walls\":[[10,10],[9,10],[8,10],[7,10],[6,10],[5,10],[4,10],[3,10],[2,10],[1,10],[0,10],[10,9],[0,9],[10,8],[0,8],[10,7],[0,7],[10,6],[0,6],[10,5],[0,5],[10,4],[0,4],[10,3],[0,3],[10,2],[0,2],[10,1],[0,1],[10,0],[9,0],[8,0],[7,0],[6,0],[5,0],[4,0],[3,0],[2,0],[1,0],[0,0]],\"items\":[[7,7],[1,1]],\"boss\":[],\"kaidan\":[[1,9]],\"events\":[],\"ha2\":[]}";
            //var data = JObject.Parse(json);

            //Point current_pos = new Point((int)data["player"]["pos"]["x"], (int)data["player"]["pos"]["y"]);
            //ha2ne2AI.JArrayToInt2dArray(((JArray)data["walls"])).Select(p => p.ToString2())
            //    .ForEach(s => wall_table[s] = true);

            //ha2ne2AI.JArrayToInt2dArray(((JArray)data["blocks"])).Select(p => p.ToString2())
            //    .ForEach(s => wall_table[s] = true);

            //Point goal = new Point((int)data["kaidan"][0][0], (int)data["kaidan"][0][1]);
            //Func<Point, int> cost_fn = (p) => ha2ne2AI.ManhattanDistance(goal, p);
            //Predicate<Path> goal_p = (path) => path.p == goal;
            //Func<Path, IEnumerable<Path>> successor = (path) => new List<Point>{
            //        new Point(path.x, path.y + 1),
            //        new Point(path.x, path.y - 1),
            //        new Point(path.x + 1, path.y),
            //        new Point(path.x - 1, path.y)
            //    }.Where(p => ha2ne2AI.IsMovable(p, wall_table)).Select(point => new Path(point, path.g + 1, cost_fn(path.p), path));
            //int h = cost_fn(current_pos);

            //Path.PathToList(PathFinder.AStarSearch(new Path(current_pos, 0, h, null), goal_p, successor));
        }
    }
}