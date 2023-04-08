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
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Math;

public abstract class AbstractArithmeticOperator : PreprocessableArithmeticOperator, KnowledgeBaseConsumer
{
    protected KnowledgeBase knowledgeBase = new();
    protected ArithmeticOperators Operators 
        =>this.knowledgeBase.ArithmeticOperators;
    public AbstractArithmeticOperator() { }
    public KnowledgeBase KnowledgeBase
    {
        get => this.knowledgeBase;
        set => this.knowledgeBase = value;
    }

    public virtual Numeric Calculate(Term[] args) => args.Length switch
    {
        1 => Calculate(this.Operators.GetNumeric(args[0])),
        2 => Calculate(this.Operators.GetNumeric(args[0]), this.Operators.GetNumeric(args[1])),
        _ => throw CreateWrongNumberOfArgumentsException(args.Length),
    };

    public virtual Numeric Calculate(Numeric n) 
        => throw CreateWrongNumberOfArgumentsException(1);

    public virtual Numeric Calculate(Numeric n1, Numeric n2) 
        => throw CreateWrongNumberOfArgumentsException(2);

    private ArgumentException CreateWrongNumberOfArgumentsException(int numberOfArguments) 
        => throw new ("The ArithmeticOperator: "
            + this.GetType() + " does not accept the number of arguments: " + numberOfArguments);


    public virtual ArithmeticOperator Preprocess(Term expression)
    {
        if (!IsPure) return this;
        var arguments = expression.Args;
        return arguments.Length == 1
            ? PreprocessUnaryOperator(arguments[0])
            : arguments.Length == 2
                ? PreprocessBinaryOperator(arguments[0], arguments[1])
                : throw CreateWrongNumberOfArgumentsException(arguments.Length);
    }

    /**
     * Indicates if this operator is pure and so can be preprocessed.
     * <p>
     * An operator is pure if multiple calls with identical arguments always produce the same result.
     *
     * @return true if pure and so can be preprocessed, else false
     */
    public virtual bool IsPure => true;
    
    private ArithmeticOperator PreprocessUnaryOperator(Term argument)
    {
        var o = Operators?.GetPreprocessedArithmeticOperator(argument);
        return o is Numeric numeric ? Calculate(numeric) : o != null
            ? new PreprocessedUnaryOperator(this, o) : (ArithmeticOperator)this;
    }

    private ArithmeticOperator PreprocessBinaryOperator(Term argument1, Term argument2)
    {
        var o1 = Operators?.GetPreprocessedArithmeticOperator(argument1);
        var o2 = Operators?.GetPreprocessedArithmeticOperator(argument2);
        return o1 is Numeric numeric && o2 is Numeric numeric1
            ? Calculate(numeric, numeric1)
            : o1 != null || o2 != null 
            ? new PreprocessedBinaryOperator(this, o1, o2) : this;
    }

    public class PreprocessedUnaryOperator : ArithmeticOperator
    {
        readonly AbstractArithmeticOperator op;
        readonly ArithmeticOperator o;

        public PreprocessedUnaryOperator(AbstractArithmeticOperator op,ArithmeticOperator o)
        {
            this.o = o;
            this.op = op;
        }

        public virtual Numeric Calculate(Term[] args) 
            => op.Calculate(o.Calculate(args[0].Args));

    }

    public class PreprocessedBinaryOperator : ArithmeticOperator
    {
        readonly AbstractArithmeticOperator op;
        readonly ArithmeticOperator? o1;
        readonly ArithmeticOperator? o2;

        public PreprocessedBinaryOperator(AbstractArithmeticOperator op,ArithmeticOperator? o1, ArithmeticOperator? o2)
        {
            this.op = op;
            this.o1 = o1;
            this.o2 = o2;
        }

        public virtual Numeric Calculate(Term[] args) => op.Calculate(
                o1 == null ? op.Operators.GetNumeric(args[0]) : o1.Calculate(args[0].Args),
                o2 == null ? op.Operators.GetNumeric(args[1]) : o2.Calculate(args[1].Args));
    }
}
