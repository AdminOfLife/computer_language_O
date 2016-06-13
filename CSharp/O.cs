// ���������� ����� "�"
using System;

public class O {

static void Init() {
   Text.Reset();
   if( !Text.Ok )
      Error.Message(Text.Message);
   Scan.Init();
   Gen.Init();
}

static void Done() {
   Text.Close();
}

static void Main(string[] args) {
   Console.WriteLine("\n���������� ����� �");
   if( args.Length == 0 )
      Location.Path = null;
   else
      Location.Path = args[0];
   Init();         // �������������
   Pars.Compile(); // ����������
   OVM.Run();      // ����������
   Done();         // ����������
}

}
