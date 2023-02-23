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
using Org.NProlog.Core.Predicate.Builtin.Construct;

namespace Org.NProlog.Core.Predicate.Builtin.List;

[TestClass]
public class SelectTest
{
    private static readonly string SELECT_PROLOG =
                //
                "select_(X, [Head|Tail], Rest) :- select3_(Tail, Head, X, Rest)." +
                "select3_(Tail, Head, Head, Tail)." +
                "select3_([Head2|Tail], Head, X, [Head|Rest]) :- select3_(Tail, Head2, X, Rest).";

    private static readonly ListPredicateAssert PREDICATE_ASSERT = new ("select", 3, SELECT_PROLOG);
    readonly string[] vs1 = {
               "a [a,b,c] X",
               "b [a,b,c] X",
               "c [a,b,c] X",
               "z [a,b,c] X",
               "a [a,b,a,c,a] X",
               "c [a,b,c,c,c] X",
               "Y [q,w,e,r,t,y,u,i,o,p,a,s,d,f,g,h,j,k,l,z,x,c,v,b,n,m] X",
               "Y [_,w,e,r,t,y,u,i,o,p,a,s,d,f,g,h,j,k,l,z,x,c,v,b,n,m] X",
               "Y [q,w,e,r,t,y,u,i,o,p,a,s,d,f,g,h,j,k,l,z,x,c,v,b,n,_] X",
               "Y [q,_,e,_,t,_,_,_,o,p,a,s,d,f,g,_,j,_,l,z,_,c,_,b,n,_] X",
               "c [a,b,c,c] [a,b,c]",
               "z [a,b,c] [a,b,c]",
               "Q [a,b,c,d,e] [a,X,Y,Z]",
               "Q [a,b,c,d,e] [b,X,Y,Z]",
               "Q [a,b,c,d,e] [X,b,Y,Z]",
               "Q [a,b,c,d,e] [X,b,Y,d]",
               "Q [a,b,c,d,e] [W,X,Y,Z]",
               "Q [a,c,b,c,c,d,e] [T,W,X,X,Y,Z]",
               "a [x|X] [x,y,z]",
               "X Y Z",};
    // TODO "X X X"
    [TestMethod]
    // TODO more partial list examples
    public void Test()
    {
        foreach(var query in vs1)
        {
            var args = query.Split(' ');
            PREDICATE_ASSERT.AssertArgs(args[0], args[1], args[2]);
        }
    }
    readonly string[] vs2 = {
               "length(X,10000),numbervars(X),last(X,Last),select(Last,X,Result).",
               "length(X,10000),numbervars(X),last(X,Last),select(Last,X,Result).",};
    [TestMethod]
    public void TestLongLists()
    {
        foreach(var query in vs2)
            PREDICATE_ASSERT.AssertQuery(query);
    }
}
