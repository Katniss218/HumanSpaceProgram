grammar DFDSL;

// main

program
    : statement_or_transformation* EOF
    ;

//
// parser tokens
//

// statements 'do something'.

statement_or_transformation
    : statement
    | transformation
    ;

statement
    : assignment SEMI
    ;

assignment
    : path ('=' expression)*
    ;

transformation
    : LPAREN transformation_header+ RPAREN
     (LBRACKET statement_or_transformation* RBRACKET)
    ;
    
transformation_header
    : FOR_TRANSFORMATION_IDENTIFIER path
    | WHERE_TRANSFORMATION_IDENTIFIER expression
    ;

// expressions return a value.

expression
    : logical
    ;

// operators listed in order of precedence. Lowest to highest.
// operators at the bottom execute first.

logical
    : equality ((AND | LAND | OR | LOR | XOR) equality)*
    ;

equality
    : comparison ((EQ | NEQ) comparison)*
    ;

comparison
    : shift ((LT | LTE | GT | GTE) shift)*
    ;

shift
    : addition ((LSHIFT | RSHIFT) addition)*
    ;

addition
    : multiplication ((PLUS | MINUS) multiplication)*
    ;

multiplication
    : exponentiation ((ASTERISK | DIV | MOD | FLOORDIV) exponentiation)*
    ;

exponentiation
    : unary ((EXP) unary)*
    ;

unary
    : (PLUS | MINUS | NOT) unary
    | primary
    ;

primary
    : LPAREN logical RPAREN
    | value
    ;

value
    : VALUEOF path
    | transformation
    | literal
    ;

literal
    : TRUE
    | FALSE
    | NULL
    | QUOTED_STRING
    | number
    ;

number
    : MINUS? INT ('.' INT)?
    (('e'|'E') (PLUS|MINUS) INT)?
    ;

// paths (used to address data elements)

path
    : segment ('.' segment)*
    ;

segment
    : ROOT_SEGMENT
    | QUOTED_STRING
    | arraySegment
    ;

arraySegment
    : '[' (INT | RANGE | ASTERISK) ']'
    ;


//
// lexer tokens
//

LPAREN: '(';
RPAREN: ')';
LBRACKET: '{';
RBRACKET: '}';
SEMI : ';';

PLUS        : '+';
MINUS       : '-';
ASTERISK    : '*';
DIV    : '/';
FLOORDIV: '//';
MOD    : '%';
EXP     : '**';

LSHIFT: '<<';
RSHIFT: '>>';

INC : '++';
DEC : '--';

VALUEOF : '$';

TRUE : 'true';
FALSE: 'false';

AND : '&';
OR  : '|';
XOR : '^';
NOT: '!';

LAND: '&&';
LOR : '||';

GT  : '>';
GTE : '>=';
LT  : '<';
LTE : '<=';
EQ  : '==';
NEQ : '!=';

NULL
    : 'null'
    ;

INT 
    : [0-9]+
    ;

QUOTED_STRING
    : '"' (~["\\] | '\\' .)* '"'
    ;

RANGE
    : INT? '..' INT?
    ;

ROOT_SEGMENT
    : 'this' | 'any'
    ;

FOR_TRANSFORMATION_IDENTIFIER
    : 'FOR'
    ;

WHERE_TRANSFORMATION_IDENTIFIER
    : 'WHERE'
    ;

WS  : [ \t\r\n]+ -> skip;