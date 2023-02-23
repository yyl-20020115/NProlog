/*
 * Copyright 2020 S. Webber
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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate.Udp;

public interface KeyFactory
{
    object CreateKey(int[] positions, Term[] args);
}


public class KeyFactories
{
    private static readonly KeyFactory[] FACTORIES = {
               new KeyFactory0(),
               new KeyFactory1(),
               new KeyFactory2(),
               new KeyFactory3(),
    };

    public static readonly int MAX_ARGUMENTS_PER_INDEX = FACTORIES.Length - 1;


    public static KeyFactory GetKeyFactory(int numArgs) => FACTORIES[numArgs];

    public class KeyFactory0 : KeyFactory
    {
        public KeyFactory0() { }
        public object CreateKey(int[] positions, Term[] args) => new();

    }
    public class KeyFactory1 : KeyFactory
    {
        public object CreateKey(int[] positions, Term[] args) =>
            // if only one indexable term than rely on its hashCode and Equals to be the key
            args[positions[0]];
    }

    public class KeyFactory2 : KeyFactory
    {
        public static Key2 CreateKey(int[] positions, Term[] args) => new (args[positions[0]], args[positions[1]]);

        object KeyFactory.CreateKey(int[] positions, Term[] args) => CreateKey(positions, args);
    }

    public class Key2
    {
        readonly Term t1;
        readonly Term t2;
        readonly int hashCode;

        public Key2(Term t1, Term t2)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.hashCode = t1.GetHashCode() + (7 * t2.GetHashCode());
        }

        public override int GetHashCode() => hashCode;
        public override bool Equals(object? o)
            => o is Key2 k && t1.Equals(k.t1) && t2.Equals(k.t2);
    }

    public class KeyFactory3 : KeyFactory
    {
        public static Key3 CreateKey(int[] positions, Term[] args) => new (args[positions[0]], args[positions[1]], args[positions[2]]);

        object KeyFactory.CreateKey(int[] positions, Term[] args) => CreateKey(positions, args);
    }

    public class Key3
    {
        readonly Term t1;
        readonly Term t2;
        readonly Term t3;
        readonly int hashCode;

        public Key3(Term t1, Term t2, Term t3)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
            this.hashCode = t1.GetHashCode() + (7 * t2.GetHashCode()) + (11 * t3.GetHashCode());
        }

        public override int GetHashCode() => hashCode;

        public override bool Equals(object? o) 
            => o is Key3 k && t1.Equals(k.t1) && t2.Equals(k.t2) && t3.Equals(k.t3);
    }
}
