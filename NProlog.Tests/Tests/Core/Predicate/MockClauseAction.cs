using Org.NProlog.Core.Predicate.Udp;
using Org.NProlog.Core.Terms;

namespace Org.NProlog.Core.Predicate;

public class MockClauseAction : ClauseAction
{
    protected Predicate predicate = new MockPredicate();
    protected ClauseModel model = ClauseModel.CreateClauseModel(new Atom("a"));
    public ClauseModel Model => this.model;

    public bool IsRetryable => true;

    public bool IsAlwaysCutOnBacktrack => true;

    public Predicate GetPredicate(Term[] input) => this.predicate;
}
