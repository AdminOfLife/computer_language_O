unit AsmTable;
{ ������ ����        }
{ �.���फ�� 11.05.97 }

interface

uses
	AsmScan;

type

	tObj = ^tObjRec;

	tObjRec = record
		Name        : tName;
		Addr        : integer;
		Prev        : tObj;
	end;

procedure InitNameTable;
procedure NewName( Addr : integer );
procedure Find( var Addr : integer );

{=========================================================================}

implementation

uses
	OError;

var
	Top   : tObj;

procedure InitNameTable;
begin
	Top := nil;
end;

procedure NewName(Addr : integer);
var
	Obj : tObj;
begin
	Obj := Top;
	while ( Obj <> nil) and ( Obj^.Name <> Name ) do
		Obj := Obj^.Prev;
	if Obj = nil then begin
		New(Obj);
		Obj^.Name := Name;
		Obj^.Addr := Addr;
		Obj^.Prev := Top;
		Top := Obj;
		end
	else
		Error('����୮� ������� �����');
end;

procedure Find(var Addr : integer);
var
	Obj : tObj;
begin
	Obj := Top;
	while ( Obj <> nil ) and ( Obj^.Name <> Name ) do
		Obj := Obj^.Prev;
	if Obj=nil then
		Error('���������� ���')
	else
		Addr := Obj^.Addr;
end;


end.
