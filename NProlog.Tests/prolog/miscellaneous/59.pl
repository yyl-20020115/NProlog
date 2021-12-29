a(X,Y,Z):- 
 repeat(3),
 X='text in "double quotes"', write(X), nl,
 Y='text in ''single quote''', write(Y), nl,
 Z='!"$%^&*()-_+-=[]{}:;@''#~?/.>,<\\|`', write(Z), nl.

%?- a(X,Y,Z), !
%OUTPUT
%text in "double quotes"
%text in 'single quote'
%!"$%^&*()-_+-=[]{}:;@'#~?/.>,<\|`
%
%OUTPUT
% X=text in "double quotes"
% Y=text in 'single quote'
% Z=!"$%^&*()-_+-=[]{}:;@'#~?/.>,<\|`
%NO
