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



/** An implementation of {@code ConstraintStore} for use in Projog. */
public class CoreConstraintStore : ConstraintStore {
   private readonly List<Constraint> queue = new ();
   private readonly HashSet<Constraint> matched = new ();

    public CoreConstraintStore() {
   }

    public CoreConstraintStore(Constraint c) {
      queue.Add(c);
   }

    public CoreConstraintStore(List<Constraint> c) {
      queue.AddRange(c);
   }

    public bool resolve() {
      while (queue.Count > 0) {
         Constraint c = queue.Remove(0);
         if (!matched.Contains(c)) {
            ConstraintResult result = c.enforce(this);
            if (result == ConstraintResult.FAILED) {
               return false;
            }
            if (result == ConstraintResult.MATCHED) {
               matched.Add(c);
            }
         }
      }
      return true;
   }

   
   public long getMin(Expression id) {
      throw new InvalidOperationException();
   }

   
   public long getMax(Expression id) {
      throw new InvalidOperationException();
   }

   
   public ExpressionResult setValue(Expression id, long value) {
      return update(id, v => v.setValue(value));
   }

   
   public ExpressionResult setMin(Expression id, long min) {
      return update(id, v => v.setMin(min));
   }

   
   public ExpressionResult setMax(Expression id, long max) {
      return update(id, v => v.setMax(max));
   }

   
   public ExpressionResult setNot(Expression id, long not) {
      return update(id, v => v.setNot(not));
   }

   // TODO can this be more efficient? copying ClpVariable every time and backtracking on failure
   private ExpressionResult update(Expression id, Function<VariableState, VariableStateResult> f) {
      ClpVariable original = ((ClpVariable) id).Term;
      ClpVariable copy = original.copy();
      VariableStateResult r = f.apply(copy.State);
      if (r == VariableStateResult.UPDATED) {
         addConstraints(copy);
      } else if (r == VariableStateResult.FAILED) {
         original.Backtrack();
      }
      return r == VariableStateResult.FAILED ? ExpressionResult.INVALID : ExpressionResult.VALID;
   }

   private void addConstraints(ClpVariable copy) {
      foreach (Constraint c in copy.Constraints) {
         if (!matched.Contains(c) && !queue.Contains(c)) {
            queue.Add(c);
         }
      }
   }
}
