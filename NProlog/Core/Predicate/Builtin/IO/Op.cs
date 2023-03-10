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
using Org.NProlog.Core.Terms;
namespace Org.NProlog.Core.Predicate.Builtin.IO;


/* TEST
'~'(X,Y) :- X>Y-4, X<Y+4.

%TRUE op(1000,xfx,'~')

%TRUE 4 ~ 7
%TRUE 7 ~ 7
%TRUE 10 ~ 7
%FAIL 11 ~ 7
%FAIL 3 ~ 7

% Example of invalid arguments
%?- op(X,xfx,'><')
%ERROR Expected Numeric but got: VARIABLE with value: X

%?- op(1000,Y,'><')
%ERROR Expected an atom but got: VARIABLE with value: Y

%?- op(1000,xfx,Z)
%ERROR Expected an atom but got: VARIABLE with value: Z

%?- op(1000,zfz,'><')
%ERROR Cannot add operand with associativity of: zfz as the only values allowed are: [xfx, xfy, yfx, fx, fy, xf, yf]

% Create some prefix and postfix operators for the later examples below.

%TRUE op(550, fy, 'fyExample')
%TRUE op(650, fx, 'fxExample')
%TRUE op(600, yf, 'yfExample')
%TRUE op(500, xf, 'xfExample')

% Example of nested prefix operators.

%?- X = fxExample fyExample fyExample a, write_canonical(X), nl
%OUTPUT
%fxExample(fyExample(fyExample(a)))
%
%OUTPUT
% X=fxExample fyExample fyExample a

% Example of a postfix operator.

%?- X = 123 yfExample, write_canonical(X), nl
%OUTPUT
%yfExample(123)
%
%OUTPUT
% X=123 yfExample

% Example of nested postfix operators.

%?- X = a xfExample yfExample yfExample, write_canonical(X), nl
%OUTPUT
%yfExample(yfExample(xfExample(a)))
%
%OUTPUT
% X=a xfExample yfExample yfExample

% Example of combining post and prefix operators where the postfix operator has the higher precedence.

%?- X = fyExample a yfExample, write_canonical(X), nl
%OUTPUT
%yfExample(fyExample(a))
%
%OUTPUT
% X=fyExample a yfExample

% Example of combining post and prefix operators where the prefix operator has the higher precedence.

%TRUE op(700, fy, 'fyExampleB')

%?- X = fyExampleB a yfExample, write_canonical(X), nl
%OUTPUT
%fyExampleB(yfExample(a))
%
%OUTPUT
% X=fyExampleB a yfExample

% Examples of how an "x" in an associativity (i.e. "fx" or "xf") means that the argument can contain operators of only a lower level of priority than the operator represented by "f".

%?- X = a xfExample xfExample
%ERROR Invalid postfix: xfExample 500 and term: xfExample 500 Line: X = a xfExample xfExample.

%?- X = fxExample fxExample a
%ERROR Invalid prefix: fxExample level: 650 greater than current level: 649 Line: X = fxExample fxExample a.
*/
/**
 * <code>op(X,Y,Z)</code>
 * <p>
 * Allows functors (names of predicates) to be defined as "operators". The use of operators allows syntax to be easier
 * to write and read. <code>Z</code> is the atom that we want to be an operator, <code>X</code> is the precedence class
 * (an integer), and <code>Y</code> the associativity specifier. e.g. <code>op(1200,xfx,':-')</code>
 * </p>
 */
public class Op : AbstractSingleResultPredicate
{

    protected override bool Evaluate(Term arg1, Term arg2, Term arg3)
    {
        var precedence = TermUtils.ToInt(arg1);
        var associativity = TermUtils.GetAtomName(arg2);
        var name = TermUtils.GetAtomName(arg3);
        Operands.AddOperand(name, associativity, precedence);
        return true;
    }
}
