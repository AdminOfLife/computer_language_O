unit OPars;
{ ��ᯮ�����⥫� }

interface

procedure Compile;

{======================================================}

implementation

uses
   OScan, OError, OTable, OGen, OVM;

const
   spABS    = 1;
   spMAX    = 2;
   spMIN    = 3;
   spDEC    = 4;
   spODD    = 5;
   spHALT   = 6;
   spINC    = 7;
   spInOpen = 8;
   spInInt  = 9;
   spOutInt = 10;
   spOutLn  = 11;

procedure StatSeq; forward;
procedure Expression(var t: tType); forward;

procedure Check(L: tLex; M: string);
begin
   if Lex <> L then
      Expected(M)
   else
      NextLex;
end;

(* ["+" | "-"] (��᫮ | ���) *)
procedure ConstExpr(var V: integer);
var
   X  : tObj;
   Op : tLex;
begin
   Op := lexPlus;
   if Lex in [lexPlus, lexMinus] then begin
      Op := Lex;
      NextLex;
   end;
   if Lex = lexNum then begin
      V := Num;
      NextLex;
      end
   else if Lex = lexName then begin
      Find(Name, X);
      if X^.Cat = catGuard then
         Error('����� ��।����� ����⠭�� �१ ᥡ�')
      else if X^.Cat <> catConst then
         Expected( '��� ����⠭��' )
      else
         V := X^.Val;
      NextLex;
      end
   else
      Expected( '����⠭⭮� ��ࠦ����' );
   if Op = lexMinus then
      V := -V;
end;

(* ��� "=" ������ࠦ *)
procedure ConstDecl;
var
   ConstRef: tObj; {��뫪� �� ��� � ⠡���}
begin
   NewName(Name, catGuard, ConstRef);
   NextLex;
   Check(lexEQ, '"="');
   ConstExpr(ConstRef^.Val);
   ConstRef^.Typ := typInt; {����⠭� ��㣨� ⨯�� ���}
   ConstRef^.Cat := catConst;
end;

procedure ParseType;
var
   TypeRef : tObj;
begin
   if Lex <> lexName then
      Expected('���')
   else begin
      Find(Name, TypeRef);
      if TypeRef^.Cat <> catType then
         Expected('��� ⨯�')
      else if TypeRef^.Typ <> typInt then
         Expected('楫� ⨯');
      NextLex;
   end;
end;

(* ��� {"," ���} ":" ��� *)
procedure VarDecl;
var
   NameRef : tObj;
begin
   if Lex <> lexName then
      Expected('���')
   else begin
      NewName(Name, catVar, NameRef);
      NameRef^.Typ := typInt;
      NextLex;
   end;
   while Lex = lexComma do begin
      NextLex;
      if Lex <> lexName then
         Expected('���')
      else begin
         NewName(Name, catVar, NameRef);
         NameRef^.Typ := typInt;
         NextLex;
      end;
   end;
   Check(lexColon, '":"');
   ParseType;
end;

(* {CONST {�������� ";"} 
    |VAR {�����६ ";"} } *)
procedure DeclSeq;
begin
   while Lex in [lexCONST, lexVAR] do begin
      if Lex = lexCONST then begin
         NextLex;
         while Lex = lexName do begin
            ConstDecl; {������� ����⠭��}
            Check( lexSemi, '";"' );
         end;
         end
      else begin
         NextLex; { VAR }
         while Lex = lexName do begin
            VarDecl;   {������� ��६�����}
            Check( lexSemi, '";"' );
         end;
      end;
   end;
end;

procedure IntExpression;
var
   T : tType;
begin
   Expression(T);
   if T <> typInt then
      Expected('��ࠦ���� 楫��� ⨯�');
end;

procedure StFunc(F: integer; var T: tType);
begin
   case F of
   spABS:
      begin
         IntExpression;
         GenAbs;
         T := typInt;
      end;
   spMAX:
      begin
         ParseType;
         Gen(MaxInt);
         T := typInt;
      end;
   spMIN:
      begin
         ParseType;
         GenMin;
         T := typInt;
      end;
   spODD:
      begin
         IntExpression;
         GenOdd;
         T := typBool;
      end;
   end;
end;

procedure Factor(var T : tType);
var
   X : tObj;
begin
   if Lex = lexName then begin
      Find(Name, X);
      if X^.Cat = catVar then begin
         GenAddr(X);    {���� ��६�����}
         Gen( cmLoad );
         T := X^.Typ;
         NextLex;
         end
      else if X^.Cat = catConst then begin
         GenConst(X^.Val);
         T := X^.Typ;
         NextLex;
         end
      else if (X^.Cat=catStProc) and (X^.Typ<>typNone)
      then begin
         NextLex;
         Check(lexLPar, '"("');
         StFunc(X^.Val, T);
         Check(lexRPar, '")"');
         end
      else
         Expected(
         '��६�����, ����⠭� ��� ��楤��-�㭪樨'
         );
      end
   else if Lex = lexNum then begin
      T := typInt;
      GenConst(Num);
      NextLex
      end
   else if Lex = lexLPar then begin
      NextLex;
      Expression(T);
      Check(lexRPar, '")"');
      end
   else
      Expected('���, �᫮ ��� "("');
end;

procedure Term(var T: tType);
var
   Op : tLex;
begin
   Factor(T);
   if Lex in [lexMult, lexDIV, lexMOD] then begin
      if T <> typInt then
         Error('��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��');
      repeat
         Op := Lex;
         NextLex;
         Factor(T);
         if T <> typInt then
            Expected('��ࠦ���� 楫��� ⨯�');                 case Op of
         lexMult: Gen(cmMult);
         lexDIV:  Gen(cmDIV);
         lexMOD:  Gen(cmMOD);
         end;
      until not(Lex in [lexMult, lexDIV, lexMOD]);
   end;
end;

(* ["+"|"-"] ��������� {�������� ���������} *)
procedure SimpleExpr(var T : tType);
var
   Op : tLex;
begin
   if Lex in [lexPlus, lexMinus] then begin
      Op := Lex;
      NextLex;
      Term(T);
      if T <> typInt then
         Expected('��ࠦ���� 楫��� ⨯�');
      if Op = lexMinus then
         Gen(cmNeg);
      end
   else
      Term(T);
   if Lex in [lexPlus, lexMinus] then begin
      if T <> typInt then
         Error('��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��');
      repeat
         Op := Lex;
         NextLex;
         Term(T);
         if T <> typInt then
            Expected('��ࠦ���� 楫��� ⨯�');
         case Op of
         lexPlus:  Gen(cmAdd);
         lexMinus: Gen(cmSub);
         end;
      until not( Lex in [lexPlus, lexMinus] );
   end;
end;

(* ���⮥��ࠦ [�⭮襭�� ���⮥��ࠦ] *)
procedure Expression(var T : tType);
var
   Op : tLex;
begin
   SimpleExpr(T); 
   if Lex in [lexEQ, lexNE, lexGT, lexGE, lexLT, lexLE]
   then begin
      Op := Lex;
      if T <> typInt then
         Error('��ᮮ⢥��⢨� ����樨 ⨯� ���࠭��');
      NextLex;
      SimpleExpr(T); {�ࠢ� ���࠭� �⭮襭��}
      if T <> typInt then
         Expected('��ࠦ���� 楫��� ⨯�');
      GenComp(Op);   {������� �᫮����� ���室�}
      T := typBool;
   end; {���� ⨯ ࠢ�� ⨯� ��ࢮ�� ���⮣� ��ࠦ����}
end;

(* ��६����� = ���. *)
procedure Variable;
var
   X : tObj;
begin
   if Lex <> lexName then
      Expected('���')
   else begin
      Find(Name, X);
      if X^.Cat <> catVar then
         Expected('��� ��६�����');
     GenAddr(X);
     NextLex;
   end;
end;

procedure StProc(sp: integer);
var
   c : integer;
begin
   case sp of
   spDEC:
      begin
         Variable;
         Gen(cmDup);
         Gen(cmLoad);
         if Lex = lexComma then begin
            NextLex;
            IntExpression;
            end
         else
            Gen(1);
         Gen(cmSub);
         Gen(cmSave);
      end;
   spINC:
      begin
         Variable;
         Gen(cmDup);
         Gen(cmLoad);
         if Lex = lexComma then begin
            NextLex;
            IntExpression;
            end
         else
            Gen(1);
         Gen(cmAdd);
         Gen(cmSave);
      end;
   spInOpen:
      { ���� };
   spInInt:
      begin
         Variable;
         Gen(cmIn);
         Gen(cmSave);
      end;
   spOutInt:
      begin
         IntExpression;
         Check(lexComma , '","');
         IntExpression;
         Gen(cmOut);
      end;
   spOutLn:
      Gen(cmOutLn);
   spHalt:
      begin
         ConstExpr(c);
         GenConst(c);
         Gen(cmStop);
      end;
   end;
end;

procedure BoolExpression;
var
   T : tType;
begin
   Expression(T);
   if T <> typBool then
      Expected('�����᪮� ��ࠦ����');
end;

(* ��६����� ":=" ��ࠦ *)
procedure AssStatement;
begin
   Variable;
   if Lex = lexAss then begin
      NextLex;
      IntExpression;
      Gen(cmSave);
      end
   else
      Expected('":="')
end;

(* ��� ["(" { ��ࠦ | ��६����� } ")"] *)
procedure CallStatement(sp : integer);
begin
   Check(lexName, '��� ��楤���');
   if Lex = lexLPar then begin
      NextLex;
      StProc(sp);
      Check( lexRPar, '")"' );
      end
   else if sp in [spOutLn, spInOpen] then
      StProc(sp)
   else
      Expected('"("');
end;

procedure IfStatement;
var
   CondPC   : integer;
   LastGOTO : integer;
begin
   Check(lexIF, 'IF');
   LastGOTO := 0;     {�।��饣� ���室� ���        }
   BoolExpression;
   CondPC := PC;      {������. ��������� ��. ���室� }
   Check(lexTHEN, 'THEN');
   StatSeq;
   while Lex = lexELSIF do begin
      Gen(LastGOTO);  {���⨢�� ����, 㪠�뢠�騩    }
      Gen(cmGOTO);    {�� ���� �।��饣� ���室�.  }
      LastGOTO := PC; {��������� ���� GOTO            }
      NextLex;
      Fixup(CondPC);  {��䨪�. ���� �᫮����� ���室�}
      BoolExpression;
      CondPC := PC;   {������. ��������� ��. ���室� }
      Check(lexTHEN, 'THEN');
      StatSeq;
   end;
   if Lex = lexELSE then begin
      Gen(LastGOTO);  {���⨢�� ����, 㪠�뢠�騩    }
      Gen(cmGOTO);    {�� ���� �।��饣� ���室�   }
      LastGOTO := PC; {��������� ���� ��᫥����� GOTO }
      NextLex;
      Fixup(CondPC);  {��䨪�. ���� �᫮����� ���室�}
      StatSeq;
      end
   else
      Fixup(CondPC); {�᫨ ELSE ���������          }
   Check( lexEND, 'END' );
   Fixup(LastGOTO);     {���ࠢ��� � �� GOTO        }
end;

procedure WhileStatement;
var
   WhilePC  : integer;
   CondPC   : integer;
begin
   WhilePC := PC;
   Check(lexWHILE, 'WHILE');
   BoolExpression;
   CondPC := PC;
   Check(lexDO, 'DO');
   StatSeq;
   Check(lexEND, 'END');
   Gen(WhilePC);
   Gen(cmGOTO);
   Fixup(CondPC);
end;

procedure Statement;
var
   X : tObj;
begin
   if Lex = lexName then begin
      Find(Name, X);
      if X^.Cat = catModule then begin
         NextLex;
         Check(lexDot, '"."');
         if    (Lex = lexName) and
            (Length(X^.Name)+Length(Name) < NameLen)
         then
            Find(X^.Name+'.'+Name, X)
         else
            Expected('��� �� ����� '+ X^.Name);
      end;
      if X^.Cat = catVar then
         AssStatement          {��ᢠ������}
      else if (X^.Cat=catStProc) and (X^.Typ=typNone) then
         CallStatement(X^.Val) {�맮� ��楤���}
      else
         Expected(
            '������祭�� ��६����� ��� ��楤���'
         );
      end
   else if Lex = lexIF then
      IfStatement
   else if Lex = lexWHILE then
      WhileStatement
end;

(* ������ {";" ������} *)
procedure StatSeq;
begin
   Statement;    {������}
   while Lex = lexSemi do begin
      NextLex;
      Statement; {������}
   end;
end;

procedure ImportModule;
var
   ImpRef: tObj;
begin
   if Lex = lexName then begin
      NewName(Name, catModule, ImpRef);
      if Name = 'In' then begin
         Enter( 'In.Open', catStProc, typNone, spInOpen );
         Enter( 'In.Int', catStProc, typNone, spInInt );
         end
      else if Name = 'Out' then begin
         Enter( 'Out.Int', catStProc, typNone, spOutInt );
         Enter( 'Out.Ln', catStProc, typNone, spOutLn );
         end
      else
         Error('��������� �����');
      NextLex;
      end
   else
      Expected('��� �������㥬��� �����');
end;

(* IMPORT ��� { "," ��� } ";" *)
procedure Import;
begin
   Check(lexIMPORT, 'IMPORT');
   ImportModule;    {��ࠡ�⪠ ����� �������㥬��� �����}
   while Lex = lexComma do begin
      NextLex;
      ImportModule; {��ࠡ�⪠ ����� �������㥬��� �����}
   end;
   Check(lexSemi, '";"');
end;

(* MODULE ��� ";" [������] ��ᫎ��
   [BEGIN ��ᫎ����஢] END ��� "." *)
procedure Module;
var
   ModRef: tObj; {��뫪� �� ��� ����� � ⠡���}
begin
   Check(lexMODULE, 'MODULE');
   if Lex <> lexName then
      Expected('��� �����')
   else {��� ����� - � ⠡���� ����}
      NewName(Name, catModule, ModRef);
   NextLex;
   Check(lexSemi, '";"');
   if Lex = lexIMPORT then
      Import;
   DeclSeq;
   if Lex = lexBEGIN then begin
      NextLex;
      StatSeq;
   end;
   Check(lexEND, 'END');

   {�ࠢ����� ����� ����� � ����� ��᫥ END}
      if Lex <> lexName then
         Expected('��� �����')
      else if Name <> ModRef^.Name then
         Expected('��� ����� "'+ModRef^.Name+'"')
      else
         NextLex;
   if Lex <> lexDot then
      Expected('"."');
   Gen(0);            {��� ������}
   Gen(cmStop);       {������� ��⠭���}
   AllocateVariables; {�����饭�� ��६�����}
end;

procedure Compile;
begin
   InitNameTable;
   OpenScope; {���� �⠭������ ����}
   Enter( 'ABS', catStProc, typInt, spABS );
   Enter( 'MAX', catStProc, typInt, spMAX );
   Enter( 'MIN', catStProc, typInt, spMIN );
   Enter( 'DEC', catStProc, typNone, spDEC );
   Enter( 'ODD', catStProc, typBool, spODD );
   Enter( 'HALT', catStProc, typNone, spHALT );
   Enter( 'INC', catStProc, typNone, spINC );
   Enter( 'INTEGER', catType, typInt, 0 );
   OpenScope;  {���� �����}
   Module;
   CloseScope; {���� �����}
   CloseScope; {���� �⠭������ ����}
   WriteLn;
   WriteLn('��������� �����襭�');
end;

end.

