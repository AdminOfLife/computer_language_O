{��ᥬ���� ����㠫쭮� ��設� ���}
uses
	AsmText, AsmScan, AsmTable, AsmUnit, OVM;

procedure Init;
begin
	AsmTable.InitNameTable;
	AsmText.OpenText;
	AsmScan.InitScan;
end;

procedure Done;
begin
	AsmText.CloseText;
end;

begin
	WriteLn('��ᥬ���� ����㠫쭮� �-��設�');
	Init;
	AsmUnit.Assemble;
	OVM.Run;
	Done;
end.

