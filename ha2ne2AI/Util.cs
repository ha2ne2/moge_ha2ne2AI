using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;

public static class Util { 
    
    public static int to_i(this string s) {
        return int.Parse(s);
    }

    public static int to_i(this double d) {
        return (int)d;
    }

    //public static T add<T>(T a, T b) {
    //    return a + b;
    //}

    public static int add(int a, int b) {
        return a + b;
    }

    public static string add(string a, string b) {
        return a + b;
    }

    public static string get(this Match m, String name) {
        return m.Groups[name].Value;
    }

    public static bool IsMatch(string regex, string target) {
        return new Regex(regex).Match(target).Success;
    }

    public static bool IsNull<T>(T x) {
        return x == null;
    }

    // Func<Func<int, int>, int, int> fact = (rec, x) => x == 0 ? 1 : x * rec(x - 1);
    // Y(fact)(5) //=>120
    public static Func<T, TResult> Y<T, TResult>(Func<Func<T, TResult>, T, TResult> f) {
        Func<T, TResult> rec = null;
        return rec = (x => f(rec, x));
    }

    public static IEnumerable<T> Repeat<T>(this T x) {
        while (true) {
            yield return x;
        }
    }

    public static Match match(string re_str, string str) {
        var m = new Regex(re_str).Match(str);
        return m.Success ? m : throw new Exception($"マッチに失敗: {re_str}, {str}");
    }

    public static Boolean IsEmpty<T>(this IEnumerable<T> source) {
        if (source == null)
            return true; // or throw an exception
        return !source.Any();
    }

    public static IEnumerable<TResult> MapList<T, TResult>(this IEnumerable<T> xs,
                                                  Func<IEnumerable<T>, TResult> f) {
        return Enumerable.Range(0, xs.Count()).Select(i => f(xs.Skip(i)));
    }

    public static IEnumerable<int> conv(int i, IEnumerable<int> lengths) {
        return lengths.MapList(ls => ls.Skip(1).Aggregate(i, (acc, x) => (int)(acc / x))
          % ls.First());
    }

    private static IEnumerable<int> GetLengths(Array ary) {
        return Enumerable.Range(0, ary.Rank).Select(i => ary.GetLength(i));
    }

    public static bool IsZero(int i) {
        return i == 0;
    }

    private static int CountZero(IEnumerable<int> indices) {
        return indices.Reverse().TakeWhile(IsZero).Count();
    }

    public static string ArrayToString(Array ary) {
        var lengths = GetLengths(ary);
        var result = new String('[', ary.Rank);
        result += ary.GetValue(conv(0, lengths).ToArray()).ToString2();
        for (var i = 1; i < ary.Length; i++) {
            var indices = conv(i, lengths).ToArray();
            var c = CountZero(indices);
            result += IsZero(c) ?
                ", " :
                new String(']', c) + ", " + new String('[', c);
            result += ary.GetValue(indices).ToString2();
        }
        result += new String(']', ary.Rank);
        return result;
    }


    public static string ToString2(this Object obj) {
        switch (obj) {
            case string s:
                return s;
            case Array s:
                return ArrayToString(s);
            case System.Collections.IEnumerable xs:
                return "[" + xs.Cast<object>().Select(ToString2).Aggregate((acc, x) => acc + ", " + x) + "]";
            default:
                return obj.ToString();
        }
    }

    public static void say<T>(this T x) {
        Console.WriteLine(ToString2(x));
    }

    public static void ForEach<T>(this IEnumerable<T> xs, Action<T> f) {
        foreach (var x in xs) {
            f(x);
        }
    }

    // 関数宣言つらい
    public static Dictionary<TKey, IList<TValue>> groupBy<TKey, TValue>(Func<TValue, TKey> f, IEnumerable<TValue> xs) {
        var result = new Dictionary<TKey, IList<TValue>>();
        foreach (var x in xs) {
            var key = f(x);
            if (result.ContainsKey(key)) {
                result[key].Add(x);
            } else {
                result[key] = new List<TValue> { x };
            }
        }
        return result;
    }

    public static string[] split(this string str, string splitter) {
        return str.Split(new String[] { splitter }, StringSplitOptions.None);
    }

    // 途中の空行も省略されてしまうので直す
    public static string[] splitLines(this string str) {
        // したらばが\rを素通しするせいで酷い目にあった。
        return str.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string replace(this string str, String[,] pairs) {
        for (int i = 0; i < pairs.GetLength(0); i++) {
            str = str.Replace(pairs[i, 0], pairs[i, 1]);
        }
        return str;
    }

    public static Func<A, C> compose<A, B, C>(this Func<B, C> f, Func<A, B> g) {
        return a => f(g(a));
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

    public struct Rect {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    public static Rect GetProcessRectByNameAndTitle(string name, string title) {
        Rect rect = new Rect();
        foreach (var p in Process.GetProcessesByName(name)) {
            if (p.MainWindowTitle.Contains(title)) {
                GetWindowRect(p.MainWindowHandle, ref rect);
                return rect;
            }
        }

        throw new Exception(title + "が見つかりませんでした。");
    }

    public static bool IsProcessAlive(string name, string title) {
        return Process.GetProcessesByName(name).Any(p => p.MainWindowTitle.Contains(title));
    }
}