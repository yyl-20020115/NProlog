/*
 * Copyright 2013 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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


/**
 * Collection of input and output streams.
 * <p>
 * Each {@link org.projog.core.kb.KnowledgeBase} has a single unique {@code FileHandles} instance.
 *
 * @see KnowledgeBase#getFileHandles()
 */
public class FileHandles
{
    /**
     * The handle of the "standard" output stream.
     * <p>
     * By default the "standard" output stream will be {@code System._out}.
     */
    public static readonly Atom USER_OUTPUT_HANDLE = new("user_output");
    /**
     * The handle of the "standard" input stream.
     * <p>
     * By default the "standard" input stream will be {@code System.in}.
     */
    public static readonly Atom USER_INPUT_HANDLE = new("user_input");

    private readonly object syncRoot = new();
    private readonly Dictionary<string, TextReader> inputHandles = new();
    private readonly Dictionary<string, TextWriter> outputHandles = new();

    /** Current input used by get_char and read */
    private Term currentInputHandle;
    /** Current output used by put_char, nl, write and write_canonical */
    private Term currentOutputHandle;

    private TextReader reader;
    private TextWriter writer;

    public FileHandles()
    {
        var userInputHandle = USER_INPUT_HANDLE;
        var userOutputHandle = USER_OUTPUT_HANDLE;
        this.currentInputHandle = userInputHandle;
        this.currentOutputHandle = userOutputHandle;
        inputHandles.Add(userInputHandle.Name,this.reader = Console.In);
        outputHandles.Add(userOutputHandle.Name,this.writer = Console.Out);
        SetInput(userInputHandle);
        SetOutput(userOutputHandle);
    }

    /**
     * Return the {@code Term} representing the current input stream.
     * <p>
     * By default this will be an {@code Atom} with the name "{@code user_input}".
     */
    public Term CurrentInputHandle => currentInputHandle;

    /**
     * Return the {@code Term} representing the current output stream.
     * <p>
     * By default this will be an {@code Atom} with the name "{@code user_output}".
     */
    public Term CurrentOutputHandle => currentOutputHandle;

    /**
     * Return the current input stream.
     * <p>
     * By default this will be {@code System.in}.
     */
    public TextReader CurrentReader => reader;

    /**
     * Return the current output stream.
     * <p>
     * By default this will be {@code System._out}.
     */
    public TextWriter CurrentWriter => writer;

    /**
     * Reassigns the "standard" input stream.
     *
     * @see #USER_INPUT_HANDLE
     */
    public void SetUserInput(TextReader reader)
    {
        lock (this.syncRoot)
        {
            inputHandles[USER_INPUT_HANDLE.Name]= reader;
            if (USER_INPUT_HANDLE.Equals(currentInputHandle))
                SetInput(USER_INPUT_HANDLE);
        }
    }

    /**
     * Reassigns the "standard" output stream.
     *
     * @see #USER_OUTPUT_HANDLE
     */
    public void SetUserOutput(TextWriter writer)
    {
        lock (this.syncRoot)
        {
            outputHandles[USER_OUTPUT_HANDLE.Name] = writer;
            if (USER_OUTPUT_HANDLE.Equals(currentOutputHandle))
                SetOutput(USER_OUTPUT_HANDLE);
        }
    }

    /**
     * Sets the current input stream to the input stream represented by the specified {@code Term}.
     *
     * @throws ProjogException if the specified {@link Term} does not represent an {@link Atom}
     */
    public void SetInput(Term handle)
    {
        var handleName = TermUtils.GetAtomName(handle);
        lock (this.syncRoot)
        {
            if (inputHandles.ContainsKey(handleName))
            {
                currentInputHandle = handle;
                reader = inputHandles[(handleName)];
            }
            else
                throw new PrologException("cannot find file input handle with name: " + handleName);
        }
    }

    /**
     * Sets the current output stream to the output stream represented by the specified {@code Term}.
     *
     * @throws ProjogException if the specified {@link Term} does not represent an {@link Atom}
     */
    public void SetOutput(Term handle)
    {
        var handleName = TermUtils.GetAtomName(handle);
        lock (this.syncRoot)
        {
            if (outputHandles.ContainsKey(handleName))
            {
                currentOutputHandle = handle;
                writer = outputHandles[(handleName)];
            }
            else
                throw new PrologException("cannot find file output handle with name: " + handleName);
        }
    }

    /**
     * Creates an intput file stream to read from the file with the specified name
     *
     * @param fileName the system-dependent filename
     * @return a reference to the newly created stream (as required by {@link #setInput(Term)} and {@link #close(Term)})
     * @throws ProjogException if this object's collection of input streams already includes the specified file
     * @ if the file cannot be opened for reading
     */
    public Atom OpenInput(string fileName)
    {
        var handleName = fileName + "_input_handle";
        lock (this.syncRoot)
        {
            if (inputHandles.ContainsKey(handleName))
                throw new PrologException("Can not open input for: " + fileName + " as it is already open");
            else
            {
                var reader = new StreamReader(fileName);
                inputHandles.Add(handleName, reader);
            }
        }
        return new Atom(handleName);
    }

    /**
     * Creates an output file stream to write to the file with the specified name
     *
     * @param fileName the system-dependent filename
     * @return a reference to the newly created stream (as required by {@link #setOutput(Term)} and {@link #close(Term)})
     * @throws ProjogException if this object's collection of output streams already includes the specified file
     * @ if the file cannot be opened
     */
    public Atom OpenOutput(string fileName)
    {
        var handleName = fileName + "_output_handle";
        lock (this.syncRoot)
        {
            if (outputHandles.ContainsKey(handleName))
                throw new PrologException("Can not open output for: " + fileName + " as it is already open");
            else
            {
                var os = new StreamWriter(fileName);
                outputHandles.Add(handleName, (os));
            }
        }
        return new Atom(handleName);
    }

    /**
     * Closes the stream represented by the specified {@code Term}.
     *
     * @throws ProjogException if the specified {@link Term} does not represent an {@link Atom}
     * @ if an I/O error occurs
     */
    public void Close(Term handle)
    {
        var handleName = TermUtils.GetAtomName(handle);
        lock (this.syncRoot)
        {
            if (outputHandles.TryGetValue(handleName,out var writer))
            {
                outputHandles.Remove(handleName);
                writer.Close();
                return;
            }
            if (inputHandles.TryGetValue(handleName,out var reader))
            {
                inputHandles.Remove(handleName);
                reader.Close();
                return;
            }
        }
    }

    public bool IsHandle(string handle)
        => inputHandles.ContainsKey(handle) || outputHandles.ContainsKey(handle);
}
