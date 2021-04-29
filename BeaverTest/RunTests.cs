using System;
using BeaverTest;

namespace BeaverTest
    /// <summary>
    /// this script contains a Main method for the test project 
    /// i.e. use "dotnet test" to run all tests or use "dotnet run" to run project via this script
    /// GenerateProgramFile should be set to false in .csproj to avoid conflicts 
    /// more info: https://andrewlock.net/fixing-the-error-program-has-more-than-one-entry-point-defined-for-console-apps-containing-xunit-tests/#:~:text=If%20you%20already%20have%20a,that%20contains%20the%20entry%20point.
    /// </summary>
{
    class Program
    {
        static void Main()
        {
            TestULS test = new TestULS();    
            test.TestULSDesign(); 
        }
    }
}
