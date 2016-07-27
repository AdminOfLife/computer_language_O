/* �������������� (pars.c) */

#include <limits.h>
#include <string.h>
#include <stdio.h>

#include "scan.h"
#include "table.h"
#include "gen.h"
#include "error.h"
#include "ovm.h"

#define spABS    1
#define spMAX    2
#define spMIN    3
#define spDEC    4
#define spODD    5
#define spHALT   6
#define spINC    7
#define spInOpen 8
#define spInInt  9
#define spOutInt 10
#define spOutLn  11

static void StatSeq(void);
static tType Expression(void);

static void Check(tLex L, char* M) {
	if( Lex != L )
		Expected(M);
	else
		NextLex();
}

/* ["+" | "-"] (����� | ���) */
static int ConstExpr(void) {
	int v;
	tObj* X;
	tLex Op;

	Op = lexPlus;
	if( Lex == lexPlus || Lex == lexMinus ) {
		Op = Lex;
		NextLex();
	}
	if( Lex == lexNum ) {
		v = Num;
		NextLex();
	} else if( Lex == lexName ) {
		X = Find(Name);
		if( X->Cat == catGuard )
			Error((char*)"������ ���������� ��������� ����� ����");
		else if( X->Cat != catConst )
			Expected((char*)"��� ���������" );
		else {
			v = X->Val;
			NextLex();
		}
	} else
		Expected((char*) "����������� ���������" );
	if( Op == lexMinus )
		return -v;
	return v;
}

/* ��� "=" ���������� */
static void ConstDecl(void) {
	tObj* ConstRef; /* ������ �� ��� � ������� */
	ConstRef = NewName(Name, catGuard);
	NextLex();
	Check(lexEQ, (char*)"\"=\"");
	ConstRef->Val = ConstExpr();
	ConstRef->Typ = typInt; /* �������� ������ ����� ��� */
	ConstRef->Cat = catConst;
}

static void ParseType(void) {
	tObj* TypeRef;
	if( Lex != lexName )
		Expected((char*)"���");
	else {
		TypeRef = Find(Name);
		if( TypeRef->Cat != catType )
			Expected((char*)"��� ����");
		else if( TypeRef->Typ != typInt )
			Expected((char*)"����� ���");
		NextLex();
	}
}

/* ��� {"," ���} ":" ��� */
static void VarDecl(void) {
	tObj* NameRef;
	if( Lex != lexName )
		Expected((char*)"���");
	else {
		NameRef = NewName(Name, catVar);
		NameRef->Typ = typInt;
		NextLex();
	}
	while( Lex == lexComma ) {
		NextLex();
		if( Lex != lexName )
			Expected((char*)"���");
		else {
			NameRef = NewName(Name, catVar );
			NameRef->Typ = typInt;
			NextLex();
		}
	}
	Check(lexColon,(char*)"\":\"");
	ParseType();
}

/* {CONST {����������� ";"}
    |VAR {����������� ";"} } */
static void DeclSeq(void) {
	while( Lex == lexCONST || Lex == lexVAR ) {
		if( Lex == lexCONST ) {
			NextLex();
			while( Lex == lexName ) {
				ConstDecl(); /* ���������� ��������� */
				Check(lexSemi, (char*)"\";\"");
			}
		} else {
			NextLex(); /* VAR */
			while( Lex == lexName ) {
				VarDecl();   /* ���������� ���������� */
				Check(lexSemi,(char*)"\";\"");
			}
		}
	}
}

static void IntExpression(void) {
	if( Expression() != typInt )
		Expected((char*)"��������� ������ ����");
}

static tType StFunc(int F) {
	switch( F ) {
		case spABS:
			IntExpression();
			GenAbs();
			return typInt;
		case spMAX:
			ParseType();
			Gen(INT_MAX);
			return typInt;
		case  spMIN:
			ParseType();
			GenMin();
			return typInt;
		case spODD:
			IntExpression();
			GenOdd();
			return typBool;
	}
	return typNone; /* ���� �� ���� �������������� */
}

static tType Factor(void) {
	tObj* X;
	tType T;

	if( Lex == lexName ) {
		if( (X = Find(Name))->Cat == catVar ) {
			GenAddr(X);    /* ����� ���������� */
			Gen(cmLoad);
			NextLex();
			return X->Typ;
		} else if( X->Cat == catConst ) {
			GenConst(X->Val);
			NextLex();
			return X->Typ;
		} else if( X->Cat == catStProc && X->Typ != typNone ) {
			NextLex();
			Check(lexLPar, (char*)"\"(\"");
			T = StFunc(X->Val);
			Check(lexRPar, (char*)"\")\"");
		} else
			Expected((char*)
			         "����������, ��������� ��� ���������-�������"
			        );
	} else if( Lex == lexNum ) {
		GenConst(Num);
		NextLex();
		return typInt;
	} else if( Lex == lexLPar ) {
		NextLex();
		T = Expression();
		Check(lexRPar, (char*)"\")\"");
	} else
		Expected((char*)"���, ����� ��� \"(\"");
	return T;
}

static tType Term(void) {
	tLex Op;
	tType T = Factor();
	if( Lex == lexMult || Lex == lexDIV || Lex == lexMOD ) {
		if( T != typInt )
			Error((char*)"�������������� �������� ���� ��������");
		do {
			Op = Lex;
			NextLex();
			if( (T = Factor()) != typInt )
				Expected((char*)"��������� ������ ����");
			switch(Op) {
				case lexMult:
					Gen(cmMult);
					break;
				case lexDIV:
					Gen(cmDiv);
					break;
				case lexMOD:
					Gen(cmMod);
					break;
			}
		} while( Lex == lexMult || Lex == lexDIV || Lex == lexMOD );
	}
	return T;
}

/* ["+"|"-"] ��������� {�������� ���������} */
static tType SimpleExpr(void) {
	tType T;
	tLex Op;

	if( Lex == lexPlus || Lex == lexMinus ) {
		Op = Lex;
		NextLex();
		if( (T = Term()) != typInt )
			Expected((char*)"��������� ������ ����");
		if( Op == lexMinus )
			Gen(cmNeg);
	} else
		T = Term();
	if( Lex == lexPlus || Lex == lexMinus ) {
		if( T != typInt )
			Error((char*)"�������������� �������� ���� ��������");
		do {
			Op = Lex;
			NextLex();
			if( (T = Term()) != typInt )
				Expected((char*)"��������� ������ ����");
			switch(Op) {
				case lexPlus:
					Gen(cmAdd);
					break;
				case lexMinus:
					Gen(cmSub);
					break;
			}
		} while( Lex == lexPlus || Lex == lexMinus );
	}
	return T;
}

/* ������������ [��������� ������������] */
static tType Expression(void) {
	tLex  Op;
	tType T = SimpleExpr();
	if( Lex == lexEQ || Lex == lexNE || Lex == lexGT ||
	        Lex == lexGE || Lex == lexLT || Lex == lexLE ) {
		Op = Lex;
		if( T != typInt )
			Error((char*)"�������������� �������� ���� ��������");
		NextLex();
		if( (T = SimpleExpr()) != typInt )
			Expected((char*)"��������� ������ ����");
		GenComp(Op);   /* ��������� ��������� ��������*/
		T = typBool;
	} /* ����� ��� ����� ���� ������� �������� ��������� */
	return T;
}

/* ���������� = ��� */
static void Variable(void) {
	tObj* X;

	if( Lex != lexName )
		Expected((char*)"���");
	else {
		if( (X = Find(Name))->Cat != catVar )
			Expected((char*)"��� ����������");
		GenAddr(X);
		NextLex();
	}
}

static void StProc(int P) {
	switch( P ) {
		case spDEC:
			Variable();
			Gen(cmDup);
			Gen(cmLoad);
			if( Lex == lexComma ) {
				NextLex();
				IntExpression();
			} else
				Gen(1);
			Gen(cmSub);
			Gen(cmSave);
			return;
		case spINC:
			Variable();
			Gen(cmDup);
			Gen(cmLoad);
			if( Lex == lexComma ) {
				NextLex();
				IntExpression();
			} else
				Gen(1);
			Gen(cmAdd);
			Gen(cmSave);
			return;
		case spInOpen:
			/* ����� */
			;
			return;
		case spInInt:
			Variable();
			Gen(cmIn);
			Gen(cmSave);
			return;
		case spOutInt:
			IntExpression();
			Check(lexComma ,(char*) "\",\"");
			IntExpression();
			Gen(cmOut);
			return;
		case spOutLn:
			Gen(cmOutLn);
			return;
		case spHALT:
			GenConst(ConstExpr());
			Gen(cmStop);
			return;
	}
}

static void BoolExpression(void) {
	if( Expression() != typBool )
		Expected((char*)"���������� ���������");
}

/* ���������� "=" ����� */
static void AssStatement(void) {
	Variable();
	if( Lex == lexAss ) {
		NextLex();
		IntExpression();
		Gen(cmSave);
	} else
		Expected((char*)"\":=\"");
}

/* ��� ["(" { ����� | ���������� } ")"] */
static void CallStatement(int sp) {
	Check(lexName, (char*)"��� ���������");
	if( Lex == lexLPar ) {
		NextLex();
		StProc(sp);
		Check( lexRPar, (char*)"\")\"" );
	} else if( sp == spOutLn || sp == spInOpen )
		StProc(sp);
	else
		Expected((char*)"\"(\"");
}

static void IfStatement(void) {
	int CondPC;
	int LastGOTO;

	Check(lexIF, (char*)"IF");
	LastGOTO = 0;      /* ����������� �������� ���        */
	BoolExpression();
	CondPC = PC;       /* ������. ��������� ���. �������� */
	Check(lexTHEN, (char*)"THEN");
	StatSeq();
	while( Lex == lexELSIF ) {
		Gen(LastGOTO);  /* ��������� �����, �����������    */
		Gen(cmGOTO);    /* �� ����� ����������� ��������.  */
		LastGOTO = PC;  /* ��������� ����� GOTO            */
		NextLex();
		Fixup(CondPC);  /* ������. ����� ��������� ��������*/
		BoolExpression();
		CondPC = PC;    /* ������. ��������� ���. �������� */
		Check(lexTHEN, (char*)"THEN");
		StatSeq();
	}
	if( Lex == lexELSE ) {
		Gen(LastGOTO);  /* ��������� �����, �����������    */
		Gen(cmGOTO);    /* �� ����� ����������� ��������   */
		LastGOTO = PC;  /* ��������� ����� ���������� GOTO */
		NextLex();
		Fixup(CondPC);  /* ������. ����� ��������� ��������*/
		StatSeq();
	} else
		Fixup(CondPC);  /* ���� ELSE �����������          */
	Check( lexEND, (char*)"END" );
	Fixup(LastGOTO);   /* ��������� ���� ��� GOTO        */
}

static void WhileStatement(void) {
	int CondPC;
	int WhilePC = PC;
	Check(lexWHILE, (char*)"WHILE");
	BoolExpression();
	CondPC = PC;
	Check(lexDO, (char*)"DO");
	StatSeq();
	Check(lexEND, (char*)"END");
	Gen(WhilePC);
	Gen(cmGOTO);
	Fixup(CondPC);
}

static void Statement(void) {
	tObj* X;
	char designator[NAMELEN+1];
	char msg[80];

	if( Lex == lexName ) {
		if( (X=Find(Name))->Cat == catModule ) {
			NextLex();
			Check(lexDot, (char*)"\".\"");
			if( Lex == lexName && strlen(X->Name) + strlen(Name) <= NAMELEN ) {
				strcpy(designator, X->Name);
				strcat(designator, ".");
				X = Find(strcat(designator, Name));
			} else {
				strcpy(msg, "��� �� ������ ");
				Expected(strcat(msg, X->Name));
			}
		}
		if( X->Cat == catVar )
			AssStatement();        /* ������������ */
		else if( X->Cat == catStProc && X->Typ == typNone )
			CallStatement(X->Val); /* ����� ��������� */
		else
			Expected((char*)"����������� ���������� ��� ���������");
	} else if( Lex == lexIF )
		IfStatement();
	else if( Lex == lexWHILE )
		WhileStatement();
	/* ����� ������ �������� */
}

/* �������� {";" ��������} */
static void StatSeq(void) {
	Statement();    /* �������� */
	while( Lex == lexSemi ) {
		NextLex();
		Statement(); /* �������� */
	}
}

static void ImportModule(void) {
	if( Lex == lexName ) {
		NewName(Name, catModule);
		if( strcmp(Name, "In") == 0 ) {
			Enter((char*)"In.Open", catStProc, typNone, spInOpen);
			Enter((char*)"In.Int", catStProc, typNone, spInInt);
		} else if( strcmp(Name, "Out") == 0 ) {
			Enter((char*)"Out.Int", catStProc, typNone, spOutInt);
			Enter((char*)"Out.Ln", catStProc, typNone, spOutLn);
		} else
			Error((char*)"����������� ������");
		NextLex();
	} else
		Expected((char*)"��� �������������� ������");
}

/* IMPORT ��� { "," ��� } ";" */
static void Import(void) {
	Check(lexIMPORT, (char*)"IMPORT");
	ImportModule();    /* ��������� ����� �������������� ������ */
	while( Lex == lexComma ) {
		NextLex();
		ImportModule(); /* ��������� ����� �������������� ������ */
	}
	Check(lexSemi, (char*)"\";\"");
}

/* MODULE ��� ";" [������] ����������
   [BEGIN ��������������] END ��� "." */
static void Module(void) {
	tObj* ModRef; /* ������ �� ��� ������ � ������� */
	char msg[80];

	Check(lexMODULE, (char*)"MODULE");
	if( Lex != lexName )
		Expected((char*)"��� ������");
	else /* ��� ������ - � ������� ���� */
		ModRef = NewName(Name, catModule);
	NextLex();
	Check(lexSemi, (char*)"\";\"");
	if( Lex == lexIMPORT )
		Import();
	DeclSeq();
	if( Lex == lexBEGIN ) {
		NextLex();
		StatSeq();
	}
	Check(lexEND, (char*)"END");

	/* ��������� ����� ������ � ����� ����� END */
	if( Lex != lexName )
		Expected((char*)"��� ������");
	else if( strcmp(Name, ModRef->Name) ) {
		strcpy(msg, "��� ������ \"");
		strcat(msg, ModRef->Name);
		Expected(strcat(msg, "\""));
	} else
		NextLex();
	if( Lex != lexDot )
		Expected((char*)"\".\"");
	Gen(0);              /* ��� ��������*/
	Gen(cmStop);         /* ������� ��������*/
	AllocateVariables(); /* ���������� ���������� */
}

void Compile(void) {
	InitNameTable();
	OpenScope(); /* ���� ����������� ���� */
	Enter((char*)"ABS", catStProc, typInt, spABS);
	Enter((char*)"MAX", catStProc, typInt, spMAX);
	Enter((char*)"MIN", catStProc, typInt, spMIN);
	Enter((char*)"DEC", catStProc, typNone, spDEC);
	Enter((char*)"ODD", catStProc, typBool, spODD);
	Enter((char*)"HALT", catStProc, typNone, spHALT);
	Enter((char*)"INC", catStProc, typNone, spINC);
	Enter((char*)"INTEGER", catType, typInt, 0);
	OpenScope();  /* ���� ������ */
	Module();
	CloseScope(); /* ���� ������ */
	CloseScope(); /* ���� ����������� ���� */
	puts("\n���������� ���������");
}
