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
public class LengthTest
{
    private static readonly string LENGTH_PROLOG =
                //
                "length_(L, N) :- length_(L, 0, N)." +
                "length_(L,_,_) :- nonvar(L), \\+ L==[], \\+ L=[_|_], _ is _+1." +
                "length_(_,_,N) :- nonvar(N), \\+ number(N), _ is N+1." +
                "length_([],N,N)." +
                "length_([_|L],N0,N) :- (var(N);number(N)), N1 is N0+1,length_(L, N1, N).";

    private static readonly ListPredicateAssert PREDICATE_ASSERT
         = new ListPredicateAssert("length", 2, LENGTH_PROLOG);
    private static readonly string[] value = new string[]{
        "[] X",
               "[] -1",
               "[] 0",
               "[] 1",
               "[a] X",
               "[a] -1",
               "[a] 0",
               "[a] 1",
               "[a] 2",
               "[a] 3",
               "[a] 4",
               "[a,b] X",
               "[a,b] -1",
               "[a,b] 0",
               "[a,b] 1",
               "[a,b] 2",
               "[a,b] 3",
               "[a,b] 4",
               "[a,b,c] X",
               "[X,b,c] X",
               "[a,X,c] X",
               "[a,b,X] X",
               "[X,X,X] X",
               "[a,b,c] -1",
               "[a,b,c] 0",
               "[a,b,c] 1",
               "[a,b,c] 2",
               "[a,b,c] 3",
               "[a,b,c] 4",
               "[a|X] Y",
               "[Y|X] Y",
               "[a,b|X] Y",
               "[a,b,c|X] Y",
               "[Y,b,c|X] Y",
               "[a,Y,c|X] Y",
               "[a,b,Y|X] Y",
               "[Y,Y,Y|X] Y",
               "[X] Y",
               "[X] X",
               "[X|Y] X",
               "[X|X] X",
               "[a|X] X",
               "[a,b,c|X] X",
               "a X",
               "X a",
               "X Y",
               "X X",};

    [TestMethod]
    public void Test()
    {
        foreach (var item in value)
        {
            var args = item.Split(' ');

            PREDICATE_ASSERT.AssertArgs(args[0], args[1]);
        }
    }
}
