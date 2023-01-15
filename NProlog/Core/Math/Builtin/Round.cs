/*
 * Copyright 2018 S. Webber
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

namespace Org.NProlog.Core.Math.Builtin;


/* TEST
%?- X is integer(7.0)
% X=7
%?- X is integer(7.1)
% X=7
%?- X is integer(7.2)
% X=7
%?- X is integer(7.3)
% X=7
%?- X is integer(7.4)
% X=7
%?- X is integer(7.49999)
% X=7
%?- X is integer(7.5)
% X=8
%?- X is integer(7.50001)
% X=8
%?- X is integer(7.6)
% X=8
%?- X is integer(7.7)
% X=8
%?- X is integer(7.8)
% X=8
%?- X is integer(7.9)
% X=8
%?- X is integer(8.0)
% X=8

%?- X is integer(-7.0)
% X=-7
%?- X is integer(-7.1)
% X=-7
%?- X is integer(-7.2)
% X=-7
%?- X is integer(-7.3)
% X=-7
%?- X is integer(-7.4)
% X=-7
%?- X is integer(-7.49999)
% X=-7
% Note: in some Prolog implementations the result of "integer(-7.5)" would be -8
%?- X is integer(-7.5)
% X=-7
%?- X is integer(-7.50001)
% X=-8
%?- X is integer(-7.6)
% X=-8
%?- X is integer(-7.7)
% X=-8
%?- X is integer(-7.8)
% X=-8
%?- X is integer(-7.9)
% X=-8
%?- X is integer(-8.0)
% X=-8

%?- X is integer(1.25+6.25)
% X=8

%?- X is integer(1.25+6.24)
% X=7

%?- X is integer(0.0)
% X=0

%?- X is integer(7)
% X=7

%?- X is integer(-7)
% X=-7

%?- X is integer(9223372036854775806)
% X=9223372036854775806
%?- X is integer(-9223372036854775807)
% X=-9223372036854775807
*/
/**
 * <code>integer(X)</code> - round X to the nearest integer value.
 */
public class Round : AbstractArithmeticOperator
{

    public override Numeric Calculate(Numeric n)
        => n.Type == TermType.INTEGER 
            ? n : IntegerNumberCache.ValueOf((long)System.Math.Round(n.Double));
}
