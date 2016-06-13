// ��ᯮ�����⥫�
class Pars {

static final int
   spABS    = 1,
   spMAX    = 2,
   spMIN    = 3,
   spDEC    = 4,
   spODD    = 5,
   spHALT   = 6,
   spINC    = 7,
   spInOpen = 8,
   spInInt  = 9,
   spOutInt = 10,
   spOutLn  = 11;

static void Check(int L, String M) {
   if( Scan.Lex != L )
      Error.Expected(M);
   else
      Scan.NextLex();
}

// ["+" | "-"] (��᫮ | ���) 
static int ConstExpr() {
   int v = 0;
   Obj X;
   int Op;

   Op = Scan.lexPlus;
   if( Scan.Lex == Scan.lexPlus ||
      Scan.Lex == Scan.lexMinus ) 
   {
      Op = Scan.Lex;
      Scan.NextLex();
   }
   if( Scan.Lex == Scan.lexNum ) {
      v = Scan.Num;
      Scan.NextLex();
      }
   else if( Scan.Lex == Scan.lexName ) {
      X = Table.Find(Scan.Name);
      if( X.Cat == Table.catGuard )
         Error.Message(
            "����� ��।����� ����⠭�� �१ ᥡ�"
         );
      else if( X.Cat != Table.catConst )
         Error.Expected( "��� ����⠭��" );
      else {
         v = X.Val;
         Scan.NextLex();
      }
      }
   else
      Error.Expected( "����⠭⭮� ��ࠦ����" );
   if( Op == Scan.lexMinus )
      return -v;
   return v;
}

// ��� "=" ������ࠦ 
static void ConstDecl() {
   Obj ConstRef; // ��뫪� �� ��� � ⠡���

   ConstRef = Table.NewName(Scan.Name, Table.catGuard);
   Scan.NextLex();
   Check(Scan.lexEQ, "\"=\"");
   ConstRef.Val = ConstExpr();
   ConstRef.Typ = Table.typInt; //����⠭� ��㣨� ⨯�� ���
   ConstRef.Cat = Table.catConst;
}

static void ParseType() {
   Obj TypeRef;
   if( Scan.Lex != Scan.lexName )
      Error.Expected("���");
   else {
      TypeRef = Table.Find(Scan.Name);
      if( TypeRef.Cat != Table.catType )
         Error.Expected("��� ⨯�");
      else if( TypeRef.Typ != Table.typInt )
         Error.Expected("楫� ⨯");
      Scan.NextLex();
   }
}

// ��� {"," ���} ":" ��� 
static void VarDecl() {
   Obj NameRef;
   
   if( Scan.Lex != Scan.lexName )
      Error.Expected("���");
   else {
      NameRef = Table.NewName(Scan.Name, Table.catVar);
      NameRef.Typ = Table.typInt;
      Scan.NextLex();
   }
   while( Scan.Lex == Scan.lexComma ) {
      Scan.NextLex();
      if( Scan.Lex != Scan.lexName )
         Error.Expected("���");
      else {
         NameRef = Table.NewName(Scan.Name, Table.catVar );
         NameRef.Typ = Table.typInt;
         Scan.NextLex();
      }
   }
   Check(Scan.lexColon, "\":\"");
   ParseType();
}

// { CONST {�������� ";"} | VAR {�����६ ";"} } 
static void DeclSeq() {
   while( Scan.Lex == Scan.lexCONST || 
      Scan.Lex == Scan.lexVAR )
   {
      if( Scan.Lex == Scan.lexCONST ) {
         Scan.NextLex();
         while( Scan.Lex == Scan.lexName ) {
            ConstDecl(); //������� ����⠭��
            Check(Scan.lexSemi, "\";\"");
         }
         }
      else {
         Scan.NextLex(); // VAR 
         while( Scan.Lex == Scan.lexName ) {
            VarDecl();   //������� ��६�����
            Check(Scan.lexSemi, "\";\"");
         }
      }
   }
}

static void IntExpression() {
   if( Expression() != Table.typInt )
      Error.Expected("��ࠦ���� 楫��� ⨯�");
}

static int StFunc(int F) {
   switch( F ) {
   case spABS:
      IntExpression();
      Gen.Abs();
      return Table.typInt;
   case spMAX:
      ParseType();
      Gen.Cmd(Integer.MAX_VALUE);
      return Table.typInt;
   case spMIN:
      ParseType();
      Gen.Min();
      return Table.typInt;
   case spODD:
      IntExpression();
      Gen.Odd();
      return Table.typBool;
   }
   return Table.typNone; // �⮡ �� �뫮 �।�०����� 
}

static int Factor() {
   Obj X;
   int T = 0; // �⮡ �� �뫮 �।�०����� 

   if( Scan.Lex == Scan.lexName ) {
      if( (X = Table.Find(Scan.Name)).Cat == Table.catVar ) {
         Gen.Addr(X);    //���� ��६�����
         Gen.Cmd(OVM.cmLoad);
         Scan.NextLex();
         return X.Typ;
         }
      else if( X.Cat == Table.catConst ) {
         Gen.Const(X.Val);
         Scan.NextLex();
         return X.Typ;
         }
      else if( X.Cat == Table.catStProc && 
         X.Typ != Table.typNone ) 
      {
         Scan.NextLex();
         Check(Scan.lexLPar, "\"(\"");
         T = StFunc(X.Val);
         Check(Scan.lexRPar, "\")\"");
         }
      else
         Error.Expected(
         "��६�����, ����⠭� ��� ��楤��-�㭪樨"
         );
      }
   else if( Scan.Lex == Scan.lexNum ) {
      Gen.Const(Scan.Num);
      Scan.NextLex();
      return Table.typInt;
      }
   else if( Scan.Lex == Scan.lexLPar ) {
      Scan.NextLex();
      T = Expression();
      Check(Scan.lexRPar, "\")\"");
      }
   else
      Error.Expected("���, �᫮ ��� \"(\"");
   return T;
}

static int Term() {
   int Op;
   int T = Factor();
   if( Scan.Lex == Scan.lexMult || Scan.Lex == Scan.lexDIV
      || Scan.Lex == Scan.lexMOD ) 
   {
      if( T != Table.typInt )
         Error.Message(
            "��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��"
         );
      do {
         Op = Scan.Lex;
         Scan.NextLex();
         if( (T = Factor()) != Table.typInt )
            Error.Expected("��ࠦ���� 楫��� ⨯�");
         switch( Op ) {
         case Scan.lexMult: Gen.Cmd(OVM.cmMult); break;
         case Scan.lexDIV:  Gen.Cmd(OVM.cmDiv); break;
         case Scan.lexMOD:  Gen.Cmd(OVM.cmMod); break;
         }
      } while( Scan.Lex == Scan.lexMult || 
         Scan.Lex == Scan.lexDIV ||
         Scan.Lex == Scan.lexMOD );
   }
   return T;
}

// ["+"|"-"] ��������� {�������� ���������} 
static int SimpleExpr() {
   int T;
   int Op;

   if( Scan.Lex == Scan.lexPlus ||
      Scan.Lex == Scan.lexMinus ) 
   {
      Op = Scan.Lex;
      Scan.NextLex();
      if( (T = Term()) != Table.typInt )
         Error.Expected("��ࠦ���� 楫��� ⨯�");
      if( Op == Scan.lexMinus )
         Gen.Cmd(OVM.cmNeg);
      }
   else
      T = Term();
   if( Scan.Lex == Scan.lexPlus || 
      Scan.Lex == Scan.lexMinus ) 
   {
      if( T != Table.typInt )
         Error.Message(
            "��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��"
         );
      do {
         Op = Scan.Lex;
         Scan.NextLex();
         if( (T = Term()) != Table.typInt )
            Error.Expected("��ࠦ���� 楫��� ⨯�");
         switch(Op) {
         case Scan.lexPlus:  Gen.Cmd(OVM.cmAdd); break;
         case Scan.lexMinus: Gen.Cmd(OVM.cmSub); break;
         }
      } while( Scan.Lex == Scan.lexPlus ||
         Scan.Lex == Scan.lexMinus );
   }
   return T;
}

// ���⮥��ࠦ [�⭮襭�� ���⮥��ࠦ] 
static int Expression() {
   int   Op;

   int T = SimpleExpr();
   if( Scan.Lex == Scan.lexEQ || Scan.Lex == Scan.lexNE ||
      Scan.Lex == Scan.lexGT || Scan.Lex == Scan.lexGE ||
      Scan.Lex == Scan.lexLT || Scan.Lex == Scan.lexLE )
   {
      Op = Scan.Lex;
      if( T != Table.typInt )
         Error.Message(
            "��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��"
         );
      Scan.NextLex();
      if( (T = SimpleExpr()) != Table.typInt )
         Error.Expected("��ࠦ���� 楫��� ⨯�");
      Gen.Comp(Op);   //������� �᫮����� ���室�
      T = Table.typBool;
   } //���� ⨯ ࠢ�� ⨯� ��ࢮ�� ���⮣� ��ࠦ����
   return T;
}

// ��६����� = ��� 
static void Variable() {
   Obj X;

   if( Scan.Lex != Scan.lexName )
      Error.Expected("���");
   else {
      if( (X = Table.Find(Scan.Name)).Cat != Table.catVar )
         Error.Expected("��� ��६�����");
      Gen.Addr(X);
      Scan.NextLex();
   }
}

static void StProc(int P) {
   switch( P ) {
   case spDEC:
      Variable();
      Gen.Cmd(OVM.cmDup);
      Gen.Cmd(OVM.cmLoad);
      if( Scan.Lex == Scan.lexComma ) {
         Scan.NextLex();
         IntExpression();
         }
      else
         Gen.Cmd(1);
      Gen.Cmd(OVM.cmSub);
      Gen.Cmd(OVM.cmSave);
      return;
   case spINC:
      Variable();
      Gen.Cmd(OVM.cmDup);
      Gen.Cmd(OVM.cmLoad);
      if( Scan.Lex == Scan.lexComma ) {
         Scan.NextLex();
         IntExpression();
         }
      else
         Gen.Cmd(1);
      Gen.Cmd(OVM.cmAdd);
      Gen.Cmd(OVM.cmSave);
      return;
   case spInOpen:
      // ���� ;
      return;
   case spInInt:
      Variable();
      Gen.Cmd(OVM.cmIn);
      Gen.Cmd(OVM.cmSave);
      return;
   case spOutInt:
      IntExpression();
      Check(Scan.lexComma , "\",\"");
      IntExpression();
      Gen.Cmd(OVM.cmOut);
      return;
   case spOutLn:
      Gen.Cmd(OVM.cmOutLn);
      return;
   case spHALT:
      Gen.Const(ConstExpr());
      Gen.Cmd(OVM.cmStop);
      return;
   }
}

static void BoolExpression() {
   if( Expression() != Table.typBool )
      Error.Expected("�����᪮� ��ࠦ����");
}

// ��६����� "=" ��ࠦ 
static void AssStatement() {
   Variable();
   if( Scan.Lex == Scan.lexAss ) {
      Scan.NextLex();
      IntExpression();
      Gen.Cmd(OVM.cmSave);
      }
   else
      Error.Expected("\":=\"");
}

// ��� ["(" // ��ࠦ | ��६�����  ")"] 
static void CallStatement(int sp) {
   Check(Scan.lexName, "��� ��楤���");
   if( Scan.Lex == Scan.lexLPar ) {
      Scan.NextLex();
      StProc(sp);
      Check( Scan.lexRPar, "\")\"" );
      }
   else if( sp == spOutLn || sp == spInOpen )
      StProc(sp);
   else
      Error.Expected("\"(\"");
}

static void IfStatement() {
   int CondPC;
   int LastGOTO;

   Check(Scan.lexIF, "IF");
   LastGOTO = 0;      //�।��饣� ���室� ���        
   BoolExpression();
   CondPC = Gen.PC;        //������. ��������� ��. ���室� 
   Check(Scan.lexTHEN, "THEN");
   StatSeq();
   while( Scan.Lex == Scan.lexELSIF ) {
      Gen.Cmd(LastGOTO);   //���⨢�� ����, 㪠�뢠�騩    
      Gen.Cmd(OVM.cmGOTO); //�� ���� �।��饣� ���室�
      LastGOTO = Gen.PC;   //��������� ���� GOTO            
      Scan.NextLex();
      Gen.Fixup(CondPC);   //��䨪�. ���� �᫮����� ���室�
      BoolExpression();
      CondPC = Gen.PC;     //������. ��������� ��. ���室� 
      Check(Scan.lexTHEN, "THEN");
      StatSeq();
   }
   if( Scan.Lex == Scan.lexELSE ) {
      Gen.Cmd(LastGOTO);   //���⨢�� ����, 㪠�뢠�騩    
      Gen.Cmd(OVM.cmGOTO); //�� ���� �।��饣� ���室�   
      LastGOTO = Gen.PC;   //��������� ���� ��᫥����� GOTO
      Scan.NextLex();
      Gen.Fixup(CondPC);   //��䨪�. ���� �᫮����� ���室�
      StatSeq();
      }
   else
      Gen.Fixup(CondPC);    //�᫨ ELSE ���������          
   Check( Scan.lexEND, "END" );
   Gen.Fixup(LastGOTO);     //���ࠢ��� � �� GOTO        
}

static void WhileStatement() {
   int WhilePC = Gen.PC;
   Check(Scan.lexWHILE, "WHILE");
   BoolExpression();
   int CondPC = Gen.PC;
   Check(Scan.lexDO, "DO");
   StatSeq();
   Check(Scan.lexEND, "END");
   Gen.Cmd(WhilePC);
   Gen.Cmd(OVM.cmGOTO);
   Gen.Fixup(CondPC);
}

static void Statement() {
   Obj X;

   if( Scan.Lex == Scan.lexName ) {
      if( (X=Table.Find(Scan.Name)).Cat == Table.catModule ) 
      {
         Scan.NextLex();
         Check(Scan.lexDot, "\".\"");
         if( Scan.Lex == Scan.lexName && 
            X.Name.length() + Scan.Name.length() <= 
               Scan.NAMELEN
         )
            X = Table.Find( X.Name + "." + Scan.Name);
         else
            Error.Expected("��� �� ����� " + X.Name);
      }
      if( X.Cat == Table.catVar )
         AssStatement();        //��ᢠ������
      else if( X.Cat == Table.catStProc && 
         X.Typ == Table.typNone  
      )
         CallStatement(X.Val); //�맮� ��楤���
      else
         Error.Expected(
            "������祭�� ��६����� ��� ��楤���"
         );
      }
   else if( Scan.Lex == Scan.lexIF )
      IfStatement();
   else if( Scan.Lex == Scan.lexWHILE )
      WhileStatement();
   // ���� ���⮩ ������ 
}

// ������ {";" ������} 
static void StatSeq() {
   Statement();    //������
   while( Scan.Lex == Scan.lexSemi ) {
      Scan.NextLex();
      Statement(); //������
   }
}

static void ImportName() {
   if( Scan.Lex == Scan.lexName ) {
      Table.NewName(Scan.Name, Table.catModule);
      if( Scan.Name.compareTo("In") == 0 ) { 
         Table.Enter("In.Open",
            Table.catStProc, Table.typNone, spInOpen);
         Table.Enter("In.Int", 
            Table.catStProc, Table.typNone, spInInt);
         }
      else if( Scan.Name.compareTo("Out") == 0 ) {
         Table.Enter("Out.Int",
            Table.catStProc, Table.typNone, spOutInt);
         Table.Enter("Out.Ln",
            Table.catStProc, Table.typNone, spOutLn);
         }
      else
         Error.Message("��������� �����");
      Scan.NextLex();
      }
   else
      Error.Expected("��� �������㥬��� �����");
}

// IMPORT ��� { "," ��� } ";" 
static void Import() {
   Check(Scan.lexIMPORT, "IMPORT");
   ImportName();    //��ࠡ�⪠ ����� �������㥬��� �����
   while( Scan.Lex == Scan.lexComma ) {
      Scan.NextLex();
      ImportName(); //��ࠡ�⪠ ����� �������㥬��� �����
   }
   Check(Scan.lexSemi, "\";\"");
}

// MODULE ��� ";" [������] ��ᫎ�� [BEGIN ��ᫎ����஢]
// END ��� "." 
static void Module() {
   Obj ModRef; //��뫪� �� ��� ����� � ⠡���

   Check(Scan.lexMODULE, "MODULE");
   if( Scan.Lex != Scan.lexName )
      Error.Expected("��� �����");
   //��� ����� - � ⠡���� ����
      ModRef = Table.NewName(Scan.Name, Table.catModule);
   Scan.NextLex();
   Check(Scan.lexSemi, "\";\"");
   if( Scan.Lex == Scan.lexIMPORT )
      Import();
   DeclSeq();
   if( Scan.Lex == Scan.lexBEGIN ) {
      Scan.NextLex();
      StatSeq();
   }
   Check(Scan.lexEND, "END");

   //�ࠢ����� ����� ����� � ����� ��᫥ END
      if( Scan.Lex != Scan.lexName )
         Error.Expected("��� �����");
      else if( Scan.Name.compareTo(ModRef.Name) != 0 ) 
         Error.Expected(
            "��� ����� \"" + ModRef.Name + "\""
         );
      else
         Scan.NextLex();
   if( Scan.Lex != Scan.lexDot )
      Error.Expected("\".\"");
   Gen.Cmd(0);              // ��� ������
   Gen.Cmd(OVM.cmStop);     // ������� ��⠭���
   Gen.AllocateVariables(); // �����饭�� ��६�����
}

static void Compile() {
   Table.Init();
   Table.OpenScope(); //���� �⠭������ ����
   Table.Enter("ABS",
      Table.catStProc, Table.typInt, spABS);
   Table.Enter("MAX",
      Table.catStProc, Table.typInt, spMAX);
   Table.Enter("MIN",
      Table.catStProc, Table.typInt, spMIN);
   Table.Enter("DEC",
      Table.catStProc, Table.typNone, spDEC);
   Table.Enter("ODD",
      Table.catStProc, Table.typBool, spODD);
   Table.Enter("HALT",
      Table.catStProc, Table.typNone, spHALT);
   Table.Enter("INC",
      Table.catStProc, Table.typNone, spINC);
   Table.Enter("INTEGER",
      Table.catType, Table.typInt, 0);
   Table.OpenScope();  //���� �����
   Module();
   Table.CloseScope(); //���� �����
   Table.CloseScope(); //���� �⠭������ ����
   System.out.println("\n��������� �����襭�");
}

}
