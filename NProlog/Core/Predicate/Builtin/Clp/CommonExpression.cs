/*
 * Copyright 2022 S. Webber
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
using System.Linq.Expressions;

namespace Org.NProlog.Core.Predicate.Builtin.Clp;



public class CommonExpression : ExpressionFactory {
   private readonly Function<Expression[], Expression> function;

   public static ExpressionFactory add() {
      return new CommonExpression(args => new Add(args[0], args[1]));
   }

   public static ExpressionFactory subtract() {
      return new CommonExpression(args => new Subtract(args[0], args[1]));
   }

   public static ExpressionFactory multiply() {
      return new CommonExpression(args => new Multiply(args[0], args[1]));
   }

   public static ExpressionFactory divide() {
      return new CommonExpression(args => new Divide(args[0], args[1]));
   }

   public static ExpressionFactory minimum() {
      return new CommonExpression(args => new Minimum(args[0], args[1]));
   }

   public static ExpressionFactory maximum() {
      return new CommonExpression(args => new Maximum(args[0], args[1]));
   }

   public static ExpressionFactory absolute() {
      return new CommonExpression(args => new Absolute(args[0]));
   }

   public static ExpressionFactory minus() {
      return new CommonExpression(args => new Minus(args[0]));
   }

   private CommonExpression(Function<Expression[], Expression> function) {
      this.function = function;
   }

   
   public Expression createExpression(Expression[] args) {
      return function.apply(args);
   }
}
