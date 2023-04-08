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
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;


/**
 * Represents all predicates that a {@code KnowledgeBase} has no definition of.
 * <p>
 * Always fails to evaluate successfully.
 *
 * @see Predicates#getPredicateFactory(PredicateKey)
 * @see Predicates#getPredicateFactory(Term)
 */
public class UnknownPredicate : PreprocessablePredicateFactory
{
    private readonly KnowledgeBase kb;
    private readonly PredicateKey key;
    private PredicateFactory? actualPredicateFactory;
    
    public UnknownPredicate(KnowledgeBase kb, PredicateKey key)
    {
        this.kb = kb;
        this.key = key;
    }


    public Predicate GetPredicate(Term[] args)
    {
        InstantiatePredicateFactory();

        return actualPredicateFactory == null 
            ? PredicateUtils.FALSE : actualPredicateFactory.GetPredicate(args);
    }


    public PredicateFactory Preprocess(Term arg)
    {
        InstantiatePredicateFactory();

        return actualPredicateFactory == null
          ? this
          : actualPredicateFactory is PreprocessablePredicateFactory factory ? factory.Preprocess(arg) : actualPredicateFactory;
    }

    private void InstantiatePredicateFactory()
    {
        if (actualPredicateFactory != null) return;

        lock (key)
        {
            if (actualPredicateFactory == null)
            {
                var pf = kb.Predicates.GetPredicateFactory(key);
                if (pf is UnknownPredicate)
                {
                    kb.PrologListeners.NotifyWarn("Not defined: " + key);
                }
                else
                {
                    actualPredicateFactory = pf;
                }
            }
        }
    }

    public bool IsRetryable => true;

}
