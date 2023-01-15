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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Clp;

/* TEST
%?- X #/\ Y
% X=1
% Y=1

%?- X #\/ Y, label([X,Y])
% X = 0
% Y = 1
% X = 1
% Y = 0
% X = 1
% Y = 1
%NO

%?- X #\ Y, label([X,Y])
% X = 0
% Y = 1
% X = 1
% Y = 0
%NO

%?- #\ X
% X = 0

%?- X #<==> Y, label([X,Y])
% X = 0
% Y = 0
% X = 1
% Y = 1
%NO

%?- X #==> Y, label([X,Y])
% X = 0
% Y = 0
% X = 0
% Y = 1
% X = 1
% Y = 1
%NO

%?- X #<== Y, label([X,Y])
% X = 0
% Y = 0
% X = 1
% Y = 0
% X = 1
% Y = 1
%NO

%?- X in 4..5, Y in 5..6, Z#<==> X#<Y, label([X,Y,Z])
% X = 4
% Y = 5
% Z = 1
% X = 4
% Y = 6
% Z = 1
% X = 5
% Y = 5
% Z = 0
% X = 5
% Y = 6
% Z = 1
%NO

%?- X in 2..3, X#<==>Y
%NO

%?- X #<==> Append(_,_,_)
%ERROR Cannot create CLP constraint from term: Append(_, _, _)
*/
/**
 * CLP predicates for comparing bool values.
 * <p>
 * Integer values are used to represent bool values. 1 repesents true and 0 represents false.
 * <ul>
 * <li><code>#/\</code> and</li>
 * <li><code>#\/</code> or</li>
 * <li><code>#\</code> exclusive or</li>
 * <li><code>#&lt;==&gt;</code> equivalent</li>
 * <li><code>#==&gt;</code> implication</li>
 * </ul>
 */
public class BooleanConstraintPredicate : PredicateFactory, ConstraintFactory, KnowledgeBaseConsumer {
   public static BooleanConstraintPredicate equivalent() {
      return new BooleanConstraintPredicate(args => new Equivalent(args[0], args[1]));
   }

   public static BooleanConstraintPredicate leftImpliesRight() {
      return new BooleanConstraintPredicate(args => new Implication(args[0], args[1]));
   }

   public static BooleanConstraintPredicate rightImpliesLeft() {
      return new BooleanConstraintPredicate(args => new Implication(args[1], args[0]));
   }

   public static BooleanConstraintPredicate and() {
      return new BooleanConstraintPredicate(args => new And(args[0], args[1]));
   }

   public static BooleanConstraintPredicate or() {
      return new BooleanConstraintPredicate(args => new Or(args[0], args[1]));
   }

   public static BooleanConstraintPredicate xor() {
      return new BooleanConstraintPredicate(args => new Xor(args[0], args[1]));
   }

   public static BooleanConstraintPredicate not() {
      return new BooleanConstraintPredicate(args => new Not(args[0]));
   }

   private readonly Func<Constraint[], Constraint> constraintGenerator;
   private Predicates predicates;

   private BooleanConstraintPredicate(Func<Constraint[], Constraint> constraintGenerator) {
      this.constraintGenerator = constraintGenerator;
   }

   
   public Predicate GetPredicate(Term[] args) {
      HashSet<ClpVariable> vars = new ();
      Constraint[] constraints = new Constraint[args.Length];
      for (int i = 0; i < args.Length; i++) {
         constraints[i] = toConstraint(args[i], vars);
      }
      Constraint rule = constraintGenerator.apply(constraints);
      foreach (ClpVariable c in vars) {
         if (c.State.isCorrupt()) {
            return PredicateUtils.FALSE;
         }

         c.addConstraint(rule);
      }
      return PredicateUtils.toPredicate(new CoreConstraintStore(rule).resolve());
   }

   private Constraint toConstraint(Term t, HashSet<ClpVariable> vars) {
      switch (t.Type) {
         case VARIABLE:
            ClpVariable c = new ClpVariable();
            restrictValues(c);
            t.Unify(c);
            vars.Add(c);
            return c;
         case CLP_VARIABLE:
            ClpVariable e = (ClpVariable) t.Term;
            restrictValues(e);
            vars.Add(e);
            return e;
         case INTEGER:
            return new FixedValue(TermUtils.CastToNumeric(t).Long);
         case ATOM:
         case TokenType.STRUCTURE:
            PredicateKey key = PredicateKey.CreateForTerm(t);
            PredicateFactory factory = predicates.GetPredicateFactory(key);
            if (factory is ConstraintFactory) {
               return ((ConstraintFactory) factory).CreateConstraint(t.Args, vars);
            } else {
               throw new ProjogException("Cannot create CLP constraint from term: " + t);
            }
         default:
            throw new ProjogException("Cannot get CLP expression for term: " + t + " of type: " + t.Type);
      }
   }

   private void restrictValues(ClpVariable c) {
      VariableState s = c.State;
      if (s.setMin(0) != VariableStateResult.FAILED) {
         s.setMax(1);
      }
   }


    public bool IsRetryable => false;

    public KnowledgeBase KnowledgeBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Constraint CreateConstraint(Term[] args, HashSet<ClpVariable> vars) {
      Constraint[] constraints = new Constraint[args.Length];
      for (int i = 0; i < args.Length; i++) {
         constraints[i] = toConstraint(args[i], vars);
      }
      return constraintGenerator.Apply(constraints);
   }
}
