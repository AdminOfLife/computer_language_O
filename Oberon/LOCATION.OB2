MODULE Location;
VAR
   Line*    : INTEGER; (* ����� ��ப� *)
   Pos*     : INTEGER; (* ����� ����樨 *)
   LexPos*  : INTEGER; (* ������ ��砫� ���ᥬ� *)
   Path*    : ARRAY 256 OF CHAR;
BEGIN
   Line := 0;
   Pos := 0;
   LexPos := 0;
   Path := "";
END Location.
