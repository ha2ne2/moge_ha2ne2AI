using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;


namespace ha2ne2AI {
    public class Point {
        public int x;
        public int y;
        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        // 名前が微妙
        public string Diff(Point next) {
            if (this.x - next.x == -1) {
                return "RIGHT";
            } else if (this.x - next.x == 1) {
                return "LEFT";
            } else if (this.y - next.y == -1) {
                return "DOWN";
            } else {
                return "UP";
            }
        }

        public override string ToString() {
            return new int[] { x, y }.ToString2();
        }

        public bool Equals(Point p) {
            return this.x == p.x && this.y == p.y;
        }

        public Point Copy() {
            return new Point(x, y);
        }
    }

    public class Path : IComparable {
        public Point p;
        public int x { get { return p.x; } }
        public int y { get { return p.y; } }
        public Path prev;
        public int f;
        public int g;
        public int h;
        public string targetBlock;

        public Path(Point p, int g, int h, Path prev) {
            this.p = p;
            this.g = g;
            this.h = h;
            this.f = g + h;
            this.prev = prev;
        }

        public int CompareTo(Object obj) {
            if (obj is Path) {
                var p = (Path)obj;
                return (f > p.f) ? -1 :
                       (f == p.f) ? 0 :
                       1;
            } else {
                return 0;
            }
        }

        public Path Copy() {
            return new Path(p, g, h, prev);
        }

        public override string ToString() {
            return p.ToString();
        }

        public List<Path> ToList() {
            var result = new List<Path>();
            var path = this;
            for (; path != null; path = path.prev) {
                result.Add(path);
            }
            result.Reverse();
            
            return result;
        }

        public IEnumerable<Point> ToPointList() {
            return this.ToList().Select(path => path.p);
        }
    }

    public static class PathFinder {
        public static Path AStarSearch(
            Path start_path,
            Predicate<Path> goal_p,
            Func<Path, IEnumerable<Path>> successor){

            var openList = new PriorityQueue<Path>(1000, 1);
            var closeList = new Dictionary<string, bool>();

            openList.Push(start_path);

            while (openList.Count() != 0) {
                var path = openList.Pop();
                var id = path.ToString();

                if (closeList.ContainsKey(id))
                    continue;

                closeList[id] = true;

                if (goal_p(path)) {
                    return path;
                } else {
                    successor(path).ForEach(p1 => openList.Push(p1));
                }
            }

            throw new Exception("パスが見つかりませんでした");
        }
    }

    public class PriorityQueue<T> where T : IComparable {
        private IComparer<T> _comparer = null;
        private int _type = 0;

        private T[] _heap;
        private int _sz = 0;

        private int _count = 0;

        /// <summary>
        /// Priority Queue with custom comparer
        /// </summary>
        public PriorityQueue(int maxSize, IComparer<T> comparer) {
            _heap = new T[maxSize];
            _comparer = comparer;
        }

        /// <summary>
        /// Priority queue
        /// </summary>
        /// <param name="maxSize">max size</param>
        /// <param name="type">0: asc, 1:desc</param>
        public PriorityQueue(int maxSize, int type = 0) {
            _heap = new T[maxSize];
            _type = type;
        }

        private int Compare(T x, T y) {
            if (_comparer != null)
                return _comparer.Compare(x, y);
            return _type == 0 ? x.CompareTo(y) : y.CompareTo(x);
        }

        public void Push(T x) {
            _count++;

            //node number
            var i = _sz++;

            while (i > 0) {
                //parent node number
                var p = (i - 1) / 2;

                if (Compare(_heap[p], x) <= 0)
                    break;

                _heap[i] = _heap[p];
                i = p;
            }

            _heap[i] = x;
        }

        public T Pop() {
            _count--;

            T ret = _heap[0];
            T x = _heap[--_sz];

            int i = 0;
            while (i * 2 + 1 < _sz) {
                //children
                int a = i * 2 + 1;
                int b = i * 2 + 2;

                if (b < _sz && Compare(_heap[b], _heap[a]) < 0)
                    a = b;

                if (Compare(_heap[a], x) >= 0)
                    break;

                _heap[i] = _heap[a];
                i = a;
            }

            _heap[i] = x;

            return ret;
        }

        public int Count() {
            return _count;
        }

        public T Peek() {
            return _heap[0];
        }

        public bool Contains(T x) {
            for (int i = 0; i < _sz; i++)
                if (x.Equals(_heap[i]))
                    return true;
            return false;
        }

        public void Clear() {
            while (this.Count() > 0)
                this.Pop();
        }

        public IEnumerator<T> GetEnumerator() {
            var ret = new List<T>();

            while (this.Count() > 0) {
                ret.Add(this.Pop());
            }

            foreach (var r in ret) {
                this.Push(r);
                yield return r;
            }
        }

        public T[] ToArray() {
            T[] array = new T[_sz];
            int i = 0;

            foreach (var r in this) {
                array[i++] = r;
            }

            return array;
        }
    }
}
//public class Point {
//    public int x { get; }
//    public int y { get; }

//    public Point(int x, int y) {
//        this.x = x;
//        this.y = y;
//    }

//    public bool Equals(Point p) {
//        return this.x == p.x && this.y == p.y;
//    }

//    public string Diff(Point next) {
//        if (this.x - next.x == -1) {
//            return "RIGHT";
//        } else if (this.x - next.x == 1) {
//            return "LEFT";
//        } else if (this.y - next.y == -1) {
//            return "DOWN";
//        } else {
//            return "UP";
//        }
//    }
//}


//public class Path {
//    public Point pos;
//    public Path prev;
//    public int f;
//    public int g;
//    public int h;

//    public int x { get { return pos.x; } }
//    public int y { get { return pos.y; } }

//    public Path(Point cur_pos, int g, int h, Path prev) {
//        this.pos = cur_pos;
//        this.prev = prev;
//        this.g = g;
//        this.h = h;
//        this.f = g + h;
//    }

//    public bool Equals(Path p) {
//        return this.pos == p.pos;
//    }

//    public static IList<Path> PathToList(Path path) {
//        var result = new List<Path>();
//        while(path.prev != null) {
//            result.Add(path.prev);
//        }
//        return result;
//    }
//}

//public static class PathFinder {
//    public static bool IsBetterPath(Path a, Path b) {
//        return a.f < b.f;
//    }

//    private static Func<T, bool> complement<T>(Func<T, bool> pred) {
//        return (x) => !pred(x);
//    }
    
//    private static IList<Path> BetterPaths(IList<Path>[] xs) {
//        var result = new List<Path>();
//        for (int i = 0; i< xs[0].Count(); i++) {
//            result.Add(xs[0][i].f < xs[1][i].f ? xs[0][i] : xs[1][i]);
//        }
//        return result;
//    }
    
//    public static Path AStarSearch(
//        IList<Path> openList,
//        IList<Path> closeList,
//        Predicate<Path> goal_p,
//        Func<Point,IList<Point>> successor,
//        Func<Point, int> cost_fn) {
//        Console.Error.WriteLine("HERE");
//        Console.Error.WriteLine("HERE2");

//        if (openList.IsEmpty()) {
//            Console.Error.WriteLine("openlist is empty");
//            return null;
//        } else {
//            Console.Error.WriteLine("HERE2");
//            var path = openList.OrderBy(p => p.f).First();
//            openList.Remove(path);
//            Console.Error.WriteLine("HERE3");
//            if (goal_p(path)) {
//                return path;
//            } else {
//                Console.Error.WriteLine("HERE4");
//                closeList.Add(path);
//                if (path.g < 15) {
//                    // なんか汚いなぁ。。。
//                    successor(path.pos).Select(new_pos => new Path(new_pos, path.g + 1, cost_fn(new_pos), path))
//                        .ForEach(p => {
//                            Console.Error.WriteLine("HERE  !!!");
//                            var old = openList.Where(p2 => p == p2).FirstOrDefault();
//                            var old2 = closeList.Where(p2 => p == p2).FirstOrDefault();
//                            if (old != null) {
//                                if (IsBetterPath(p, old)) {
//                                    openList.Remove(old);
//                                    openList.Add(p);
//                                }
//                            } else if (old2 != null) {
//                                if (IsBetterPath(p, old)) {
//                                    closeList.Remove(old);
//                                    openList.Add(p);
//                                }
//                            } else {
//                                openList.Add(p);
//                            }
//                        });
//                    return AStarSearch(openList, closeList, goal_p, successor, cost_fn);
//                }
//            }
//        }
//        return null;
//    }
//}


////def astar(open_list, close_list, goal_p, successor, cost_fn)
////  if (open_list.empty?)
////    nil
////  else
////    path = open_list.sort_by!(&:f).shift
////    if (goal_p.(path.value))
////      path
////    else
////      close_list.push(path)
////      if path.g< 15
////        successor.(path.value).each{ |val|
////        path2 = Path.new(value: val, g: path.g+1, h: cost_fn.(val),
////                         prev: path)
////        old = nil
////        if (old = open_list.find{|old| old == path2})
////          if (better_path?(path2, old))
////            open_list.delete(old)
////            open_list.push(path2)
////          end
////        elsif(old = close_list.find{| old | old == path2})
////          if (better_path?(path2, old))
////            close_list.delete(old)
////            open_list.push(path2)
////          end
////        else
////          open_list.push(path2)
////        end
////        }
////      end
////      astar(open_list, close_list, goal_p, successor, cost_fn)
////    end
////  end
////end

////def path_to_list(path)
////  if path.nil?
////    return []
////end
////  result = []
////  rec = ->(path){
////    result.push(path.value)
////    if (path.prev)
////      rec.(path.prev)
////    end
////  }
////  rec.(path)
////  result.reverse
////end