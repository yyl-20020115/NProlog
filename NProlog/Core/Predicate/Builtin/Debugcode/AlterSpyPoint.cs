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
using Org.NProlog.Core.Exceptions;
using Org.NProlog.Core.Kb;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Builtin.Debug;




/* TEST
%LINK prolog-debugging
*/
/**
 * <code>spy(X)</code> / <code>nospy(X)</code> - add or Remove a spy point for a predicate.
 * <p>
 * <code>spy(X)</code> - add a spy point for a predicate. By adding a spy point for the predicate name instantiated to
 * <code>X</code> the programmer will be informed how it is used in the resolution of a goal.
 * </p>
 * <p>
 * <code>nospy(X)</code> - removes a spy point for a predicate. By removing a spy point for the predicate name
 * instantiated to <code>X</code> the programmer will no longer be informed how it is used in the resolution of a goal.
 * </p>
 */
public class AlterSpyPoint : AbstractSingleResultPredicate
{
    public static AlterSpyPoint Spy() => new (true);

    public static AlterSpyPoint NoSpy() => new (false);

    private readonly bool valueToSetSpyPointTo;

    /**
     * The {@code valueToSetSpyPointTo} parameter specifies whether spy points matched by the {@link #evaluate(Term)}
     * method should be enabled or disabled.
     *
     * @param valueToSetSpyPointTo {@code true} to enable spy points, {@code false} to disable spy points
     */
    private AlterSpyPoint(bool valueToSetSpyPointTo)
    {
        this.valueToSetSpyPointTo = valueToSetSpyPointTo;
    }


    protected override bool Evaluate(Term t) => t.Type switch
    {
        var tt when tt == TermType.ATOM =>
            SetSpyPoints(KnowledgeBaseUtils.GetPredicateKeysByName(KnowledgeBase, t.Name)),
        var tt when tt == TermType.STRUCTURE =>
            SetSpyPoint(PredicateKey.CreateFromNameAndArity(t)),
        _ => throw new PrologException("Expected an atom or a structure but got a " + t.Type + " with value: " + t),
    };

    private bool SetSpyPoints(List<PredicateKey> keys)
    {
        foreach (PredicateKey key in keys)
        {
            SetSpyPoint(key);
        }
        return true;
    }

    private bool SetSpyPoint(PredicateKey key)
    {
        SpyPoints.SetSpyPoint(key, valueToSetSpyPointTo);
        return true;
    }
}
