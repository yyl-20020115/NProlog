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
namespace Org.NProlog.Core.Math.Builtin;


/* TEST
%?- X is 13 << 0
% X=13

%?- X is 13 << 1
% X=26

%?- X is 13 << 2
% X=52

%?- X is 13 << 3
% X=104

%?- X is 13 << 4
% X=208

% Note bit shifting using a negative value gives different results than in some other Prolog implementations.
%?- X is 13 << -1
% X=-9223372036854775808
*/
/**
 * <code>&lt;&lt;</code> - left shift bits.
 */
public class ShiftLeft : AbstractBinaryIntegerArithmeticOperator
{
    // DONE for both this class and ShiftRight, review what happens when the second argument is negative
    protected override long CalculateLong(long n1, long n2) => n2 >= 0 ? (n1 << (int)n2) : (n1 >> -(int)n2);
}
