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

namespace Org.NProlog.Core.Predicate.Builtin.IO;


/* TEST
write_to_file(X) :-
   open('get_char_test.tmp', write, Z),
   set_output(Z),
   writef(X),
   close(Z),
   set_output('user_output').

read_from_file :-
   open('get_char_test.tmp', read, Z),
   set_input(Z),
   Write_contents,
   close(Z).

Write_contents :-
   repeat,
   get_char(C),
   write(C),
   nl,
   C=='end_of_file',
   !.

%TRUE write_to_file('abc\nxyz')

%?- read_from_file
%OUTPUT
%a
%b
%c
%
%
%x
%y
%z
%end_of_file
%
%OUTPUT
%YES

force_error :-
   open('get_char_test.tmp', read, Z),
   set_input(Z),
   close(Z),
   Write_contents.

%?- force_error
%ERROR Could not read next character from input stream

%LINK prolog-io
*/
/**
 * <code>get_char(X)</code> - reads the next character from the input stream.
 * <p>
 * The goal succeeds if <code>X</code> can be unified with next character read from the current input stream. Succeeds
 * only once and the operation of moving to the next character is not undone on backtracking.
 * </p>
 * <p>
 * If there are no more characters to read from the current input stream (i.e. if the end of the stream has been
 * reached) then an attempt is made to unify <code>X</code> with an atom with the value <code>end_of_file</code>.
 * </p>
 */
public class GetChar : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term argument)
    {
        try
        {
            int c = FileHandles.CurrentReader.Read();
            var next = ToAtom(c);
            return argument.Unify(next);
        }
        catch (Exception e)
        {
            throw new PrologException("Could not read next character from input stream", e);
        }
    }

    private static Atom ToAtom(int c) => new (ToString(c));

    private static string ToString(int c) => c == -1 ? "end_of_file" : ((char)c).ToString();
}
