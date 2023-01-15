using Org.NProlog.Core.Math;
using Org.NProlog.Core.Terms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NProlog.Tests.Tests.Core.Math;

public class MockArithmeticOperator : ArithmeticOperator
{
    public Numeric Calculate(Term[] args) => new IntegerNumber(0);
}
public class MockPreprocessableArithmeticOperator : MockArithmeticOperator, PreprocessableArithmeticOperator
{
    public ArithmeticOperator Preprocess(Term arg) => this;
}
