/* ������� ��������� ������ (text.h) */
#ifndef TEXT
#define TEXT

#define  chSpace ' '    /*������      */
#define  chTab   '\t'   /*���������   */
#define  chEOL   '\n'   /*����� ������*/
#define  chEOT   '\0'   /*����� ������*/

extern char ResetError;
extern char* Message;
extern int Ch;

void ResetText(void);
void CloseText(void);
void NextCh(void);

#endif
