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
namespace Org.NProlog.Core.Parser;

[TestClass]
public class CharacterParserTest
{
    [TestMethod]
    public void TestEmpty()
    {
        var p = CreateParser("");
        Assert.AreEqual(-1, p.Peek());
        Assert.AreEqual(-1, p.GetNext());
        Assert.AreEqual(-1, p.Peek());
        Assert.AreEqual(-1, p.GetNext());
    }

    [TestMethod]
    public void TestSingleCharacter()
    {
        var p = CreateParser("a");
        Assert.AreEqual('a', p.Peek());
        Assert.AreEqual('a', p.Peek());
        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual('\n', p.Peek());
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.Peek());
        Assert.AreEqual(-1, p.GetNext());
        p.Rewind();
        Assert.AreEqual('\n', p.Peek());
        p.Rewind();
        Assert.AreEqual('a', p.Peek());
        Assert.AreEqual('a', p.GetNext());
    }

    [TestMethod]
    public void TestSingleLine()
    {
        var s = "qwerty";
        var p = CreateParser(s);
        foreach (var c in s.ToCharArray())
        {
            Assert.AreEqual(c, p.GetNext());
        }
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
    }

    [TestMethod]
    public void TestGetColumnNumber()
    {
        var s = "abc";
        var p = CreateParser(s);
        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual(1, p.ColumnNumber);
        Assert.AreEqual('b', p.GetNext());
        Assert.AreEqual(2, p.ColumnNumber);
        Assert.AreEqual('c', p.Peek());
        Assert.AreEqual(2, p.ColumnNumber);
        p.Rewind();
        Assert.AreEqual(1, p.ColumnNumber);
    }

    [TestMethod]
    public void TestMultipleLine()
    {
        var s = "qwerty\nasdf";
        var p = CreateParser(s);

        Assert.AreEqual('q', p.GetNext());
        Assert.AreEqual("qwerty", p.Line);
        Assert.AreEqual(1, p.LineNumber);

        p.SkipLine();

        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual("asdf", p.Line);
        Assert.AreEqual(2, p.LineNumber);
    }

    [TestMethod]
    public void TestRewind()
    {
        var s = "qwerty";
        var p = CreateParser(s);
        Assert.AreEqual('q', p.GetNext());
        Assert.AreEqual(1, p.ColumnNumber);
        p.Rewind();
        Assert.AreEqual('q', p.GetNext());
        Assert.AreEqual('w', p.GetNext());
        Assert.AreEqual('e', p.GetNext());
        Assert.AreEqual('r', p.GetNext());
        Assert.AreEqual('t', p.GetNext());
        Assert.AreEqual('y', p.GetNext());
        Assert.AreEqual(6, p.ColumnNumber);
        p.Rewind();
        Assert.AreEqual(5, p.ColumnNumber);
        Assert.AreEqual('y', p.GetNext());
        p.Rewind(3);
        Assert.AreEqual(3, p.ColumnNumber);
        Assert.AreEqual('r', p.GetNext());
        Assert.AreEqual(4, p.ColumnNumber);
        p.Rewind(4);
        Assert.AreEqual('q', p.GetNext());
        Assert.AreEqual(1, p.ColumnNumber);
    }

    [TestMethod]
    public void TestEndOfStreamTrailingNoLine()
    {
        var s = "a\n";
        var p = CreateParser(s);
        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.Peek());
        p.Rewind();
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
        p.Rewind();
        Assert.AreEqual('\n', p.GetNext());
    }

    [TestMethod]
    public void TestEndOfStreamNoTrailingNoLine()
    {
        var s = "a";
        var p = CreateParser(s);
        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.Peek());
        p.Rewind();
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
        p.Rewind();
        Assert.AreEqual('\n', p.GetNext());
    }

    [TestMethod]
    public void TestBlankLines()
    {
        var s = "\n\n";
        var p = CreateParser(s);
        Assert.AreEqual('\n', p.Peek());
        p.SkipLine();
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual(-1, p.GetNext());
        Assert.AreEqual(-1, p.Peek());
    }

    [TestMethod]
    public void TestRewindException()
    {
        var s = "a\nbcdef";
        var p = CreateParser(s);
        Assert.AreEqual('a', p.GetNext());
        Assert.AreEqual('\n', p.GetNext());
        Assert.AreEqual('b', p.GetNext());
        Assert.AreEqual('c', p.GetNext());
        Assert.AreEqual('d', p.GetNext());
        p.Rewind(3);
        Assert.AreEqual('b', p.GetNext());
        Assert.AreEqual('c', p.GetNext());
        Assert.AreEqual('d', p.GetNext());
        try
        {
            p.Rewind(4);
        }
        catch (ParserException e)
        {
            Assert.AreEqual("Cannot rewind past start of current line Line: bcdef", e.Message);
            Assert.AreEqual(3, e.ColumnNumber);
            Assert.AreEqual(2, e.LineNumber);
        }
    }

    private static CharacterParser CreateParser(string s) => new (new StringReader(s));
}
