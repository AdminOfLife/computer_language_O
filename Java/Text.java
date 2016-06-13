// �ࠩ��� ��室���� ⥪��

import java.io.*;

class Text {

   static final int  TABSIZE = 3;
   static final char chSPACE = ' ';    // �஡��      
   static final char chTAB   = '\t';   // �������   
   static final char chEOL   = '\n';   // ����� ��ப�
   static final char chEOT   = '\0';   // ����� ⥪��

   static boolean Ok = false;
   static String Message = "���� �� �����";
   static int Ch = chEOT;

   private static InputStream f;

   static void NextCh() {
      try {
         if( (Ch = f.read()) == -1 )
            Ch = chEOT;
         else if( Ch == '\n' ) {
            System.out.println();
            Location.Line++; Location.Pos = 0; Ch = chEOL;
            }
         else if( Ch == '\r' )
            NextCh();
         else if( Ch != '\t' ) {
            System.out.write(Ch); Location.Pos++;
            }
         else
            do
               System.out.print(' ');
            while( ++Location.Pos % TABSIZE != 0 );
      } catch (IOException e) {};
   }

   static void Reset() {
      if( Location.Path == null ) {
         System.out.println(
            "��ଠ� �맮��:\n   O <�室��� 䠩�>");
         System.exit(1);
         }
      else 
         try {
            f = new FileInputStream(Location.Path);
            Ok = true; Message = "Ok";
            Location.Pos = 0; Location.Line = 1;
            NextCh(); 
         } catch (IOException e) {
            Ok = false;
            Message = "�室��� 䠩� �� ������";
         }
   }

   static void Close() {
      try {
         f.close();
      } catch (IOException e) {};
   }

}
