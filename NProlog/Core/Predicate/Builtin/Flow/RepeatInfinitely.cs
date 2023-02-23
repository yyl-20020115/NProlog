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
namespace Org.NProlog.Core.Predicate.Builtin.Flow;


/* TEST
%?- repeat
%YES
%YES
%YES
%YES
%YES
%YES
%YES
%QUIT

write_to_file(X) :-
   open('io_test.tmp', write, Z),
   set_output(Z),
   write(X),
   close(Z),
   set_output('user_output').

read_from_file :-
   open('io_test.tmp', read, Z),
   set_input(Z),
   Write_first_sentence,
   close(Z).

Write_first_sentence :-
   repeat,
   get_char(C),
   write(C),
   C=='.',
   !.

%TRUE write_to_file('The first sentence. The second sentence.')

%?- read_from_file
%OUTPUT The first sentence.
%YES
*/
/**
 * <code>repeat</code> - always succeeds.
 * <p>
 * <code>repeat</code> <i>always</i> succeeds even when an attempt is made to re-satisfy it.
 * </p>
 */
public class RepeatInfinitely : AbstractPredicateFactory
{
    private static readonly RepeatInfinitelyPredicate SINGLETON = new ();

    protected override Predicate GetPredicate() => SINGLETON;

    public class RepeatInfinitelyPredicate : Predicate
    {
        public bool Evaluate() => true;

        public bool CouldReevaluationSucceed => true;
    }
}
