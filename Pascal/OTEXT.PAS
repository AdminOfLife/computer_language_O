unit OText;
{�ࠩ��� ��室���� ⥪��}

interface

const
   chSpace = ' ';     {�஡��      }
   chTab   = chr(9);  {�������   }
   chEOL   = chr(10); {����� ��ப�}
   chEOT   = chr(0);  {����� ⥪��}

var
   Ch    : char;    {��।��� ᨬ���      }
   Line  : integer; {����� ��ப�          }
   Pos   : integer; {����� ᨬ���� � ��ப�}

procedure ResetText;
procedure CloseText;
procedure NextCh;

{======================================================}

implementation

uses
   OError;

const
   TabSize = 3;

var
   f : text;

procedure ResetText;
begin
   if ParamCount < 1 then begin
      Writeln('��ଠ� �맮��:');
      Writeln('   O <�室��� 䠩�>');
      Halt;
      end
   else begin
      Assign(f, ParamStr(1));
      {$i-} Reset(f); {$i+}
      if IOResult <> 0 then
         Error('�室��� 䠩� �� ������')
      else begin
         Pos := 0;
         Line := 1;
         NextCh;
      end;
   end;
end;

procedure CloseText;
begin
   Close(f);
end;

procedure NextCh;
begin
   if eof(f) then
      Ch := chEOT
   else if eoln(f) then begin
      ReadLn(f);
      WriteLn;
      Line := Line + 1;
      Pos := 0;
      Ch := chEOL;
      end
   else begin
      Read(f, Ch);
      if Ch <> chTab then begin
         Write(Ch);
         Pos := Pos+1;
         end
      else
         repeat
            Write(' ');
            Pos := Pos+1;
         until Pos mod TabSize = 0;
   end;
end;

end.

