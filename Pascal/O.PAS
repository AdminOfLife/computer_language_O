program O;
{��������� �몠 �}

uses
  OText, OScan, OPars, OVM, OGen;

procedure Init;
begin
   ResetText;
   InitScan;
   InitGen;
end;

procedure Done;
begin
   CloseText;
end;

begin
   WriteLn('��������� �몠 �');
   Init;    {���樠������}
   Compile; {���������}
   Run;     {�믮������}
   Done;    {�����襭��}
end.


