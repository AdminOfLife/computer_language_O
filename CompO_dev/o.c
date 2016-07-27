/* ���������� ����� � (o.c) */

#include <string.h>
#include <stdio.h>

#include "text.h"
#include "scan.h"
#include "ovm.h"
#include "scan.h"
#include "gen.h"
#include "location.h"
#include "pars.h"
#include "error.h"
#include "ovmtobin.h"

void Init(void) {
	ResetText();
	if( ResetError )
		Error(Message);
	InitScan();
	InitGen();
}

void Done(void) {
	CloseText();
}

int main(int argc, char *argv[]) {
	puts((char*)"\n���������� ����� �");
	if( argc <= 1 )
		Path = NULL;
	else
		Path = argv[1];
	Init();    /* ������������� */
	Compile(); /* ����������    */
	Test_Out_M();
	Run();     /* ����������    */
	Done();    /* ����������    */
	return 0;
}

