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

/**
 * Maintains a collection of {@link ExpressionFactory} instances.
 * <p>
 * This class provides a mechanism for "plugging in" or "injecting" implementations of {@code org.projog.clp.Expression}
 * at runtime. This mechanism provides an easy way to configure and extend the CLP expressions supported by Projog.
 * </p>
 * <p>
 * Each {@link org.projog.core.kb.KnowledgeBase} has at most one unique {@code ClpExpressions} instance.
 * </p>
 */
public class ExpressionFactories {
   private readonly KnowledgeBase kb;
   private readonly object _lock = new object();
   private readonly Dictionary<PredicateKey, string> factoryClassNames = new ();
   private readonly Dictionary<PredicateKey, ExpressionFactory> factoryInstances = new ();

   public ExpressionFactories(KnowledgeBase kb) {
      this.kb = kb;
   }

   /**
    * Associates a {@link ExpressionFactory} with this {@code KnowledgeBase}.
    *
    * @param key The name and arity to associate the {@code ExpressionFactory} with.
    * @param operatorClassName The class name of the {@code ExpressionFactory} to be associated with {@code key}.
    * @throws ProjogException if there is already a {@code ExpressionFactory} associated with the {@code PredicateKey}
    */
   public void addExpressionFactory(PredicateKey key, string operatorClassName) {
      lock (_lock) {
         if (factoryClassNames.ContainsKey(key)) {
            throw new ProjogException("Already defined CLP expression: " + key);
         } else {
            factoryClassNames.Add(key, operatorClassName);
         }
      }
   }

   public Expression toExpression(Term t, HashSet<ClpVariable> vars) {
      switch (t.Type) {
         case VARIABLE:
            ClpVariable c = new ClpVariable();
            t.Unify(c);
            vars.Add(c);
            return c;
         case CLP_VARIABLE:
            ClpVariable e = (ClpVariable) t.getTerm();
            vars.Add(e);
            return e;
         case INTEGER:
            return new FixedValue(castToNumeric(t).getLong());
         case ATOM:
         case STRUCTURE:
            var key = PredicateKey.CreateForTerm(t);
            var factory = getExpressionFactory(key);
            Expression[] args = new Expression[t.NumberOfArguments];
            for (int i = 0; i < args.Length; i++) {
               args[i] = toExpression(t.GetArgument(i), vars);
            }
            return factory.createExpression(args);
         default:
            throw new ProjogException("Cannot get CLP expression for term: " + t + " of type: " + t.Type);
      }
   }

   private ExpressionFactory getExpressionFactory(PredicateKey key) {
      ExpressionFactory e = factoryInstances.get(key);
      if (e != null) {
         return e;
      } else if (factoryClassNames.ContainsKey(key)) {
         return instantiateExpressionFactory(key);
      } else {
         throw new ProjogException("Cannot find CLP expression: " + key);
      }
   }

   private ExpressionFactory instantiateExpressionFactory(PredicateKey key) {
      lock (_lock) {
         ExpressionFactory factory = factoryInstances.get(key);
         if (factory == null) {
            factory = instantiateExpressionFactory(factoryClassNames.get(key));
            factoryInstances.Add(key, factory);
         }
         return factory;
      }
   }

   private ExpressionFactory instantiateExpressionFactory(string className) {
      try {
         return KnowledgeBaseUtils.instantiate(kb, className);
      } catch (Exception e) {
         throw new SystemException("Could not create new ExpressionFactory using: " + className, e);
      }
   }
}
