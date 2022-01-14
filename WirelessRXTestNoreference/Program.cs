using System;
using System.IO;
using System.Threading;
using System.Reflection;

namespace WirelessRXTestNoreference
{
    class Program
    {
        //Ignore this, pretend this is balsa itself.
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            Assembly portsLib = AppDomain.CurrentDomain.Load(File.ReadAllBytes("../WirelessRXTest/bin/Debug/net6.0/System.IO.Ports.dll"));
            Assembly assLib = AppDomain.CurrentDomain.Load(File.ReadAllBytes("../WirelessRXTest/bin/Debug/net6.0/WirelessRXLib.dll"));
            Assembly ass = AppDomain.CurrentDomain.Load(File.ReadAllBytes("../WirelessRXTest/bin/Debug/net6.0/WirelessRXTest.dll"));
            Type programType = ass.GetType("WirelessRXTest.Program");
            Thread t = new Thread(new ThreadStart(TestMain));
            t.Start();
            programType.GetMethod("Main").Invoke(null, new object[] { args });
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (ass.GetName().FullName == args.Name)
                {
                    return ass;
                }
            }
            return null;
        }

        private static void TestMain()
        {
            //We need a startup delay so it can init itself, in game just wait a few frames
            Thread.Sleep(1000);
            Start();
            while (true)
            {
                Update();
                Thread.Sleep(1000);
            }
        }

        //This is what you need to do but instead of WirelessRXTest, it will be WirelessRXMain.
        private static void Start()
        {
            Console.WriteLine("Adding sensor");
            object[] sensorArray = null;
            Type sensorType = null;
            Type sensorEnumType = null;
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (ass.GetName().Name == "WirelessRXTest")
                {
                    sensorArray = ass.GetType("WirelessRXTest.Program")?.GetProperty("Sensors")?.GetGetMethod()?.Invoke(null, null) as object[];
                }
                if (ass.GetName().Name == "WirelessRXLib")
                {
                    sensorType = ass.GetType("WirelessRXLib.Sensor");
                    sensorEnumType = ass.GetType("WirelessRXLib.SensorType");
                }
            }
            if (sensorType == null || sensorEnumType == null || sensorArray == null)
            {
                Console.WriteLine("Failed to find types");
                return;
            }
            object sensorCell = sensorEnumType.GetField("CELL").GetValue(null);
            //This compile warning is incorrect, we actually want to pass the method, not call it and pass its value.
            //Warning goes away if cast to delegate
            object newSensor = Activator.CreateInstance(sensorType, new object[] { sensorCell, (Delegate)GetVoltage });
            sensorArray[2] = newSensor;
            Console.WriteLine("Breakpoint here in order to check that it works");
        }

        private static void Update()
        {
            Console.WriteLine("Looping, update getvoltage in here and set it somewhere because unity will crap out on threads");
        }

        private static int GetVoltage()
        {
            return 390;
        }
    }
}
