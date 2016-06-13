// ��������������
class Pars {

const int
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

static void Check(tLex L, string M) {
   if( Scan.Lex != L )
      Error.Expected(M);
   else
      Scan.NextLex();
}

// ["+" | "-"] (����� | ���) 
static int ConstExpr() {
   int v = 0;
   Obj X;
   tLex Op;

   Op = tLex.lexPlus;
   if( Scan.Lex == tLex.lexPlus ||
      Scan.Lex == tLex.lexMinus ) 
   {
      Op = Scan.Lex;
      Scan.NextLex();
   }
   if( Scan.Lex == tLex.lexNum ) {
      v = Scan.Num;
      Scan.NextLex();
      }
   else if( Scan.Lex == tLex.lexName ) {
      X = Table.Find(Scan.Name);
      if( X.Cat == tCat.Guard )
         Error.Message(
            "������ ���������� ��������� ����� ����"
         );
      else if( X.Cat != tCat.Const )
         Error.Expected( "��� ���������" );
      else {
         v = X.Val;
         Scan.NextLex();
      }
      }
   else
      Error.Expected( "����������� ���������" );
   if( Op == tLex.lexMinus )
      return -v;
   return v;
}

// ��� "=" ���������� 
static void ConstDecl() {
   Obj ConstRef = Table.NewName(Scan.Name, tCat.Guard);
   Scan.NextLex();
   Check(tLex.lexEQ, "\"=\"");
   ConstRef.Val = ConstExpr();
   ConstRef.Type = tType.Int; //�������� ������ ����� ���
   ConstRef.Cat = tCat.Const;
}

static void ParseType() {
   Obj TypeRef;
   
   if( Scan.Lex != tLex.lexName )
      Error.Expected("���");
   else {
      TypeRef = Table.Find(Scan.Name);
      if( TypeRef.Cat != tCat.Type )
         Error.Expected("��� ����");
      else if( TypeRef.Type != tType.Int )
         Error.Expected("����� ���");
      Scan.NextLex();
   }
}

// ��� {"," ���} ":" ��� 
static void VarDecl() {
   Obj NameRef;
   
   if( Scan.Lex != tLex.lexName )
      Error.Expected("���");
   else {
      NameRef = Table.NewName(Scan.Name, tCat.Var);
      NameRef.Type = tType.Int;
      Scan.NextLex();
   }
   while( Scan.Lex == tLex.lexComma ) {
      Scan.NextLex();
      if( Scan.Lex != tLex.lexName )
         Error.Expected("���");
      else {
         NameRef = Table.NewName(Scan.Name, tCat.Var );
         NameRef.Type = tType.Int;
         Scan.NextLex();
      }
   }
   Check(tLex.lexColon, "\":\"");
   ParseType();
}

// { CONST {����������� ";"} | VAR {����������� ";"} } 
static void DeclSeq() {
   while( Scan.Lex == tLex.lexCONST || 
      Scan.Lex == tLex.lexVAR )
   {
      if( Scan.Lex == tLex.lexCONST ) {
         Scan.NextLex();
         while( Scan.Lex == tLex.lexName ) {
            ConstDecl(); //���������� ���������
            Check(tLex.lexSemi, "\";\"");
         }
         }
      else {
         Scan.NextLex(); // VAR 
         while( Scan.Lex == tLex.lexName ) {
            VarDecl();   //���������� ����������
            Check(tLex.lexSemi, "\";\"");
         }
      }
   }
}

static void IntExpression() {
   if( Expression() != tType.Int )
      Error.Expected("��������� ������ ����");
}

static tType StFunc(int F) {
   switch( F ) {
   case spABS:
      IntExpression();
      Gen.Abs();
      return tType.Int;
   case spMAX:
      ParseType();
      Gen.Cmd(int.MaxValue);
      return tType.Int;
   case spMIN:
      ParseType();
      Gen.Min();
      return tType.Int;
   case spODD:
      IntExpression();
      Gen.Odd();
      return tType.Bool;
   }
   return tType.None; // ���� �� ���� �������������� 
}

// ��� {"(" ����� | ��� ")"} | ����� | "(" ����� ")"
static tType Factor() {
   Obj X;
   tType T = tType.None;
   
   if( Scan.Lex == tLex.lexName ) {
      if( (X = Table.Find(Scan.Name)).Cat == tCat.Var ) {
         Gen.Addr(X);    //����� ����������
         Gen.Cmd(OVM.cmLoad);
         Scan.NextLex();
         return X.Type;
         }
      else if( X.Cat == tCat.Const ) {
         Gen.Const(X.Val);
         Scan.NextLex();
         return X.Type;
         }
      else if( X.Cat == tCat.StProc && 
         X.Type != tType.None ) 
      {
         Scan.NextLex();
         Check(tLex.lexLPar, "\"(\"");
         T = StFunc(X.Val);
         Check(tLex.lexRPar, "\")\"");
         }
      else
         Error.Expected(
         "����������, ��������� ��� ���������-�������"
         );
      }
   else if( Scan.Lex == tLex.lexNum ) {
      Gen.Const(Scan.Num);
      Scan.NextLex();
      return tType.Int;
      }
   else if( Scan.Lex == tLex.lexLPar ) {
      Scan.NextLex();
      T = Expression();
      Check(tLex.lexRPar, "\")\"");
      }
   else
      Error.Expected("���, ����� ��� \"(\"");
   return T;
}

// ��������� {������� ���������}
static tType Term() {
   tLex Op;
   tType T = Factor();
   if( Scan.Lex == tLex.lexMult || Scan.Lex == tLex.lexDIV
      || Scan.Lex == tLex.lexMOD ) 
   {
      if( T != tType.Int )
         Error.Message(
            "�������������� �������� ���� ��������"
         );
      do {
         Op = Scan.Lex;
         Scan.NextLex();
         if( (T = Factor()) != tType.Int )
            Error.Expected("��������� ������ ����");
         switch( Op ) {
         case tLex.lexMult: Gen.Cmd(OVM.cmMult); break;
         case tLex.lexDIV:  Gen.Cmd(OVM.cmDiv); break;
         case tLex.lexMOD:  Gen.Cmd(OVM.cmMod); break;
         }
      } while( Scan.Lex == tLex.lexMult || 
         Scan.Lex == tLex.lexDIV ||
         Scan.Lex == tLex.lexMOD );
   }
   return T;
}

// ["+"|"-"] ��������� {�������� ���������} 
static tType SimpleExpr() {
   tType T;
   tLex Op;

   if( Scan.Lex == tLex.lexPlus ||
      Scan.Lex == tLex.lexMinus ) 
   {
      Op = Scan.Lex;
      Scan.NextLex();
      if( (T = Term()) != tType.Int )
         Error.Expected("��������� ������ ����");
      if( Op == tLex.lexMinus )
         Gen.Cmd(OVM.cmNeg);
      }
   else
      T = Term();
   if( Scan.Lex == tLex.lexPlus || 
      Scan.Lex == tLex.lexMinus ) 
   {
      if( T != tType.Int )
         Error.Message(
            "�������������� �������� ���� ��������"
         );
      do {
         Op = Scan.Lex;
         Scan.NextLex();
         if( (T = Term()) != tType.Int )
            Error.Expected("��������� ������ ����");
         switch(Op) {
         case tLex.lexPlus:  Gen.Cmd(OVM.cmAdd); break;
         case tLex.lexMinus: Gen.Cmd(OVM.cmSub); break;
         }
      } while( Scan.Lex == tLex.lexPlus ||
         Scan.Lex == tLex.lexMinus );
   }
   return T;
}

// ������������ [��������� ������������] 
static tType Expression() {
   tLex Op;

   tType T = SimpleExpr();
   if( Scan.Lex == tLex.lexEQ || Scan.Lex == tLex.lexNE ||
      Scan.Lex == tLex.lexGT || Scan.Lex == tLex.lexGE ||
      Scan.Lex == tLex.lexLT || Scan.Lex == tLex.lexLE )
   {
      Op = Scan.Lex;
      if( T != tType.Int )
         Error.Message(
            "�������������� �������� ���� ��������"
         );
      Scan.NextLex();
      if( (T = SimpleExpr()) != tType.Int )
         Error.Expected("��������� ������ ����");
      Gen.Comp(Op);   //��������� ��������� ��������
      T = tType.Bool;
   } //����� ��� ����� ���� ������� �������� ���������
   return T;
}

// ���������� = ��� 
static void Variable() {
   Obj X;

   if( Scan.Lex != tLex.lexName )
      Error.Expected("���");
   else {
      if( (X = Table.Find(Scan.Name)).Cat != tCat.Var )
         Error.Expected("��� ����������");
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
      if( Scan.Lex == tLex.lexComma ) {
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
      if( Scan.Lex == tLex.lexComma ) {
         Scan.NextLex();
         IntExpression();
         }
      else
         Gen.Cmd(1);
      Gen.Cmd(OVM.cmAdd);
      Gen.Cmd(OVM.cmSave);
      return;
   case spInOpen:
      // ����� ;
      return;
   case spInInt:
      Variable();
      Gen.Cmd(OVM.cmIn);
      Gen.Cmd(OVM.cmSave);
      return;
   case spOutInt:
      IntExpression();
      Check(tLex.lexComma , "\",\"");
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
   if( Expression() != tType.Bool )
      Error.Expected("���������� ���������");
}

// ���������� "=" ����� 
static void AssStatement() {
   Variable();
   if( Scan.Lex == tLex.lexAss ) {
      Scan.NextLex();
      IntExpression();
      Gen.Cmd(OVM.cmSave);
      }
   else
      Error.Expected("\":=\"");
}

// ��� ["(" // ����� | ����������  ")"] 
static void CallStatement(int sp) {
   Check(tLex.lexName, "��� ���������");
   if( Scan.Lex == tLex.lexLPar ) {
      Scan.NextLex();
      StProc(sp);
      Check( tLex.lexRPar, "\")\"" );
      }
   else if( sp == spOutLn || sp == spInOpen )
      StProc(sp);
   else
      Error.Expected("\"(\"");
}

static void IfStatement() {
   int CondPC;
   int LastGOTO;

   Check(tLex.lexIF, "IF");
   LastGOTO = 0;      //����������� �������� ���        
   BoolExpression();
   CondPC = Gen.PC;        //������. ��������� ���. �������� 
   Check(tLex.lexTHEN, "THEN");
   StatSeq();
   while( Scan.Lex == tLex.lexELSIF ) {
      Gen.Cmd(LastGOTO);   //��������� �����, �����������    
      Gen.Cmd(OVM.cmGOTO); //�� ����� ����������� ��������
      LastGOTO = Gen.PC;   //��������� ����� GOTO            
      Scan.NextLex();
      Gen.Fixup(CondPC);   //������. ����� ��������� ��������
      BoolExpression();
      CondPC = Gen.PC;     //������. ��������� ���. �������� 
      Check(tLex.lexTHEN, "THEN");
      StatSeq();
   }
   if( Scan.Lex == tLex.lexELSE ) {
      Gen.Cmd(LastGOTO);   //��������� �����, �����������    
      Gen.Cmd(OVM.cmGOTO); //�� ����� ����������� ��������   
      LastGOTO = Gen.PC;   //��������� ����� ���������� GOTO
      Scan.NextLex();
      Gen.Fixup(CondPC);   //������. ����� ��������� ��������
      StatSeq();
      }
   else
      Gen.Fixup(CondPC);    //���� ELSE �����������          
   Check( tLex.lexEND, "END" );
   Gen.Fixup(LastGOTO);     //��������� ���� ��� GOTO        
}

static void WhileStatement() {
   int WhilePC = Gen.PC;
   Check(tLex.lexWHILE, "WHILE");
   BoolExpression();
   int CondPC = Gen.PC;
   Check(tLex.lexDO, "DO");
   StatSeq();
   Check(tLex.lexEND, "END");
   Gen.Cmd(WhilePC);
   Gen.Cmd(OVM.cmGOTO);
   Gen.Fixup(CondPC);
}

static void Statement() {
   Obj X;

   if( Scan.Lex == tLex.lexName ) {
      if( (X=Table.Find(Scan.Name)).Cat == tCat.Module ) 
      {
         Scan.NextLex();
         Check(tLex.lexDot, "\".\"");
         if( Scan.Lex == tLex.lexName && 
            X.Name.Length + Scan.Name.Length <= 
               Scan.NAMELEN
         )
            X = Table.Find( X.Name + "." + Scan.Name);
         else
            Error.Expected("��� �� ������ " + X.Name);
      }
      if( X.Cat == tCat.Var )
         AssStatement();        //������������
      else if( X.Cat == tCat.StProc && 
         X.Type == tType.None 
      )
         CallStatement(X.Val); //����� ���������
      else
         Error.Expected(
            "����������� ���������� ��� ���������"
         );
      }
   else if( Scan.Lex == tLex.lexIF )
      IfStatement();
   else if( Scan.Lex == tLex.lexWHILE )
      WhileStatement();
   // ����� ������ �������� 
}

// �������� {";" ��������} 
static void StatSeq() {
   Statement();    //��������
   while( Scan.Lex == tLex.lexSemi ) {
      Scan.NextLex();
      Statement(); //��������
   }
}

static void ImportModule() {
   if( Scan.Lex == tLex.lexName ) {
      Table.NewName(Scan.Name, tCat.Module);
      if( Scan.Name == "In" ) {
         Table.Enter("In.Open", tCat.StProc, tType.None, spInOpen);
         Table.Enter("In.Int", tCat.StProc, tType.None, spInInt);
         }
      else if( Scan.Name == "Out" ) {
         Table.Enter("Out.Int", tCat.StProc, tType.None, spOutInt);
         Table.Enter("Out.Ln", tCat.StProc, tType.None, spOutLn);
         }
      else
         Error.Message("����������� ������");
      Scan.NextLex();
      }
   else
      Error.Expected("��� �������������� ������");
}

// IMPORT ��� { "," ��� } ";" 
static void Import() {
   Check(tLex.lexIMPORT, "IMPORT");
   ImportModule();    //��������� ����� �������������� ������
   while( Scan.Lex == tLex.lexComma ) {
      Scan.NextLex();
      ImportModule(); //��������� ����� �������������� ������
   }
   Check(tLex.lexSemi, "\";\"");
}

// MODULE ��� ";" [������] ���������� [BEGIN ��������������]
// END ��� "." 
static void Module() {
   Obj ModRef; //������ �� ��� ������ � �������

   Check(tLex.lexMODULE, "MODULE");
   if( Scan.Lex != tLex.lexName )
      Error.Expected("��� ������");
   //��� ������ - � ������� ����
      ModRef = Table.NewName(Scan.Name, tCat.Module);
   Scan.NextLex();
   Check(tLex.lexSemi, "\";\"");
   if( Scan.Lex == tLex.lexIMPORT )
      Import();
   DeclSeq();
   if( Scan.Lex == tLex.lexBEGIN ) {
      Scan.NextLex();
      StatSeq();
   }
   Check(tLex.lexEND, "END");

   //��������� ����� ������ � ����� ����� END
      if( Scan.Lex != tLex.lexName )
         Error.Expected("��� ������");
      else if( Scan.Name != ModRef.Name ) 
         Error.Expected(
            "��� ������ \"" + ModRef.Name + "\""
         );
      else
         Scan.NextLex();
   if( Scan.Lex != tLex.lexDot )
      Error.Expected("\".\"");
   Gen.Cmd(0);              // ��� ��������
   Gen.Cmd(OVM.cmStop);     // ������� ��������
   Gen.AllocateVariables(); // ���������� ����������
}

public static void Compile() {
   Table.Init();
   Table.OpenScope(); //���� ����������� ����
   Table.Enter("ABS", tCat.StProc, tType.Int, spABS);
   Table.Enter("MAX", tCat.StProc, tType.Int, spMAX);
   Table.Enter("MIN", tCat.StProc, tType.Int, spMIN);
   Table.Enter("DEC", tCat.StProc, tType.None, spDEC);
   Table.Enter("ODD", tCat.StProc, tType.Bool, spODD);
   Table.Enter("HALT", tCat.StProc, tType.None, spHALT);
   Table.Enter("INC", tCat.StProc, tType.None, spINC);
   Table.Enter("INTEGER", tCat.Type, tType.Int, 0);
   Table.OpenScope();  //���� ������
   Module();
   Table.CloseScope(); //���� ������
   Table.CloseScope(); //���� ����������� ����
   System.Console.WriteLine("\n���������� ���������");
}

}
