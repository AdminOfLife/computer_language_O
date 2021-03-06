MODULE  Error;
(* ��ࠡ�⪠ �訡�� *)

IMPORT Location, Text, Utils, Out, In, Strings;

PROCEDURE Message*(Msg : ARRAY OF CHAR);
VAR
   ELine : INTEGER;
   i     : INTEGER;
BEGIN
   ELine := Location.Line;
   WHILE (Text.Ch # Text.chEOL) & (Text.Ch # Text.chEOT)
   DO Text.NextCh
   END;
   IF Text.Ch = Text.chEOT THEN Out.Ln END;
   i := 1;
   WHILE i < Location.LexPos DO
      Out.Char(" ");
      INC(i);
   END;
   Out.String("^"); Out.Ln;
   Out.String("(��ப� "); Out.Int(ELine, 0);
   Out.String(") �訡��: "); Out.String(Msg); Out.Ln;
   Out.String("������ ����");
   Utils.ReadLn;
   HALT(1);
END Message;

PROCEDURE Expected*(Msg: ARRAY OF CHAR);
VAR
   s : ARRAY 80 OF CHAR;
BEGIN
   s := "��������� ";
   Strings.Append(Msg, s);
   Message(s);
END Expected;

PROCEDURE Warning*(Msg : ARRAY OF CHAR);
BEGIN
   Out.Ln;
   Out.String("�।�०�����: ");
   Out.String(Msg);
   Out.Ln;
END Warning;

END Error.
