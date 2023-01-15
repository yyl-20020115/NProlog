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

namespace Org.NProlog.Core.Predicate.Builtin.Clp;


/* TEST
%?- X in 7..9, label([X])
% X=7
% X=8
% X=9
%NO

%?- X in 7..9, Y#=X*2, label([X])
% X=7
% Y=14
% X=8
% Y=16
% X=9
% Y=18
%NO

%?- Z#=X+Y, X in 7..9, Y in 4..5, label([X,Y])
% X=7
% Y=4
% Z=11
% X=8
% Y=4
% Z=12
% X=9
% Y=4
% Z=13
% X=7
% Y=5
% Z=12
% X=8
% Y=5
% Z=13
% X=9
% Y=5
% Z=14
%NO

%?- Z#=X+Y, X in 7..9, Y in 4..5, label([X,Y]), Z=12
% X = 8
% Y = 4
% Z = 12
% X = 7
% Y = 5
% Z = 12
%NO

%?- Vars=[X,Y,Z], all_different(Vars), Vars ins 1..2
% Vars=[1..2,1..2,1..2]
% X=1..2
% Y=1..2
% Z=1..2
%FAIL Vars=[X,Y,Z], all_different(Vars), Vars ins 1..2, label(Vars)

%?- X#=1, label([X])
% X=1
%?- X=1, label([X])
% X=1
%TRUE label([1])

%?- label(x)
%ERROR Expected LIST but got: ATOM with value: x
%?- label([x])
%ERROR Unexpected term of type: ATOM with value: x

%?- X#=Y, label([X,Y])
%ERROR java.lang.InvalidOperationException: Variables not sufficiently bound. Too many possibilities.
*/
/**
 * <code>label([X])</code> - assigns concrete values to the given CLP variables.
 */
public class Resolve : AbstractPredicateFactory {
   
   public Predicate getPredicate(Term arg) {
      ClpConstraintStore.Builder builder = new ClpConstraintStore.Builder();

      // find all variables in input argument, and all variables connected to them via constraints
      HashSet<ClpVariable> variablesList = getAllVariables(arg);
      if (variablesList.Count == 0) {
         return PredicateUtils.TRUE; // if no variables found then return now, as nothing to resolve
      }

      // map each ClpVariable to a projog-clp Variable
      // doing this as BruteForceSearch uses projog-clp's Variable rather than projog's ClpVariable
      Dictionary<ClpVariable, Variable> variablesSet = new ();
      foreach (ClpVariable v in variablesList) {
         variablesSet.computeIfAbsent(v, notUsed => builder.createVariable());
      }

      // for all constraints replace each ClpVariable with its corresponding Variable
      foreach (Constraint c in getConstraints(variablesList)) {
         Constraint replacement = c.replace(e => {
            if (e is ClpVariable) {
               Variable v = variablesSet.get(((ClpVariable) e).Term);
               if (v == null) {
                  throw new InvalidOperationException("Have no record of " + e + " in " + variablesSet);
               }
               return v;
            } else {
               return null;
            }
         });
         builder.addConstraint(replacement);
      }

      BruteForceSearch bruteForceSearch = createBruteForceSearch(builder, variablesSet);
      return new ClpResolvePredicate(bruteForceSearch, variablesSet);
   }

   /** find all variables in input argument, and all variables connected to them via constraints */
   private HashSet<ClpVariable> getAllVariables(Term arg) {
      HashSet<ClpVariable> variables = getVariablesFromInputArgument(arg);

      List<Constraint> queue = new (getConstraints(variables));
      HashSet<Constraint> processed = new ();
      while (queue.Count > 0) {
         Constraint constraint = queue.Remove(0);
         processed.Add(constraint);
         constraint.walk(e => {
            if (e is ClpVariable) {
               ClpVariable v = ((ClpVariable) e).Term;
               if (variables.Add(v)) {
                  foreach (Constraint c in v.Constraints) {
                     if (!processed.Contains(c)) { // to avoid infinite loop, only add each constraint once
                        queue.Add(c);
                     }
                  }
               }
            }
         });
      }

      return variables;
   }

   /** find all variables in the input argument - input argument could be a single variable or a list of variables */
   private HashSet<ClpVariable> getVariablesFromInputArgument(Term arg) {
      HashSet<ClpVariable> variables = new ();

      if (arg.Type == TermType.CLP_VARIABLE) {
         variables.Add((ClpVariable) arg.Term);
         return variables;
      }

      while (arg != EmptyList.EMPTY_LIST) {
         TermUtils.AssertType(arg, TermType.LIST);

         Term head = arg.GetArgument(0);
         if (head.Type == TermType.CLP_VARIABLE) {
            variables.Add((ClpVariable) head.Term);
         } else if (head.Type != TermType.INTEGER) {
            throw new ProjogException("Unexpected term of type: " + head.Type + " with value: " + head);
         }

         arg = arg.GetArgument(1);
      }

      return variables;
   }

   /** return all constraints of all of the given variables */
   private HashSet<Constraint> getConstraints(HashSet<ClpVariable> variables) {
      HashSet<Constraint> constraints = new ();

      foreach (ClpVariable v in variables) {
         constraints.UnionWith(v.Constraints);
      }

      return constraints;
   }

   private BruteForceSearch createBruteForceSearch(ClpConstraintStore.Builder builder, Dictionary<ClpVariable, Variable> variables) {
      ClpConstraintStore environment = builder.build();

      // for each variable set its minimum and maximum values
      foreach (Dictionary.Entry<ClpVariable, Variable> entry in variables.entrySet()) {
         entry.getValue().setMax(environment, entry.getKey().getMax(environment));
         entry.getValue().setMin(environment, entry.getKey().getMin(environment));
      }

      return new BruteForceSearch(environment);
   }

   private class  ClpResolvePredicate : Predicate {
      private readonly BruteForceSearch bruteForceSearch;
      private readonly Dictionary<ClpVariable, Variable> variables;

      private ClpResolvePredicate(BruteForceSearch bruteForceSearch, Dictionary<ClpVariable, Variable> variables) {
         this.bruteForceSearch = bruteForceSearch;
         this.variables = variables;
      }

      
      public bool Evaluate() {
         ClpConstraintStore result = next();

         if (result != null) {
            // need to Backtrack *all* variables before assigning to any, else constraints may fail
            foreach (ClpVariable v in variables.keySet()) {
               v.Backtrack();
            }

            foreach (Dictionary.Entry<ClpVariable, Variable> entry in variables.entrySet()) {
               long i = result.getValue(entry.getValue());
               // TODO don't do unify as forces resolve - add setvalue method instead
               if (!entry.getKey().Unify(IntegerNumberCache.ValueOf(i))) {
                  throw new SystemException(entry.getKey() + " " + entry.getKey().Type + " " + i);
               }
            }

            return true;
         } else {
            return false;
         }
      }

      private ClpConstraintStore next() {
         try {
            return bruteForceSearch.next();
         } catch (SystemException e) {
            throw new ProjogException(e.ToString(), e);
         }
      }


        public bool CouldReevaluationSucceed => true;
    }
}
