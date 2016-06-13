// ��������� ������
using System;

class Error {

public static void Message(string Msg) {
   int ELine = Location.Line;
   while( Text.Ch != Text.chEOL && Text.Ch != Text.chEOT )
      Text.NextCh();
   if( Text.Ch == Text.chEOT ) Console.WriteLine();
   for( int i = 1; i < Location.LexPos; i++ )
      Console.Write(' ');
   Console.WriteLine("^");
   Console.WriteLine(
      "(������ " + ELine + ") ������: " + Msg
   );
   Console.WriteLine();
   Console.Write("������� ����");
   Console.ReadLine();
   Environment.Exit(0);
}

public static void Expected(string Msg) {
   Message("��������� " + Msg);
}

public static void Warning(string Msg) {
   Console.WriteLine();
   Console.WriteLine("��������������: " + Msg);
}

}
