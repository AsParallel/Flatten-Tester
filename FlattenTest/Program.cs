using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlattenTest
{
    public static class Steamroller<T> where T : class
    {
        public static Func<IEnumerable<IEnumerable<T>>, IEnumerable<T>> Flatten = lol =>
        {
            List<T> flat = new List<T>();
            lol.ToList().ForEach(l =>
            {
                flat.AddRange(l);
            });
            return flat;
        };

        public static Func<IEnumerable<IEnumerable<T>>, IEnumerable<T>> FlattenLinq = lol =>
        {
            return lol.SelectMany(l => l);
        };
    }
    public class Program
    {
        private static async Task Evaluate(Func<IEnumerable<IEnumerable<object>>, IEnumerable<object>> getItems, IEnumerable<IEnumerable<object>> data, bool SupressOutput = false)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var result = await Task<int>.Run(() =>
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                var items = getItems(data).ToList();
                return items.Count();
            });
            s.Stop();

            if (!SupressOutput) await Console.Out.WriteLineAsync(string.Format("Processed {0} items in {1}ms", result, s.ElapsedMilliseconds.ToString()));
        }

        static Func<List<List<object>>> CreateSampleData = () =>
        {
            Random r = new Random();
            Func<List<object>> fobjectMaker = () =>
            {
                List<object> fobjectList = new List<object>(1000);
                Enumerable.Range(1, 1000).AsParallel().ForAll(
                    x => fobjectList.Add(r.Next(1, 1000)));
                return fobjectList;
            };
            Func<List<List<object>>> genFObjectList = () =>
            {
                List<List<object>> lobjects = new List<List<object>>(1000);
                Enumerable.Range(1, 1000).AsParallel().ForAll(x =>
                {
                    lobjects.Add(fobjectMaker());
                });
                return lobjects;
            };

            return genFObjectList();
        };

        private static async Task RunTest()
        {
            var sampleSet = CreateSampleData();
            Console.Out.WriteLine("Burn in started...");
            await Evaluate(Steamroller<object>.Flatten, sampleSet, true);
            await Evaluate(Steamroller<object>.FlattenLinq, sampleSet, true);
            await Console.Out.WriteLineAsync(string.Format("Testing a sample set consisting of {0} collections", sampleSet.Count().ToString()));
            await Console.Out.WriteLineAsync("Testing Linq");
            await Evaluate(Steamroller<object>.FlattenLinq, sampleSet);
            await Console.Out.WriteLineAsync("Testing Enumeration");
            await Evaluate(Steamroller<object>.Flatten, sampleSet);
            await Console.Out.WriteLineAsync("Testing Linq");
            await Evaluate(Steamroller<object>.FlattenLinq, sampleSet);
            await Console.Out.WriteLineAsync("Testing Enumeration");
            await Evaluate(Steamroller<object>.Flatten, sampleSet);
            await Console.Out.WriteLineAsync("Testing Linq");
            await Evaluate(Steamroller<object>.FlattenLinq, sampleSet);
            await Console.Out.WriteLineAsync("Testing Enumeration");
            await Evaluate(Steamroller<object>.Flatten, sampleSet);
        }

        public static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Task.WaitAll(Task.Run(async () => {
                await RunTest();
            }));
            Console.WriteLine("Derp derp");
            Console.Read();
        }

    }
}

