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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.IO;

[TestClass]
public class FileHandlesTest : TestUtils
{
    [TestMethod]
    public void TestUserInputHandle() => Assert.AreEqual("user_input", FileHandles.USER_INPUT_HANDLE.Name);

    [TestMethod]
    public void TestUserOutputHandle() => Assert.AreEqual("user_output", FileHandles.USER_OUTPUT_HANDLE.Name);

    [TestMethod]
    public void TestDefaultInputStream()
    {
        FileHandles fh = new();
        Assert.AreSame(Console.In, fh.CurrentReader);
    }

    [TestMethod]
    public void TestDefaultOutputStream()
    {
        FileHandles fh = new();
        Assert.AreSame(Console.Out, fh.CurrentWriter);
    }

    [TestMethod]
    public void TestDefaultInputHandle()
    {
        FileHandles fh = new();
        var expected = new Atom("user_input");
        var actual = fh.CurrentInputHandle;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestDefaultOutputHandle()
    {
        FileHandles fh = new();
        var expected = new Atom("user_output");
        var actual = fh.CurrentOutputHandle;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestSetUserInputWhenCurrent()
    {
        FileHandles fh = new();

        // given the standard stream is also the current stream
        Assert.AreSame(FileHandles.USER_INPUT_HANDLE, fh.CurrentInputHandle);

        // when we reassign the standard stream
        var reader = new StringReader("");
        fh.SetUserInput(reader);

        // then the current stream should be updated
        Assert.AreSame(reader, fh.CurrentReader);
    }

    [TestMethod]
    public void TestSetUserInputWhenNotCurrent()
    {
        FileHandles fh = new();

        // set input to something other than the standard stream
        var filename = CreateFileName("testSetUserInputWhenNotCurrentInput");
        
        var handle = fh.OpenInput(filename);
        fh.SetInput(handle);

        // reassign the standard stream
        var reader = new StringReader("");
        fh.SetUserInput(reader);

        // confirm that reassigning the standard stream has not altered the current input
        Assert.AreSame(handle, fh.CurrentInputHandle);
        Assert.AreNotSame(reader, fh.CurrentReader);

        // switch back to the standard stream and confirm it has been reassigned
        fh.SetInput(FileHandles.USER_INPUT_HANDLE);
        Assert.AreSame(reader, fh.CurrentReader);
    }

    [TestMethod]
    public void TestSetUserOutputWhenCurrent()
    {
        FileHandles fh = new();

        // given the standard stream is also the current stream
        Assert.AreSame(FileHandles.USER_OUTPUT_HANDLE, fh.CurrentOutputHandle);

        // when we reassign the standard stream
        var ps = new StringWriter();
        fh.SetUserOutput(ps);

        // then the current stream should be updated
        Assert.AreSame(ps, fh.CurrentWriter);
    }

    [TestMethod]
    public void TestSetUserOutputWhenNotCurrent()
    {
        FileHandles fh = new();

        // set output to something other than the standard stream
        var handle = fh.OpenOutput(CreateFileName("testSetUserOutputWhenNotCurrentOutput"));
        fh.SetOutput(handle);

        // reassign the standard stream
        var writer = new StringWriter();
        fh.SetUserOutput(writer);

        // confirm that reassigning the standard stream has not altered the current output
        Assert.AreSame(handle, fh.CurrentOutputHandle);
        Assert.AreNotSame(writer, fh.CurrentWriter);

        // switch back to the standard stream and confirm it has been reassigned
        fh.SetOutput(FileHandles.USER_OUTPUT_HANDLE);
        Assert.AreSame(writer, fh.CurrentWriter);
    }

    [TestMethod]
    public void TestSetInputFailure()
    {
        FileHandles fh = new();
        var t = Atom("test");
        try
        {
            fh.SetInput(t);
            Assert.Fail("could set input for unopened file");
        }
        catch (PrologException e)
        {
            Assert.AreEqual("cannot find file input handle with name: test", e.Message);
        }
    }

    [TestMethod]
    public void TestSetOutputFailure()
    {
        FileHandles fh = new();
        var t = Atom("test");
        try
        {
            fh.SetInput(t);
            Assert.Fail("could set output for unopened file");
        }
        catch (PrologException e)
        {
            Assert.AreEqual("cannot find file input handle with name: test", e.Message);
        }
    }

    [TestMethod]
    public void TestWriteAndRead()
    {
        var fh = new FileHandles();
        var filename = CreateFileName("testWriteAndRead");
        var contentsToWrite = "test";
        Write(fh, filename, contentsToWrite);
        var contentsRead = Read(fh, filename);
        Assert.AreEqual(contentsToWrite, contentsRead);
    }

    [TestMethod]
    public void TestIsHandle()
    {
        FileHandles fh = new();
        var filename = CreateFileName("testIsHandle");
        var handle = OpenOutput(fh, filename);
        Assert.IsTrue(fh.IsHandle(handle.Name));
        fh.Close(handle);
        Assert.IsFalse(fh.IsHandle(handle.Name));
    }

    private string CreateFileName(string name)
    {
        var fn = GetType().Name + "_" + name + "_" + DateTime.Now.Millisecond + ".tmp";
        File.Create(fn).Close();

        return fn;
    }

    private static void Write(FileHandles fh, string filename, string contents)
    {
        var handle = OpenOutput(fh, filename);
        fh.SetOutput(handle);
        Assert.AreSame(handle, fh.CurrentOutputHandle);
        var writer = fh.CurrentWriter;
        writer.Write(contents);
        fh.Close(handle);
        //Assert.IsFalse(ps.checkError());
        //ps.Append("extra stuff after close was called");
        //Assert.IsTrue(ps.checkError());
    }

    private static string Read(FileHandles fh, string filename)
    {
        var handle = OpenInput(fh, filename);
        fh.SetInput(handle);
        Assert.AreSame(handle, fh.CurrentInputHandle);
        var reader = fh.CurrentReader;
        var contents = "";
        int next;
        while ((next = reader.Read()) != -1)
        {
            contents += (char)next;
        }
        fh.Close(handle);
        try
        {
            reader.Read();
            Assert.Fail("could read from closed input stream");
        }
        catch (Exception)
        {
            Assert.IsTrue(true);
            // expected now stream has been closed
        }
        return contents;
    }

    private static Term OpenOutput(FileHandles fh, string filename)
    {
        var handle = fh.OpenOutput(filename);
        try
        {
            fh.OpenOutput(filename);
            Assert.Fail("was able to reopen already opened file for output");
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Can not open output for: " + filename + " as it is already open", e.Message);
        }
        return handle;
    }

    private static Term OpenInput(FileHandles fh, string filename)
    {
        var handle = fh.OpenInput(filename);
        try
        {
            fh.OpenInput(filename);
            Assert.Fail("was able to reopen already opened file for input");
        }
        catch (PrologException e)
        {
            Assert.AreEqual("Can not open input for: " + filename + " as it is already open", e.Message);
        }
        return handle;
    }
}
