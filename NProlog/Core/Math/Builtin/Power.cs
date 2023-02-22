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
%?- X is 2 ** 1
% X=2

%?- X is 2 ** 2
% X=4

%?- X is 2 ** 5
% X=32

%?- X is 5 ** 3
% X=125

%?- X is 5.0 ** 3
% X=125.0

%?- X is 5 ** 3.0
% X=125.0

%?- X is 5.0 ** 3.0
% X=125.0

%?- X is 2 + 5 ** 3 - 1
% X=126

%?- X is -2 ** 2
% X=4

% Note: in some Prolog implementations the result would be 0.25
%?- X is -2 ** -2
% X=0

% Note: in some Prolog implementations the result would be 0.25
%?- X is 2 ** -2
% X=0

%?- X is 0.5 ** 2
% X=0.25

% Note: "^" is a synonym for "**".
%?- X is 3^7
% X=2187
*/
/**
 * <code>**</code> - calculates the result of the first argument raised to the power of the second argument.
 */
public class Power : AbstractBinaryArithmeticOperator
{

    protected override double CalculateDouble(double n1, double n2)
        => System.Math.Pow(n1, n2);


    protected override long CalculateLong(long n1, long n2)
        => (long)System.Math.Pow(n1, n2);
}
