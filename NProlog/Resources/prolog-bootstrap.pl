% prolog-bootstrap.pl
% This file Contains Prolog syntax that is interpreted when a projog console is started.
% This file Contains code that configures the projog environment with
% "core" built-in predicates (e.g. "true", "consult", etc.) and numerical operations (e.g. "+", "-", etc.).
% It also defines operators in order to provide a more convenient syntax for writing terms.
% This file is included in the projog-core.jar that Contains the projog class files.
% This file can be overridden by providing another file named "projog-bootstrap.pl" 
% in the root directory where the console is launched, or in the classpath before the projog-core.jar.
% See http://projog.org/javadoc/org/projog/core/KnowledgeBaseUtils.html#bootstrap(Org.NProlog.Core.KnowledgeBase)

'?-'( pl_add_predicate('/'(op, 3), 'Org.NProlog.Core.Predicate.Builtin.IO.Op') ).
'?-'( op(1200, fx, '?-') ).
?- op(400, yfx, '/').

% bool
?- pl_add_predicate(true/0, 'Org.NProlog.Core.Predicate.Builtin.Bool.True').
?- pl_add_predicate(fail/0, 'Org.NProlog.Core.Predicate.Builtin.Bool.Fail').

% classify
?- pl_add_predicate(var/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsVar').
?- pl_add_predicate(nonvar/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsNonVar').
?- pl_add_predicate(atom/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsAtom').
?- pl_add_predicate(number/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsNumber').
?- pl_add_predicate(atomic/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsAtomic').
?- pl_add_predicate(integer/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsInteger').
?- pl_add_predicate(float/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsFloat').
?- pl_add_predicate(compound/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsCompound').
?- pl_add_predicate(is_list/1, 'Org.NProlog.Core.Predicate.Builtin.Classify.IsList').
?- pl_add_predicate(char_type/2, 'Org.NProlog.Core.Predicate.Builtin.Classify.CharType').

% compare
?- pl_add_predicate('='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.Equal').
?- pl_add_predicate('=='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.StrictEquality').
?- pl_add_predicate('\\=='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NotStrictEquality').
?- pl_add_predicate('=:='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericEquality').
?- pl_add_predicate('=\\='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericInequality').
?- pl_add_predicate('<'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericLessThan').
?- pl_add_predicate('=<'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericLessThanOrEqual').
?- pl_add_predicate('>'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericGreaterThan').
?- pl_add_predicate('>='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NumericGreaterThanOrEqual').
?- pl_add_predicate('@<'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.TermLessThan').
?- pl_add_predicate('@>'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.TermGreaterThan').
?- pl_add_predicate('@>='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.TermGreaterThanOrEqual').
?- pl_add_predicate('@=<'/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.TermLessThanOrEqual').
?- pl_add_predicate('\\='/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.NotUnifiable').
?- pl_add_predicate(compare/3, 'Org.NProlog.Core.Predicate.Builtin.Compare.Compare').
?- pl_add_predicate(predsort/3, 'Org.NProlog.Core.Predicate.Builtin.Compare.PredSort').
?- pl_add_predicate(between/3, 'Org.NProlog.Core.Predicate.Builtin.Compare.Between').
?- pl_add_predicate(unify_with_occurs_check/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.UnifyWithOccursCheck').
?- pl_add_predicate(is/2, 'Org.NProlog.Core.Predicate.Builtin.Compare.Is').

% compound
?- pl_add_predicate(','/2, 'Org.NProlog.Core.Predicate.Builtin.Compound.Conjunction').
?- pl_add_predicate(';'/2, 'Org.NProlog.Core.Predicate.Builtin.Compound.Disjunction').
?- pl_add_predicate('/'('\\+', 1), 'Org.NProlog.Core.Predicate.Builtin.Compound.Not').
?- pl_add_predicate(not/1, 'Org.NProlog.Core.Predicate.Builtin.Compound.Not').
?- pl_add_predicate(call/1, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/2, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/3, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/4, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/5, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/6, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/7, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/8, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/9, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(call/10, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(time/1, 'Org.NProlog.Core.Predicate.Builtin.Compound.Call').
?- pl_add_predicate(once/1, 'Org.NProlog.Core.Predicate.Builtin.Compound.Once').
?- pl_add_predicate(bagof/3, 'Org.NProlog.Core.Predicate.Builtin.Compound.BagOf').
?- pl_add_predicate(findall/3, 'Org.NProlog.Core.Predicate.Builtin.Compound.FindAll').
?- pl_add_predicate(setof/3, 'Org.NProlog.Core.Predicate.Builtin.Compound.SetOf').
?- pl_add_predicate('->'/2, 'Org.NProlog.Core.Predicate.Builtin.Compound.IfThen').
?- pl_add_predicate(limit/2, 'Org.NProlog.Core.Predicate.Builtin.Compound.Limit').

% construct
?- pl_add_predicate(functor/3, 'Org.NProlog.Core.Predicate.Builtin.Construct.Functor').
?- pl_add_predicate(arg/3, 'Org.NProlog.Core.Predicate.Builtin.Construct.Arg').
?- pl_add_predicate('=..'/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.Univ').
?- pl_add_predicate(atom_chars/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.TermSplit/AtomChars').
?- pl_add_predicate(atom_codes/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.TermSplit/AtomCodes').
?- pl_add_predicate(number_chars/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.TermSplit/NumberChars').
?- pl_add_predicate(number_codes/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.TermSplit/NumberCodes').
?- pl_add_predicate(atom_concat/3, 'Org.NProlog.Core.Predicate.Builtin.Construct.AtomConcat').
?- pl_add_predicate(numbervars/1, 'Org.NProlog.Core.Predicate.Builtin.Construct.NumberVars').
?- pl_add_predicate(numbervars/3, 'Org.NProlog.Core.Predicate.Builtin.Construct.NumberVars').
?- pl_add_predicate(copy_term/2, 'Org.NProlog.Core.Predicate.Builtin.Construct.CopyTerm').

% debug
?- pl_add_predicate(debugging/0, 'Org.NProlog.Core.Predicate.Builtin.Debug.Debugging').
?- pl_add_predicate(nodebug/0, 'Org.NProlog.Core.Predicate.Builtin.Debug.NoDebug').
?- pl_add_predicate(trace/0, 'Org.NProlog.Core.Predicate.Builtin.Debug.Trace').
?- pl_add_predicate(notrace/0, 'Org.NProlog.Core.Predicate.Builtin.Debug.NoTrace').
?- pl_add_predicate(spy/1, 'Org.NProlog.Core.Predicate.Builtin.Debug.AlterSpyPoint/Spy').
?- pl_add_predicate(nospy/1, 'Org.NProlog.Core.Predicate.Builtin.Debug.AlterSpyPoint/NoSpy').

% io
?- pl_add_predicate(close/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Close').
?- pl_add_predicate(current_input/1, 'Org.NProlog.Core.Predicate.Builtin.IO.CurrentInput').
?- pl_add_predicate(seeing/1, 'Org.NProlog.Core.Predicate.Builtin.IO.CurrentInput').
?- pl_add_predicate(see/1, 'Org.NProlog.Core.Predicate.Builtin.IO.See').
?- pl_add_predicate(seen/0, 'Org.NProlog.Core.Predicate.Builtin.IO.Seen').
?- pl_add_predicate(tab/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Tab').
?- pl_add_predicate(tell/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Tell').
?- pl_add_predicate(told/0, 'Org.NProlog.Core.Predicate.Builtin.IO.Told').
?- pl_add_predicate(current_output/1, 'Org.NProlog.Core.Predicate.Builtin.IO.CurrentOutput').
?- pl_add_predicate(get_char/1, 'Org.NProlog.Core.Predicate.Builtin.IO.GetChar').
?- pl_add_predicate(get_code/1, 'Org.NProlog.Core.Predicate.Builtin.IO.GetCode').
?- pl_add_predicate(get0/1, 'Org.NProlog.Core.Predicate.Builtin.IO.GetCode').
?- pl_add_predicate(nl/0, 'Org.NProlog.Core.Predicate.Builtin.IO.NewLine').
?- pl_add_predicate(open/3, 'Org.NProlog.Core.Predicate.Builtin.IO.Open').
?- pl_add_predicate(put_char/1, 'Org.NProlog.Core.Predicate.Builtin.IO.PutChar').
?- pl_add_predicate(read/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Read').
?- pl_add_predicate(set_input/1, 'Org.NProlog.Core.Predicate.Builtin.IO.SetInput').
?- pl_add_predicate(set_output/1, 'Org.NProlog.Core.Predicate.Builtin.IO.SetOutput').
?- pl_add_predicate(write/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Write/DoWrite').
?- pl_add_predicate(writeln/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Write/DoWriteLn').
?- pl_add_predicate(write_canonical/1, 'Org.NProlog.Core.Predicate.Builtin.IO.WriteCanonical').
?- pl_add_predicate(writef/1, 'Org.NProlog.Core.Predicate.Builtin.IO.Writef').
?- pl_add_predicate(writef/2, 'Org.NProlog.Core.Predicate.Builtin.IO.Writef').

% Kb (knowledge base)
?- pl_add_predicate(pl_add_arithmetic_operator/2, 'Org.NProlog.Core.Predicate.Builtin.Kb.AddArithmeticOperator').
?- pl_add_predicate(arithmetic_function/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.AddUserDefinedArithmeticOperator').
?- pl_add_predicate(asserta/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Assert/AssertA').
?- pl_add_predicate(assertz/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Assert/AssertZ').
?- pl_add_predicate(assert/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Assert/AssertZ').
?- pl_add_predicate(listing/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Listing').
?- pl_add_predicate(clause/2, 'Org.NProlog.Core.Predicate.Builtin.Kb.Inspect/inspectClause').
?- pl_add_predicate(retract/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Inspect/retract').
?- pl_add_predicate(retractall/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.RetractAll').
?- pl_add_predicate(consult/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.Consult').
?- pl_add_predicate('.'/2, 'Org.NProlog.Core.Predicate.Builtin.Kb.ConsultList').
?- pl_add_predicate(ensure_loaded/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.EnsureLoaded').
?- pl_add_predicate(flag/3, 'Org.NProlog.Core.Predicate.Builtin.Kb.Flag').
?- pl_add_predicate(current_predicate/1, 'Org.NProlog.Core.Predicate.Builtin.Kb.CurrentPredicate').
?- pl_add_predicate('/'('dynamic', 1), 'Org.NProlog.Core.Predicate.Builtin.Kb.Dynamic').

% Db (recorded database)
?- pl_add_predicate(erase/1, 'Org.NProlog.Core.Predicate.Builtin.Db.Erase').
?- pl_add_predicate(recorded/2, 'Org.NProlog.Core.Predicate.Builtin.Db.Recorded').
?- pl_add_predicate(recorded/3, 'Org.NProlog.Core.Predicate.Builtin.Db.Recorded').
?- pl_add_predicate(recorda/2, 'Org.NProlog.Core.Predicate.Builtin.Db.InsertRecord/RecordA').
?- pl_add_predicate(recorda/3, 'Org.NProlog.Core.Predicate.Builtin.Db.InsertRecord/RecordA').
?- pl_add_predicate(recordz/2, 'Org.NProlog.Core.Predicate.Builtin.Db.InsertRecord/RecordZ').
?- pl_add_predicate(recordz/3, 'Org.NProlog.Core.Predicate.Builtin.Db.InsertRecord/RecordZ').

% flow control
?- pl_add_predicate(repeat/0, 'Org.NProlog.Core.Predicate.Builtin.Flow.RepeatInfinitely').
?- pl_add_predicate(repeat/1, 'Org.NProlog.Core.Predicate.Builtin.Flow.RepeatSetAmount').
?- pl_add_predicate('!'/0, 'Org.NProlog.Core.Predicate.Builtin.Flow.Cut').

% list
?- pl_add_predicate(length/2, 'Org.NProlog.Core.Predicate.Builtin.List.Length').
?- pl_add_predicate(reverse/2, 'Org.NProlog.Core.Predicate.Builtin.List.Reverse').
?- pl_add_predicate(member/2, 'Org.NProlog.Core.Predicate.Builtin.List.Member').
?- pl_add_predicate(memberchk/2, 'Org.NProlog.Core.Predicate.Builtin.List.MemberCheck').
?- pl_add_predicate(min_list/2, 'Org.NProlog.Core.Predicate.Builtin.List.ExtremumList/MinList').
?- pl_add_predicate(max_list/2, 'Org.NProlog.Core.Predicate.Builtin.List.ExtremumList/MaxList').
?- pl_add_predicate(append/3, 'Org.NProlog.Core.Predicate.Builtin.List.Append').
?- pl_add_predicate(append/2, 'Org.NProlog.Core.Predicate.Builtin.List.AppendListOfLists').
?- pl_add_predicate(subtract/3, 'Org.NProlog.Core.Predicate.Builtin.List.SubtractFromList').
?- pl_add_predicate(keysort/2, 'Org.NProlog.Core.Predicate.Builtin.List.KeySort').
?- pl_add_predicate(flatten/2, 'Org.NProlog.Core.Predicate.Builtin.List.Flatten').
?- pl_add_predicate(sort/2, 'Org.NProlog.Core.Predicate.Builtin.List.SortAsSet').
?- pl_add_predicate(msort/2, 'Org.NProlog.Core.Predicate.Builtin.List.Sort').
?- pl_add_predicate(delete/3, 'Org.NProlog.Core.Predicate.Builtin.List.Delete').
?- pl_add_predicate(subset/2, 'Org.NProlog.Core.Predicate.Builtin.List.Subset').
?- pl_add_predicate(select/3, 'Org.NProlog.Core.Predicate.Builtin.List.Select').
?- pl_add_predicate(nth0/3, 'Org.NProlog.Core.Predicate.Builtin.List.Nth/Nth0').
?- pl_add_predicate(nth1/3, 'Org.NProlog.Core.Predicate.Builtin.List.Nth/Nth1').
?- pl_add_predicate(nth/3, 'Org.NProlog.Core.Predicate.Builtin.List.Nth/Nth1').
?- pl_add_predicate(maplist/2, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/3, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/4, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/5, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/6, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/7, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/8, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/9, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(maplist/10, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/2, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/3, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/4, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/5, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/6, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/7, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/8, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/9, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(checklist/10, 'Org.NProlog.Core.Predicate.Builtin.List.MapList').
?- pl_add_predicate(include/3, 'Org.NProlog.Core.Predicate.Builtin.List.SubList').
?- pl_add_predicate(sublist/3, 'Org.NProlog.Core.Predicate.Builtin.List.SubList').
?- pl_add_predicate(foldl/4, 'Org.NProlog.Core.Predicate.Builtin.List.Fold').
?- pl_add_predicate(last/2, 'Org.NProlog.Core.Predicate.Builtin.List.Last').
?- pl_add_predicate(atomic_list_concat/2, 'Org.NProlog.Core.Predicate.Builtin.List.AtomicListConcat').
?- pl_add_predicate(atomic_list_concat/3, 'Org.NProlog.Core.Predicate.Builtin.List.AtomicListConcat').
?- pl_add_predicate(pairs_keys/2, 'Org.NProlog.Core.Predicate.Builtin.List.PairsElements/Keys').
?- pl_add_predicate(pairs_values/2, 'Org.NProlog.Core.Predicate.Builtin.List.PairsElements/Values').

% clp
?- pl_add_predicate(in/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.In').
?- pl_add_predicate(ins/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.In').
?- pl_add_predicate(label/1, 'Org.NProlog.Core.Predicate.Builtin.Clp.Resolve').
?- pl_add_predicate(all_different/1, 'Org.NProlog.Core.Predicate.Builtin.Clp.Distinct').
?- pl_add_predicate(all_distinct/1, 'Org.NProlog.Core.Predicate.Builtin.Clp.Distinct').
?- pl_add_predicate('#<'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/LessThan').
?- pl_add_predicate('#>'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/GreaterThan').
?- pl_add_predicate('#=<'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/LessThanOrEqualTo').
?- pl_add_predicate('#>='/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/GreaterThanOrEqualTo').
?- pl_add_predicate('#='/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/EqualTo').
?- pl_add_predicate('#\\='/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.NumericConstraintPredicate/NotEqualTo').
?- pl_add_predicate('#<==>'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/Equivalent').
?- pl_add_predicate('#==>'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/LeftImpliesRight').
?- pl_add_predicate('#<=='/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/RightImpliesLeft').
?- pl_add_predicate('#/\\'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/And').
?- pl_add_predicate('#\\/'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/Or').
?- pl_add_predicate('/'('#\\', 2), 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/Xor').
?- pl_add_predicate('/'('#\\', 1), 'Org.NProlog.Core.Predicate.Builtin.Clp.BooleanConstraintPredicate/Not').
?- pl_add_predicate(pl_add_clp_expression/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.AddExpressionFactory').
?- pl_add_clp_expression('+'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Add').
?- pl_add_clp_expression('/'('-', 2), 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Subtract').
?- pl_add_clp_expression('*'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Multiply').
?- pl_add_clp_expression('//'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Divide').
?- pl_add_clp_expression('min'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Minimum').
?- pl_add_clp_expression('max'/2, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Maximum').
?- pl_add_clp_expression('abs'/1, 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Absolute').
?- pl_add_clp_expression('/'('-', 1), 'Org.NProlog.Core.Predicate.Builtin.Clp.CommonExpression/Minus').

% time
?- pl_add_predicate(get_time/1, 'Org.NProlog.Core.Predicate.Builtin.Time.GetTime').
?- pl_add_predicate(convert_time/2, 'Org.NProlog.Core.Predicate.Builtin.Time.ConvertTime').

% numerical operations
?- pl_add_arithmetic_operator('+'/2, 'Org.NProlog.Core.Math.Builtin.Add').
?- pl_add_arithmetic_operator('/'('-', 1), 'Org.NProlog.Core.Math.Builtin.Minus').
?- pl_add_arithmetic_operator('/'('-', 2), 'Org.NProlog.Core.Math.Builtin.Subtract').
?- pl_add_arithmetic_operator('/'/2, 'Org.NProlog.Core.Math.Builtin.Divide').
?- pl_add_arithmetic_operator('//'/2, 'Org.NProlog.Core.Math.Builtin.IntegerDivide').
?- pl_add_arithmetic_operator('*'/2, 'Org.NProlog.Core.Math.Builtin.Multiply').
?- pl_add_arithmetic_operator('**'/2, 'Org.NProlog.Core.Math.Builtin.Power').
?- pl_add_arithmetic_operator('^'/2, 'Org.NProlog.Core.Math.Builtin.Power').
?- pl_add_arithmetic_operator(mod/2, 'Org.NProlog.Core.Math.Builtin.Modulo').
?- pl_add_arithmetic_operator(rem/2, 'Org.NProlog.Core.Math.Builtin.Remainder').
?- pl_add_arithmetic_operator(random/1, 'Org.NProlog.Core.Math.Builtin.Random').
?- pl_add_arithmetic_operator(integer/1, 'Org.NProlog.Core.Math.Builtin.Round').
?- pl_add_arithmetic_operator('/\\'/2, 'Org.NProlog.Core.Math.Builtin.BitwiseAnd').
?- pl_add_arithmetic_operator('\\/'/2, 'Org.NProlog.Core.Math.Builtin.BitwiseOr').
?- pl_add_arithmetic_operator(xor/2, 'Org.NProlog.Core.Math.Builtin.BitwiseXor').
?- pl_add_arithmetic_operator('<<'/2, 'Org.NProlog.Core.Math.Builtin.ShiftLeft').
?- pl_add_arithmetic_operator('>>'/2, 'Org.NProlog.Core.Math.Builtin.ShiftRight').
?- pl_add_arithmetic_operator(max/2, 'Org.NProlog.Core.Math.Builtin.Max').
?- pl_add_arithmetic_operator(min/2, 'Org.NProlog.Core.Math.Builtin.Min').
?- pl_add_arithmetic_operator(abs/1, 'Org.NProlog.Core.Math.Builtin.Abs').

% definite clause grammers (DCG)
?- op(1200, xfx, '-->').
?- op(901, fx, '{').
?- op(900, xf, '}').

% operators
?- op(1200, xfx, ':-').
?- op(1200, fx, ':-').
?- op(1100, fx, dynamic).
?- op(1100, xfy, ';').
?- op(1050, xfy, '->').
?- op(1000, xfy, ',').
?- op(900, fy, '\\+').
?- op(700, xfx, '=').
?- op(700, xfx, '==').
?- op(700, xfx, '=:=').
?- op(700, xfx, '=\\=').
?- op(700, xfx, '=..').
?- op(700, xfx, '<').
?- op(700, xfx, '>').
?- op(700, xfx, '=<').
?- op(700, xfx, '>=').
?- op(700, xfx, '@<').
?- op(700, xfx, '@=<').
?- op(700, xfx, '@>').
?- op(700, xfx, '@>=').
?- op(700, xfx, '\\=').
?- op(700, xfx, '\\==').
?- op(700, xfx, is).
?- op(700, xfx, in).
?- op(700, xfx, ins).
?- op(700, xfx, '#=').
?- op(700, xfx, '#\\=').
?- op(700, xfx, '#<').
?- op(700, xfx, '#>').
?- op(700, xfx, '#=<').
?- op(700, xfx, '#>=').
?- op(760, yfx, '#<==>').
?- op(750, xfy, '#==>').
?- op(750, xfy, '#<==').
?- op(720, yfx, '#/\\').
?- op(740, yfx, '#\\/').
?- op(730, xfy, '#\\').
?- op(710, fy, '#\\').
?- op(600, xfy, '..').
?- op(600, xfy, ':').
?- op(500, yfx, '+').
?- op(500, yfx, '-').
?- op(400, yfx, '*').
?- op(400, yfx, '**').
?- op(400, yfx, '^').
?- op(400, yfx, '//').
?- op(400, yfx, mod).
?- op(400, yfx, rem).
?- op(400, yfx, '/\\').
?- op(400, yfx, '\\/').
?- op(400, yfx, xor).
?- op(400, yfx, '<<').
?- op(400, yfx, '>>').
?- op(200, fy, '-').
