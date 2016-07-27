/* ������� ��������� ������ (text.c) */

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "text.h"
#include "location.h"

#define TABSIZE   3
#define TRUE      1
#define FALSE     0

char ResetError = TRUE;
char* Message = (char*)"���� �� ������";
int Ch = chEOT;

static FILE *f;

void NextCh() {
	if( (Ch = fgetc(f)) == EOF )
		Ch = chEOT;
	else if( Ch == '\n' ) {
		puts("");
		Line++;
		Pos = 0;
		Ch = chEOL;
	} else if( Ch == '\r' )
		NextCh();
	else if( Ch != '\t' ) {
		putchar(Ch);
		Pos++;
	} else
		do
			putchar(' ');
		while( ++Pos % TABSIZE );
}

void ResetText() {
	if( Path == NULL ) {
		puts("������ ������:\n   O <������� ����>");
		exit(1);
	} else if( (f = fopen(Path, "r")) == NULL ) {
		ResetError = TRUE;
		Message = (char*)"������� ���� �� ������";
	} else {
		ResetError = FALSE;
		Message = (char*)"Ok";
		Pos = 0;
		Line = 1;
		NextCh();
	}
}

void CloseText() {
	fclose(f);
}

