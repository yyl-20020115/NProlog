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
    public void TestUserInputHandle()
    {
        Assert.AreEqual("user_input", FileHandles.USER_INPUT_HANDLE.Name);
    }

    [TestMethod]
    public void TestUserOutputHandle()
    {
        Assert.AreEqual("user_output", FileHandles.USER_OUTPUT_HANDLE.Name);
    }

    [TestMethod]
    public void TestDefaultInputStream()
    {
        FileHandles fh = new FileHandles();
        Assert.AreSame(Console.In, fh.CurrentReader);
    }

    [TestMethod]
    public void TestDefaultOutputStream()
    {
        FileHandles fh = new FileHandles();
        Assert.AreSame(Console.Out, fh.CurrentWriter);
    }

    [TestMethod]
    public void TestDefaultInputHandle()
    {
        FileHandles fh = new FileHandles();
        Term expected = new Atom("user_input");
        Term actual = fh.CurrentInputHandle;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestDefaultOutputHandle()
    {
        FileHandles fh = new FileHandles();
        Term expected = new Atom("user_output");
        Term actual = fh.CurrentOutputHandle;
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestSetUserInputWhenCurrent()
    {
        FileHandles fh = new FileHandles();

        // given the standard stream is also the current stream
        Assert.AreSame(FileHandles.USER_INPUT_HANDLE, fh.CurrentInputHandle);

        // when we reassign the standard stream
        var sr = new StringReader("");
        fh.SetUserInput(sr);

        // then the current stream should be updated
        Assert.AreSame(sr, fh.CurrentReader);
    }

    [TestMethod]
    public void TestSetUserInputWhenNotCurrent()
    {
        FileHandles fh = new FileHandles();

        // set input to something other than the standard stream
        string filename = CreateFileName("testSetUserInputWhenNotCurrentInput");
        
        Term handle = fh.OpenInput(filename);
        fh.SetInput(handle);

        // reassign the standard stream
        var _is = new StringReader("");
        fh.SetUserInput(_is);

        // confirm that reassigning the standard stream has not altered the current input
        Assert.AreSame(handle, fh.CurrentInputHandle);
        Assert.AreNotSame(_is, fh.CurrentReader);

        // switch back to the standard stream and confirm it has been reassigned
        fh.SetInput(FileHandles.USER_INPUT_HANDLE);
        Assert.AreSame(_is, fh.CurrentReader);
    }

    [TestMethod]
    public void TestSetUserOutputWhenCurrent()
    {
        FileHandles fh = new FileHandles();

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
        FileHandles fh = new FileHandles();

        // set output to something other than the standard stream
        Term handle = fh.OpenOutput(CreateFileName("testSetUserOutputWhenNotCurrentOutput"));
        fh.SetOutput(handle);

        // reassign the standard stream
        var ps = new StringWriter();
        fh.SetUserOutput(ps);

        // confirm that reassigning the standard stream has not altered the current output
        Assert.AreSame(handle, fh.CurrentOutputHandle);
        Assert.AreNotSame(ps, fh.CurrentWriter);

        // switch back to the standard stream and confirm it has been reassigned
        fh.SetOutput(FileHandles.USER_OUTPUT_HANDLE);
        Assert.AreSame(ps, fh.CurrentWriter);
    }

    [TestMethod]
    public void TestSetInputFailure()
    {
        FileHandles fh = new FileHandles();
        Term t = Atom("test");
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
        FileHandles fh = new FileHandles();
        Term t = Atom("test");
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
        string filename = CreateFileName("testWriteAndRead");
        string contentsToWrite = "test";
        Write(fh, filename, contentsToWrite);
        string contentsRead = Read(fh, filename);
        Assert.AreEqual(contentsToWrite, contentsRead);
    }

    [TestMethod]
    public void TestIsHandle()
    {
        FileHandles fh = new FileHandles();
        string filename = CreateFileName("testIsHandle");
        Term handle = OpenOutput(fh, filename);
        Assert.IsTrue(fh.IsHandle(handle.Name));
        fh.Close(handle);
        Assert.IsFalse(fh.IsHandle(handle.Name));
    }

    private string CreateFileName(string name)
    {
        string fn = GetType().Name + "_" + name + "_" + DateTime.Now.Millisecond + ".tmp";
        File.Create(fn).Close();

        return fn;
    }

    private void Write(FileHandles fh, string filename, string contents)
    {
        Term handle = OpenOutput(fh, filename);
        fh.SetOutput(handle);
        Assert.AreSame(handle, fh.CurrentOutputHandle);
        var ps = fh.CurrentWriter;
        ps.Write(contents);
        fh.Close(handle);
        //Assert.IsFalse(ps.checkError());
        //ps.Append("extra stuff after close was called");
        //Assert.IsTrue(ps.checkError());
    }

    private string Read(FileHandles fh, string filename)
    {
        Term handle = OpenInput(fh, filename);
        fh.SetInput(handle);
        Assert.AreSame(handle, fh.CurrentInputHandle);
        var _is = fh.CurrentReader;
        string contents = "";
        int next;
        while ((next = _is.Read()) != -1)
        {
            contents += (char)next;
        }
        fh.Close(handle);
        try
        {
            _is.Read();
            Assert.Fail("could read from closed input stream");
        }
        catch (Exception e)
        {
            Assert.IsTrue(true);
            // expected now stream has been closed
        }
        return contents;
    }

    private Term OpenOutput(FileHandles fh, string filename)
    {
        Term handle = fh.OpenOutput(filename);
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

    private Term OpenInput(FileHandles fh, string filename)
    {
        Term handle = fh.OpenInput(filename);
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
