using System;
using EcoGuardian.Tests;
static class Program
{
    static int Main(){try {int count=RuleChecks.Run(Console.WriteLine);Console.WriteLine("PASS: "+count+" checks. Pure C# simulation only; Unity import, rendering and builds are NOT covered.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
