grammar FunctEngine;

// Un programa es una secuencia de sentencias
program: block EOF;

// Un bloque es una secuencia de sentencias entre llaves o simplemente una sentencia
block: '{' statement* '}' | statement;

// Tipos de sentencias
statement:
      variableDeclaration ';'
    | assignment ';'
    | functionCall ';'
    | forStatement
    | whileStatement
    ;

// Declaración de variable: var x = 10;
variableDeclaration: 'var' IDENTIFIER ('=' expression)?;

// Asignación: x = 20;
assignment: IDENTIFIER '=' expression;

// Bucle 'for': for(var i=0; i < 10; i++) { ... }
forStatement: 'for' '(' forInit? ';' expression? ';' forUpdate? ')' block;

// Inicialización del for: puede ser declaración o asignación
forInit: variableDeclaration | assignment;

// Actualización del for: solo asignación
forUpdate: assignment;

// Bucle 'while': while(x < 10) { ... }
whileStatement: 'while' '(' expression ')' block;

// Expresiones (ordenadas por precedencia de menor a mayor)
expression:
    orExpression
    ;

orExpression:
    andExpression ('||' andExpression)*
    ;

andExpression:
    comparisonExpression ('&&' comparisonExpression)*
    ;

comparisonExpression:
    addSubExpression (('<'|'>'|'<='|'>='|'=='|'!=') addSubExpression)*
    ;

addSubExpression:
    multDivExpression (('+'|'-') multDivExpression)*
    ;

multDivExpression:
    atom (('*'|'/') atom)*
    ;

// La unidad más pequeña de una expresión
atom:
      '(' expression ')' // Expresiones agrupadas
    | functionCall
    | literal
    | IDENTIFIER
    ;

functionCall: IDENTIFIER '(' argumentList? ')';
argumentList: expression (',' expression)*;

literal:
      NUMBER
    | STRING
    | BOOLEAN
    | arrayLiteral
    | NULL
    ;

arrayLiteral: '[' argumentList? ']';

// --- TOKENS (REGLAS DEL LEXER) ---

// Palabras clave
FOR:    'for';
WHILE:  'while';
VAR:    'var';
NULL:   'null';
BOOLEAN:'true' | 'false';

// Identificadores y literales
IDENTIFIER: [a-zA-Z_][a-zA-Z_0-9]*;
NUMBER:     [0-9]+ ('.' [0-9]+)?; // Soporta enteros y decimales
STRING:     ('"' ( ~["\r\n\\] | '\\' . )* '"') | ('\'' ( ~['\r\n\\] | '\\' . )* '\''); // Comillas dobles o simples con escapado

// Operadores de comparación (tokens más largos primero)
GTE:    '>=';
LTE:    '<=';
EQUALS: '==';
NOTEQUALS: '!=';
GT:     '>';
LT:     '<';

// Operadores lógicos
AND:    '&&';
OR:     '||';

// Operadores aritméticos y de asignación
ASSIGN: '=';
ADD:    '+';
SUB:    '-';
MUL:    '*';
DIV:    '/';

// Delimitadores
LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
LBRACK: '[';
RBRACK: ']';
SEMI:   ';';
COMMA:  ',';

// Ignorar espacios en blanco y comentarios
WS:         [ \t\r\n]+ -> skip;
COMMENT:    '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
