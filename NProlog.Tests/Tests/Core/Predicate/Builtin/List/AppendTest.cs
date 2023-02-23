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

namespace Org.NProlog.Core.Predicate.Builtin.List;

[TestClass]
public class AppendTest
{
    private static readonly string APPEND_PROLOG =
                //
                "append_([],L,L)." +
                "append_([X|L1],L2,[X|L3]) :- append_(L1,L2,L3).";

    private static readonly ListPredicateAssert PREDICATE_ASSERT = new ("append", 3, APPEND_PROLOG);
    private static readonly string[] strings = new string[] {
               "[a,b,c] [d,e,f] X",
               "[a,b,c] [d,e,f] [a,b,c,d,e,f]",
               "[a,b,c] [d,e,f] [a,b,c,d,e]",
               "[a,b,c] [d,e,f] [a,b,c,d,e,f,g]",
               "[a,b,c] [d,e,f] [a,b,c,d,e,g]",
               "[a,b,c] [d,e,f] [a,b,c,d,f,e]",
               "[A,B,C] [D,E,F] [a,b,c,d,e,f]",
               "[a,b,c] [d,e,f] [A,B,C,D,E,F]",
               "[A,b,C] [d,E,f] [a,B,c,D,e,F]",
               "[p(X,Y,Z),p(b,X),C|T] X [p(A,B,C),p(B,a),c,D,e,F]",
               "[p(X,Y,Z),p(b,X),C|T] Q [p(A,B,C),p(B,a),c,D,e,F]",
               "[] [] X",
               "[] [] []",
               "[] [] [a]",
               "[a,b,c] [] X",
               "[] [a,b,c] X",
               "X Y []",
               "X Y [a]",
               "X Y [a,b,c]",
               "X X [a,b,c]",
               "X X [a,b,a]",
               "X X [a,b,c,a,b,c]",
               "[] Y [a,b,c]",
               "[a] Y [a,b,c]",
               "[a,b] Y [a,b,c]",
               "[a,b,c] Y [a,b,c]",
               "[a,b,c,d] Y [a,b,c]",
               "X [] [a,b,c]",
               "X [a] [a,b,c]",
               "X [a,b] [a,b,c]",
               "X [a,b,c] [a,b,c]",
               "X [a,b,c,d] [a,b,c]",
               "[a|X] Y [a,b,c,d]",
               "[a,b|X] Y [a,b,c,d]",
               "[a,b,c|X] Y [a,b,c,d]",
               "[a,b,c,4|X] Y [a,b,c,d]",
               "[a,b,c,4,5|X] Y [a,b,c,d]",
               "X [a|Y] Z",
               "[a|X] [b|Y] Z",
               "[a|X] [b|X] Z",
               "[a,b|X] [d,e|Y] Z",
               "[a,b|X] [d,e|X] Z",
               "X Y Z",
               "X X Z",
               "X Y X"};
    // TODO "X Y Y"
    // TODO "X X X"
    [TestMethod]
    public void Test()
    {
        foreach(var s in strings)
        {
            var args = s.Split(' ');
            PREDICATE_ASSERT.AssertArgs(args[0], args[1], args[2]);
        }
    }

    readonly string[] vs = new string[] {
               "length(X,1000),length(Y,1000),numbervars(X:Y),append(X,Y,Z).",
               "length(X,1000),length(Y,1000),append(X,Y,Z),numbervars(X:Y).",
               "length(X,1000),length(Y,1000),length(Z,2000),append(X,Y,Z),numbervars(Z).",
               "length(X,1000),length(Y,1000),length(Z,2000),numbervars(Z),append(X,Y,Z).",
               "length(X,1000),length(Y,1000),length(Z,2000),numbervars(X:Y),append(X,Y,Z).",
   };
    [TestMethod]
    public void TestLongLists()
    {
        foreach(var query in vs)
        {
            PREDICATE_ASSERT.AssertQuery(query);
        }
    }
}
