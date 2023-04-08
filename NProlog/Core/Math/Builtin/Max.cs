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
namespace Org.NProlog.Core.Math.Builtin;


/* TEST
%?- X is max(5,5)
% X=5
%?- X is max(7,8)
% X=8
%?- X is max(3,2)
% X=3
%?- X is max(2.5,2.5)
% X=2.5
%?- X is max(2.75,2.5)
% X=2.75
%?- X is max(1,1.5)
% X=1.5
%?- X is max(2,1.5)
% X=2
%?- X is max(-3,2)
% X=2
%?- X is max(-3,-2)
% X=-2
%?- X is max(-2.5,-2.25)
% X=-2.25
%?- X is max(0,0)
% X=0
%?- X is max(0.0,0.0)
% X=0.0
%?- X is max(0,0.0)
% X=0.0
%?- X is max(0.0,0)
% X=0
*/
/**
 * <code>max</code> - finds the maximum of two numbers.
 */
public class Max : AbstractArithmeticOperator
{
    public override Numeric Calculate(Numeric? n1, Numeric? n2) 
        => n1.Double > n2.Double ? n1 : n2;
}
