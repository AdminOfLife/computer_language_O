using System;

// ������� ������� ����
public class Obj {      // ��� ������ ������� ����
   public string  Name; // ���� ������            
   public tCat    Cat;  // ��������� �����        
   public tType   Type; // ���                    
   public int     Val;  // ��������               
   public Obj     Prev; // ��������� �� ����. ��� 
}

// ��������� ���
   public enum tCat {
      Const, Var, Type, StProc, Module, Guard
   };

// ����
   public enum tType {
      None, Int, Bool
   }

// ������� ����
class Table {

private static Obj   Top;    //��������� �� ������� ������    
private static Obj   Bottom; //��������� �� ����� (���) ������
private static Obj   CurrObj;

// ������������� �������
public static void Init() {
   Top = null;
}

// ���������� ��������
public static void Enter(string N, tCat C, tType T, int V) {
   Obj P = new Obj();
   P.Name = String.Copy(N);
   P.Cat = C;
   P.Type = T;
   P.Val = V;
   P.Prev = Top;
   Top = P;
}

public static void OpenScope() {
   Enter("", tCat.Guard, tType.None, 0);
   if ( Top.Prev == null )
      Bottom = Top;
}

public static void CloseScope() {
   while( Top.Cat != tCat.Guard ){
      Top = Top.Prev;
   }
   Top = Top.Prev;
}

public static Obj NewName(string Name, tCat cat) {
   Obj obj = Top;

   while( obj.Cat != tCat.Guard && obj.Name != Name )
      obj = obj.Prev;
   if ( obj.Cat == tCat.Guard ) {
      obj = new Obj();
      obj.Name = String.Copy(Name);
      obj.Cat = cat;
      obj.Val = 0;
      obj.Prev = Top;
      Top = obj;
      }
   else
      Error.Message("��������� ���������� �����");
   return obj;
}

public static Obj Find(string Name) {
   Obj obj;

   Bottom.Name = String.Copy(Name);
   for( obj=Top; obj.Name != Name; obj=obj.Prev );
   if( obj == Bottom )
      Error.Message("������������� ���");
   return obj;
}

public static Obj FirstVar() {
   CurrObj = Top;
   return NextVar();
}

public static Obj NextVar() {
   Obj VRef;

   while( CurrObj != Bottom && CurrObj.Cat != tCat.Var )
      CurrObj = CurrObj.Prev;
   if( CurrObj == Bottom )
      return null;
   else {
      VRef = CurrObj;
      CurrObj = CurrObj.Prev;
      return VRef;
   }
}

}
