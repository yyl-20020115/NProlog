/**
 * Provides a programming interface for Java applications to interact with Prolog.
 * <p>
 * As well as interacting with Prolog using the console application it is also possible to embed Prolog in your Java
 * applications. The steps required for applications to interact with Prolog are as follows:
 * <ul>
 * <li>Create a new {@link org.prolog.api.Prolog} instance.</li>
 * <li>Load in clauses and facts using {@link org.prolog.api.Prolog#consultFile(File)} or
 * {@link org.prolog.api.Prolog#consultReader(TextReader)}.</li>
 * <li>Create a {@link org.prolog.api.QueryStatement} by using
 * {@link org.prolog.api.Prolog#createStatement(string)}.</li>
 * <li>Create a {@link org.prolog.api.QueryResult} by using {@link org.prolog.api.QueryStatement#executeQuery()}.</li>
 * <li>Iterate through all possible solutions to the query by using {@link org.prolog.api.QueryResult#next()}.</li>
 * <li>For each solution get the {@link org.prolog.core.term.Term} instantiated to a
 * {@link org.prolog.core.term.Variable} in the query by calling
 * {@link org.prolog.api.QueryResult#getTerm(string)}.</li>
 * </ul>
 */
namespace Org.NProlog.Api;
