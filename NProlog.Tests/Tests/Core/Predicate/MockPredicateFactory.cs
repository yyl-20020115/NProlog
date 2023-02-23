using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;
public class MockPredicate : Predicate
{
    public bool CouldReevaluationSucceed => true;

    public bool Evaluate() => true;
}
public class MockPredicateFactory : AbstractSingleResultPredicate, PredicateFactory
{
    public MockPredicateFactory() { }
    protected override bool Evaluate() => true;
    protected override bool Evaluate(Term arg) => true;
    protected override bool Evaluate(Term arg1, Term arg2) => true;
    protected override bool Evaluate(Term arg1, Term arg2, Term arg3) => true;
    protected override bool Evaluate(Term arg1, Term arg2, Term arg3, Term arg4) => true;
    public override bool Evaluate(Term[] args) => true;
}
public class MockPreprocessablePredicateFactory : MockPredicateFactory, PreprocessablePredicateFactory
{
    public PredicateFactory Preprocess(Term arg) => this;
}

