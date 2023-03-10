p2(1).
p2(2).
p2(3).

p(a).
p(b) :- p2(3).
p(c).

p(x, y).

% Enable exhaustive tracing.

%TRUE trace

% Run queries - note debug information for both p and p2.

%?- p(X)
%OUTPUT
%[1] CALL p(X)
%[1] EXIT p(a)
%
%OUTPUT
% X=a
%OUTPUT
%[1] REDO p(a)
%[2] CALL p2(3)
%[2] EXIT p2(3)
%[1] EXIT p(b)
%
%OUTPUT
% X=b
%OUTPUT
%[1] REDO p(b)
%[1] EXIT p(c)
%
%OUTPUT
% X=c

%?- p(d)
%OUTPUT
%[1] CALL p(d)
%[1] FAIL p(d)
%
%OUTPUT

% Disable exhaustive tracing.

%TRUE notrace

% Re-run same queries - note no debug information.

%?- p(X)
% X=a
% X=b
% X=c

%FAIL p(d)

% Add spypoint

%TRUE debugging

%TRUE spy(p)

%?- debugging
%OUTPUT
%p/1
%p/2
%
%OUTPUT
%YES

% Run query - note debug information for p only.

%?- p(X)
%OUTPUT
%[1] CALL p(X)
%[1] EXIT p(a)
%
%OUTPUT
% X=a
%OUTPUT
%[1] REDO p(a)
%[1] EXIT p(b)
%
%OUTPUT
% X=b
%OUTPUT
%[1] REDO p(b)
%[1] EXIT p(c)
%
%OUTPUT
% X=c

%?- p(d)
%OUTPUT
%[1] CALL p(d)
%[1] FAIL p(d)
%
%OUTPUT

% Remove spypoint.

%TRUE nospy(p)

%TRUE debugging

% Add spypoints then remove all of them at once with 'nodebug'.

%TRUE spy(p)
%TRUE spy(p2)
%?- debugging
%OUTPUT
%p/1
%p/2
%p2/1
%
%OUTPUT
%YES
%TRUE nodebug
%TRUE debugging

% If you provide "spy" with an atom it will apply spypoints to all predicates with that name.
% To be more specific you can provide a structure in order to only apply a spypoint for the predicate with both the specified name (functor) and number of arguments (arity).

%TRUE spy('/'(p,1))

%?- debugging
%OUTPUT
%p/1
%
%OUTPUT
%YES

%TRUE nodebug

% Spy points can only be applied to atoms and structures

%?- spy(1)
%ERROR Expected an atom or a structure but got a INTEGER with value: 1
