/*
 * Copyright 2021 S. Webber
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
using Newtonsoft.Json.Linq;

namespace Org.NProlog.Core.Predicate.Builtin.List;

[TestClass]
public class ReverseTest
{
    private static readonly string REVERSE_PROLOG =
                //
                "reverse_(Xs, Ys) :- reverse_(Xs, [], Ys, Ys)."
                + "reverse_([], Ys, Ys, [])."
                + "reverse_([X|Xs], Rs, Ys, [_|Bound]) :- reverse_(Xs, [X|Rs], Ys, Bound).";

    private static readonly ListPredicateAssert PREDICATE_ASSERT = new ("reverse", 2, REVERSE_PROLOG);
    readonly string[] vs1 = {
               "[a,b,c] [c,b,a]",
               "[a(X),b(Y),c(Z)] [c(z),b(y),a(x)]",
               "[a(X),b(Y),c(Z)] [c(Z),b(Y),a(X)]",
               "[a,b,c] [z,c,b,a]",
               "[a,b,c] [a,b,c]",
               "[a,b,c] [a,b,c]",
               "[a,b,c] X",
               "[a,b,c] [X|Y]",
               "[a|X] Y",
               "[a,b,c|X] Y",
               "[a,b,c|X] [Y|Z]",
               "[a,b,c|X] [a|Z]",
               "[a,b,c|X] [a,b|Z]",
               "[a,b,c|X] [a,b,c|Z]",
               "[a,b,c|X] [z,x,c,b,a|Z]",
               "[a,b,c|X] [z,x,c,b,a,q|Z]",
               "[a,b,c|X] [z,x,c,q,b,a|Z]",
               "[a,b,c|X] [z,x,c,b|Z]",
               "[a,b,c|X] [z,x,c|Z]",
               "[a,b,c|X] [a|Z]",
               "[a,b,c|X] [b,a|Z]",
               "[a,b,c|X] [c,b,a|Z]",
               "[a,b,c|X] [d,c,b,a]",
               "[a,b,c|X] [e,d,b,a]",
               "[a,b,c|X] [e,d,c,b,a]",
               "[a,a,a|X] [a,a,a|Z]",
               "[a,a,a|X] [b,a,a,a|Z]",
               "[a,a,a|X] [a,a,a,b|Z]",
               "[a,a,a|X] [a|Z]",
               "X Y",
               "X X",};
    [TestMethod]
    public void Test()
    {
        foreach (var item in vs1)
        {
            var args = item.Split(' ');

            PREDICATE_ASSERT.AssertArgs(args[0], args[1]);
            if (!args[0].Equals(args[1]))
            {
                PREDICATE_ASSERT.AssertArgs(args[1], args[0]);
            }
        }
    }

    readonly string[] vs2 =  {
               "length(X,1000),reverse(X,Y),numbervars(X),reverse(X,Z),reverse(Q,Z).",
               "length(X,1000),length(Y,1000),length(Z,2000),numbervars(X),reverse(X,Y),reverse(Z,X).",
    };
    [TestMethod]
    public void TestLongLists()
    {
        foreach (var query in vs2)
            PREDICATE_ASSERT.AssertQuery(query);
    }
}
