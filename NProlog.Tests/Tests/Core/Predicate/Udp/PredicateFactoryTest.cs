/*
 * Copyright 2021 S. Webber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a Copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

[TestClass]
public class PredicateFactoryTest
{
    public class Pf : PredicateFactory
    {

        public bool IsRetryable => true;

        public Predicate GetPredicate(Term[] args) => throw new InvalidOperationException();
        public bool IsAlwaysCutOnBacktrack => false;

    }
    /** Assert default implementation of PredicateFactory.isAlwaysCutOnBacktrack */
    [TestMethod]
    public void TestIsAlwaysCutOnBacktrack() => Assert.IsFalse(new Pf().IsAlwaysCutOnBacktrack);
}
