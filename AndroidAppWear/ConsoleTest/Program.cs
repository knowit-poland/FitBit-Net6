// See https://aka.ms/new-console-template for more information
using ConsoleTest;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        var classTest = new Class1()
        {
            Name = "Test",
            Level = 1,
            HealthPints = 1,
        };

        string toWrite = JsonConvert.SerializeObject(classTest);

        File.WriteAllText(@"C:\Users\adakuc\source\repos\AndroidAppWear\ConsoleTest\json\json.json", toWrite);
    }
}
