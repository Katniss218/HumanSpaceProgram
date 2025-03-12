grammar DFDSL;

// main

program
    : statement* EOF
    ;

//
// parser tokens
//

// statements 'do something'.

statement
    : assignment SEMI
    | transformation SEMI
    ;

assignment
    : path ('=' expression)*
    ;

transformation
    : LPAREN transformation_header+ RPAREN
     (LBRACKET statement* RBRACKET)?
    ;
    
transformation_header
    : TRANSFORMATION_IDENTIFIER expression (',' expression)*
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
    | KEY_SEGMENT // string literal is the same as key
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
    | KEY_SEGMENT
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


INT 
    : [0-9]+
    ;

RANGE
    : INT? '..' INT?
    ;

ROOT_SEGMENT
    : 'this' | 'any'
    ;

KEY_SEGMENT
    : '"' (~["\\] | '\\' .)* '"'
    ;

TRANSFORMATION_IDENTIFIER
    : [a-zA-Z_] [a-zA-Z0-9_]*
    ;

WS  : [ \t\r\n]+ -> skip;