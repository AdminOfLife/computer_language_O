/* ������� ���� (table.h) */
#ifndef TABLE
#define TABLE

#include "scan.h"

/*��������� ���*/
typedef enum {
   catConst, catVar, catType,
   catStProc, catModule, catGuard
}tCat ;

/*����*/
typedef enum {
   typNone, typInt, typBool
}tType ;

typedef struct tObjDesc{   /* ��� ������ ������� ���� */
   char Name[NAMELEN+1];   /* ���� ������             */
   tCat Cat;               /* ��������� �����         */
   tType Typ;              /* ���                     */
   int Val;                /* ��������                */
   struct tObjDesc* Prev;  /* ��������� �� ����. ���  */
} tObj;

/* ������������� ������� */
   void InitNameTable(void);
/* ���������� ��������*/
   void Enter(char* N, tCat C, tType T, int V);
/* ��������� ������ ����� */
   tObj* NewName(char* Name, tCat Cat);
/* ����� ����� */
   tObj* Find(char* Name);
/* �������� ������� ��������� (�����) */
   void OpenScope(void);
/* �������� ������� ��������� (�����) */
   void CloseScope(void);
/* ����� ������ ����������*/
   tObj* FirstVar(void);
/* ����� ��������� ���������� */
   tObj* NextVar(void);

#endif
