using System;
using System.ComponentModel;

namespace IOMonad
{
    class Program
    {
        static void Main(string[] args)
        {
            MainInternal(args).SelectMany(_ => new IO<Unit>(new Lazy<Unit>(Unit.Instance)));
        }

        static IO<Unit> MainInternal(string[] args)
        {
            return
                from _ in Console.WriteLine("What's your name?")
                from name in Console.ReadLine()
                from now in Clock.GetLocalTime()

                let greeting = Greeter.Greet(now, name)

                from res in Console.WriteLine(greeting)
                select res;
        }
    }

    public static class Greeter
    {
        public static string Greet(DateTime now, string name)
        {
            var greeting = "Hello";
            if (IsMorning(now))
            {
                greeting = "Good morning";
            }
            else if (IsAfternoon(now))
            {
                greeting = "Good afternoon";
            }
            else if (IsEvening(now))
            {
                greeting = "Good evening";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return $"{greeting}.";
            }

            return $"{greeting}, {name.Trim()}";
        }

        private static bool IsMorning(DateTime now) => now.Hour < 12;

        private static bool IsAfternoon(DateTime now) => now.Hour < 18;

        private static bool IsEvening(DateTime now) => now.Hour < 24;
    }

    public sealed class Unit
    {
        public static readonly Unit Instance;
    }

    public sealed class IO<T>
    {
        private readonly Lazy<T> item;

        public IO(Lazy<T> item) => this.item = item;

        public IO<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            return SelectMany(x => new IO<TResult>(new Lazy<TResult>(selector(x))));
        }

        public IO<TResult> SelectMany<TResult>(Func<T, IO<TResult>> selector) =>
            selector(item.Value);

        public IO<TResult> SelectMany<U, TResult>(
            Func<T, IO<U>> k,
            Func<T, U, TResult> s)
        {
            return SelectMany(x =>
                k(x)
                .SelectMany(y => new IO<TResult>(new Lazy<TResult>(s(x, y)))));
        }
    }

    public static class Console
    {
        public static IO<string> ReadLine() =>
            new IO<string>(new Lazy<string>(System.Console.ReadLine()));

        public static IO<Unit> WriteLine(string value) =>
            new IO<Unit>(new Lazy<Unit>(() =>
        {
            System.Console.WriteLine(value);
            return Unit.Instance;
        }));
    }

    public static class Clock
    {
        public static IO<DateTime> GetLocalTime() =>
            new IO<DateTime>(new Lazy<DateTime>(DateTime.Now));
    }
}
