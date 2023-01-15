/*
 * Copyright 2013-2014 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Org.NProlog.Api;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Examples;

[TestClass]
public class PrologExample
{
    [TestMethod]
    public void DoTest()
    {
        Assert.IsTrue(true);
        //_Main(new string[0]);
    }
    public static void _Main(string[] args)
    {
        var p = new Prolog();
        p.ConsultFile(new("test.pl"));
        var s1 = p.CreateStatement("test(X,Y).");
        var r1 = s1.ExecuteQuery();
        while (r1.Next())
        {
            Console.WriteLine("X = " + r1.GetTerm("X") + " Y = " + r1.GetTerm("Y"));
        }
        s1.SetTerm("X", (Term)new Atom("d"));
        QueryResult r2 = s1.ExecuteQuery();
        while (r2.Next())
        {
            Console.WriteLine("Y = " + r2.GetTerm("Y"));
        }

        var s2 = p.CreateStatement("testRule(X).");
        var r3 = s2.ExecuteQuery();
        while (r3.Next())
        {
            Console.WriteLine("X = " + r3.GetTerm("X"));
        }

        var s3 = p.CreateStatement("test(X, Y), Y<3.");
        var r4 = s3.ExecuteQuery();
        while (r4.Next())
        {
            Console.WriteLine("X = " + r4.GetTerm("X") + " Y = " + r4.GetTerm("Y"));
        }
    }
}
