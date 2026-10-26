using NUnitLite;
public static class Program
{
 public static int Main(string[] args) { return new AutoRun(typeof(Program).Assembly).Execute(args); }
}
