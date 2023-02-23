/*
 * Copyright 2015 S. Webber
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
namespace Org.NProlog;


/** Tests that the contents of every Java source file starts with the license header. */
[TestClass]
public class SourceHeaderTest
{
    [TestMethod]
    public void TestSourceHeaders()
    {
        var csfiles = GetCSharpSourceFiles();
        foreach (var f in csfiles)
            AssertSourceHeader(f);
    }

    /** @return all Java source files for the project */
    private static List<string> GetCSharpSourceFiles()
    {
        var d = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "."));
        var files = d.GetFiles("*.cs");
        return files.Select(f => f.FullName).ToList();
    }

    /** Asserts that the specified Java source file starts with the license header. */
    private static void AssertSourceHeader(string p)
    {
        var lines = File.ReadAllLines(p);
        var failureMessage = "No source header found for " + p;
        Assert.IsTrue(lines.Length > 15, failureMessage);
        Assert.AreEqual("/*", lines[(0)], failureMessage);
        Assert.AreEqual(" * Licensed under the Apache License, Version 2.0 (the \"License\");", lines[(3)], failureMessage);
        Assert.AreEqual(" * you may not use this file except in compliance with the License.", lines[(4)], failureMessage);
        Assert.AreEqual(" * You may obtain a Copy of the License at", lines[(5)], failureMessage);
        Assert.AreEqual(" *     http://www.apache.org/licenses/LICENSE-2.0", lines[(7)], failureMessage);
    }
}
